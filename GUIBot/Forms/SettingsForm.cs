using System;
using System.ComponentModel;
using System.Windows.Forms;
using ClassicBot.Classes;
using GUIBot.Libraries;

namespace GUIBot.Forms {
    public partial class SettingsForm : Form {
        public ClassicubeInteraction LoginWorker;
        public MainForm Mf;
        private Settings _sFile;
        #region Settings

        public string Username, Password, CommandPrefix;
        private int _port;
        private bool _verifyNames;
        #endregion

        public SettingsForm(MainForm mainForm) {
            Mf = mainForm;
            InitializeComponent();
        }

        private void btnServers_Click(object sender, EventArgs e) {
            LoginWorker.Username = txtUN.Text;
            LoginWorker.Password = txtPW.Text;

            var listing = new ServerList(this);
            listing.Show();
        }

        private void Settings_Load(object sender, EventArgs e) {
            _sFile = Mf.ProgramSettings.RegisterFile("Settings.ini", true, LoadSettings);
            Mf.ProgramSettings.ReadSettings(_sFile);

            LoginWorker = new ClassicubeInteraction(Username, Password);

            txtUN.Text = Username;
            txtPW.Text = Password;
            //txtPort.Text = _port.ToString();
            chkVerify.Checked = _verifyNames;
        }

        private void SaveSettings() {
            Mf.ProgramSettings.SelectGroup(_sFile, "Settings");
            Mf.ProgramSettings.SaveSetting(_sFile, "un", txtUN.Text);
            Mf.ProgramSettings.SaveSetting(_sFile, "pw", txtPW.Text);
            //Mf.ProgramSettings.SaveSetting(_sFile, "port", txtPort.Text);
            Mf.ProgramSettings.SaveSetting(_sFile, "verify", chkVerify.Checked.ToString());
            Mf.ProgramSettings.SaveSetting(_sFile, "prefix", txtPrefix.Text);

            Mf.ProgramSettings.SelectGroup(_sFile, "Admins");
            Mf.ProgramSettings.SelectGroup(_sFile, "Favorites");
            Mf.ProgramSettings.SelectGroup(_sFile, "Design");

            Mf.ProgramSettings.SaveSettings(_sFile);
        }

        private void SettingsForm_Closing(object sender, CancelEventArgs e) {
            SaveSettings();
        }

        private void LoadSettings() {
            Mf.ProgramSettings.SelectGroup(_sFile, "Settings");
            Username = Mf.ProgramSettings.ReadSetting(_sFile, "un", "");
            Password = Mf.ProgramSettings.ReadSetting(_sFile, "pw", "");
            _port = Mf.ProgramSettings.ReadSetting(_sFile, "port", 25565);
            _verifyNames = bool.Parse(Mf.ProgramSettings.ReadSetting(_sFile, "verify", "true"));
            CommandPrefix = Mf.ProgramSettings.ReadSetting(_sFile, "prefix", "!");

            Mf.ProgramSettings.SelectGroup(_sFile, "Admins");
            Mf.ProgramSettings.SelectGroup(_sFile, "Favorites");
            Mf.ProgramSettings.SelectGroup(_sFile, "Design");

            Mf.ProgramSettings.SaveSettings(_sFile);
        }
    }
}
