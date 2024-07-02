//File from SCP: Secret Laboratory game source code
//Written by: Łukasz "zabszk" Jurczyk, 2024

using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SCP_SL_Query_Client.Misc;

namespace SCP_SL_Query_Client.Cryptography
{
	/// <summary>
	/// SHA (Secure Hash Algorithm) methods
	/// </summary>
	public static class Sha
	{
		/// <summary>
		/// Calculates a SHA1 hash
		/// </summary>
		/// <param name="message">Data to hash</param>
		/// <returns>Hash</returns>
		public static byte[] Sha1(byte[] message)
		{
			// Disposed un-disposed SHA1 @ Dankrushen
			using (var sha1 = SHA1.Create())
			{
				return sha1.ComputeHash(message);
			}
		}

		/// <summary>
		/// Calculates a SHA1 hash
		/// </summary>
		/// <param name="message">Data to hash</param>
		/// <param name="offset">Data offset</param>
		/// <param name="length">Data length</param>
		/// <returns>Hash</returns>
		public static byte[] Sha1(byte[] message, int offset, int length)
		{
			// Disposed un-disposed SHA1 @ Dankrushen
			using (var sha1 = SHA1.Create())
			{
				return sha1.ComputeHash(message, offset, length);
			}
		}

		/// <summary>
		/// Calculates a SHA1 hash
		/// </summary>
		/// <param name="message">Data to hash</param>
		/// <returns>Hash</returns>
		public static byte[] Sha1(string message)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(message.Length));
			int length = Utf8.GetBytes(message, buffer);
			byte[] result = Sha1(buffer, 0, length);
			ArrayPool<byte>.Shared.Return(buffer);
			return result;
		}

		/// <summary>
		/// Calculates a SHA256 hash
		/// </summary>
		/// <param name="message">Data to hash</param>
		/// <returns>Hash</returns>
		public static byte[] Sha256(byte[] message)
		{
			// Disposed un-disposed SHA256 @ Dankrushen
			using (var sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(message);
			}
		}

		/// <summary>
		/// Calculates a SHA256 hash
		/// </summary>
		/// <param name="message">Data to hash</param>
		/// <param name="offset">Data offset</param>
		/// <param name="length">Data length</param>
		/// <returns>Hash</returns>
		public static byte[] Sha256(byte[] message, int offset, int length)
		{
			// Disposed un-disposed SHA256 @ Dankrushen
			using (var sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(message, offset, length);
			}
		}

		/// <summary>
		/// Calculates a SHA256 hash
		/// </summary>
		/// <param name="message">Data to hash</param>
		/// <returns>Hash</returns>
		public static byte[] Sha256(string message)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(message.Length));
			int length = Utf8.GetBytes(message, buffer);
			byte[] result = Sha256(buffer, 0, length);
			ArrayPool<byte>.Shared.Return(buffer);
			return result;
		}

#if !HEADLESS
		/// <summary>
		/// Calculates a SHA256 hash of a file
		/// </summary>
		/// <param name="path">Path to a file to hash</param>
		/// <returns>Hash</returns>
		public static byte[] Sha256File(string path)
		{
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sha256 = SHA256.Create())
				return sha256.ComputeHash(fs);
		}
#endif

		/// <summary>
		/// Calculate a	SHA256 HMAC (Hash-based Message Authentication Code)
		/// </summary>
		/// <param name="key">Secret key</param>
		/// <param name="message">Data to hash</param>
		/// <returns>HMAC</returns>
		public static byte[] Sha256Hmac(byte[] key, byte[] message)
		{
			// Disposed un-disposed HMACSHA256 @ Dankrushen
			using (var hash = new HMACSHA256(key))
			{
				return hash.ComputeHash(message);
			}
		}

		/// <summary>
		/// Calculates a SHA512 hash
		/// </summary>
		/// <param name="message">Data to hash</param>
		/// <returns>Hash</returns>
		public static byte[] Sha512(string message)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(message.Length));
			int length = Utf8.GetBytes(message, buffer);
			byte[] result = Sha512(buffer, 0, length);
			ArrayPool<byte>.Shared.Return(buffer);
			return result;
		}

		/// <summary>
		/// Calculates a SHA512 hash
		/// </summary>
		/// <param name="message">Data to hash</param>
		/// <returns>Hash</returns>
		public static byte[] Sha512(byte[] message)
		{
			// Disposed un-disposed SHA512 @ Dankrushen
			using (var sha512 = SHA512.Create())
			{
				return sha512.ComputeHash(message);
			}
		}

		/// <summary>
		/// Calculates a SHA512 hash
		/// </summary>
		/// <param name="message">Data to hash</param>
		/// <param name="offset">Data offset</param>
		/// <param name="length">Data length</param>
		/// <returns>Hash</returns>
		public static byte[] Sha512(byte[] message, int offset, int length)
		{
			// Disposed un-disposed SHA512 @ Dankrushen
			using (var sha512 = SHA512.Create())
			{
				return sha512.ComputeHash(message, offset, length);
			}
		}

		/// <summary>
		/// Calculate a	SHA512 HMAC (Hash-based Message Authentication Code)
		/// </summary>
		/// <param name="key">Secret key</param>
		/// <param name="message">Data to hash</param>
		/// <returns>HMAC</returns>
		public static byte[] Sha512Hmac(byte[] key, byte[] message)
		{
			// Disposed un-disposed HMACSHA512 @ Dankrushen
			using (var hash = new HMACSHA512(key))
			{
				return hash.ComputeHash(message);
			}
		}

		/// <summary>
		/// Calculate a	SHA512 HMAC (Hash-based Message Authentication Code)
		/// </summary>
		/// <param name="key">Secret key</param>
		/// <param name="data">Data to hash</param>
		/// <returns>HMAC</returns>
		public static byte[] Sha512Hmac(byte[] key, string data)
		{
			byte[] buffer = null;

			try
			{
				buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(data));
				int length = Utf8.GetBytes(data, buffer);
				return Sha512Hmac(key, 0, length, buffer);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}

		/// <summary>
		/// Calculates a SHA512 HMAC (Hash-based Message Authentication Code)
		/// </summary>
		/// <param name="key">Secret key</param>
		/// <param name="offset">Data offset</param>
		/// <param name="length">Data length</param>
		/// <param name="message">Data to hash</param>
		/// <returns>HMAC</returns>
		public static byte[] Sha512Hmac(byte[] key, int offset, int length, byte[] message)
		{
			// Disposed un-disposed HMACSHA512 @ Dankrushen
			using (var hash = new HMACSHA512(key))
			{
				return hash.ComputeHash(message, offset, length);
			}
		}
	}
}
