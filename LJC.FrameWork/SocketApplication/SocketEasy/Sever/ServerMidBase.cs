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
    public class ServerMidBase:SocketBase
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
        public ServerMidBase(string[] bindips, int port)
        {
            this.bindIpArray = bindips;
            this.ipPort = port;
        }

        public ServerMidBase()
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

                _socketReadTimer = TaskHelper.SetInterval(1, () => { ReadSocketList(); return false; }, 0, true);

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
                    appSocket.Port = endPoint.Port;
                    appSocket.IsValid = true;
                    appSocket.SessionID = SocketApplicationComm.GetSeqNum();
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

            if (readlist.Count > 0)
            {
                int taskcount = (int)Math.Ceiling(readlist.Count / 1000.0);
                taskcount = Math.Max(taskcount, 60);
                taskcount = Math.Min(readlist.Count, taskcount);

                TaskHelper.RunTask<Socket>(readlist, taskcount, (o) =>
                    {
                        var sublist = ((List<Socket>)o);

                        Socket.Select(sublist, null, null, 1);

                        Session s = null;
                        int delcount = 0;
                        foreach (var item in sublist)
                        {
                            try
                            {
                                _connectSocketDic.TryGetValue(item.Handle.ToInt64().ToString(), out s);
                                if (!(s.IsValid && s.Socket.Connected))
                                {
                                    lock (errlist)
                                    {
                                        errlist.Add(item);
                                    }
                                    continue;
                                }

                                delcount = 0;
                                while (item.Poll(1, SelectMode.SelectRead)&&delcount<100)
                                {
                                    byte[] buff4 = new byte[4];
                                    int count = item.Receive(buff4);

                                    int dataLen = BitConverter.ToInt32(buff4, 0);
                                    if (dataLen > MaxPackageLength)
                                    {
                                        throw new Exception("超过了最大字节数：" + MaxPackageLength);
                                    }

                                    count = item.Receive(buff4);
                                    var crc32 = BitConverter.ToInt32(buff4, 0);

                                    MemoryStream ms = new MemoryStream();
                                    dataLen -= 4;
                                    int readLen = 0;
                                    byte[] reciveBuffer = new byte[1024];

                                    while (readLen < dataLen)
                                    {
                                        count = item.Receive(reciveBuffer, Math.Min(dataLen - readLen, reciveBuffer.Length), SocketFlags.None);
                                        readLen += count;
                                        ms.Write(reciveBuffer, 0, count);
                                    }
                                    var buffer = ms.ToArray();
                                    ms.Close();

                                    var calcrc32 = LJC.FrameWork.Comm.HashEncrypt.GetCRC32(buffer, 0);
                                    if (crc32 != calcrc32)
                                    {
                                        throw new Exception("数据校验错误");
                                    }

                                    ThreadPool.QueueUserWorkItem(new WaitCallback((buf) =>
                                    {
                                        Message message = EntityBufCore.DeSerialize<Message>((byte[])buf);
                                        FormApp(message, _connectSocketDic[item.Handle.ToInt64().ToString()]);
                                    }), buffer);

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
                    
                    if(!removesession.Close("ReadSocketList error", false))
                    {
                        item.Close();
                    }
                    //item.Close();
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
