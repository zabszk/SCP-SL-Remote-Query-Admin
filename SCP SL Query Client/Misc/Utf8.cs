//File from SCP: Secret Laboratory game source code

using System;
using System.Text;

namespace SCP_SL_Query_Client.Misc
{
    /// <summary>
    /// UTF8 encoding
    /// </summary>
    public static class Utf8
    {
        private static readonly UTF8Encoding Encoding = new UTF8Encoding(false);

        /// <summary>Calculates the number of bytes produced by encoding the characters in the specified <see cref="T:System.String" />.</summary>
        /// <param name="data">The <see cref="T:System.String" /> containing the set of characters to encode.</param>
        public static int GetLength(string data)
        {
            return Encoding.GetByteCount(data);
        }

        /// <summary>When overridden in a derived class, encodes all the characters in the specified string into a sequence of bytes.</summary>
        /// <param name="data">The <see cref="T:System.String" /> containing the set of characters to encode.</param>
        public static byte[] GetBytes(string data)
        {
            return Encoding.GetBytes(data);
        }

        /// <summary>When overridden in a derived class, encodes all the characters in the specified string into a sequence of bytes.</summary>
        public static int GetBytes(string data, byte[] buffer)
        {
            return Encoding.GetBytes(data, 0, data.Length, buffer, 0);
        }

        /// <summary>When overridden in a derived class, encodes all the characters in the specified string into a sequence of bytes.</summary>
        public static int GetBytes(string data, byte[] buffer, int offset)
        {
            return Encoding.GetBytes(data, 0, data.Length, buffer, offset);
        }

        /// <summary>When overridden in a derived class, decodes all the bytes in the specified byte array into a string.</summary>
        public static string GetString(byte[] data)
        {
            return Encoding.GetString(data);
        }

        /// <summary>When overridden in a derived class, decodes all the bytes in the specified byte array into a string.</summary>
        public static string GetString(byte[] data, int offset, int count)
        {
            return Encoding.GetString(data, offset, count);
        }
    }
}