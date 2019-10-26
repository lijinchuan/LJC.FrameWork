using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.SocketApplication.SocketSTD
{
    public class MessageApp : IDisposable
    {
        private AutoResetEvent _startSign = new AutoResetEvent(false);
        protected volatile Socket socketClient;
        protected Socket socketServer;
        /// <summary>
        /// 用来收发广播组播
        /// </summary>
        private Socket udpBCSocket;
        /// <summary>
        /// 用来发组播
        /// </summary>
        private UdpClient udpMCClient;
        private Socket udpMCSocket;
        protected volatile bool isStartClient = false;
        protected volatile bool stop = false;
        protected volatile bool errorResume = true;
        protected string ipString;
        protected int ipPort;
        protected volatile bool isStartServer = false;
        private Thread listeningThread = null;

        /// <summary>
        /// 是否采用安全连接
        /// </summary>
        protected bool isSecurity = false;
        protected string rsaPubKey = string.Empty;
        protected string rsaRrivateKey = string.Empty;
        /// <summary>
        /// 安全连接key
        /// </summary>
        private string encryKey = string.Empty;

        /// <summary>
        /// 是否正在启动客户端
        /// </summary>
        private bool _isStartingClient = false;

        public event Action OnClientReset;

        private bool _enbaleBCast = false;
        /// <summary>
        /// 是否接收广播
        /// </summary>
        public bool EnableBroadCast
        {
            get
            {
                return _enbaleBCast;
            }
            set
            {
                _enbaleBCast = value;
                if (_enbaleBCast)
                {
                    if (udpBCSocket == null)
                    {
                        udpBCSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        udpBCSocket.ExclusiveAddressUse = false;
                        udpBCSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        udpBCSocket.EnableBroadcast = true;
                        udpBCSocket.Bind(new IPEndPoint(IPAddress.Any,SocketApplicationComm.BCAST_PORT));
                        udpBCSocket.ReceiveBufferSize = 32000;
                        udpBCSocket.SendBufferSize = 32000;

                        new Action(ReceivingBroadCast).BeginInvoke(null, null);
                    }
                }
            }
        }

        /// <summary>
        /// 是否接收组播
        /// </summary>
        private bool _enableMCast = false;
        public bool EnableMultiCast
        {
            get
            {
                return _enableMCast;
            }
            set
            {
                _enableMCast = value;
                if (_enableMCast)
                {
                    if (udpMCClient == null)
                    {
                        udpMCSocket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
                        udpMCSocket.ExclusiveAddressUse = false;
                        udpMCSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        //udpMCSocket.EnableBroadcast = true;
                        udpMCSocket.MulticastLoopback = true;
                        udpMCSocket.Ttl = 10;
                        udpMCSocket.Bind(new IPEndPoint(IPAddress.Any, SocketApplicationComm.MCAST_PORT));

                        MulticastOption optionValue = new MulticastOption(SocketApplicationComm.MCAST_ADDR);
                        udpMCSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
                        //udpMCSocket.JoinMulticastGroup(SocketApplicationComm.MCAST_ADDR);

                        //udpMCClient = new UdpClient(SocketApplicationComm.MCAST_PORT);
                        //udpMCClient.JoinMulticastGroup(SocketApplicationComm.MCAST_ADDR);

                        new Action(ReceivingMultiCast).BeginInvoke(null, null);
                    }
                }
            }
        }

        /// <summary>
        /// 广播
        /// </summary>
        public event Action<Message> OnBroadCast;
        /// <summary>
        /// 组播
        /// </summary>
        public event Action<Message> OnMultiCast;

        public event Action<Exception> Error;
        /// <summary>
        /// 对象清理之前的事件
        /// </summary>
        public event Action BeforRelease;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="stop">如果为true,不会断开自动重连</param>
        public MessageApp(string ip, int port, bool errorResume = true,bool isSecurity=false)
        {
            this.ipString = ip;
            this.ipPort = port;
            this.errorResume = errorResume;
            this.isSecurity = isSecurity;
        }

        public MessageApp()
        {

        }

        public bool StartServer()
        {
            try
            {
                if (socketServer == null)
                {
                    socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //socketServer.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                    socketServer.ReceiveBufferSize = 32000;
                    socketServer.SendBufferSize = 32000;
                    socketServer.NoDelay = true;
                    socketServer.Bind(new IPEndPoint(IPAddress.Any, ipPort));
                }

                socketServer.Listen(int.MaxValue);

                if (!isStartServer)
                {
                    listeningThread = new Thread(Listening);
                    listeningThread.Start();
                }

                isStartServer = true;
                return true;
            }
            catch (Exception e)
            {
                OnError(e);
                return false;
            }
        }

        public void CloseClient()
        {
            try
            {
                if (socketClient != null&&socketClient.Connected)
                {
                    socketClient.Shutdown(SocketShutdown.Both);
                    socketClient.Close();
                }
                isStartClient = false;
            }
            catch (Exception ex)
            {

            }
        }

        public bool StartClient()
        {
            if (_isStartingClient)
            {
                return false;
            }
            _isStartingClient = true;

            try
            {
                if (socketClient != null && socketClient.Connected)
                    return true;

                bool isResetClient = false;
                if (socketClient != null)
                {
                    try
                    {
                        socketClient.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {

                    }
                    socketClient.Close();
                    isResetClient = true;
                }

                if (!string.IsNullOrWhiteSpace(encryKey))
                {
                    encryKey = string.Empty;
                }

                socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketClient.ReceiveBufferSize = 32000;
                socketClient.SendBufferSize = 32000;
                socketClient.NoDelay = true;

                try
                {
                    if (!string.IsNullOrEmpty(ipString))
                        socketClient.Connect(IPAddress.Parse(ipString), ipPort);
                    else
                        socketClient.Connect(IPAddress.Any, ipPort);

                    if (isSecurity)
                    {
                        RsaEncryHelper.GenPair(out rsaPubKey, out rsaRrivateKey);
                        var msg = new Message(MessageType.NEGOTIATIONENCRYR);
                        encryKey = null;
                        msg.SetMessageBody(new NegotiationEncryMessage
                        {
                            PublicKey = rsaPubKey
                        });
                        _startSign.Reset();
                        socketClient.SendMessage(msg, string.Empty);
                        var buffer = ReceivingNext(socketClient);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), buffer);
                        _startSign.WaitOne(30 * 1000);
                        if (string.IsNullOrWhiteSpace(encryKey))
                        {
                            throw new Exception("协商加密失败");
                        }
                    }
                }
                catch (SocketException e)
                {
                    var ne = new Exception(string.Format("连接到远程服务器{0}失败，端口:{1}，原因:{2},网络错误号:{3}",
                        ipString, ipPort, e.Message, e.SocketErrorCode));
                    throw ne;

                }
                catch (Exception e)
                {
                    throw e;
                }

                if (!isStartClient && listeningThread == null)
                {
                    listeningThread = new Thread(Receiving);
                    listeningThread.Start();
                }

                isStartClient = true;

                if (isResetClient && OnClientReset != null)
                {
                    OnClientReset();
                }

                return true;
            }
            catch (Exception e)
            {
                //OnError(e);
                LogManager.LogHelper.Instance.Error("StartClient error", e);

                return false;
            }
            finally
            {
                _isStartingClient = false;
            }
        }

        public void BroadCast(Message message)
        {
            SocketApplicationComm.Broadcast(this.udpBCSocket, message);
        }

        public void MultiCast(Message message)
        {
            SocketApplicationComm.MulitBroadcast(this.udpMCSocket, message);
        }

        private void ReceivingBroadCast()
        {
            EndPoint endPoint=new IPEndPoint(SocketApplicationComm.BROADCAST_ADDR,SocketApplicationComm.BCAST_PORT);
            while (!stop)
            {
                try
                {

                    byte[] buffer = new byte[SocketApplicationComm.Udp_MTU];
                    int count = this.udpBCSocket.ReceiveFrom(buffer, ref endPoint);

                    if (count == 0)
                        break;

                    //byte[] buffer = new UdpClient().Receive(ref endPoint);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessBoradCastMessage), buffer);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
        }

        private void ReceivingMultiCast()
        {
            EndPoint multicast = new IPEndPoint(SocketApplicationComm.MCAST_ADDR, SocketApplicationComm.MCAST_PORT + 1);

            while (!stop)
            {
                try
                {

                    byte[] buffer = new byte[SocketApplicationComm.Udp_MTU];
                    this.udpMCSocket.ReceiveFrom(buffer,ref multicast);
                    if (buffer.Count() == 0)
                        break;

                    //byte[] buffer = this.udpMCClient.Receive(ref multicast);


                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMultiCastMessage), buffer);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
        }

        private static byte[] ReceivingNext(Socket s)
        {
            int readLen = 0, timeout = 0, count = 0;
            byte[] buff4 = new byte[4];
            if(s.Receive(buff4, 0, 4, SocketFlags.None) != 4)
            {
                throw new SocketApplicationException("读取长度错误");
            }

            int dataLen = BitConverter.ToInt32(buff4, 0);
            if(s.Receive(buff4, 0, 4, SocketFlags.None) != 4)
            {
                throw new SocketApplicationException("读取长度错误");
            }
            var crc32 = BitConverter.ToInt32(buff4, 0);


            //MemoryStream ms = new MemoryStream();
            readLen = 0;
            timeout = 0;

            dataLen -= 4;
            byte[] buffer = new byte[dataLen];

            if (SocketApplicationEnvironment.TraceSocketDataBag)
            {
                LogManager.LogHelper.Instance.Debug(s.Handle + "准备接收数据：" + dataLen);
            }

            while (readLen < dataLen)
            {
                count = s.Receive(buffer, readLen, dataLen - readLen, SocketFlags.None);

                if (count == 0)
                {
                    Thread.Sleep(1);
                    timeout += 1;
                    if (timeout > 10000)
                        break;
                    continue;
                }
                readLen += count;
            }

            var calcrc32 = LJC.FrameWork.Comm.HashEncrypt.GetCRC32(buffer, 0);
            if (calcrc32 != crc32)
            {
                var ex = new SocketApplicationException("检查校验码出错");
                ex.Data.Add("crc32", crc32);
                ex.Data.Add("calcrc32", calcrc32);
                ex.Data.Add("data", Convert.ToBase64String(buffer));
                throw ex;
            }

            if (SocketApplicationEnvironment.TraceSocketDataBag)
            {
                LogManager.LogHelper.Instance.Debug(s.Handle + "接收数据," + readLen + "," + Convert.ToBase64String(buffer));
            }

            return buffer;
        }

        private void Receiving()
        {
            while (!this.stop/* && socketClient.Connected*/)
            {
                try
                {
                    var buffer = ReceivingNext(socketClient);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), buffer);
                }
                catch (SocketException e)
                {
                    OnError(e);
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
            try
            {
                socketClient.Shutdown(SocketShutdown.Both);
            }
            catch
            {

            }
            socketClient.Close();
        }

        private void ProcessMessage(object buffer)
        {
            try
            {
                byte[] data = (byte[])buffer;
                if (!string.IsNullOrWhiteSpace(this.encryKey))
                {
                    data = AesEncryHelper.AesDecrypt(data, this.encryKey);
                }
                Message message = EntityBufCore.DeSerialize<Message>(data);
                if (message.IsMessage(MessageType.NEGOTIATIONENCRYR))
                {
                    var nmsg = message.GetMessageBody<NegotiationEncryMessage>();
                    this.encryKey = Encoding.ASCII.GetString(RsaEncryHelper.RsaDecrypt(Convert.FromBase64String(nmsg.EncryKey), this.rsaRrivateKey));
                    Console.WriteLine("收到加密串:" + encryKey);
                    LogManager.LogHelper.Instance.Info("通信已经加密:"+encryKey);
                    _startSign.Set();
                }
                else
                {
                    OnMessage(message);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void ProcessBoradCastMessage(object buffer)
        {
            try
            {
                byte[] data = (byte[])buffer;
                Message message = EntityBufCore.DeSerialize<Message>(data);

                if (OnBroadCast != null)
                {
                    OnBroadCast(message);
                }
            }
            catch (Exception e)
            {

            }
        }

        private void ProcessMultiCastMessage(object buffer)
        {
            try
            {
                byte[] data = (byte[])buffer;
                Message message = EntityBufCore.DeSerialize<Message>(data);

                if (OnMultiCast != null)
                {
                    OnMultiCast(message);
                }
            }
            catch (Exception e)
            {

            }
        }

        //private int sendMessageTryCountLimit = 3;
        public bool SendMessage(Message message)
        {
            try
            {
                return socketClient.SendMessage(message, this.encryKey) > 0;
            }
            catch (Exception e)
            {
                OnError(e);
                throw e;
            }
        }

        protected virtual void OnMessage(Message message)
        {
           
        }

        protected virtual void OnError(Exception e)
        {
            if (stop)
                return;

            if (socketClient != null &&errorResume&& !socketClient.Connected)
            {
                e.Data.Add("checksocket", "需要发起重连");
                new Action(() => StartClient()).BeginInvoke(null, null);
            }
            else
            {
                e.Data.Add("errorResume", errorResume);
                e.Data.Add("socketClient.Connected", socketClient.Connected);
                e.Data.Add("checksocket", "不需要发起重连");
            }

            if (Error != null)
            {
                Error(e);
            }
        }

        #region server

        private void Listening()
        {
            while (!stop)
            {
                try
                {
                    Socket socket = socketServer.Accept();

                    Thread thread = new Thread(new ParameterizedThreadStart(OnSocket));
                    thread.Start(socket);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
            socketServer.Shutdown(SocketShutdown.Both);
            socketServer.Close();
            SocketApplicationComm.Debug("关闭服务器套接字!");
        }

        private void OnSocket(object obj)
        {
            Socket socket = (Socket)obj;
            socket.NoDelay = true;
            IPEndPoint endPoint = (IPEndPoint)socket.RemoteEndPoint;

            Session appSocket = new Session();

            appSocket.IPAddress = endPoint.Address.ToString();
            appSocket.IsValid = true;
            appSocket.SessionID = SocketApplicationComm.GetSeqNum();
            appSocket.Socket = socket;
            

            while (appSocket.IsValid&&appSocket.Socket.Connected)
            {
                try
                {
                    var buffer = ReceivingNext(socket);

                    //搞成异步的
                    new Action<byte[], Session>((b, s) =>
                        {
                            Message message = EntityBufCore.DeSerialize<Message>(b);
                            s.LastSessionTime = DateTime.Now;
                            FormApp(message, s);
                        }).BeginInvoke(buffer, appSocket, null, null);
                }
                catch (SessionAbortException exp)
                {
                    SocketApplicationComm.Debug(exp.Message);
                    break;
                }
                catch (SocketException exp)
                {
                    SocketApplicationComm.Debug(exp.Message);
                    break;
                }
                catch (Exception exp)
                {
                    SocketApplicationComm.Debug(exp.Message);
                    OnError(exp);
                    break;
                }
            }

            try
            {
                socket.Shutdown(SocketShutdown.Both);
                
            }
            catch
            {

            }
            socket.Close();
            SocketApplicationComm.Debug(string.Format("服务器关闭套接字：{0}", appSocket.SessionID));
        }

        protected virtual void FormApp(Message message, Session session)
        {

        }

        #endregion

        private void Dispose(bool disposed)
        {
            if (disposed)
            {
                if (BeforRelease != null)
                {
                    try
                    {
                        BeforRelease();
                    }
                    catch { }
                }


                if (socketServer != null)
                {
                    try
                    {
                        socketServer.Shutdown(SocketShutdown.Both);
                        socketServer.Close();
                    }
                    catch { }
                }

                if (socketClient != null)
                {
                    try
                    {
                        socketClient.Shutdown(SocketShutdown.Both);
                        socketClient.Close();
                    }
                    catch { }
                }

                if (udpBCSocket != null)
                {
                    try
                    {
                        udpBCSocket.Close();
                    }
                    catch { }
                }

                if (udpMCClient != null)
                {
                    try
                    {
                        udpMCClient.DropMulticastGroup(SocketApplicationComm.MCAST_ADDR);
                        udpMCClient.Close();
                    }
                    catch { }
                }

                if (listeningThread != null)
                {
                    try
                    {
                        listeningThread.Abort();
                    }
                    catch { }
                }

                stop = true;
                isStartClient = false;

                GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~MessageApp()
        {
            Dispose(false);
        }
    }
}
