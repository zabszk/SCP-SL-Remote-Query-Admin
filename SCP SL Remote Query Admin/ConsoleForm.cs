using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Forms;
using SCP_SL_Query;

namespace SCP_SL_Remote_Query_Admin
{
    public partial class ConsoleForm : MetroForm
    {
        private QueryClient _client;
        private readonly ClientInterface _i;
        private readonly PrivateFontCollection _fonts;

        public ConsoleForm()
        {
            InitializeComponent();
            RichTextBox.CheckForIllegalCrossThreadCalls = false;
            _i = new ClientInterface(this);
            _fonts = new PrivateFontCollection();
            _fonts.AddFontFile("DroidSansMono.ttf");
            console.Font = new Font(_fonts.Families[0], 9);
        }

        private void commandBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            if (executeButton.Enabled) executeButton_Click(sender, e);
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            _client.Send(commandBox.Text);
            commandBox.Text = "";
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            connectButton.Enabled = false;
            ipTextBox.Enabled = false;
            passwordTextBox.Enabled = false;
            keepAliveLabel.Visible = false;

            _client = null;

            if (ipTextBox.Text.Contains(":"))
            {
                var dt = ipTextBox.Text.Split(':');
                if (dt.Length != 2) MetroMessageBox.Show(this, "Please enter correct IP or IP:Port", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    var port = -1;
                    try
                    {
                        port = Convert.ToInt32(dt[1]);
                    }
                    catch
                    {
                        MetroMessageBox.Show(this, "Please enter correct IP or IP:Port", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (port != -1)
                    {
                        _client = new QueryClient(dt[0], port, _i);
                    }
                }
            }
            else
            {
                _client = new QueryClient(ipTextBox.Text, 7777, _i);
            }

            if (_client != null) _client.Connect();
            else
            {
                connectButton.Enabled = true;
                ipTextBox.Enabled = true;
                passwordTextBox.Enabled = true;
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            _client.Disconnect();
        }

        internal void Connected()
        {
            executeButton.Enabled = true;
            disconnectButton.Enabled = true;
            keepAliveLabel.Visible = true;
            keepAliveLabel.Text = "Last Keepalive:";
            if (passwordTextBox.Text != "") _client.Password = passwordTextBox.Text;
        }

        internal void KeepaliveReceived()
        {
            keepAliveLabel.Text = "Last Keepalive:" + Environment.NewLine + DateTime.Now.ToString("s");
        }

        internal void Disconnected()
        {
            executeButton.Enabled = false;
            disconnectButton.Enabled = false;
            connectButton.Enabled = true;

            ipTextBox.Enabled = true;
            passwordTextBox.Enabled = true;

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new License().ShowDialog();
        }

        internal void AppendConsole(int type, int signed, string message)
        {
            ColorizeSyntax(console, type, signed, message);
        }

        private static void ColorizeSyntax(RichTextBox box, int type, int signed, string message)
        {
            box.AppendText("<" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "> ", Color.RoyalBlue);
            switch (type)
            {
                case 0:
                    box.AppendText("INCOMING ", Color.DarkTurquoise);
                    break;
                case 1:
                    box.AppendText("OUTGOING ", Color.MediumSpringGreen);
                    break;
                case 2:
                    box.AppendText("INTERNAL ", Color.Magenta);
                    break;
                case 3:
                    box.AppendText("ERROR    ", Color.Orange);
                    break;
            }
            switch (signed)
            {
                case 0:
                    box.AppendText("UNSIGNED ", Color.Crimson);
                    break;
                case 1:
                    box.AppendText(" SIGNED  ", Color.Cyan);
                    break;
            }
            box.AppendText(message + Environment.NewLine);
        }
    }

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
