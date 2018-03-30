namespace SCP_SL_Query.Interfaces
{
    public interface IClient
    {
        void Receive(string message, bool authenticated);

        void Sent(string message, bool authenticated);

        void KeepAliveReceived();

        void MalformedMessageReceived();

        void ExpiredMessageReceived();

        void AuthenticationFailure();

        void ConnectionFailure();

        void ConnectionLost();

        void Disconnected();

        void ConnectionEstablished();

        void Error(string text);
    }
}
