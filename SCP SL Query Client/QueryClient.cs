using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Security;
using SCP_SL_Query_Client.Cryptography;
using SCP_SL_Query_Client.NetworkObjects;

namespace SCP_SL_Query_Client
{
    /// <summary>
    /// SCP:SL Query protocol client
    /// </summary>
    public class QueryClient : IDisposable
    {
        private bool _stop, _started;
        private ushort _txSizeLimit, _timeout;
        private uint _rxCounter, _txCounter;

        private readonly byte[] _encryptionKey;
        private readonly ushort _rxBufferSize;
        private readonly int _port;
        private readonly string _ip;
        private readonly object _writeLock = new object();

        private TcpClient _c;
        private NetworkStream _s;
        private Exception _lastRxError;
        private Stopwatch _sw;

        private readonly SecureRandom _random = new SecureRandom();

        /// <summary>
        /// SCP:SL Query client constructor
        /// </summary>
        /// <param name="ip">IP Address (or hostname) of the SCP:SL query server</param>
        /// <param name="port">Port of the SCP:SL query server</param>
        /// <param name="password">Password of the SCP:SL query server</param>
        /// <param name="rxBufferSize">Receive buffer size</param>
        /// <param name="txSizeLimit">Sent message size limit</param>
        public QueryClient(string ip, int port, string password, ushort rxBufferSize = 16384, ushort txSizeLimit = ushort.MaxValue)
        {
            _ip = ip;
            _port = port;
            _rxBufferSize = rxBufferSize;
            _txSizeLimit = txSizeLimit;
            _encryptionKey = Sha.Sha256(password);
        }

        /// <summary>
        /// Indicates if the client is connected to the server.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public bool Connected { get; private set; }

        public delegate void MessageReceived(QueryMessage message);
        public delegate void ConnectedToServer();
        public delegate void DisconnectedFromServer(DisconnectionReason reason);

        /// <summary>
        /// Event called when a message is received from query server.
        /// </summary>
        public event MessageReceived OnMessageReceived;

        /// <summary>
        /// Event called when a connection to the server is successfully established.
        /// </summary>
        public event ConnectedToServer OnConnectedToServer;

        /// <summary>
        /// Event called when the connection to the server is disconnected or connection failed.
        /// </summary>
        public event DisconnectedFromServer OnDisconnectedFromServer;

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Disconnect() => _stop = true;

        /// <summary>
        /// Gets last read exception.
        /// </summary>
        public Exception GetLastRxError() => _lastRxError;

        /// <summary>
        /// Connects to a server
        /// </summary>
        /// <param name="connectionTimeout">Connection timeout in milliseconds</param>
        /// <param name="flags">Client flags</param>
        /// <param name="username">Client username</param>
        /// <param name="permissions">Requested permissions (if you want to have lower permissions than granted by the server)</param>
        /// <param name="kickPower">Requested kick power (if you want to have lower kick power than granted by the server)</param>
        /// <param name="internalSleep">Delay (in milliseconds) in which the query client should check if there are new packets from the server</param>
        /// <returns>Query client thread</returns>
        /// <exception cref="Exception">Thrown if client has been already started.</exception>
        /// <exception cref="ArgumentException"><see cref="internalSleep"/> must be greater than 0.</exception>
        public Thread Connect(ushort connectionTimeout = 5000, QueryHandshake.ClientFlags flags = QueryHandshake.ClientFlags.None, string username = null, ulong permissions = ulong.MaxValue, byte kickPower = byte.MaxValue, int internalSleep = 40)
        {
            if (_started)
                throw new Exception("Server has been already started.");

            if (internalSleep < 1)
                throw new ArgumentException("Internal sleep must be greater than 0.", nameof(internalSleep));

            _started = true;
            _timeout = connectionTimeout;

            if (username != null && string.IsNullOrWhiteSpace(username))
                username = null;

            flags = flags.SetFlag(QueryHandshake.ClientFlags.SpecifyLogUsername, username != null);
            flags = flags.SetFlag(QueryHandshake.ClientFlags.RestrictPermissions, permissions != ulong.MaxValue || kickPower != byte.MaxValue);

            Thread t = new Thread(() => ConnectInternal(flags, username, permissions, kickPower, internalSleep))
            {
                IsBackground = true,
                Name = "SCP SL Query Client Thread"
            };
            t.Start();

            return t;
        }

        private void ConnectInternal(QueryHandshake.ClientFlags flags, string username, ulong permissions, byte kickPower, int internalSleep)
        {
            try
            {
                _c = new TcpClient();
                _c.NoDelay = true;
                _c.Connect(_ip, _port);
                _s = _c.GetStream();
                _s.ReadTimeout = 150;
                _s.WriteTimeout = 150;
            }
            catch (Exception e)
            {
                _lastRxError = e;
                _c?.Dispose();
                _s?.Dispose();
                Connected = false;
                _stop = true;
                OnDisconnectedFromServer?.Invoke(DisconnectionReason.ConnectionFailed);
                return;
            }

            _sw = new Stopwatch();
            _sw.Start();
            byte[] buffer = new byte[_rxBufferSize];
            byte[] cipherBuffer = new byte[_rxBufferSize];
            byte[] nonce = new byte[AES.NonceSizeBytes];
            int lengthToRead = 0;
            DisconnectionReason error = DisconnectionReason.DisconnectedByClient;

            while (!_stop)
            {
                try
                {
                    if (!_c.Connected || !_s.CanRead || !_s.CanWrite)
                    {
                        error = DisconnectionReason.ConnectionLost;
                        break;
                    }

                    if (_sw.ElapsedMilliseconds >= _timeout)
                    {
                        if (!Connected)
                        {
                            error = DisconnectionReason.ConnectionFailed;
                            break;
                        }

                        Send("noop", QueryMessage.QueryContentTypeToServer.SuppressedOutputCommand);
                    }

                    if (lengthToRead == 0)
                    {
                        if (_c.Available < 2)
                        {
                            Thread.Sleep(internalSleep);
                            continue;
                        }

                        if (_s.Read(buffer, 0, 2) != 2)
                        {
                            error = DisconnectionReason.CannotReadLength;
                            break;
                        }

                        lengthToRead = BinaryPrimitives.ReadUInt16BigEndian(buffer);

                        if (lengthToRead > _rxBufferSize)
                        {
                            error = DisconnectionReason.RxBufferSizeExceeded;
                            break;
                        }
                    }

                    if (_c.Available < lengthToRead)
                    {
                        Thread.Sleep(internalSleep);
                        continue;
                    }

                    if (_s.Read(buffer, 0, lengthToRead) != lengthToRead)
                    {
                        error = DisconnectionReason.ReadError;
                        break;
                    }

                    if (Connected)
                    {
                        int outputSize;

                        try
                        {
                            AES.ReadNonce(nonce, buffer);
                            GcmBlockCipher cipher = AES.AesGcmDecryptInit(nonce, _encryptionKey, lengthToRead, out outputSize);
                            AES.AesGcmDecrypt(cipher, buffer, cipherBuffer, 0, lengthToRead);
                        }
                        catch (Exception e)
                        {
                            error = DisconnectionReason.DecryptionFailed;
                            _lastRxError = e;
                            break;
                        }

                        try
                        {
                            QueryMessage qm = QueryMessage.Deserialize(new ReadOnlySpan<byte>(cipherBuffer, 0, outputSize));

                            if (!qm.Validate(_rxCounter++))
                            {
                                error = DisconnectionReason.MessageValidationFailed;
                                break;
                            }

                            OnMessageReceived?.Invoke(qm);
                        }
                        catch (Exception e)
                        {
                            error = DisconnectionReason.DeserializationFailed;
                            _lastRxError = e;
                            break;
                        }

                        lengthToRead = 0;
                        continue;
                    }

                    QueryHandshake qh = QueryHandshake.Deserialize(new ReadOnlySpan<byte>(buffer, 0, lengthToRead));

                    if (!qh.Validate())
                    {
                        error = DisconnectionReason.HandshakeValidationFailed;
                        break;
                    }

                    if (qh.MaxPacketSize < 3072)
                    {
                        error = DisconnectionReason.ServerRxBufferToSmall;
                        break;
                    }

                    if (qh.MaxPacketSize < _txSizeLimit)
                        _txSizeLimit = qh.MaxPacketSize;

                    if (qh.ServerTimeoutThreshold > 1000)
                        _timeout = (ushort)(qh.ServerTimeoutThreshold * 0.8);
                    else _timeout = (ushort)(qh.ServerTimeoutThreshold * 0.65);

                    try
                    {
                        QueryHandshake response = new QueryHandshake(_rxBufferSize, qh.AuthChallenge, flags, permissions, kickPower, username, 500);
                        Send(response, response.SizeToServer);
                    }
                    catch (Exception e)
                    {
                        error = DisconnectionReason.SendingHandshakeResponseFailed;
                        _lastRxError = e;
                        break;
                    }

                    lengthToRead = 0;
                    Connected = true;
                    OnConnectedToServer?.Invoke();
                }
                catch (Exception e)
                {
                    error = DisconnectionReason.ReadError;
                    _lastRxError = e;
                    break;
                }
            }

            try
            {
                _c?.Dispose();
                _s?.Dispose();
            }
            catch
            {
                //Ignore
            }

            Connected = false;
            _stop = true;
            OnDisconnectedFromServer?.Invoke(error);
        }

        /// <summary>
        /// Sends data to query server
        /// </summary>
        /// <param name="msg">Data to send</param>
        /// <param name="contentType">Content type of the data</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Send(string msg, QueryMessage.QueryContentTypeToServer contentType)
        {
            if (!Connected)
                throw new Exception("Not connected to server.");

            if (string.IsNullOrWhiteSpace(msg))
                return;

            Send(new QueryMessage(msg, ++_txCounter, (byte)contentType));
        }

        /// <summary>
        /// Sends data to query server
        /// </summary>
        /// <param name="msg">Data to send</param>
        /// <param name="contentType">Content type of the data</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Send(byte[] msg, QueryMessage.QueryContentTypeToServer contentType)
        {
            if (!Connected)
                throw new Exception("Not connected to server.");

            if (msg.Length == 0)
                return;

            Send(new QueryMessage(msg, ++_txCounter, (byte)contentType));
        }

        private void Send(QueryMessage qm) => Send(qm, qm.SerializedSize);

        private void Send(IQueryMessage qm, int serializedSize)
        {
            if (serializedSize > _txSizeLimit)
                throw new Exception("Message exceed Tx size limit.");

            byte[] buffer = ArrayPool<byte>.Shared.Rent(serializedSize);
            byte[] cipherBuffer = null;
            byte[] nonce = ArrayPool<byte>.Shared.Rent(AES.NonceSizeBytes);
            const int headerSize = 2;

            try
            {
                int len = qm.Serialize(buffer);

                AES.GenerateNonce(nonce, _random);
                GcmBlockCipher cipher = AES.AesGcmEncryptInit(len, _encryptionKey, nonce, out int cipherTextLen);

                int finalLen = cipherTextLen + headerSize;

                if (finalLen > _txSizeLimit)
                    throw new Exception("Encrypted message exceed Tx size limit.");

                cipherBuffer = ArrayPool<byte>.Shared.Rent(finalLen);

                BinaryPrimitives.WriteUInt16BigEndian(cipherBuffer, (ushort)cipherTextLen);
                AES.AesGcmEncrypt(cipher, nonce, buffer, 0, len, cipherBuffer, headerSize);

                SendRaw(cipherBuffer, 0, finalLen);
            }
            catch
            {
                _txCounter--;
                throw;
            }
            finally
            {
                if (cipherBuffer != null)
                    ArrayPool<byte>.Shared.Return(cipherBuffer);

                ArrayPool<byte>.Shared.Return(buffer);
                ArrayPool<byte>.Shared.Return(nonce);
            }
        }

        private void SendRaw(byte[] msg, int offset, int len)
        {
            lock (_writeLock)
            {
                _s.Write(msg, offset, len);

                if (Connected)
                    _sw.Restart();
            }
        }

        public void Dispose() => Disconnect();
    }
}
