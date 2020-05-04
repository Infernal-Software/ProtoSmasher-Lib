using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtoSmasher_Lib;

namespace SampleApp
{
    class Program
    {
        private ProtoSmasherLib _protoSmasher;
        private TaskCompletionSource<bool> _attachAwait;
        private bool _isAlive;

        public Program()
        {
            _isAlive = false;

            _attachAwait = new TaskCompletionSource<bool>();
            _protoSmasher = new ProtoSmasherLib();
            _protoSmasher.OnDettach += _protoSmasher_OnDettach;
            _protoSmasher.OnAttach += _protoSmasher_OnAttach;
            _protoSmasher.OnOutput += _protoSmasher_OnOutput;
            _protoSmasher.OnSettings += _protoSmasher_OnSettings;
        }

        private void _protoSmasher_OnSettings(object sender, ProtoSmasher_Lib.Events.Settings e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Output] Got Settings: {0}", JsonConvert.SerializeObject(e));

            e.ChamsEnable = true;
            e.RaiseProperty("ChamsEnable");
        }

        public async Task StartAsync()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (await _protoSmasher.AttachAsync())
            {
                Console.WriteLine("[Debug] Successfully connected to ProtoSmasher");
                await _attachAwait.Task;
            } else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("[Error] Failed to connect to ProtoSmasher");
            }

            await Task.Run(() =>
            {
                while (_isAlive)
                {
                    var code = Console.ReadLine();
                    _protoSmasher.ExecuteScript(code);
                }
            });


            await Task.Delay(-1);
        }

        // source: https://stackoverflow.com/questions/1988833/converting-color-to-consolecolor
        // too lazy to make my own
        static ConsoleColor ClosestConsoleColor(byte r, byte g, byte b)
        {
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }

        static void Main(string[] args)
        {
            new Program().StartAsync().GetAwaiter().GetResult();
        }

        #region Library Events
        private void _protoSmasher_OnDettach(object sender, EventArgs e)
        {
            _isAlive = false;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[Debug] Detached from ProtoSmasher");
        }

        private void _protoSmasher_OnOutput(object sender, ProtoSmasher_Lib.Events.Output e)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (Console.CursorLeft == 0)
                Console.Write("[Output] ");

            var color = ClosestConsoleColor(e.Color.R, e.Color.G, e.Color.B);
            Console.ForegroundColor = color;

            if (e.NewLine)
                Console.WriteLine("{0}", e.Message);
            else
                Console.Write("{0}", e.Message);
        }

        private void _protoSmasher_OnAttach(object sender, EventArgs e)
        {
            _isAlive = true;
            _attachAwait.TrySetResult(_isAlive);
        }
        #endregion
    }
}
