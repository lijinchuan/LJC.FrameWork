using LJC.FrameWork.Comm;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketEasy.Sever
{
    public class ServerHugeBase2:SocketBase
    {
        protected Socket socketServer;
       
        protected string[] bindIpArray;
        protected int ipPort;
        protected bool isStartServer = false;
        private ConcurrentDictionary<string, Session> _connectSocketDic = new ConcurrentDictionary<string, Session>();
        
        /// <summary>
        /// 对象清理之前的事件
        /// </summary>
        public event Action BeforRelease;

        private int _maxPackageLength = 1024 * 1024 * 8;
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
        public ServerHugeBase2(string[] bindips, int port)
        {
            this.bindIpArray = bindips;
            this.ipPort = port;
        }

        public ServerHugeBase2()
        {

        }

        public bool StartServer()
        {
            try
            {
                if (socketServer == null)
                {
                    socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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

                //_socketReadTimer = TaskHelper.SetInterval(1, () => { ReadSocketList(); return false; }, 0, true);

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

        private SocketAsyncEventArgs GetSocketAsyncEventArgs()
        {
            var args = new SocketAsyncEventArgs();
            args.Completed += Args_Completed;

            return args;
        }

        void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.ConnectSocket == null)
                return;

            Socket socket = e.ConnectSocket;
            socket.NoDelay = true;
            socket.UseOnlyOverlappedIO = true;

            IPEndPoint endPoint = (IPEndPoint)socket.RemoteEndPoint;
            Session appSocket = new Session();
            appSocket.IPAddress = endPoint.Address.ToString();
            appSocket.IsValid = true;
            appSocket.SessionID = socket.Handle.ToInt64().ToString(); //SocketApplicationComm.GetSeqNum();
            appSocket.Socket = socket;

            var socketAsyncEventArgs = e as IOCPSocketAsyncEventArgs;
            socketAsyncEventArgs.AcceptSocket = socket;
            socketAsyncEventArgs.DisconnectReuseSocket = true;
            socketAsyncEventArgs.Completed += SocketAsyncEventArgs_Completed;
            socketAsyncEventArgs.UserToken = appSocket.SessionID;
            byte[] buffer = new byte[4];
            socketAsyncEventArgs.SetBuffer(buffer, 0, 4);
            if (!socket.ReceiveAsync(socketAsyncEventArgs))
            {
                throw new Exception(socketAsyncEventArgs.SocketError.ToString());
            }

            //_connectSocketBagList.Add(appSocket);
            _connectSocketDic.TryAdd(appSocket.SessionID, appSocket);
        }

        private void SetAcceptAsync()
        {

        }

        private void Listening()
        {
            //while (!stop)
            {
                try
                {
                    socketServer.AcceptAsync(GetSocketAsyncEventArgs());
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }

            //socketServer.Close();
            SocketApplicationComm.Debug("关闭服务器套接字!");
        }

        void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= SocketAsyncEventArgs_Completed;
            var args = e as IOCPSocketAsyncEventArgs;

            if (args.BytesTransferred == 0)
            {
                Session removesession;
                //用户断开了
                if (_connectSocketDic.TryRemove(args.UserToken.ToString(), out removesession))
                {
                    removesession.Close();
                }
            }
            else
            {
                if(!args.IsReadPackLen)
                {
                    byte[] bt4=new byte[4];
                    e.Buffer.CopyTo(bt4, 0);
                    int dataLen = BitConverter.ToInt32(bt4, 0);
                    if (dataLen > MaxPackageLength)
                    {
                        e.ConnectSocket.Close();
                    }
                    else
                    {
                        args.IsReadPackLen = true;
                        byte[] readbuffer = new byte[dataLen];
                        args.SetBuffer(readbuffer, 0, dataLen);
                    }
                }
                else
                {
                    byte[] bt=new byte[args.BytesTransferred];
                    e.Buffer.CopyTo(bt, 0);

                    ThreadPool.QueueUserWorkItem(new WaitCallback((buf) =>
                    {
                        Message message = EntityBufCore.DeSerialize<Message>((byte[])buf, SocketApplicationComm.IsMessageCompress);
                        FormApp(message, _connectSocketDic[args.UserToken.ToString()]);
                    }), bt);

                    byte[] bt4 = new byte[4];
                    args.IsReadPackLen = false;
                    args.SetBuffer(bt4, 0, 4);
                }

                e.Completed += SocketAsyncEventArgs_Completed;
                e.AcceptSocket.ReceiveAsync(e);
            }
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
