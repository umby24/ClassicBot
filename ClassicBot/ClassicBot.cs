using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ClassicBot.Common;
using ClassicBot.Network;
using Sockets;

namespace ClassicBot {
    public class ClassicBot {
        public string Username;
        public string Mppass = "";
        public Client McClient;
        public List<string> SupportedExtensions;

        private string _host;
        private int _port;
        
        public ClassicBot(string username, string ip, int port) {
            Username = username;
            _host = ip;
            _port = port;
            var sock = new ClientSocket(_host, _port);
            McClient = new Client(sock, this); // -- Starts the classicbot :x
        }

        public ClassicBot(string username, string ip, int port, string mppass) {
            Username = username;
            Mppass = mppass;
            _host = ip;
            _port = port;
            var sock = new ClientSocket(_host, _port);
            McClient = new Client(sock, this); // -- Starts the classicbot :x
        }

        public void Connect() {
            McClient.Connect();
        }

        #region Events

        public event StringEventArgs InfoMessage;
        public event StringEventArgs DebugMessage;
        public event StringEventArgs VerboseMessage;
        public event StringEventArgs ErrorMessage;
        public event StringEventArgs WarningMessage;

        public event StringEventArgs OnMessage;
        #endregion

        #region Event Invokers
        internal void OnWarningMessage(string value) {
            WarningMessage?.Invoke(value);
        }

        internal void OnErrorMessage(string value) {
            ErrorMessage?.Invoke(value);
        }

        internal void OnVerboseMessage(string value) {
            VerboseMessage?.Invoke(value);
        }

        internal void OnDebugMessage(string value) {
            DebugMessage?.Invoke(value);
        }

        internal void OnInfoMessage(string value) {
            InfoMessage?.Invoke(value);
        }
        #endregion

        internal void OnChatMessage(string value) {
            OnMessage?.Invoke(value);
        }
    }
}


