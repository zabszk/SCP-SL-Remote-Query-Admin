namespace SCP_SL_Remote_Query_Admin
{
    partial class ConsoleForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ipTextBox = new MetroFramework.Controls.MetroTextBox();
            this.passwordTextBox = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.connectButton = new MetroFramework.Controls.MetroButton();
            this.disconnectButton = new MetroFramework.Controls.MetroButton();
            this.console = new System.Windows.Forms.RichTextBox();
            this.commandBox = new MetroFramework.Controls.MetroTextBox();
            this.executeButton = new MetroFramework.Controls.MetroButton();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.keepAliveLabel = new MetroFramework.Controls.MetroLabel();
            this.SuspendLayout();
            // 
            // ipTextBox
            // 
            // 
            // 
            // 
            this.ipTextBox.CustomButton.Image = null;
            this.ipTextBox.CustomButton.Location = new System.Drawing.Point(152, 1);
            this.ipTextBox.CustomButton.Name = "";
            this.ipTextBox.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.ipTextBox.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.ipTextBox.CustomButton.TabIndex = 1;
            this.ipTextBox.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.ipTextBox.CustomButton.UseSelectable = true;
            this.ipTextBox.CustomButton.Visible = false;
            this.ipTextBox.Lines = new string[] {
        "127.0.0.1:7777"};
            this.ipTextBox.Location = new System.Drawing.Point(23, 117);
            this.ipTextBox.MaxLength = 32767;
            this.ipTextBox.Name = "ipTextBox";
            this.ipTextBox.PasswordChar = '\0';
            this.ipTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.ipTextBox.SelectedText = "";
            this.ipTextBox.SelectionLength = 0;
            this.ipTextBox.SelectionStart = 0;
            this.ipTextBox.ShortcutsEnabled = true;
            this.ipTextBox.Size = new System.Drawing.Size(174, 23);
            this.ipTextBox.TabIndex = 0;
            this.ipTextBox.Text = "127.0.0.1:7777";
            this.ipTextBox.UseSelectable = true;
            this.ipTextBox.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.ipTextBox.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // passwordTextBox
            // 
            // 
            // 
            // 
            this.passwordTextBox.CustomButton.Image = null;
            this.passwordTextBox.CustomButton.Location = new System.Drawing.Point(152, 1);
            this.passwordTextBox.CustomButton.Name = "";
            this.passwordTextBox.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.passwordTextBox.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.passwordTextBox.CustomButton.TabIndex = 1;
            this.passwordTextBox.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.passwordTextBox.CustomButton.UseSelectable = true;
            this.passwordTextBox.CustomButton.Visible = false;
            this.passwordTextBox.Lines = new string[0];
            this.passwordTextBox.Location = new System.Drawing.Point(23, 179);
            this.passwordTextBox.MaxLength = 32767;
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.PasswordChar = '●';
            this.passwordTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.passwordTextBox.SelectedText = "";
            this.passwordTextBox.SelectionLength = 0;
            this.passwordTextBox.SelectionStart = 0;
            this.passwordTextBox.ShortcutsEnabled = true;
            this.passwordTextBox.Size = new System.Drawing.Size(174, 23);
            this.passwordTextBox.TabIndex = 1;
            this.passwordTextBox.UseSelectable = true;
            this.passwordTextBox.UseSystemPasswordChar = true;
            this.passwordTextBox.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.passwordTextBox.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(23, 95);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(63, 19);
            this.metroLabel1.TabIndex = 2;
            this.metroLabel1.Text = "IP : Port :";
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(23, 157);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(110, 19);
            this.metroLabel2.TabIndex = 3;
            this.metroLabel2.Text = "Admin password:";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(23, 218);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseSelectable = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // disconnectButton
            // 
            this.disconnectButton.Enabled = false;
            this.disconnectButton.Location = new System.Drawing.Point(122, 218);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(75, 23);
            this.disconnectButton.TabIndex = 5;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseSelectable = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // console
            // 
            this.console.BackColor = System.Drawing.Color.Black;
            this.console.ForeColor = System.Drawing.SystemColors.Control;
            this.console.Location = new System.Drawing.Point(237, 63);
            this.console.Name = "console";
            this.console.Size = new System.Drawing.Size(790, 383);
            this.console.TabIndex = 6;
            this.console.Text = "";
            // 
            // commandBox
            // 
            this.commandBox.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.commandBox.CustomButton.Image = null;
            this.commandBox.CustomButton.Location = new System.Drawing.Point(674, 1);
            this.commandBox.CustomButton.Name = "";
            this.commandBox.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.commandBox.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.commandBox.CustomButton.TabIndex = 1;
            this.commandBox.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.commandBox.CustomButton.UseSelectable = true;
            this.commandBox.CustomButton.Visible = false;
            this.commandBox.ForeColor = System.Drawing.Color.Black;
            this.commandBox.Lines = new string[0];
            this.commandBox.Location = new System.Drawing.Point(237, 452);
            this.commandBox.MaxLength = 32767;
            this.commandBox.Name = "commandBox";
            this.commandBox.PasswordChar = '\0';
            this.commandBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.commandBox.SelectedText = "";
            this.commandBox.SelectionLength = 0;
            this.commandBox.SelectionStart = 0;
            this.commandBox.ShortcutsEnabled = true;
            this.commandBox.Size = new System.Drawing.Size(696, 23);
            this.commandBox.TabIndex = 7;
            this.commandBox.UseSelectable = true;
            this.commandBox.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.commandBox.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.commandBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.commandBox_KeyDown);
            // 
            // executeButton
            // 
            this.executeButton.Enabled = false;
            this.executeButton.Location = new System.Drawing.Point(939, 452);
            this.executeButton.Name = "executeButton";
            this.executeButton.Size = new System.Drawing.Size(88, 23);
            this.executeButton.TabIndex = 8;
            this.executeButton.Text = "Execute";
            this.executeButton.UseSelectable = true;
            this.executeButton.Click += new System.EventHandler(this.executeButton_Click);
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(23, 427);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(36, 19);
            this.metroLabel3.TabIndex = 9;
            this.metroLabel3.Text = "v. 1.0";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(20, 462);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(142, 13);
            this.linkLabel1.TabIndex = 10;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Licensed under MIT License";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // keepAliveLabel
            // 
            this.keepAliveLabel.AutoSize = true;
            this.keepAliveLabel.Location = new System.Drawing.Point(23, 263);
            this.keepAliveLabel.Name = "keepAliveLabel";
            this.keepAliveLabel.Size = new System.Drawing.Size(93, 19);
            this.keepAliveLabel.TabIndex = 11;
            this.keepAliveLabel.Text = "Last Keepalive:";
            this.keepAliveLabel.Visible = false;
            // 
            // ConsoleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1050, 493);
            this.Controls.Add(this.keepAliveLabel);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.metroLabel3);
            this.Controls.Add(this.executeButton);
            this.Controls.Add(this.commandBox);
            this.Controls.Add(this.console);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.metroLabel2);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.ipTextBox);
            this.MaximizeBox = false;
            this.Name = "ConsoleForm";
            this.Resizable = false;
            this.Text = "SCP: Secret Laboratory Remote Query Admin";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroTextBox ipTextBox;
        private MetroFramework.Controls.MetroTextBox passwordTextBox;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroButton connectButton;
        private MetroFramework.Controls.MetroButton disconnectButton;
        private System.Windows.Forms.RichTextBox console;
        private MetroFramework.Controls.MetroTextBox commandBox;
        private MetroFramework.Controls.MetroButton executeButton;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private MetroFramework.Controls.MetroLabel keepAliveLabel;
    }
}

