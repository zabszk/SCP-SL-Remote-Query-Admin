//File from SCP: Secret Laboratory game source code
//Written by: Łukasz "zabszk" Jurczyk, 2024

using System;
using System.Buffers;
using System.Buffers.Binary;
using SCP_SL_Query_Client.Misc;

namespace SCP_SL_Query_Client.NetworkObjects
{
	/// <summary>
	/// Handshake between query client and SCP:SL server
	/// </summary>
	public readonly struct QueryHandshake : IQueryMessage
	{
		/// <summary>
		/// Maximum supported received packet size
		/// </summary>
		public readonly ushort MaxPacketSize;

		/// <summary>
		/// Handshake generation timestamp
		/// </summary>
		public readonly long Timestamp;

		/// <summary>
		/// Handshake authentication challenge
		/// </summary>
		public readonly byte[] AuthChallenge;

		/// <summary>
		/// Query client flags
		/// Ignored when handshake is sent to the client
		/// </summary>
		public readonly ClientFlags Flags;

		/// <summary>
		/// Permissions requested by the client
		/// </summary>
		public readonly ulong Permissions;

		/// <summary>
		/// Kick power requested by the client
		/// </summary>
		public readonly byte KickPower;

		/// <summary>
		/// Query client username (only for logging purposes)
		/// </summary>
		public readonly string Username;

		/// <summary>
		/// Challenge length
		/// </summary>
		public const int ChallengeLength = 24;

		/// <summary>
		/// Size of challenge when sent to the client
		/// </summary>
		public const int SizeToClient = sizeof(ushort) + sizeof(long) + ChallengeLength;

		/// <summary>
		/// Size of challenge when sent to the server
		/// </summary>
		public int SizeToServer => SizeToClient + sizeof(byte) + (Flags.HasFlagFast(ClientFlags.RestrictPermissions) ? sizeof(ulong) + sizeof(byte) : 0) + (Flags.HasFlagFast(ClientFlags.SpecifyLogUsername) ? Utf8.GetLength(Username) : 0);

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="maxPacketSize">Maximum supported received packet size</param>
		/// <param name="authChallenge">Handshake authentication challenge</param>
		/// <param name="flags">Client flags</param>
		/// <param name="permissions">Permissions requested by query user</param>
		/// <param name="kickPower">Kick power requested by query user</param>
		/// <param name="username">Query client username</param>
		/// <exception cref="ArgumentException">Invalid challenge length or username specification flag is set, but username is null, empty or whitespace</exception>
		public QueryHandshake(ushort maxPacketSize, byte[] authChallenge, ClientFlags flags = ClientFlags.None, ulong permissions = ulong.MaxValue, byte kickPower = byte.MaxValue, string username = null) : this(maxPacketSize, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), authChallenge, flags, permissions, kickPower, username) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="maxPacketSize">Maximum supported received packet size</param>
		/// <param name="timestamp">Handshake generation timestamp</param>
		/// <param name="authChallenge">Handshake authentication challenge</param>
		/// <param name="flags">Client flags</param>
		/// <param name="permissions">Permissions requested by query user</param>
		/// <param name="kickPower">Kick power requested by query user</param>
		/// <param name="username">Query client username</param>
		/// <exception cref="ArgumentException">Invalid challenge length or username specification flag is set, but username is null, empty or whitespace</exception>
		public QueryHandshake(ushort maxPacketSize, long timestamp, byte[] authChallenge, ClientFlags flags = ClientFlags.None, ulong permissions = ulong.MaxValue, byte kickPower = byte.MaxValue, string username = null)
		{
			if (authChallenge.Length != ChallengeLength)
				throw new ArgumentException($"Auth challenge must be {ChallengeLength} bytes long.", nameof(authChallenge));

			if (flags.HasFlagFast(ClientFlags.SpecifyLogUsername) && string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username must be specified (and not be empty or whitespace) when ClientFlags.SpecifyLogUsername is set.", nameof(username));

			MaxPacketSize = maxPacketSize;
			Timestamp = timestamp;
			AuthChallenge = authChallenge;
			Flags = flags;
			Permissions = permissions;
			KickPower = kickPower;
			Username = username;
		}

		/// <summary>
		/// Validates handshake expiration
		/// </summary>
		/// <param name="timeTolerance">Allowed time difference (in seconds) between server and client</param>
		/// <returns></returns>
		public bool Validate(int timeTolerance = 120) => Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Timestamp) <= timeTolerance;

		/// <summary>
		/// Serializes into a byte span
		/// </summary>
		/// <param name="buffer">Output data</param>
		public int Serialize(Span<byte> buffer)
		{
			BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(0, 2), MaxPacketSize);
			BinaryPrimitives.WriteInt64BigEndian(buffer.Slice(2, 8), Timestamp);
			AuthChallenge.CopyTo(buffer.Slice(10, ChallengeLength));

			buffer[ChallengeLength + 10] = (byte)Flags;
			int start = ChallengeLength + 11;

			if (Flags.HasFlagFast(ClientFlags.RestrictPermissions))
			{
				BinaryPrimitives.WriteUInt64BigEndian(buffer.Slice(start, sizeof(ulong)), Permissions);
				start += sizeof(ulong);
				buffer[start++] = KickPower;
			}

			if (Flags.HasFlagFast(ClientFlags.SpecifyLogUsername))
			{
				int len = Utf8.GetLength(Username);
				byte[] usernameBuffer = ArrayPool<byte>.Shared.Rent(len);

				try
				{
					Utf8.GetBytes(Username, usernameBuffer);
					new ReadOnlySpan<byte>(usernameBuffer, 0, len).CopyTo(buffer.Slice(start, len));
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(usernameBuffer);
				}
			}

			return SizeToServer;
		}

		/// <summary>
		/// Deserializes message from a byte span
		/// </summary>
		/// <param name="buffer">Serialized data</param>
		/// <returns>Deserialized message</returns>
		public static QueryHandshake Deserialize(ReadOnlySpan<byte> buffer)
		{
			ushort maxPacketSize = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(0, 2));
			long timestamp = BinaryPrimitives.ReadInt64BigEndian(buffer.Slice(2, 8));
			byte[] authChallenge = buffer.Slice(10, ChallengeLength).ToArray();

            return new QueryHandshake(maxPacketSize, timestamp, authChallenge);
		}

		/// <summary>
		/// Query client flags
		/// </summary>
		[Flags]
		public enum ClientFlags : byte
		{
			/// <summary>
			/// No flags
			/// </summary>
			None = 0,

			/// <summary>
			/// Command responses are not sent to the query client executing them
			/// </summary>
			SuppressCommandResponses = 1,

			/// <summary>
			/// Server console feed is subscribed after authentication
			/// </summary>
			SubscribeServerConsole = 1 << 1,

			/// <summary>
			/// Server logs feed is subscribed after authentication
			/// </summary>
			SubscribeServerLogs = 1 << 2,

			/// <summary>
			/// Responses to RA commands are serialized and contains metadata (otherwise they contain only response text, without other data)
			/// </summary>
			RemoteAdminMetadata = 1 << 3,

			/// <summary>
			/// Set when the handshake contains list of permissions requested by the client (other permissions won't be granted by the server)
			/// </summary>
			RestrictPermissions = 1 << 4,

			/// <summary>
			/// Set when username (for logging purposes) is specified in the handshake
			/// </summary>
			SpecifyLogUsername = 1 << 5,
		}
	}

	/// <summary>
	/// Utils class for <see cref="QueryHandshake.ClientFlags"/>
	/// </summary>
	public static class QueryClientFlagUtils
	{
		public static bool HasFlagFast(this QueryHandshake.ClientFlags res, QueryHandshake.ClientFlags flag) => (res & flag) == flag;
	}
}
