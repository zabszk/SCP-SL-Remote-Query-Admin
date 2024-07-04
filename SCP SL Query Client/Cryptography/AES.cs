//File from SCP: Secret Laboratory game source code
//Written by: Łukasz "zabszk" Jurczyk, 2024

using System;
using System.Buffers;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace SCP_SL_Query_Client.Cryptography
{
	/// <summary>
	/// AES (Advanced Encryption Standard) Methods
	/// </summary>
	public static class AES
	{
		/// <summary>
		/// Nonce size for GCM mode
		/// </summary>
		public const int NonceSizeBytes = 32;

		/// <summary>
		/// MAC size for GCM mode
		/// </summary>
		private const int MacSizeBits = 128;

		/// <summary>
		/// Generates nonce for <see cref="AesGcmEncrypt(Org.BouncyCastle.Crypto.Modes.GcmBlockCipher,byte[],byte[],int,int,byte[],int)"/>
		/// </summary>
		/// <param name="buffer">Output buffer for nonce, must be at least <see cref="NonceSizeBytes"/> long</param>
		/// <param name="secureRandom">Secure random used for nonce generation</param>
		public static void GenerateNonce(byte[] buffer, SecureRandom secureRandom) => secureRandom.NextBytes(buffer, 0, NonceSizeBytes);

		/// <summary>
		/// Reads nonce from ciphertext
		/// </summary>
		/// <param name="buffer">Output buffer for nonce, must be at least <see cref="NonceSizeBytes"/> long</param>
		/// <param name="cipherText">Ciphertext (data to decrypt) containing nonce</param>
		/// <param name="dataOffset">Offset of the ciphertext in <param name="cipherText"></param></param>
		public static void ReadNonce(byte[] buffer, byte[] cipherText, int dataOffset = 0) => Array.Copy(cipherText, dataOffset, buffer, 0, NonceSizeBytes);

		/// <summary>
		/// Initializes AES GCM for <see cref="AesGcmEncrypt(Org.BouncyCastle.Crypto.Modes.GcmBlockCipher,byte[],byte[],int,int,byte[],int)"/>
		/// </summary>
		/// <param name="dataLength">Length of data to encrypt</param>
		/// <param name="secret">Encryption key</param>
		/// <param name="nonce">Nonce generated using <see cref="GenerateNonce"/></param>
		/// <param name="outputSize">Ciphertext (encrypted data) length</param>
		/// <returns>Initialized <see cref="GcmBlockCipher"/> for <see cref="AesGcmEncrypt(Org.BouncyCastle.Crypto.Modes.GcmBlockCipher,byte[],byte[],int,int,byte[],int)"/></returns>
		public static GcmBlockCipher AesGcmEncryptInit(int dataLength, byte[] secret, byte[] nonce, out int outputSize)
		{
			GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
			cipher.Init(true, new AeadParameters(new KeyParameter(secret), MacSizeBits, nonce));
			outputSize = cipher.GetOutputSize(dataLength) + NonceSizeBytes;

			return cipher;
		}

		/// <summary>
		/// AES Encryption using GCM mode
		/// </summary>
		/// <param name="cipher"><see cref="GcmBlockCipher"/> initialized using <see cref="AesGcmEncryptInit"/></param>
		/// <param name="nonce">Nonce generated using <see cref="GenerateNonce"/></param>
		/// <param name="data">Data to encrypt</param>
		/// <param name="dataOffset">Data offset</param>
		/// <param name="dataLength">Data length</param>
		/// <param name="cipherText">Output array for ciphertext (encrypted data), must have length of at least outputSize (in <see cref="AesGcmEncryptInit"/>) + <see cref="NonceSizeBytes"/> + <see cref="cipherTextOffset"/></param>
		/// <param name="cipherTextOffset">Output array offset</param>
		/// <returns>Ciphertext</returns>
		public static void AesGcmEncrypt(GcmBlockCipher cipher, byte[] nonce, byte[] data, int dataOffset, int dataLength, byte[] cipherText, int cipherTextOffset)
		{
			int len = cipher.ProcessBytes(data, dataOffset, dataLength, cipherText, cipherTextOffset + NonceSizeBytes);
			cipher.DoFinal(cipherText, len + cipherTextOffset + NonceSizeBytes);
			Array.Copy(nonce, 0, cipherText, cipherTextOffset, NonceSizeBytes);
		}

		/// <summary>
		/// AES Encryption using GCM mode
		/// </summary>
		/// <param name="data">Data to encrypt</param>
		/// <param name="secret">Encryption key</param>
		/// <param name="secureRandom">SecureRandom object</param>
		/// <param name="dataOffset">Data offset</param>
		/// <param name="dataLength">Data length (0 = use array length)</param>
		/// <returns>Ciphertext</returns>
		public static byte[] AesGcmEncrypt(byte[] data, byte[] secret, SecureRandom secureRandom, int dataOffset = 0, int dataLength = 0)
		{
			if (dataLength == 0)
				dataLength = data.Length;

			byte[] nonce = ArrayPool<byte>.Shared.Rent(NonceSizeBytes);
			try
			{
				secureRandom.NextBytes(nonce, 0, nonce.Length);
				GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
				cipher.Init(true, new AeadParameters(new KeyParameter(secret), MacSizeBits, nonce));
				int outputSize = cipher.GetOutputSize(data.Length);

				byte[] cipherText = new byte[outputSize + NonceSizeBytes];

				int len = cipher.ProcessBytes(data, dataOffset, dataLength, cipherText, NonceSizeBytes);
				cipher.DoFinal(cipherText, len + NonceSizeBytes);

				Array.Copy(nonce, cipherText, NonceSizeBytes);

				return cipherText;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(nonce);
			}
		}

		/// <summary>
		/// Initializes AES GCM for <see cref="AesGcmDecrypt(Org.BouncyCastle.Crypto.Modes.GcmBlockCipher,byte[],byte[],int,int,int)"/>
		/// </summary>
		/// <param name="nonce">Ciphertext nonce, can be retrived using <see cref="ReadNonce"/></param>
		/// <param name="secret">Encryption key</param>
		/// <param name="cipherTextLength">Ciphertext length (including nonce that is at the beginning of ciphertext!)</param>
		/// <param name="outputSize">Plaintext (decrypted data) length</param>
		/// <returns>Initialized <see cref="GcmBlockCipher"/> for <see cref="AesGcmDecrypt(Org.BouncyCastle.Crypto.Modes.GcmBlockCipher,byte[],byte[],int,int,int)"/></returns>
		public static GcmBlockCipher AesGcmDecryptInit(byte[] nonce, byte[] secret, int cipherTextLength, out int outputSize)
		{
			GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
			cipher.Init(false, new AeadParameters(new KeyParameter(secret), MacSizeBits, nonce));
			outputSize = cipher.GetOutputSize(cipherTextLength - NonceSizeBytes);

			return cipher;
		}

		/// <summary>
		/// AES Decryption using GCM mode
		/// </summary>
		/// <param name="cipher"><see cref="GcmBlockCipher"/> initialized using <see cref="AesGcmDecryptInit"/></param>
		/// <param name="cipherText">Ciphertext (data to decrypt)</param>
		/// <param name="plainText">Output array for plaintext (decrypted data), must have length of at least outputSize (in <see cref="AesGcmDecryptInit"/>) + <param name="plainTextOffset"></param></param>
		/// <param name="cipherTextOffset">Ciphertext offset</param>
		/// <param name="cipherTextLength">Ciphertext length (including nonce that is at the beginning of ciphertext!)</param>
		/// <param name="plainTextOffset">Plaintext offset</param>
		public static void AesGcmDecrypt(GcmBlockCipher cipher, byte[] cipherText, byte[] plainText, int cipherTextOffset = 0, int cipherTextLength = 0, int plainTextOffset = 0)
		{
            if (cipherTextLength == 0)
                cipherTextLength = cipherText.Length;

			int len = cipher.ProcessBytes(cipherText, cipherTextOffset + NonceSizeBytes, cipherTextLength - NonceSizeBytes, plainText, plainTextOffset);
			cipher.DoFinal(plainText, plainTextOffset + len);
		}

		/// <summary>
		/// AES Decryption using GCM mode
		/// </summary>
		/// <param name="data">Ciphertext</param>
		/// <param name="secret">Encryption key</param>
		/// <param name="dataOffset">Data offset</param>
		/// <param name="dataLength">Data length (0 = entire array)</param>
		/// <returns>Decrypted data</returns>
		public static byte[] AesGcmDecrypt(byte[] data, byte[] secret, int dataOffset = 0, int dataLength = 0)
		{
			if (dataLength <= 0)
				dataLength = data.Length;

			if (dataLength < NonceSizeBytes)
				throw new ArgumentException("Data length can't be smaller than nonce size.", nameof(dataLength));

			byte[] nonce = ArrayPool<byte>.Shared.Rent(NonceSizeBytes);

			try
			{
				Array.Copy(data, dataOffset, nonce, 0, NonceSizeBytes);

				GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
				cipher.Init(false, new AeadParameters(new KeyParameter(secret), MacSizeBits, nonce));

				int dataLen = dataLength - NonceSizeBytes;
				byte[] plainText = new byte[cipher.GetOutputSize(dataLen)];

				int len = cipher.ProcessBytes(data, dataOffset + NonceSizeBytes, dataLen, plainText, 0);
				cipher.DoFinal(plainText, len);

				return plainText;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(nonce);
			}
		}
	}
}
