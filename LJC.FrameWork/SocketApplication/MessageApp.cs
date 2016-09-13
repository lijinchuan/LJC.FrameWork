using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using LJC.FrameWork.EntityBuf;

namespace LJC.FrameWork.SocketApplication
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
        /// <summary>
        /// 断线重连时间间隔
        /// </summary>
        private int reConnectClientTimeInterval = 5000;

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
                    Thread thread = new Thread(Listening);
                    thread.Start();
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

                if (socketClient != null)
                {
                    socketClient.Close();
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

                if (!isStartClient)
                {
                    Thread threadClient = new Thread(Receiving);
                    threadClient.Start();
                }

                isStartClient = true;
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

        private void Receiving()
        {
            while (!stop/* && socketClient.Connected*/)
            {
                try
                {
                    byte[] buff4 = new byte[4];
                    int count = socketClient.Receive(buff4);
                    if (count == 0)
                        break;

                    int dataLen = BitConverter.ToInt32(buff4, 0);

                    MemoryStream ms = new MemoryStream();
                    int readLen = 0,timeout=0;

                    byte[] buffer = new byte[dataLen];

                    while (readLen < dataLen)
                    {
                        count = socketClient.Receive(buffer);
                        
                        if (count == 0)
                        {
                            Thread.Sleep(1);
                            timeout += 1;
                            if (timeout > 10000)
                                break;
                            continue;
                        }
                        readLen += count;
                        ms.Write(buffer, 0, count);
                    }
                    buffer = ms.ToArray();
                    ms.Close();

                    //Thread newThread = new Thread(new ParameterizedThreadStart(ProcessMessage));
                    //newThread.Start(buffer);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), buffer);
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == (int)SocketError.ConnectionAborted)
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
                Message message = EntityBufCore.DeSerialize<Message>(data,SocketApplicationComm.IsMessageCompress);
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
                Message message = EntityBufCore.DeSerialize<Message>(data,SocketApplicationComm.IsMessageCompress);

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
                Message message = EntityBufCore.DeSerialize<Message>(data,SocketApplicationComm.IsMessageCompress);

                if (OnMultiCast != null)
                {
                    OnMultiCast(message);
                }
            }
            catch (Exception e)
            {

            }
        }

        private int sendMessageTryCountLimit = 3;
        public bool SendMessage(Message message)
        {
            try
            {
                int tryCount = 0;
                while (!socketClient.Connected && tryCount < sendMessageTryCountLimit)
                {
                    tryCount++;
                    StartClient();
                }

                if (!socketClient.Connected)
                {
                    throw new Exception("发送失败，套接字连接失败。");
                }
                
                //byte[] data = EntityBufCore.Serialize(message);
                //byte[] len = BitConverter.GetBytes(data.Length);
                //socketClient.Send(len);
                //socketClient.Send(data);

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
                    byte[] buff4 = new byte[4];
                    int count = socket.Receive(buff4);
                    
                    if (count == 0)
                    {
                        throw new SessionAbortException("接收数据出错。");
                    }

                    int dataLen = BitConverter.ToInt32(buff4, 0);


                    MemoryStream ms = new MemoryStream();
                    int readLen = 0, timeout = 0;

                    byte[] buffer = new byte[dataLen];

                    while (readLen < dataLen)
                    {
                        count = socket.Receive(buffer);

                        if (count == 0)
                        {
                            Thread.Sleep(1);
                            timeout += 1;
                            if (timeout > 10000)
                                break;
                            continue;
                        }
                        readLen += count;
                        ms.Write(buffer, 0, count);
                    }
                    buffer = ms.ToArray();
                    ms.Close();

                    //Message message = EntityBufCore.DeSerialize<Message>(buffer);
                    //FormApp(message, appSocket);

                    //搞成异步的
                    new Action<byte[], Session>((b, s) =>
                        {
                            Message message = EntityBufCore.DeSerialize<Message>(b,SocketApplicationComm.IsMessageCompress);
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

            stop = true;
            isStartClient = false;
        }

        ~MessageApp()
        {
            Dispose();
        }
    }
}
