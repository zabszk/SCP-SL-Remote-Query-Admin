using System;

namespace SCP_SL_Query_Client.NetworkObjects
{
    public interface IQueryMessage
    {
        public int Serialize(Span<byte> buffer);
    }
}