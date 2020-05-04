using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProtoSmasher_Lib
{
    internal class Injector
    {
        private readonly int _port;
        private readonly string _path;
        private Process _awaitingProcess;

        public Injector(string path, int port)
        {
            _path = path;
            _port = port;
            _awaitingProcess = null;
        }

        public static bool IsPortInUse(int port)
        {
            var tcpConInfos = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners();

            foreach (var tcpi in tcpConInfos)
                if (tcpi.Port == port)
                    return true;

            return false;
        }

        public async Task AttachAsync()
        {
            var process = StartProcess();
            await Task.Run(() => process.WaitForExit()).ConfigureAwait(false);
            _awaitingProcess = null;
            await PollForPort().ConfigureAwait(false);
        }

        public void Attach()
        {
            var process = StartProcess();
            process.WaitForExit();
            _awaitingProcess = null;
            PollForPort().GetAwaiter().GetResult();
        }

        private Task PollForPort()
        {
            var task = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsPortInUse(_port))
                        break;

                    await Task.Delay(100);
                }
            });

            // 10 second timeout
            return Task.WhenAny(task, Task.Delay(10 * 1000));
        }

        private Process StartProcess()
        {
            if (!Directory.Exists(_path))
                throw new Exception("Injector directory does not exist");

            if (!File.Exists(Path.Combine(_path, "ProtoSmasher.exe")))
                throw new Exception("Injector directory does not contain injector");

            if (!File.Exists(Path.Combine(_path, "ProtoSmasher.dll")))
                throw new Exception("Injector directory does not contain ProtoSmasher");

            if(_awaitingProcess == null)
            {
                var injectors = Process.GetProcessesByName("ProtoSmasher.exe");
                if (injectors.Length != 0)
                    _awaitingProcess = injectors[0];
                else
                    _awaitingProcess = Process.Start(new ProcessStartInfo()
                    {
                        FileName = Path.Combine(_path, "ProtoSmasher.exe"),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        UseShellExecute = true
                    });
            }

            return _awaitingProcess;
        }
    }
}
