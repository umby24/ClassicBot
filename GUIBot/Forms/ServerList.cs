using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicBot.Classes;

namespace GUIBot.Forms {
    public partial class ServerList : Form {
        private readonly SettingsForm _settingsForm;
        private ClassicubeServer[] _listing;

        public ServerList(SettingsForm sf) {
            _settingsForm = sf;
            InitializeComponent();
        }

        private void ServerList_Load(object sender, EventArgs e) {
            if (!_settingsForm.LoginWorker.Login()) {
                MessageBox.Show(
                    "Failed to get server list!" + Environment.NewLine + "Incorrect UN or PW for Classicube.net",
                    "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            var listing = _settingsForm.LoginWorker.GetAllServers();
            _listing = listing;

            foreach (var server in _listing) {
                var temp = new ListViewItem(server.Name);
                temp.SubItems.Add(server.OnlinePlayers + "/" + server.MaxPlayers);
                temp.SubItems.Add(server.Software);
                listView1.Items.Add(temp);
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            listView1.Items.Clear();

            var listing = _settingsForm.LoginWorker.GetAllServers();
            _listing = listing;

            foreach (var server in _listing) {
                var temp = new ListViewItem(server.Name);
                temp.SubItems.Add(server.OnlinePlayers + "/" + server.MaxPlayers);
                temp.SubItems.Add(server.Software);
                listView1.Items.Add(temp);
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            if (listView1.SelectedIndices.Count == 0)
                return;
            
            var server = _listing[listView1.SelectedIndices[0]];
            _settingsForm.Mf.MakeConnection(_settingsForm.LoginWorker.Username, _settingsForm.LoginWorker.Password, false, "", server.Ip, server.Port, server.Mppass);
            _settingsForm.Close();
            Close();
        }
    }
}
