namespace SCP_SL_Query_Client
{
    public enum DisconnectionReason
    {
        DisconnectedByClient,
        DisconnectedByServer,
        ConnectionLost,
        ConnectionFailed,
        CannotReadLength,
        ReadError,
        RxBufferSizeExceeded,
        HandshakeValidationFailed,
        SendingHandshakeResponseFailed,
        ServerRxBufferToSmall,
        DecryptionFailed,
        DeserializationFailed,
        MessageValidationFailed,
    }
}