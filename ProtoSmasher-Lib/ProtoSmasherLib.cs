using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace ProtoSmasher_Lib
{
    /// <summary>
    /// All interaction between ProtoSmasher externally
    /// </summary>
    public class ProtoSmasherLib
    {
        #region Internal Members
        private static readonly int    SOCKET_PORT = 45859;
        private static readonly string SOCKET_URI = $"ws://127.0.0.1:{SOCKET_PORT}";
        private static readonly string SOCKET_USERAGENT = "ProtoSmasher/External 1.0";

        private readonly string _path;
        private readonly Injector _injector;
        private readonly WebSocket _webSocket;

        private bool _isIPCAlive => _webSocket?.State == WebSocketState.Open;
        #endregion

        #region Public Members
        /// <summary>
        /// Event fires as soon as IPC with ProtoSmasher has opened, typically followed by the clear console event
        /// </summary>
        public event EventHandler OnAcknowledged;

        /// <summary>
        /// Event fires when ProtoSmasher has attached to Roblox
        /// </summary>
        public event EventHandler OnAttach;

        /// <summary>
        /// Event fires when ProtoSmasher has been closed
        /// </summary>
        public event EventHandler OnDettach;
        
        /// <summary>
        /// Event fires when the library gets a new output event from ProtoSmasher 
        /// </summary>
        public event EventHandler<Events.Output> OnOutput;

        /// <summary>
        /// Event firess when the lbirary gets a clear console event from ProtoSmasher
        /// </summary>
        public event EventHandler OnClearConsole;

        /// <summary>
        /// Event fires when ProtoSmasher settings update, or when attached to ProtoSmasher
        /// </summary>
        public event EventHandler<Events.Settings> OnSettings;

        /// <summary>
        /// Event fires if attempt to update setting failed, with the failure reason
        /// </summary>
        public event EventHandler<Events.SettingUpdateFailed> OnUpdateSettingFailure;
        #endregion

        #region Public Member Functions
        /// <summary>
        /// Initializes the library, with the path to ProtoSmasher and the Auto-Attach feature
        /// </summary>
        /// <param name="directory">Path to ProtoSmasher</param>
        public ProtoSmasherLib(string directory)
        {
            _path = directory;

            if (!Directory.Exists(_path))
                throw new Exception("Directory does not exist");

            if (!File.Exists(Path.Combine(_path, "ProtoSmasher.dll")))
                throw new Exception("ProtoSmasher does not exist in path");

            if (!File.Exists(Path.Combine(_path, "ProtoSmasher.exe")))
                throw new Exception("ProtoSmasher injector does not exist in path");


            _injector = new Injector(_path, SOCKET_PORT);
            _webSocket = new WebSocket(SOCKET_URI, userAgent: SOCKET_USERAGENT);
            _webSocket.Opened += IPCOnOpen;
            _webSocket.Closed += IPCOnClose;
            _webSocket.MessageReceived += IPCOnMessage;
        }

        /// <summary>
        /// Initializes the library, without the Auto-Attach feature
        /// </summary>
        public ProtoSmasherLib()
        {
            _webSocket = new WebSocket(SOCKET_URI, userAgent: SOCKET_USERAGENT);
            _webSocket.Opened += IPCOnOpen;
            _webSocket.Closed += IPCOnClose;
            _webSocket.MessageReceived += IPCOnMessage;
        }

        /// <summary>
        /// Attempts to connect to the ProtoSmasher client
        /// </summary>
        /// <returns>If the connection to the client was successful</returns>
        public bool Attach()
        {
            var result = Connect();
            if (!result)
            {
                if (_injector != null)
                    _injector.Attach();

                result = Connect();
            }

            return result;
        }

        /// <summary>
        /// See Attach, but aync
        /// </summary>
        /// <returns>If the connection was successful</returns>
        public async Task<bool> AttachAsync()
        {
            var result = await ConnectAsync().ConfigureAwait(false);
            if (!result)
            {
                if (_injector != null)
                    await _injector.AttachAsync().ConfigureAwait(false);

                result = await ConnectAsync().ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Executes the script on ProtoSmasher
        /// </summary>
        /// <param name="script"></param>
        public void ExecuteScript(string script)
        {
            Send(new
            {
                Action = (int)ActionTypes.ExecuteSource,
                Value = script
            });
        }

        /// <summary>
        /// See ExecuteScript, but async
        /// </summary>
        /// <param name="script"></param>
        public async Task ExecuteScriptAsync(string script)
        {
            await SendAsync(new
            {
                Action = (int)ActionTypes.ExecuteSource,
                Value = script
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the files source code on ProtoSmasher
        /// </summary>
        /// <param name="file">File path</param>
        public void ExecuteFile(string file)
        {
            Send(new
            {
                Action = (int)ActionTypes.ExecuteFile,
                Value = file
            });
        }

        /// <summary>
        /// See ExecuteFile, but async
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task ExecuteFileAsync(string file)
        {
            await SendAsync(new
            {
                Action = (int)ActionTypes.ExecuteFile,
                Value = file
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a setting on ProtoSmasher, if fails it calls the UpdateSettingFailed cevent
        /// </summary>
        /// <param name="setting">Name of the setting</param>
        /// <param name="value">Value of the setting</param>
        public void UpdateSetting(string setting, object value)
        {
            Send(new
            {
                Action = (int)ActionTypes.UpdateSetting,
                Name = setting,
                Value = value
            });
        }

        /// <summary>
        /// See UpdateSetting, but async
        /// </summary>
        /// <param name="setting">Name of the setting</param>
        /// <param name="value">Value of the setting</param>
        /// <returns>Task</returns>
        public async Task UpdateSettingAsync(string setting, object value)
        {
            await SendAsync(new
            {
                Action = (int)ActionTypes.UpdateSetting,
                Name = setting,
                Value = value
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Requests the current settings from ProtoSmasher (calls back to OnSettings)
        /// </summary>
        public void RequestSettings()
        {
            Send(new
            {
                Action = (int)ActionTypes.GetSettings
            });
        }

        /// <summary>
        /// See RequestSettings, but async
        /// </summary>
        /// <returns></returns>
        public async Task RequestSettingsAsync()
        {
            await SendAsync(new
            {
                Action = (int)ActionTypes.GetSettings
            }).ConfigureAwait(false);
        }
        #endregion

        #region Internal Member Functions
        private async Task<bool> ConnectAsync()
        {
            if (_isIPCAlive || !Injector.IsPortInUse(SOCKET_PORT))
                return false;

            return await _webSocket.OpenAsync();
        }

        private bool Connect()
        {
            if (_isIPCAlive || !Injector.IsPortInUse(SOCKET_PORT))
                return false;

            _webSocket.Open();
            return _webSocket.State == WebSocketState.Open;
        }

        private Task<bool> SendAsync(object data)
        {
            return Task.Run(() => Send(data));
        }

        private bool Send(object data)
        {
            if (!_isIPCAlive)
                return false;

            var json = JsonConvert.SerializeObject(data);
            _webSocket.Send(json);
            
            return true;
        }
        #endregion

        #region Internal WebSocket Events
        private void IPCOnOpen(object sender, EventArgs e)
        {
            OnAttach?.Invoke(sender, new EventArgs());
        }

        private void IPCOnClose(object sender, EventArgs e)
        {
            OnDettach?.Invoke(sender, new EventArgs());
        }
        
        private void IPCOnMessage(object sender, MessageReceivedEventArgs e)
        {
            var actionData = Action.FromJson(e.Message);
            switch(actionData?.Type)
            {
                case ActionTypes.Acknowledged:
                    OnAcknowledged?.Invoke(this, new EventArgs());
                    break;
                case ActionTypes.ClearConsole:
                    OnClearConsole?.Invoke(this, new EventArgs());
                    break;
                case ActionTypes.Output:
                    OnOutput?.Invoke(this, new Events.Output(actionData.Body));
                    break;
                case ActionTypes.GetSettings:
                    OnSettings?.Invoke(this, new Events.Settings(this, e.Message));
                    break;
                case ActionTypes.UpdateSettingFailed:
                    OnUpdateSettingFailure?.Invoke(this, new Events.SettingUpdateFailed(actionData.Body));
                    break;
            }
        }
        #endregion
    }
}
