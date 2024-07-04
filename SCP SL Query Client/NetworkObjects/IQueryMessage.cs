using System;

namespace SCP_SL_Query_Client.NetworkObjects
{
    public interface IQueryMessage
    {
        int Serialize(Span<byte> buffer);
    }
}