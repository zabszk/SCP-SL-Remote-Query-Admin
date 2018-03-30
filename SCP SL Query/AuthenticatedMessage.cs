using System;
using System.Collections.Generic;
using System.Text;
using SCP_SL_Query.Utils;

namespace SCP_SL_Query
{
    public class AuthenticatedMessage
    {
        public readonly string Message;
        public readonly bool Administrator;

        public AuthenticatedMessage(string m, bool a)
        {
            Message = m;
            Administrator = a;
        }

        public static string GenerateAuthenticatedMessage(string message, long timestamp, string password)
        {
            if (message.Contains(":[:BR:]:")) throw new MessageUnallowedCharsException("Message can't contain :[:BR:]:");
            var prepare = message + ":[:BR:]:" + Convert.ToString(timestamp);
            return prepare + ":[:BR:]:" + Sha.HashToString(Sha.Sha512Hmac(Sha.Sha512(password), new UTF8Encoding().GetBytes(prepare)));
        }

        public static string GenerateNonAuthenticatedMessage(string message)
        {
            if (message.Contains(":[:BR:]:")) throw new MessageUnallowedCharsException("Message can't contain :[:BR:]:");
            return message + ":[:BR:]:Guest";
        }

        public static AuthenticatedMessage AuthenticateMessage(string message, long timestamp, string password)
        {
            if (!message.Contains(":[:BR:]:")) throw new MessageAuthenticationFailureException("Malformed message.");
            var msg = message.Split(new string[] { ":[:BR:]:" }, StringSplitOptions.None);
            if (msg.Length < 2 || msg.Length > 3) throw new MessageAuthenticationFailureException("Malformed message.");
            if (msg[1] == "Guest") return new AuthenticatedMessage(msg[0], false);
            try
            {
                if (!Time.ValidateTimestamp(timestamp, Convert.ToInt64(msg[1]), 5000))
                    throw new MessageExpiredException();
            }
            catch (MessageExpiredException)
            {
                throw new MessageAuthenticationFailureException();
            }
            catch
            {
                throw new MessageAuthenticationFailureException("Malformed message - timestamp can't be converted to long.");
            }
            if (Sha.HashToString(Sha.Sha512Hmac(Sha.Sha512(password), new UTF8Encoding().GetBytes(msg[0] + ":[:BR:]:" + msg[1]))) != msg[2]) throw new MessageAuthenticationFailureException("Invalid authentication code.");
            return string.IsNullOrEmpty(password) || password == "none" ? new AuthenticatedMessage(msg[0], false) : new AuthenticatedMessage(msg[0], true);
        }

        public static byte[] Encode(byte[] data)
        {
            var result = new byte[data.Length + 4];
            var intBytes = BitConverter.GetBytes(data.Length);
            Array.Reverse(intBytes);
            Array.Copy(intBytes, 0, result, 0, intBytes.Length);
            Array.Copy(data, 0, result, 4, data.Length);
            return result;
        }

        public static List<byte[]> Decode(byte[] data)
        {
            var lst = new List<byte[]>();
            while (data.Length > 0)
            {
                byte[] len = { data[0], data[1], data[2], data[3] };
                Array.Reverse(len);
                var ln = BitConverter.ToInt16(len, 0);
                if (ln == 0) break;
                var dta = new byte[ln];
                Array.Copy(data, 4, dta, 0, ln);
                lst.Add(dta);
                dta = new byte[data.Length - ln - 4];
                Array.Copy(data, ln + 4, dta, 0, data.Length - ln - 4);
                data = dta;
            }
            return lst;
        }
    }
}
