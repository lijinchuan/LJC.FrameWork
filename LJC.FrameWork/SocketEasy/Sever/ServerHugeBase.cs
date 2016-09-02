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
    public class ServerHugeBase:SocketBase
    {
        protected Socket socketServer;
       
        protected string[] bindIpArray;
        protected int ipPort;
        protected bool isStartServer = false;

        //private ConcurrentBag<Session> _connectSocketBagList = new ConcurrentBag<Session>();
        private ConcurrentDictionary<string, Session> _connectSocketDic = new ConcurrentDictionary<string, Session>();
        private System.Timers.Timer _socketReadTimer = null;
        
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
        public ServerHugeBase(string[] bindips, int port)
        {
            this.bindIpArray = bindips;
            this.ipPort = port;
        }

        public ServerHugeBase()
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

                _socketReadTimer = TaskHelper.SetInterval(0, () => { ReadSocketList(); return false; }, 0, true);

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

                    socket.NoDelay = true;
                    IPEndPoint endPoint = (IPEndPoint)socket.RemoteEndPoint;
                    Session appSocket = new Session();
                    appSocket.IPAddress = endPoint.Address.ToString();
                    appSocket.IsValid = true;
                    appSocket.SessionID = socket.Handle.ToInt64().ToString(); //SocketApplicationComm.GetSeqNum();
                    appSocket.Socket = socket;

                    //_connectSocketBagList.Add(appSocket);
                    _connectSocketDic.TryAdd(appSocket.SessionID, appSocket);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }

            socketServer.Close();
            SocketApplicationComm.Debug("关闭服务器套接字!");
        }

        private void ReadSocketList()
        {
            var list= _connectSocketDic.Select(p=>p.Value).ToList();
            var readlist=list.Select(p=>p.Socket).ToList();
            var errlist = new List<Socket>();

            Socket.Select(readlist, null, null, 1);
            if (readlist.Count > 0)
            {
                int taskcount = (int)Math.Ceiling(readlist.Count / 1000.0);

                TaskHelper.RunTask<Socket>(readlist, taskcount, (o) =>
                    {
                        var sublist = ((List<Socket>)o);
                        Session s = null;
                        int delcount = 0;
                        foreach (var item in readlist)
                        {
                            try
                            {
                                delcount = 0;
                                while (item.Poll(1, SelectMode.SelectRead)&&delcount<100)
                                {
                                    _connectSocketDic.TryGetValue(item.Handle.ToInt64().ToString(), out s);
                                    if (!(s.IsValid && s.Socket.Connected))
                                    {
                                        lock (errlist)
                                        {
                                            errlist.Add(item);
                                        }
                                        break;
                                    }

                                    byte[] buff4 = new byte[4];
                                    int count = item.Receive(buff4, SocketFlags.None);

                                    if (count == 0)
                                    {
                                        throw new SessionAbortException("接收数据出错。");
                                    }

                                    int dataLen = BitConverter.ToInt32(buff4, 0);


                                    MemoryStream ms = new MemoryStream();
                                    int readLen = 0;

                                    byte[] buffer = new byte[dataLen];

                                    while (readLen < dataLen)
                                    {
                                        count = item.Receive(buffer);

                                        readLen += count;
                                        ms.Write(buffer, 0, count);
                                    }
                                    buffer = ms.ToArray();
                                    ms.Close();

                                    //搞成异步的
                                    new Action<byte[], Session>((b, ss) =>
                                    {
                                        Message message = EntityBufCore.DeSerialize<Message>(b);
                                        FormApp(message, ss);
                                    }).BeginInvoke(buffer, _connectSocketDic[item.Handle.ToInt64().ToString()], null, null);

                                    delcount += 1;
                                }
                            }
                            catch (Exception ex)
                            {
                                OnError(ex);
                            }
                        };
                    });
            }

            Session removesession = null;
            foreach (var item in errlist)
            {
                if (_connectSocketDic.TryRemove(item.Handle.ToInt64().ToString(), out removesession))
                {
                    item.Close();
                }
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
