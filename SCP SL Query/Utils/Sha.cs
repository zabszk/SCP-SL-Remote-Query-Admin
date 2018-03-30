using System.Security.Cryptography;
using System.Text;

namespace SCP_SL_Query.Utils
{
    public class Sha
    {
        public static byte[] Sha256(byte[] message)
        {
            var sha256 = SHA256.Create();
            return sha256.ComputeHash(message);
        }

        public static byte[] Sha256(string message)
        {
            return Sha256(new UTF8Encoding().GetBytes(message));
        }

        public static byte[] Sha256Hmac(byte[] key, byte[] message)
        {
            var hash = new HMACSHA256(key);
            return hash.ComputeHash(message);
        }

        public static byte[] Sha512(string message)
        {
            return Sha512(new UTF8Encoding().GetBytes(message));
        }

        public static byte[] Sha512(byte[] message)
        {
            var sha512 = SHA512.Create();
            return sha512.ComputeHash(message);
        }

        public static byte[] Sha512Hmac(byte[] key, byte[] message)
        {
            var hash = new HMACSHA512(key);
            return hash.ComputeHash(message);
        }

        public static string HashToString(byte[] hash)
        {
            var result = new StringBuilder();
            foreach (var t in hash)
            {
                result.Append(t.ToString("X2"));
            }
            return result.ToString();
        }
    }
}
