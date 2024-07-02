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
    public class QueryClient : IDisposable
    {
        private readonly string _ip;
        private readonly byte[] _encryptionKey;
        private readonly int _port;
        private bool _stop;
        private TcpClient _c;
        private NetworkStream _s;
        private Exception _lastRxError;
        private bool _started, _connected;

        private readonly ushort _rxBufferSize;
        private ushort _txSizeLimit;
        private uint _rxCounter, _txCounter;

        private readonly SecureRandom _random = new SecureRandom();

        private const int PingThreshold = 8000;

        public delegate void MessageReceived(QueryMessage message);
        public delegate void ConnectedToServer();
        public delegate void DisconnectedFromServer(DisconnectionReason reason);

        /// <summary>
        /// Event called when a message is received from query server.
        /// </summary>
        public event MessageReceived OnMessageReceived;

        public event ConnectedToServer OnConnectedToServer;

        public event DisconnectedFromServer OnDisconnectedFromServer;

        public QueryClient(string ip, int port, string password, ushort rxBufferSize = 16384, ushort txSizeLimit = ushort.MaxValue)
        {
            _ip = ip;
            _port = port;
            _rxBufferSize = rxBufferSize;
            _txSizeLimit = txSizeLimit;
            _encryptionKey = Sha.Sha256(password);
        }

        public bool Connected => _connected;

        public void Disconnect() => _stop = true;

        public Exception GetLastRxEror() => _lastRxError;

        //TODO: Add support for setting flags, permissions and kick power
        public void Connect()
        {
            if (_started)
                throw new Exception("Server has been already started.");

            _started = true;

            new Thread(ConnectInternal)
            {
                IsBackground = true,
                Name = "SCP SL Query Client Thread"
            }.Start();
        }

        private void ConnectInternal()
        {
            _c = new TcpClient();
            _c.NoDelay = true;
            _c.Connect(_ip, _port);
            _s = _c.GetStream();
            _s.ReadTimeout = 150;
            _s.WriteTimeout = 150;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte[] buffer = new byte[_rxBufferSize];
            byte[] cipherBuffer = new byte[_rxBufferSize];
            byte[] nonce = new byte[AES.NonceSizeBytes];
            int lengthToRead = 0;
            DisconnectionReason error = DisconnectionReason.DisconnectedByClient;

            while (!_stop)
            {
                try
                {
                    if (sw.ElapsedMilliseconds >= PingThreshold)
                    {
                        if (!_connected)
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
                            Thread.Sleep(50);
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
                        Thread.Sleep(50);
                        continue;
                    }

                    if (_s.Read(buffer, 0, lengthToRead) != lengthToRead)
                    {
                        error = DisconnectionReason.ReadError;
                        break;
                    }

                    if (_connected)
                    {
                        int outputSize;

                        try
                        {
                            AES.ReadNonce(nonce, buffer, 0);
                            GcmBlockCipher cipher = AES.AesGcmDecryptInit(buffer, _encryptionKey, lengthToRead, out outputSize);
                            AES.AesGcmDecrypt(cipher, buffer, cipherBuffer, 0, lengthToRead, 0);
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

                    try
                    {
                        QueryHandshake response = new QueryHandshake(_rxBufferSize, qh.AuthChallenge);
                        Send(response, response.SizeToServer);
                    }
                    catch (Exception e)
                    {
                        error = DisconnectionReason.SendingHandshakeResponseFailed;
                        _lastRxError = e;
                        continue;
                    }

                    _connected = true;
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

            _connected = false;
            _stop = true;
            OnDisconnectedFromServer?.Invoke(error);
        }

        /// <summary>
        /// Sends data to query server
        /// </summary>
        /// <param name="msg">Data to send</param>
        /// <param name="contentType">Content type of the data</param>
        internal void Send(string msg, QueryMessage.QueryContentTypeToServer contentType)
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
        internal void Send(byte[] msg, QueryMessage.QueryContentTypeToServer contentType)
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

        private void SendRaw(byte[] msg, int offset, int len) => _s.Write(msg, offset, len);

        public void Dispose() => Disconnect();
    }
}
