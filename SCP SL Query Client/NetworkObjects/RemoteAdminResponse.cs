//File from SCP: Secret Laboratory game source code
//Written by: Łukasz "zabszk" Jurczyk, 2024

using System;
using System.Buffers.Binary;
using SCP_SL_Query_Client.Misc;

namespace SCP_SL_Query_Client.NetworkObjects
{
    /// <summary>
    /// Remote Admin response from game server
    /// </summary>
    public readonly struct RemoteAdminResponse : IEquatable<RemoteAdminResponse>
    {
        /// <summary>
        /// Response content
        /// </summary>
        public readonly string Content;

        /// <summary>
        /// Response flags
        /// </summary>
        public readonly RemoteAdminResponseFlags Flags;

        /// <summary>
        /// Display override for the response
        /// </summary>
        public readonly string OverrideDisplay;

        /// <summary>
        /// Constructor for the struct
        /// </summary>
        public RemoteAdminResponse(string content, RemoteAdminResponseFlags flags, string overrideDisplay)
        {
            Content = content;
            Flags = flags;
            OverrideDisplay = overrideDisplay;
        }

        /// <summary>
        /// Constructor for the struct
        /// </summary>
        public RemoteAdminResponse(string content, bool isSuccess, bool logInConsole, string overrideDisplay) : this(content,
            (isSuccess ? RemoteAdminResponseFlags.Successful : 0) | (logInConsole ? RemoteAdminResponseFlags.LogInConsole : 0), overrideDisplay) { }

        private const int HeaderSize = 5;

        /// <summary>
        /// Gets length of serialized data
        /// </summary>
        public int GetLength =>
            (string.IsNullOrEmpty(Content) ? 0 : Utf8.GetLength(Content)) + (string.IsNullOrEmpty(OverrideDisplay) ? 0 : Utf8.GetLength(OverrideDisplay)) + HeaderSize;

        /// <summary>
        /// Serializes into a byte array
        /// </summary>
        /// <returns>Serialized data</returns>
        public byte[] Serialize()
        {
            byte[] array = new byte[GetLength];
            Serialize(array);
            return array;
        }

        /// <summary>
        /// Serializes into a byte array
        /// </summary>
        /// <param name="array">Output array</param>
        public void Serialize(byte[] array)
        {
            array[0] = (byte)Flags;
            BinaryPrimitives.WriteInt32BigEndian(new Span<byte>(array, 1, 4), Content.Length);
            int contentLen = Utf8.GetBytes(Content, array, HeaderSize);

            if (OverrideDisplay != null)
                Utf8.GetBytes(OverrideDisplay, array, contentLen + HeaderSize);
        }

        /// <summary>
        /// Deserializes message from a byte array
        /// </summary>
        /// <param name="array">Serialized data</param>
        /// <param name="length">Data size in array</param>
        /// <returns>Deserialized message</returns>
        public static RemoteAdminResponse Deserialize(byte[] array, int length)
        {
            int contentLen = BinaryPrimitives.ReadInt32BigEndian(new Span<byte>(array, 1, 4));
            string content = contentLen > 0 ? Utf8.GetString(array, HeaderSize, contentLen) : null;
            string overrideDisplay = contentLen + HeaderSize < length ? Utf8.GetString(array, contentLen + HeaderSize, length - contentLen - HeaderSize) : null;

            return new RemoteAdminResponse(content, (RemoteAdminResponseFlags)array[0], overrideDisplay);
        }

        /// <inheritdoc />
        public bool Equals(RemoteAdminResponse other)
        {
            return string.Equals(Content, other.Content) && Flags == other.Flags && string.Equals(OverrideDisplay, other.OverrideDisplay);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is RemoteAdminResponse other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Content != null ? Content.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Flags.GetHashCode();
                hashCode = (hashCode * 397) ^ (OverrideDisplay != null ? OverrideDisplay.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(RemoteAdminResponse left, RemoteAdminResponse right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RemoteAdminResponse left, RemoteAdminResponse right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Check whether a specific flag is set.
        /// </summary>
        /// <param name="flag">Flag to check</param>
        /// <returns>Flag state</returns>
        public bool HasFlag(RemoteAdminResponseFlags flag) => (Flags & flag) == flag;

        /// <summary>
        /// RA response flags
        /// </summary>
        [Flags]
        public enum RemoteAdminResponseFlags : byte
        {
            /// <summary>
            /// Indicates that the command executed was successful
            /// </summary>
            Successful = 1,

            /// <summary>
            /// Indicates that the output should be logged on the console
            /// </summary>
            LogInConsole = 1 << 1,
        }
    }
}