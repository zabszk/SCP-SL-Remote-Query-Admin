namespace SCP_SL_Query_Client
{
    public enum DisconnectionReason
    {
        /// <summary>
        /// Connection disconnected by the client.
        /// </summary>
        DisconnectedByClient,

        /// <summary>
        /// Connection with the server was lost.
        /// </summary>
        ConnectionLost,

        /// <summary>
        /// Failed to connect to the server.
        /// </summary>
        ConnectionFailed,

        /// <summary>
        /// Failed to read message length sent by the server.
        /// </summary>
        CannotReadLength,

        /// <summary>
        /// Failed to read data from the server.
        /// </summary>
        ReadError,

        /// <summary>
        /// Server sent too big message.
        /// This shouldn't happen, but consider increasing rxBufferSize (argument of the constructor of the <see cref="QueryClient"/> class).
        /// </summary>
        RxBufferSizeExceeded,

        /// <summary>
        /// Handshake from the server failed validation.
        /// Make sure that the time and timezone is set correctly both on this computer and on the server.
        /// </summary>
        HandshakeValidationFailed,

        /// <summary>
        /// Failed to send handshake to the server.
        /// </summary>
        SendingHandshakeResponseFailed,

        /// <summary>
        /// Server Rx buffer is smaller than the query protocol allows.
        /// </summary>
        ServerRxBufferToSmall,

        /// <summary>
        /// Failed to decrypt data from the server.
        /// Make sure password is correct!
        /// </summary>
        DecryptionFailed,

        /// <summary>
        /// Failed to deserialize data from the server.
        /// </summary>
        DeserializationFailed,

        /// <summary>
        /// Failed to validate data from the server.
        /// Make sure that the time and timezone is set correctly both on this computer and on the server.
        /// </summary>
        MessageValidationFailed,
    }
}