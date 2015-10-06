using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Windows.Forms;
using ClassicBot;
using GUIBot.Libraries;

namespace GUIBot {
    public partial class MainForm : Form {
        public PbSettingsLoader ProgramSettings;
        private Bot _bot;

        public MainForm() {
            InitializeComponent();
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e) {
            var settingsForm = new Forms.SettingsForm(this);
            settingsForm.Show();
        }

        private void MainForm_Load(object sender, EventArgs e) {
            ProgramSettings = new PbSettingsLoader();
        }

        #region Connection Setup.
        public void MakeConnection(string username, string password, bool verifyNames, string serverUrl, string ip = "", int port = 25565, string mppass = "") {
            _bot = new Bot(true, username, password, verifyNames);
            _bot.ClientSupportedExtensions.Add(CPEExtensions.CustomBlocks);
            _bot.ClientSupportedExtensions.Add(CPEExtensions.ExtPlayerList);
            RegisterEvents();

            if (ip == "")
                _bot.Connect(serverUrl, true);
            else {
                _bot.MpPass = mppass;
                _bot.Connect("", false, ip, port);
            }
        }

        public void RegisterEvents() {
            _bot.ChatMessage += _bot_ChatMessage;
            _bot.Disconnected += _bot_Disconnected;
            _bot.ErrorMessage += _bot_ErrorMessage;
            _bot.InfoMessage += _bot_InfoMessage;
            _bot.ExtPlayerListUpdate += _bot_ExtPlayerListUpdate;
        }

        public void DeregisterEvents() {
            _bot.ChatMessage -= _bot_ChatMessage;
            _bot.Disconnected -= _bot_Disconnected;
            _bot.ErrorMessage -= _bot_ErrorMessage;
            _bot.InfoMessage -= _bot_InfoMessage;
            _bot.ExtPlayerListUpdate -= _bot_ExtPlayerListUpdate;
        }
        #endregion
        #region Event Handling
        void _bot_InfoMessage(string message) {
            Invoke(new StupidArgs(LogColor), "[INFO] " + message, Color.FromArgb(0, 170, 0), true);
        }

        void _bot_ErrorMessage(string message) {
            Invoke(new StupidArgs(LogColor), "[ERROR] " + message, Color.Red, true);
        }

        void _bot_Disconnected(string message) {
            Invoke(new StupidArgs(LogColor), "[DISCONNECTED] " + message, Color.Red, true);
        }

        void _bot_ChatMessage(string message) {
            Invoke(new StupidArgs2(ParseColors), message);
        }

        void _bot_ExtPlayerListUpdate() {
            Invoke(new Bot.BlankEventArgs(UpdateExtPlayerList));
        }

        void UpdateExtPlayerList() {
            treePlayers.Nodes.Clear();

            var stuff = new Dictionary<string, TreeNode>();

            foreach (var item in _bot.NumberPlayerList.Values) {
                if (stuff.ContainsKey(item.GroupName))
                    stuff[item.GroupName].Nodes.Add(item.ListName);
                else {
                    stuff.Add(item.GroupName, new TreeNode(item.GroupName));
                    stuff[item.GroupName].Nodes.Add(item.ListName);
                    stuff[item.GroupName].Toggle();
                }
            }

            foreach (var node in stuff.Values) {
                treePlayers.Nodes.Add(node);
            }
        }
        #endregion
        #region FormLogging

        static Color MinecraftColor(char colorCode) {
            switch (colorCode) {
                case '0':
                    return Color.Black;
                case '1':
                    return Color.FromArgb(0, 0, 170);
                case '2':
                    return Color.FromArgb(0, 170, 0);
                case '3':
                    return Color.FromArgb(0, 170, 170);
                case '4':
                    return Color.FromArgb(170, 0, 0);
                case '5':
                    return Color.FromArgb(170, 0, 170);
                case '6':
                    return Color.FromArgb(255, 170, 0);
                case '7':
                    return Color.FromArgb(170, 170, 170);
                case '8':
                    return Color.FromArgb(85, 85, 85);
                case '9':
                    return Color.FromArgb(85, 85, 255);
                case 'a':
                    return Color.FromArgb(85, 255, 85);
                case 'b':
                    return Color.FromArgb(85, 255, 255);
                case 'c':
                    return Color.FromArgb(255, 85, 85);
                case 'd':
                    return Color.FromArgb(255, 85, 255);
                case 'e':
                    return Color.FromArgb(255, 255, 85);
                case 'f':
                    return Color.White;
                default:
                    return Color.Coral;
            }    
        }

        public void ParseColors(string text) {
            int start = text.IndexOf('&');

            if (start == -1) {
				LogColor(text, Color.White, false);
                return;
            }

            if (start > 0) {
                LogColor(text.Substring(0, start), Color.White, false);
            }

            while (start != -1) {
                char code = text[start + 1];
                var thisColor = MinecraftColor(code);

                int next;
                if (thisColor == Color.Coral) {
                    next = text.IndexOf('&', start + 2);

                    if (next == -1)
                        next = text.Length - (start);

                    LogColor(text.Substring(start, next), Color.White, false);
                    start = text.IndexOf('&', start + 2);
                    continue;
                }

                next = text.IndexOf('&', start + 2);

                if (next == -1)
                    next = text.Length - start;
                else
                    next -= start;

                LogColor(text.Substring(start + 2, next - 2), thisColor, false);
                start = text.IndexOf('&', start + 2);
            }
            Log("");
        }

        public void Log(string text) {
            if (InvokeRequired) {
                Invoke(new StupidArgs2(Log), text);
            }
            var oldStart = rtbChat.SelectionStart;
            var oldLen = rtbChat.SelectionLength;
            var restore = (rtbChat.SelectionStart != rtbChat.Text.Length && rtbChat.Focused || rtbChat.SelectionLength > 0);

            rtbChat.AppendText(text + Environment.NewLine);

            if (!restore) {
                rtbChat.SelectionStart = rtbChat.TextLength;
                rtbChat.ScrollToCaret();
            } else 
                rtbChat.Select(oldStart, oldLen);
        }
        private delegate void StupidArgs2(string text);
        public delegate void StupidArgs(string text, Color color, bool newline = true);

        public void LogColor(string text, Color color, bool newline = true) {
            if (InvokeRequired)
                Invoke(new StupidArgs(LogColor), text, color, newline);

            var oldStart = rtbChat.SelectionStart;
            var oldLen = rtbChat.SelectionLength;
            var restore = (rtbChat.SelectionStart != rtbChat.Text.Length && rtbChat.Focused || rtbChat.SelectionLength > 0);
            var oldboxLen = rtbChat.TextLength;

            if (newline) {
                rtbChat.AppendText(text + Environment.NewLine);
                rtbChat.Select(oldboxLen, text.Length + Environment.NewLine.Length);
            } else {
                rtbChat.AppendText(text);
                rtbChat.Select(oldboxLen, text.Length);
            }

            rtbChat.SelectionColor = color;

            if (!restore) {
                rtbChat.SelectionStart = rtbChat.TextLength;
                rtbChat.ScrollToCaret();
            } else {
                rtbChat.Select(oldStart, oldLen);
            }
        }
        #endregion

        private void btnSend_Click(object sender, EventArgs e) {
            if (_bot != null && _bot.Nm != null && _bot.Nm.BaseSock.Connected) {
                _bot.SendChat(tbInput.Text);
                tbInput.Clear();
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_bot != null && _bot.Nm != null && _bot.Nm.BaseSock.Connected) {
                _bot.Disconnect();
                DeregisterEvents();
            }
        }

        private void reconnectToolStripMenuItem_Click(object sender, EventArgs e) {
            rtbChat.Clear();
            if (_bot != null) {
                MakeConnection(_bot.Username, _bot.Password, false, "", _bot.Ip, _bot.Port, _bot.MpPass);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_bot != null && _bot.Nm != null && _bot.Nm.BaseSock.Connected) {
                _bot.Disconnect();
                DeregisterEvents();
            }
            Close();
        }
    }
}
