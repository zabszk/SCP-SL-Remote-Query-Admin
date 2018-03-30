using System.Windows.Forms;
using MetroFramework;
using SCP_SL_Query;
using SCP_SL_Query.Interfaces;

namespace SCP_SL_Remote_Query_Admin
{
    internal class ClientInterface : IClient
    {
        private readonly ConsoleForm _f;

        internal ClientInterface(ConsoleForm f)
        {
            _f = f;
        }

        public void Receive(string message, bool authenticated)
        {
            _f.AppendConsole(0, authenticated ? 1 : 0, message);
        }

        public void Sent(string message, bool authenticated)
        {
            _f.AppendConsole(1, authenticated ? 1 : 0, message);
        }

        public void KeepAliveReceived()
        {
            _f.KeepaliveReceived();
        }

        public void MalformedMessageReceived()
        {
            _f.AppendConsole(3, -1, "Malformed message received.");
        }

        public void ExpiredMessageReceived()
        {
            _f.AppendConsole(3, -1, "Expired message received.");
        }

        public void AuthenticationFailure()
        {
            _f.AppendConsole(3, -1, "Received message can't be authenticated!");
        }

        public void ConnectionFailure()
        {
            _f.AppendConsole(2, -1, "Connection failed.");
            MetroMessageBox.Show(_f, "Can't connect to provided host.", "Connection Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _f.Disconnected();
        }

        public void ConnectionLost()
        {
            _f.AppendConsole(2, -1, "Connection lost!");
        }

        public void Disconnected()
        {
            _f.AppendConsole(2, -1, "Disconnected");
            _f.Disconnected();
        }

        public void ConnectionEstablished()
        {
            _f.AppendConsole(2, -1, "Connection established.");
            _f.Connected();
        }

        public void Error(string text)
        {
            _f.AppendConsole(3, -1, text);
        }
    }
}
