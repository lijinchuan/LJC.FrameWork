using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketEasy.Sever
{
    public class ServerBase :SocketBase
    {
        protected Socket socketServer;
       
        protected string[] bindIpArray;
        protected int ipPort;
        protected bool isStartServer = false;
        
        /// <summary>
        /// 对象清理之前的事件
        /// </summary>
        public event Action BeforRelease;

        private int _maxPackageLength = 1024 * 1024 * 8 * 10;
        /// <summary>
        /// 每次最大接收的字节数byte
        /// </summary>
        public int MaxPackageLength
        {
            get
            {
                return _maxPackageLength;
            }
            set
            {
                if (value <= 0)
                {
                    return;
                }
                _maxPackageLength = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="stop">如果为true,不会断开自动重连</param>
        public ServerBase(string[] bindips, int port)
        {
            this.bindIpArray = bindips;
            this.ipPort = port;
        }

        public ServerBase()
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
                    if (bindIpArray == null)
                    {
                        socketServer.Bind(new IPEndPoint(IPAddress.Any, ipPort));
                    }
                    else
                    {
                        foreach(var ip in bindIpArray)
                        {
                            socketServer.Bind(new IPEndPoint(IPAddress.Parse(ip), ipPort));
                        }
                    }
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
            socket.ReceiveBufferSize = 32000;
            socket.SendBufferSize = 32000;
            IPEndPoint endPoint = (IPEndPoint)socket.RemoteEndPoint;

            Session appSocket = new Session();

            appSocket.IPAddress = endPoint.Address.ToString();
            appSocket.IsValid = true;
            appSocket.SessionID = SocketApplicationComm.GetSeqNum();
            appSocket.Socket = socket;

            while (appSocket.IsValid && appSocket.Socket.Connected)
            {
                try
                {
                    byte[] buff4 = new byte[4];
                    int count = socket.Receive(buff4);

                    if (count != 4)
                    {
                        throw new SessionAbortException("接收数据出错。");
                    }

                    int dataLen = BitConverter.ToInt32(buff4, 0);

                    if (dataLen > MaxPackageLength)
                    {
                        throw new Exception("超过了最大字节数：" + MaxPackageLength);
                    }

                    MemoryStream ms = new MemoryStream();
                    int readLen = 0;
                    byte[] reciveBuffer = new byte[1024];

                    while (readLen < dataLen)
                    {
                        count = socket.Receive(reciveBuffer, Math.Min(dataLen - readLen, reciveBuffer.Length), SocketFlags.None);
                        readLen += count;
                        ms.Write(reciveBuffer, 0, count);
                    }
                    var buffer = ms.ToArray();
                    ms.Close();

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

            appSocket.Close("OnSocket error");
        }

        protected virtual void FormApp(Message message, Session session)
        {

        }

        #endregion
        protected override void DoDispose()
        {
            base.DoDispose();

            if (BeforRelease != null)
            {
                BeforRelease();
            }

            if (socketServer != null)
            {
                socketServer.Close();
            }
        }
    }
}
