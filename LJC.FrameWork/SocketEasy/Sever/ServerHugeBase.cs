using LJC.FrameWork.Comm;
using LJC.FrameWork.ConfigurationSectionHandler;
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
        private ConcurrentDictionary<string, Session> _connectSocketDic = new ConcurrentDictionary<string, Session>();
        private ConcurrentQueue<IOCPSocketAsyncEventArgs> _iocpQueue = new ConcurrentQueue<IOCPSocketAsyncEventArgs>();
        private BufferPollManager _bufferpoll = null;
        
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

        static ServerHugeBase()
        {
            var threadpoolset=(ThreadPoolConfig)System.Configuration.ConfigurationManager.GetSection("ThreadPoolConfig");
            if (threadpoolset == null)
            {
                ThreadPool.SetMinThreads(1000, 1000);
                ThreadPool.SetMaxThreads(3000, 3000);
            }
            else
            {
                ThreadPool.SetMinThreads(threadpoolset.MinWorkerThreads, threadpoolset.MinCompletionPortThreads);
                ThreadPool.SetMaxThreads(threadpoolset.MaxWorkerThreads, threadpoolset.MaxCompletionPortThreads);
            }
        }

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
                if(_bufferpoll==null)
                {
                    _bufferpoll = new BufferPollManager(2000, 10240);
                }

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
                    Listening();
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

        private void RealseSocketAsyncEventArgs(IOCPSocketAsyncEventArgs e)
        {
            e.Completed -= SocketAsyncEventArgs_Completed;
            _bufferpoll.RealseBuffer(e.BufferIndex);
            e.ClearBuffer();
            //e.AcceptSocket.Disconnect(true);
            if (e.SocketError == SocketError.Success)
            {
                try
                {
                    e.AcceptSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                {

                }
            }
            e.AcceptSocket = null;
            _iocpQueue.Enqueue(e);
        }

        private void SetBuffer(IOCPSocketAsyncEventArgs e,int offset,int len)
        {
            if(offset>0)
            {
                e.SetBuffer(offset, len);
                return;
            }

            byte[] buf = null;
            if (len <= _bufferpoll.BlockSize)
            {
                if (e.BufferIndex == -1)
                {
                    var newindex = _bufferpoll.GetBuffer();
                    if (newindex == -1)
                    {
                        buf = new byte[len];
                    }
                    else
                    {
                        e.BufferIndex=newindex;
                    }
                }
            }
            else
            {
                buf = new byte[len];
            }

            if(buf!=null)
            {
                e.SetBuffer(buf, offset, len);
            }
            else
            {
                e.SetBuffer(_bufferpoll.Buffer, _bufferpoll.GetOffset(e.BufferIndex) + offset, len);
            }
        }

        private SocketAsyncEventArgs GetSocketAsyncEventArgs()
        {
            IOCPSocketAsyncEventArgs args;
            if( _iocpQueue.TryDequeue(out args))
            {
                args.Completed += Args_Completed;
                //args.ClearBuffer()
                args.IsReadPackLen = false;
            }
            else
            {
                args = new IOCPSocketAsyncEventArgs();
                args.Completed += Args_Completed;
            }

            return args;
        }

        void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            Listening();

            e.Completed -= Args_Completed;

            Socket socket = e.AcceptSocket;
            socket.NoDelay = true;

            IPEndPoint endPoint = (IPEndPoint)socket.RemoteEndPoint;
            Session appSocket = new Session();
            appSocket.IPAddress = endPoint.Address.ToString();
            appSocket.IsValid = true;
            appSocket.SessionID = SocketApplicationComm.GetSeqNum();
            appSocket.Socket = socket;

            var socketAsyncEventArgs = e as IOCPSocketAsyncEventArgs;
            socketAsyncEventArgs.AcceptSocket = socket;
            socketAsyncEventArgs.DisconnectReuseSocket = true;
            socketAsyncEventArgs.Completed += SocketAsyncEventArgs_Completed;
            socketAsyncEventArgs.UserToken = appSocket.SessionID;

            //byte[] buffer = new byte[4];
            //socketAsyncEventArgs.SetBuffer(buffer, 0, 4);
            SetBuffer(socketAsyncEventArgs, 0, 4);
            //socketAsyncEventArgs.SetBuffer(_bufferpoll.Buffer, _bufferpoll.GetOffset(socketAsyncEventArgs.BufferIndex), 4);

            _connectSocketDic.TryAdd(appSocket.SessionID, appSocket);

            if (!socket.ReceiveAsync(socketAsyncEventArgs))
            {
                //Session old;
                //_connectSocketDic.TryRemove(appSocket.SessionID, out old);
                //RealseSocketAsyncEventArgs(socketAsyncEventArgs);

                LogManager.LogHelper.Instance.Debug(socket.Handle + "同步完成，手动处理");

                SocketAsyncEventArgs_Completed(null, e);
            }
        }

        private void SetAcceptAsync()
        {

        }

        private void Listening()
        {
            socketServer.AcceptAsync(GetSocketAsyncEventArgs());
        }

        void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= SocketAsyncEventArgs_Completed;
            var args = e as IOCPSocketAsyncEventArgs;

            if (args.BytesTransferred == 0 || args.SocketError != SocketError.Success)
            {
                if (SocketApplication.SocketApplicationEnvironment.TraceSocketDataBag)
                {
                    LogManager.LogHelper.Instance.Debug(e.AcceptSocket.Handle + "异常断开:" + args.SocketError);
                }

                Session removesession;
                //用户断开了
                if (_connectSocketDic.TryRemove(args.UserToken.ToString(), out removesession))
                {
                    RealseSocketAsyncEventArgs(args);
                }
                return;
            }
            else
            {
                if (!args.IsReadPackLen)
                {
                    var offset = args.BufferIndex == -1 ? 0 : _bufferpoll.GetOffset(args.BufferIndex);

                    var dataLen = BitConverter.ToInt32(new byte[] { e.Buffer[offset], e.Buffer[offset + 1], e.Buffer[offset + 2], e.Buffer[offset + 3] }, 0);

                    if (SocketApplication.SocketApplicationEnvironment.TraceSocketDataBag)
                    {
                        LogManager.LogHelper.Instance.Debug(e.AcceptSocket.Handle + "准备接收数据:长度" + dataLen, null);
                    }

                    if (dataLen > MaxPackageLength)
                    {
                        if (SocketApplication.SocketApplicationEnvironment.TraceSocketDataBag)
                        {
                            LogManager.LogHelper.Instance.Debug(e.AcceptSocket.Handle + "异常断开,长度太长");
                        }

                        Session removesession;
                        if (_connectSocketDic.TryRemove(args.UserToken.ToString(), out removesession))
                        {
                            RealseSocketAsyncEventArgs(args);
                        }
                        return;

                    }
                    else
                    {
                        args.IsReadPackLen = true;
                        //byte[] readbuffer = new byte[dataLen];
                        args.BufferLen = dataLen;
                        args.BufferRev = 0;
                        //args.SetBuffer(readbuffer, 0, dataLen);

                        SetBuffer(args,0, dataLen);
                    }
                }
                else
                {
                    if (SocketApplication.SocketApplicationEnvironment.TraceSocketDataBag)
                    {
                        var offset1 = (args.BufferLen == args.Buffer.Length) ? 0 : _bufferpoll.GetOffset(args.BufferIndex);
                        var bytes = args.Buffer.Skip(offset1 + args.BufferRev).Take(args.BytesTransferred).ToArray();
                        LogManager.LogHelper.Instance.Debug(string.Format(e.AcceptSocket.Handle + "接收数据{0}/{1},{2}", args.BufferLen, args.BufferRev, Convert.ToBase64String(bytes)), null);
                    }

                    args.BufferRev += args.BytesTransferred;

                    if (args.BufferRev == args.BufferLen)
                    {
                        byte[] bt = null;
                        var offset = args.BufferIndex == -1 ? 0 : _bufferpoll.GetOffset(args.BufferIndex);

                        //校验
                        var crc32 = BitConverter.ToInt32(args.Buffer, offset);

                        var calcrc32 = LJC.FrameWork.Comm.HashEncrypt.GetCRC32(args.Buffer, offset + 4, args.BufferLen - 4);
                        if (calcrc32 == crc32)
                        {
                            bt = new byte[args.BufferLen - 4];
                            for (int i = 4; i < args.BufferLen; i++)
                            {
                                bt[i - 4] = args.Buffer[offset + i];
                            }

                            ThreadPool.QueueUserWorkItem(new WaitCallback((buf) =>
                            {
                                Message message = EntityBufCore.DeSerialize<Message>((byte[])buf);

                                //if (!string.IsNullOrWhiteSpace(message.MessageHeader.TransactionID))
                                //{
                                //    Console.WriteLine(message.MessageHeader.TransactionID);
                                //}

                                Session connSession;
                                if (_connectSocketDic.TryGetValue(args.UserToken.ToString(), out connSession))
                                {
                                    FormApp(message, connSession);
                                }
                            }), bt);
                        }
                        else
                        {
                            Exception ex=new Exception("检查校验码出错");
                            ex.Data.Add("crc32",crc32);
                            ex.Data.Add("calcrc32",calcrc32);
                            ex.Data.Add("data", bt == null ? "" : Convert.ToBase64String(bt));

                            LogManager.LogHelper.Instance.Error("接收数据出错", ex);

                            throw ex;
                        }

                        args.IsReadPackLen = false;
                        //args.SetBuffer(_bufferpoll.Buffer, _bufferpoll.GetOffset(args.BufferIndex), 4);
                        SetBuffer(args, 0, 4);
                    }
                    else
                    {
                        //???
                        var offset = args.BufferIndex == -1 ? 0 : _bufferpoll.GetOffset(args.BufferIndex);
                        e.SetBuffer(offset + args.BufferRev, args.BufferLen - args.BufferRev);
                        //SetBuffer(args,args.BufferRev, args.BufferLen - args.BufferRev);
                    }
                }

                e.Completed += SocketAsyncEventArgs_Completed;
                if (!e.AcceptSocket.ReceiveAsync(e))
                {
                    LogManager.LogHelper.Instance.Debug(e.AcceptSocket.Handle + "同步完成，手动处理", null);
                    SocketAsyncEventArgs_Completed(null, e);
                    //if (SocketApplication.SocketApplicationEnvironment.TraceSocketDataBag)
                    //{
                    //    LogManager.LogHelper.Instance.Debug(e.AcceptSocket.Handle + "异常断开:!e.AcceptSocket.ReceiveAsync");
                    //}
                    //Session old;
                    //_connectSocketDic.TryRemove(e.UserToken.ToString(), out old);
                    //RealseSocketAsyncEventArgs(args);
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
