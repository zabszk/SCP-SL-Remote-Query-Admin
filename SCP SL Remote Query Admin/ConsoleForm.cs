using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Forms;
using SCP_SL_Query_Client;
using SCP_SL_Query_Client.NetworkObjects;

namespace SCP_SL_Remote_Query_Admin
{
    public partial class ConsoleForm : MetroForm
    {
        private QueryClient _client;
        private readonly object _consoleLock = new object();

        public ConsoleForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            PrivateFontCollection fonts = new();
            fonts.AddFontFile("DroidSansMono.ttf");
            console.Font = new Font(fonts.Families[0], 9);
        }

        private void commandBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            if (executeButton.Enabled) executeButton_Click(sender, e);
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            AppendConsole(1, commandBox.Text);
            _client.Send(commandBox.Text, QueryMessage.QueryContentTypeToServer.Command);
            commandBox.Text = "";
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (passwordTextBox.Text == "")
            {
                MetroMessageBox.Show(this, "Please enter a password", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (ipTextBox.Text.Contains(":"))
            {
                var dt = ipTextBox.Text.Split(':');

                if (dt.Length != 2 || !ushort.TryParse(dt[1], out ushort port))
                {
                    MetroMessageBox.Show(this, "Please enter correct IP or IP:Port", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _client = new QueryClient(dt[0], port, passwordTextBox.Text);
            }
            else
            {
                _client = new QueryClient(ipTextBox.Text, 7777, passwordTextBox.Text);
            }

            EnableUI(false);

            _client.OnMessageReceived += MessageReceived;
            _client.OnConnectedToServer += Connected;
            _client.OnDisconnectedFromServer += Disconnected;

            QueryHandshake.ClientFlags flags = (consoleCheckBox.Checked ? QueryHandshake.ClientFlags.SubscribeServerConsole : 0) | (logCheckBox.Checked ? QueryHandshake.ClientFlags.SubscribeServerLogs : 0);
            _client.Connect(flags: flags, username: usernameTextBox.Text);
        }

        // ReSharper disable once InconsistentNaming
        private void EnableUI(bool state)
        {
            connectButton.Enabled = state;
            ipTextBox.Enabled = state;
            passwordTextBox.Enabled = state;
            usernameTextBox.Enabled = state;
            consoleCheckBox.Enabled = state;
            logCheckBox.Enabled = state;
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            _client.Disconnect();
        }

        private void MessageReceived(QueryMessage message)
        {
            AppendConsole(0, message.ToString());
        }

        private void Connected()
        {
            executeButton.Enabled = true;
            disconnectButton.Enabled = true;
        }

        private void Disconnected(DisconnectionReason reason)
        {
            AppendConsole(2, $"Disconnected: {reason}");

            if (reason != DisconnectionReason.DisconnectedByClient)
            {
                Exception e = _client.GetLastRxError();

                if (e != null)
                {
                    AppendConsole(2, $"Exception: {e.Message}");
                    AppendConsole(2, e.StackTrace);
                }
            }

            executeButton.Enabled = false;
            disconnectButton.Enabled = false;

            EnableUI(true);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new License().ShowDialog();
        }

        private void AppendConsole(int type, string message)
        {
            lock (_consoleLock)
            {
                console.AppendText("<" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "> ", Color.RoyalBlue);
                switch (type)
                {
                    case 0:
                        console.AppendText("INCOMING ", Color.DarkTurquoise);
                        break;
                    case 1:
                        console.AppendText("OUTGOING ", Color.MediumSpringGreen);
                        break;
                    case 2:
                        console.AppendText("INTERNAL ", Color.Magenta);
                        break;
                    case 3:
                        console.AppendText("ERROR    ", Color.Orange);
                        break;
                }

                console.AppendText(message + Environment.NewLine);
                console.ScrollToCaret();
            }
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
