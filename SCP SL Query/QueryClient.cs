using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SCP_SL_Query.Interfaces;
using SCP_SL_Query.Utils;

namespace SCP_SL_Query
{
    public class QueryClient
    {
        private readonly IClient _iclient;
        private readonly string _ip;
        private string _password;
        private readonly int _port;
        private bool _stop, _ponged;
        private Thread _thr, _keepAlive;
        private Stream _stm;
        private readonly UTF8Encoding _encoder;

        public QueryClient(string ip, int port, IClient ic)
        {
            _iclient = ic;
            _ip = ip;
            _port = port;
            _encoder = new UTF8Encoding();
        }

        public string Password
        {
            set => _password = value;
        }

        public void Connect()
        {
            _stop = false;
            _thr = new Thread(Conn) { IsBackground = true };
            _thr.Start();
        }

        public void Disconnect()
        {
            _stop = true;
        }

        private void Conn()
        {
            var tcpclnt = new TcpClient();
            try
            {
                tcpclnt.Connect(_ip, _port);
            }
            catch
            {
                _iclient.ConnectionFailure();
                return;
            }

            _stm = tcpclnt.GetStream();
            _stm.ReadTimeout = 200;
            _stm.WriteTimeout = 200;
            _ponged = true;
            _keepAlive =
                new Thread(() => Ping(5000))
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
            _keepAlive.Start();
            _iclient.ConnectionEstablished();

            while (!_stop)
            {
                try
                {
                    byte[] b = new byte[4096];
                    int k;
                    try
                    {
                        k = _stm.Read(b, 0, 4096);
                    }
                    catch
                    {
                        k = -1;
                        Thread.Sleep(10);
                    }

                    if (k <= -1) continue;
                    var rec = AuthenticatedMessage.Decode(b);
                    foreach (byte[] bb in rec)
                    {
                        try
                        {
                            var msg = _encoder.GetString(bb);
                            var message =
                                AuthenticatedMessage.AuthenticateMessage(msg, Time.CurrentTimestamp(), _password);
                            if (message.Message == "Pong")
                            {
                                _ponged = true;
                                _iclient.KeepAliveReceived();
                            }
                            else _iclient.Receive(message.Message, message.Administrator);
                        }
                        catch (MessageAuthenticationFailureException)
                        {
                            _iclient.AuthenticationFailure();
                        }
                        catch (MessageExpiredException)
                        {
                            _iclient.ExpiredMessageReceived();
                        }
                        catch
                        {
                            _iclient.MalformedMessageReceived();
                        }
                    }

                }
                catch (Exception e)
                {
                    _iclient.Error(e.StackTrace + Environment.NewLine + e.Message);
                }
            }

            try
            {
                _keepAlive?.Abort();
            }
            catch
            {
                // ignored
            }

            tcpclnt.Close();
            Thread.Sleep(30);
            _iclient.Disconnected();
        }

        private void Ping(int sleep)
        {
            while (!_stop)
            {
                if (!_ponged)
                {
                    _iclient.ConnectionLost();
                    _stop = true;
                    break;
                }
                if (_stop) break;
                _ponged = false;
                SilentSend("Ping", false);
                Thread.Sleep(sleep);
            }
        }

        public void Send(string msg)
        {
            Send(msg, !string.IsNullOrEmpty(_password));
        }

        public void Send(string msg, bool authenticate)
        {
            _iclient.Sent(msg, authenticate && !string.IsNullOrEmpty(_password));
            msg = !authenticate || string.IsNullOrEmpty(_password)
                ? AuthenticatedMessage.GenerateNonAuthenticatedMessage(msg)
                : AuthenticatedMessage.GenerateAuthenticatedMessage(msg, Time.CurrentTimestamp(), _password);
            Send(_encoder.GetBytes(msg));
        }

        private void SilentSend(string msg, bool authenticate)
        {
            msg = !authenticate || string.IsNullOrEmpty(_password)
                ? AuthenticatedMessage.GenerateNonAuthenticatedMessage(msg)
                : AuthenticatedMessage.GenerateAuthenticatedMessage(msg, Time.CurrentTimestamp(), _password);
            Send(_encoder.GetBytes(msg));
        }

        private void Send(byte[] msg)
        {
            try
            {
                var message = AuthenticatedMessage.Encode(msg);
                _stm.Write(message, 0, message.Length);
            }
            catch (Exception e)
            {
                _iclient.Error("Can't send response: " + e.StackTrace);
            }
        }
    }
}
