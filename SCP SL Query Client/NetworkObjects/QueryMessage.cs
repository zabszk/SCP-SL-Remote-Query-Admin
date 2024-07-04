//File from SCP: Secret Laboratory game source code
//Written by: Łukasz "zabszk" Jurczyk, 2024

using System;
using System.Buffers.Binary;
using SCP_SL_Query_Client.Misc;

namespace SCP_SL_Query_Client.NetworkObjects
{
    /// <summary>
    /// Query protocol message
    /// </summary>
    public readonly struct QueryMessage : IQueryMessage
    {
        /// <summary>
        /// Content type. Depending on the context, it can be either <see cref="QueryContentTypeToServer"/> or <see cref="QueryContentTypeToClient"/>
        /// </summary>
        public readonly byte QueryContentType;

        /// <summary>
        /// Message payload
        /// </summary>
        public readonly byte[] Payload;

        /// <summary>
        /// Message sequential number.
        /// Every message must have this number incremented by one.
        /// </summary>
        public readonly uint SequentialNumber;

        /// <summary>
        /// Message generation timestamp
        /// </summary>
        public readonly long Timestamp;

        private const int HeaderSize = 13;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="payload">Message payload</param>
        /// <param name="sequentialNumber">Message sequential number</param>
        /// <param name="queryContentType">Payload content type</param>
        public QueryMessage(string payload, uint sequentialNumber, byte queryContentType) : this(Utf8.GetBytes(payload), sequentialNumber, queryContentType, DateTimeOffset.UtcNow.ToUnixTimeSeconds()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="payload">Message payload</param>
        /// <param name="sequentialNumber">Message sequential number</param>
        /// <param name="queryContentType">Payload content type</param>
        public QueryMessage(byte[] payload, uint sequentialNumber, byte queryContentType) : this(payload, sequentialNumber, queryContentType, DateTimeOffset.UtcNow.ToUnixTimeSeconds()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="payload">Message payload</param>
        /// <param name="sequentialNumber">Message sequential number</param>
        /// <param name="queryContentType">Payload content type</param>
        /// <param name="timestamp">Timestamp of generation of this message</param>
        public QueryMessage(string payload, uint sequentialNumber, byte queryContentType, long timestamp) : this(Utf8.GetBytes(payload), sequentialNumber, queryContentType, timestamp) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="payload">Message payload</param>
        /// <param name="sequentialNumber">Message sequential number</param>
        /// <param name="queryContentType">Payload content type</param>
        /// <param name="timestamp">Timestamp of generation of this message</param>
        public QueryMessage(byte[] payload, uint sequentialNumber, byte queryContentType, long timestamp)
        {
            Payload = payload;
            SequentialNumber = sequentialNumber;
            QueryContentType = queryContentType;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Validates message
        /// </summary>
        /// <param name="lastRxSequentialNumber">Last received (excluding this message!) sequential number</param>
        /// <param name="timeTolerance">Allowed time difference (in seconds) between server and client</param>
        /// <returns></returns>
        public bool Validate(uint lastRxSequentialNumber, int timeTolerance = 120) =>
            SequentialNumber == lastRxSequentialNumber + 1 && Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Timestamp) <= timeTolerance;

        /// <summary>
        /// Gets length of serialized data
        /// </summary>
        public int SerializedSize => Payload.Length + HeaderSize;

        /// <summary>
        /// Decodes payload using UTF8 encoding
        /// </summary>
        /// <returns>Payload as string</returns>
        public override string ToString() => Utf8.GetString(Payload);

        /// <summary>
        /// Serializes into a byte span
        /// </summary>
        /// <param name="buffer">Output data</param>
        public int Serialize(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(0, 4), SequentialNumber);
            BinaryPrimitives.WriteInt64BigEndian(buffer.Slice(4, 8), Timestamp);
            buffer[12] = QueryContentType;

            Payload.CopyTo(buffer.Slice(HeaderSize));
            return SerializedSize;
        }

        /// <summary>
        /// Deserializes message from a byte span
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        /// <returns>Deserialized message</returns>
        public static QueryMessage Deserialize(ReadOnlySpan<byte> buffer)
        {
            uint seq = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(0, 4));
            long ts = BinaryPrimitives.ReadInt64BigEndian(buffer.Slice(4, 8));
            return new QueryMessage(buffer.Slice(HeaderSize).ToArray(), seq, buffer[12], ts);
        }

        /// <summary>
        /// Query message (to server) content type
        /// </summary>
        public enum QueryContentTypeToServer : byte
        {
            /// <summary>
            /// Command to execute
            /// </summary>
            Command = 0
        }

        /// <summary>
        /// Query message (to client) content type
        /// </summary>
        public enum QueryContentTypeToClient : byte
        {
            /// <summary>
            /// Response of a command executed by query client
            /// </summary>
            ConsoleString = 0,

            /// <summary>
            /// An error in executing query client command
            /// </summary>
            CommandException = 1,

            /// <summary>
            /// Message related to query connection status
            /// </summary>
            QueryMessage = 2,

            /// <summary>
            /// Serialized <see cref="RemoteAdminResponse"/>
            /// </summary>
            RemoteAdminSerializedResponse = 3,

            /// <summary>
            /// Plaintext remote admin response
            /// </summary>
            RemoteAdminPlaintextResponse = 4,

            /// <summary>
            /// Plaintext remote admin response with unsuccessful status
            /// </summary>
            RemoteAdminUnsuccessfulPlaintextResponse = 5,
        }
    }
}