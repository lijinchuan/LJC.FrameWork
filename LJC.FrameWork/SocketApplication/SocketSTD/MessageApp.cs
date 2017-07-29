using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using LJC.FrameWork.EntityBuf;

namespace LJC.FrameWork.SocketApplication.SocketSTD
{
    public class MessageApp : IDisposable
    {
        protected Socket socketClient;
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
        protected bool isStartClient = false;
        protected bool stop = false;
        protected bool errorResume = true;
        protected string ipString;
        protected int ipPort;
        private DateTime lastReStartClientTime;
        protected bool isStartServer = false;
        private Thread listeningThread = null;

        /// <summary>
        /// 断线重连时间间隔
        /// </summary>
        private int reConnectClientTimeInterval = 5000;

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
        public MessageApp(string ip, int port, bool errorResume = true)
        {
            this.ipString = ip;
            this.ipPort = port;
            this.errorResume = errorResume;
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
            try
            {
                if (socketClient != null && socketClient.Connected)
                    return true;

                if (DateTime.Now.Subtract(lastReStartClientTime).TotalMilliseconds <= reConnectClientTimeInterval)
                    return false;

                bool isResetClient = false;
                if (socketClient != null)
                {
                    socketClient.Close();
                    isResetClient = true;
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
                }
                catch (SocketException e)
                {
                    var ne = new Exception(string.Format("连接到远程服务器{0}失败，端口:{1}，原因:{2},网络错误号:{3}",
                        ipString, ipPort, e.Message, e.SocketErrorCode));
                    throw ne;

                }
                catch (Exception e)
                {
                    lastReStartClientTime = DateTime.Now;
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
                return false;
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
            s.Receive(buff4, 0, 4, SocketFlags.None);

            int dataLen = BitConverter.ToInt32(buff4, 0);
            s.Receive(buff4, 0, 4, SocketFlags.None);
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
                  Exception ex=new Exception("检查校验码出错");
                  ex.Data.Add("crc32", crc32);
                  ex.Data.Add("calcrc32", calcrc32);
                  ex.Data.Add("data", Convert.ToBase64String(buffer));
                  LogManager.LogHelper.Instance.Error("接收数据错误", ex);
            }

            if (SocketApplicationEnvironment.TraceSocketDataBag)
            {
                LogManager.LogHelper.Instance.Debug(s.Handle + "接收数据," + readLen + "," + Convert.ToBase64String(buffer));
            }

            return buffer;
        }

        private void Receiving()
        {
            int errertimes = 0;
            while (!stop/* && socketClient.Connected*/)
            {
                try
                {
                    var buffer = ReceivingNext(socketClient);
                    errertimes = 0;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), buffer);
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == (int)SocketError.ConnectionAborted)
                    {
                        break;
                    }
                    if (++errertimes >= 10)
                    {
                        break;
                    }
                    OnError(e);
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }

            socketClient.Close();
        }

        private void ProcessMessage(object buffer)
        {
            try
            {
                byte[] data = (byte[])buffer;
                Message message = EntityBufCore.DeSerialize<Message>(data);
                OnMessage(message);
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
                return socketClient.SendMessge(message);
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
                new Action(() => StartClient()).BeginInvoke(null, null);
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
                }
            }

            socket.Close();
            SocketApplicationComm.Debug(string.Format("服务器关闭套接字：{0}", appSocket.SessionID));
        }

        protected virtual void FormApp(Message message, Session session)
        {

        }

        #endregion

        public void Dispose()
        {
            if (BeforRelease != null)
            {
                BeforRelease();
            }

            if (socketServer != null)
            {
                socketServer.Close();
            }

            if (socketClient != null)
            {
                socketClient.Close();
            }

            if (udpBCSocket != null)
            {
                udpBCSocket.Close();
            }

            if (udpMCClient != null)
            {
                udpMCClient.DropMulticastGroup(SocketApplicationComm.MCAST_ADDR);
                udpMCClient.Close();
            }

            if (listeningThread != null)
            {
                listeningThread.Abort();
            }

            stop = true;
            isStartClient = false;
        }

        ~MessageApp()
        {
            Dispose();
        }
    }
}
