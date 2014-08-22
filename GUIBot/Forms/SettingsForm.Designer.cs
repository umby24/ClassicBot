using System;

namespace GUIBot.Forms {
    partial class SettingsForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtServerUrl = new System.Windows.Forms.TextBox();
            this.lblServerURL = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lstFavorites = new System.Windows.Forms.ListBox();
            this.grpLogin = new System.Windows.Forms.GroupBox();
            this.chkVerify = new System.Windows.Forms.CheckBox();
            this.btnServers = new System.Windows.Forms.Button();
            this.txtPW = new System.Windows.Forms.TextBox();
            this.lblPW = new System.Windows.Forms.Label();
            this.lblUN = new System.Windows.Forms.Label();
            this.txtUN = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.grpIRC = new System.Windows.Forms.GroupBox();
            this.lblIrcChan = new System.Windows.Forms.Label();
            this.txtIrcChan = new System.Windows.Forms.TextBox();
            this.btnIrcHelp = new System.Windows.Forms.Button();
            this.btnIrcoff = new System.Windows.Forms.Button();
            this.btnIrcConnect = new System.Windows.Forms.Button();
            this.dropIRCDirection = new System.Windows.Forms.ComboBox();
            this.txtNickPass = new System.Windows.Forms.TextBox();
            this.txtIrcUN = new System.Windows.Forms.TextBox();
            this.txtIrcPort = new System.Windows.Forms.TextBox();
            this.txtircServer = new System.Windows.Forms.TextBox();
            this.lblIRCDirection = new System.Windows.Forms.Label();
            this.lblIRCPass = new System.Windows.Forms.Label();
            this.lblIRCUN = new System.Windows.Forms.Label();
            this.lblIRCPort = new System.Windows.Forms.Label();
            this.lblIRCServer = new System.Windows.Forms.Label();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.lblPrefix = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.lblAbout = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.grpLogin.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.grpIRC.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(790, 231);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.btnConnect);
            this.tabPage1.Controls.Add(this.txtServerUrl);
            this.tabPage1.Controls.Add(this.lblServerURL);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.lstFavorites);
            this.tabPage1.Controls.Add(this.grpLogin);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(782, 205);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Connect";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(17, 172);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(757, 23);
            this.btnConnect.TabIndex = 9;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            // 
            // txtServerUrl
            // 
            this.txtServerUrl.Location = new System.Drawing.Point(17, 146);
            this.txtServerUrl.Name = "txtServerUrl";
            this.txtServerUrl.Size = new System.Drawing.Size(523, 20);
            this.txtServerUrl.TabIndex = 8;
            // 
            // lblServerURL
            // 
            this.lblServerURL.AutoSize = true;
            this.lblServerURL.Location = new System.Drawing.Point(14, 130);
            this.lblServerURL.Name = "lblServerURL";
            this.lblServerURL.Size = new System.Drawing.Size(66, 13);
            this.lblServerURL.TabIndex = 7;
            this.lblServerURL.Text = "Server URL:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(639, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Favorites";
            // 
            // lstFavorites
            // 
            this.lstFavorites.FormattingEnabled = true;
            this.lstFavorites.Location = new System.Drawing.Point(546, 19);
            this.lstFavorites.Name = "lstFavorites";
            this.lstFavorites.Size = new System.Drawing.Size(230, 147);
            this.lstFavorites.TabIndex = 6;
            // 
            // grpLogin
            // 
            this.grpLogin.Controls.Add(this.chkVerify);
            this.grpLogin.Controls.Add(this.btnServers);
            this.grpLogin.Controls.Add(this.txtPW);
            this.grpLogin.Controls.Add(this.lblPW);
            this.grpLogin.Controls.Add(this.lblUN);
            this.grpLogin.Controls.Add(this.txtUN);
            this.grpLogin.Location = new System.Drawing.Point(8, 6);
            this.grpLogin.Name = "grpLogin";
            this.grpLogin.Size = new System.Drawing.Size(245, 115);
            this.grpLogin.TabIndex = 0;
            this.grpLogin.TabStop = false;
            this.grpLogin.Text = "User Info";
            // 
            // chkVerify
            // 
            this.chkVerify.AutoSize = true;
            this.chkVerify.Checked = true;
            this.chkVerify.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVerify.Location = new System.Drawing.Point(9, 81);
            this.chkVerify.Name = "chkVerify";
            this.chkVerify.Size = new System.Drawing.Size(88, 17);
            this.chkVerify.TabIndex = 10;
            this.chkVerify.Text = "Verify Names";
            this.chkVerify.UseVisualStyleBackColor = true;
            // 
            // btnServers
            // 
            this.btnServers.Location = new System.Drawing.Point(103, 77);
            this.btnServers.Name = "btnServers";
            this.btnServers.Size = new System.Drawing.Size(125, 23);
            this.btnServers.TabIndex = 7;
            this.btnServers.Text = "Server List";
            this.btnServers.UseVisualStyleBackColor = true;
            this.btnServers.Click += new System.EventHandler(this.btnServers_Click);
            // 
            // txtPW
            // 
            this.txtPW.Location = new System.Drawing.Point(73, 47);
            this.txtPW.Name = "txtPW";
            this.txtPW.PasswordChar = '*';
            this.txtPW.Size = new System.Drawing.Size(152, 20);
            this.txtPW.TabIndex = 3;
            // 
            // lblPW
            // 
            this.lblPW.AutoSize = true;
            this.lblPW.Location = new System.Drawing.Point(6, 50);
            this.lblPW.Name = "lblPW";
            this.lblPW.Size = new System.Drawing.Size(56, 13);
            this.lblPW.TabIndex = 2;
            this.lblPW.Text = "Password:";
            // 
            // lblUN
            // 
            this.lblUN.AutoSize = true;
            this.lblUN.Location = new System.Drawing.Point(6, 22);
            this.lblUN.Name = "lblUN";
            this.lblUN.Size = new System.Drawing.Size(61, 13);
            this.lblUN.TabIndex = 1;
            this.lblUN.Text = "Username: ";
            // 
            // txtUN
            // 
            this.txtUN.Location = new System.Drawing.Point(73, 19);
            this.txtUN.Name = "txtUN";
            this.txtUN.Size = new System.Drawing.Size(152, 20);
            this.txtUN.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.grpIRC);
            this.tabPage2.Controls.Add(this.txtPrefix);
            this.tabPage2.Controls.Add(this.lblPrefix);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(782, 205);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Bot Settings";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // grpIRC
            // 
            this.grpIRC.Controls.Add(this.lblIrcChan);
            this.grpIRC.Controls.Add(this.txtIrcChan);
            this.grpIRC.Controls.Add(this.btnIrcHelp);
            this.grpIRC.Controls.Add(this.btnIrcoff);
            this.grpIRC.Controls.Add(this.btnIrcConnect);
            this.grpIRC.Controls.Add(this.dropIRCDirection);
            this.grpIRC.Controls.Add(this.txtNickPass);
            this.grpIRC.Controls.Add(this.txtIrcUN);
            this.grpIRC.Controls.Add(this.txtIrcPort);
            this.grpIRC.Controls.Add(this.txtircServer);
            this.grpIRC.Controls.Add(this.lblIRCDirection);
            this.grpIRC.Controls.Add(this.lblIRCPass);
            this.grpIRC.Controls.Add(this.lblIRCUN);
            this.grpIRC.Controls.Add(this.lblIRCPort);
            this.grpIRC.Controls.Add(this.lblIRCServer);
            this.grpIRC.Location = new System.Drawing.Point(479, 6);
            this.grpIRC.Name = "grpIRC";
            this.grpIRC.Size = new System.Drawing.Size(295, 191);
            this.grpIRC.TabIndex = 2;
            this.grpIRC.TabStop = false;
            this.grpIRC.Text = "IRC";
            // 
            // lblIrcChan
            // 
            this.lblIrcChan.AutoSize = true;
            this.lblIrcChan.Location = new System.Drawing.Point(6, 118);
            this.lblIrcChan.Name = "lblIrcChan";
            this.lblIrcChan.Size = new System.Drawing.Size(46, 13);
            this.lblIrcChan.TabIndex = 14;
            this.lblIrcChan.Text = "Channel";
            // 
            // txtIrcChan
            // 
            this.txtIrcChan.Location = new System.Drawing.Point(113, 115);
            this.txtIrcChan.Name = "txtIrcChan";
            this.txtIrcChan.PasswordChar = '*';
            this.txtIrcChan.Size = new System.Drawing.Size(176, 20);
            this.txtIrcChan.TabIndex = 13;
            // 
            // btnIrcHelp
            // 
            this.btnIrcHelp.Location = new System.Drawing.Point(113, 162);
            this.btnIrcHelp.Name = "btnIrcHelp";
            this.btnIrcHelp.Size = new System.Drawing.Size(75, 23);
            this.btnIrcHelp.TabIndex = 12;
            this.btnIrcHelp.Text = "Help";
            this.btnIrcHelp.UseVisualStyleBackColor = true;
            // 
            // btnIrcoff
            // 
            this.btnIrcoff.Location = new System.Drawing.Point(214, 162);
            this.btnIrcoff.Name = "btnIrcoff";
            this.btnIrcoff.Size = new System.Drawing.Size(75, 23);
            this.btnIrcoff.TabIndex = 11;
            this.btnIrcoff.Text = "Turn Off";
            this.btnIrcoff.UseVisualStyleBackColor = true;
            // 
            // btnIrcConnect
            // 
            this.btnIrcConnect.Location = new System.Drawing.Point(9, 162);
            this.btnIrcConnect.Name = "btnIrcConnect";
            this.btnIrcConnect.Size = new System.Drawing.Size(75, 23);
            this.btnIrcConnect.TabIndex = 10;
            this.btnIrcConnect.Text = "Turn On";
            this.btnIrcConnect.UseVisualStyleBackColor = true;
            // 
            // dropIRCDirection
            // 
            this.dropIRCDirection.FormattingEnabled = true;
            this.dropIRCDirection.Items.AddRange(new object[] {
            "Server -> IRC",
            "Server <- IRC",
            "Server <-> IRC"});
            this.dropIRCDirection.Location = new System.Drawing.Point(113, 137);
            this.dropIRCDirection.Name = "dropIRCDirection";
            this.dropIRCDirection.Size = new System.Drawing.Size(176, 21);
            this.dropIRCDirection.TabIndex = 9;
            // 
            // txtNickPass
            // 
            this.txtNickPass.Location = new System.Drawing.Point(113, 89);
            this.txtNickPass.Name = "txtNickPass";
            this.txtNickPass.PasswordChar = '*';
            this.txtNickPass.Size = new System.Drawing.Size(176, 20);
            this.txtNickPass.TabIndex = 8;
            // 
            // txtIrcUN
            // 
            this.txtIrcUN.Location = new System.Drawing.Point(113, 63);
            this.txtIrcUN.Name = "txtIrcUN";
            this.txtIrcUN.Size = new System.Drawing.Size(176, 20);
            this.txtIrcUN.TabIndex = 7;
            // 
            // txtIrcPort
            // 
            this.txtIrcPort.Location = new System.Drawing.Point(113, 37);
            this.txtIrcPort.Name = "txtIrcPort";
            this.txtIrcPort.Size = new System.Drawing.Size(176, 20);
            this.txtIrcPort.TabIndex = 6;
            // 
            // txtircServer
            // 
            this.txtircServer.Location = new System.Drawing.Point(113, 13);
            this.txtircServer.Name = "txtircServer";
            this.txtircServer.Size = new System.Drawing.Size(176, 20);
            this.txtircServer.TabIndex = 5;
            // 
            // lblIRCDirection
            // 
            this.lblIRCDirection.AutoSize = true;
            this.lblIRCDirection.Location = new System.Drawing.Point(6, 140);
            this.lblIRCDirection.Name = "lblIRCDirection";
            this.lblIRCDirection.Size = new System.Drawing.Size(95, 13);
            this.lblIRCDirection.TabIndex = 4;
            this.lblIRCDirection.Text = "Message Direction";
            // 
            // lblIRCPass
            // 
            this.lblIRCPass.AutoSize = true;
            this.lblIRCPass.Location = new System.Drawing.Point(6, 92);
            this.lblIRCPass.Name = "lblIRCPass";
            this.lblIRCPass.Size = new System.Drawing.Size(101, 13);
            this.lblIRCPass.TabIndex = 3;
            this.lblIRCPass.Text = "Nickserv Password:";
            // 
            // lblIRCUN
            // 
            this.lblIRCUN.AutoSize = true;
            this.lblIRCUN.Location = new System.Drawing.Point(6, 66);
            this.lblIRCUN.Name = "lblIRCUN";
            this.lblIRCUN.Size = new System.Drawing.Size(55, 13);
            this.lblIRCUN.TabIndex = 2;
            this.lblIRCUN.Text = "Username";
            // 
            // lblIRCPort
            // 
            this.lblIRCPort.AutoSize = true;
            this.lblIRCPort.Location = new System.Drawing.Point(6, 40);
            this.lblIRCPort.Name = "lblIRCPort";
            this.lblIRCPort.Size = new System.Drawing.Size(26, 13);
            this.lblIRCPort.TabIndex = 1;
            this.lblIRCPort.Text = "Port";
            // 
            // lblIRCServer
            // 
            this.lblIRCServer.AutoSize = true;
            this.lblIRCServer.Location = new System.Drawing.Point(6, 16);
            this.lblIRCServer.Name = "lblIRCServer";
            this.lblIRCServer.Size = new System.Drawing.Size(38, 13);
            this.lblIRCServer.TabIndex = 0;
            this.lblIRCServer.Text = "Server";
            // 
            // txtPrefix
            // 
            this.txtPrefix.Location = new System.Drawing.Point(100, 2);
            this.txtPrefix.MaxLength = 1;
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(34, 20);
            this.txtPrefix.TabIndex = 1;
            this.txtPrefix.Text = "!";
            // 
            // lblPrefix
            // 
            this.lblPrefix.AutoSize = true;
            this.lblPrefix.Location = new System.Drawing.Point(8, 5);
            this.lblPrefix.Name = "lblPrefix";
            this.lblPrefix.Size = new System.Drawing.Size(86, 13);
            this.lblPrefix.TabIndex = 0;
            this.lblPrefix.Text = "Command Prefix:";
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(782, 205);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Design Settings";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(782, 205);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Scripts";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.lblAbout);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(782, 205);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "About";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // lblAbout
            // 
            this.lblAbout.AutoSize = true;
            this.lblAbout.Location = new System.Drawing.Point(8, 8);
            this.lblAbout.Name = "lblAbout";
            this.lblAbout.Size = new System.Drawing.Size(434, 143);
            this.lblAbout.TabIndex = 0;
            this.lblAbout.Text = resources.GetString("lblAbout.Text");
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(259, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(262, 115);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Raw Info";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 231);
            this.Controls.Add(this.tabControl1);
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.grpLogin.ResumeLayout(false);
            this.grpLogin.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.grpIRC.ResumeLayout(false);
            this.grpIRC.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.ResumeLayout(false);

        }

        

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.GroupBox grpLogin;
        private System.Windows.Forms.TextBox txtPW;
        private System.Windows.Forms.Label lblPW;
        private System.Windows.Forms.Label lblUN;
        private System.Windows.Forms.TextBox txtUN;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtServerUrl;
        private System.Windows.Forms.Label lblServerURL;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lstFavorites;
        private System.Windows.Forms.Button btnServers;
        private System.Windows.Forms.GroupBox grpIRC;
        private System.Windows.Forms.Button btnIrcHelp;
        private System.Windows.Forms.Button btnIrcoff;
        private System.Windows.Forms.Button btnIrcConnect;
        private System.Windows.Forms.ComboBox dropIRCDirection;
        private System.Windows.Forms.TextBox txtNickPass;
        private System.Windows.Forms.TextBox txtIrcUN;
        private System.Windows.Forms.TextBox txtIrcPort;
        private System.Windows.Forms.TextBox txtircServer;
        private System.Windows.Forms.Label lblIRCDirection;
        private System.Windows.Forms.Label lblIRCPass;
        private System.Windows.Forms.Label lblIRCUN;
        private System.Windows.Forms.Label lblIRCPort;
        private System.Windows.Forms.Label lblIRCServer;
        private System.Windows.Forms.TextBox txtPrefix;
        private System.Windows.Forms.Label lblPrefix;
        private System.Windows.Forms.Label lblAbout;
        private System.Windows.Forms.Label lblIrcChan;
        private System.Windows.Forms.TextBox txtIrcChan;
        private System.Windows.Forms.CheckBox chkVerify;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}