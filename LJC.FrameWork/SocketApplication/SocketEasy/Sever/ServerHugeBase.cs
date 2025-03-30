using LJC.FrameWork.Comm;
using LJC.FrameWork.ConfigurationSectionHandler;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.LogManager;
using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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
        protected ConcurrentDictionary<string, Session> _connectSocketDic = new ConcurrentDictionary<string, Session>();
        private readonly ConcurrentQueue<IOCPSocketAsyncEventArgs> _iocpQueue = new ConcurrentQueue<IOCPSocketAsyncEventArgs>();
        private readonly ConcurrentQueue<IOCPSocketAsyncEventArgs> _deleteQueue = new ConcurrentQueue<IOCPSocketAsyncEventArgs>();
        private BufferPollManager _bufferpoll = null;
        private System.Timers.Timer _worktimer = null;

        private object _realseSocketAsyncEventArgsLocker = new object();

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
                    socketServer.ReceiveBufferSize = 64000;
                    socketServer.SendBufferSize = 64000;
                    socketServer.NoDelay = true;
                    if (bindIpArray == null)
                    {
                        socketServer.Bind(new IPEndPoint(IPAddress.Any, ipPort));
                    }
                    else
                    {
                        //foreach(var ip in bindIpArray)
                        //{
                        //    socketServer.Bind(new IPEndPoint(IPAddress.Parse(ip), ipPort));
                        //}

                        socketServer.Bind(new IPEndPoint(IPAddress.Any, ipPort));
                    }
                }
                socketServer.Listen(int.MaxValue);

                if (!isStartServer)
                {
                    Listening();
                }

                if (_worktimer == null)
                {
                    _worktimer = TaskHelper.SetInterval(30000, () =>
                        {
                            CheckConnectedClient();
                            return false;
                        });
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

        private void CheckConnectedClient()
        {
            IOCPSocketAsyncEventArgs delIocpSocketAsyncEventArgs = null;
            var delCount = _deleteQueue.Count;
            LogHelper.Instance.Debug($"CheckConnectedClient->_deleteQueue:{delCount}");
            while (delCount-- > 0 && _deleteQueue.TryDequeue(out delIocpSocketAsyncEventArgs))
            {
                try
                {
                    //RealseSocketAsyncEventArgs(delIocpSocketAsyncEventArgs, "CheckConnectedClient");
                    delIocpSocketAsyncEventArgs.AcceptSocket?.Disconnect(true);
                }
                catch(Exception ex)
                {
                    LogHelper.Instance.Error("delIocpSocketAsyncEventArgs.AcceptSocket?.Disconnect", ex);
                    _deleteQueue.Enqueue(delIocpSocketAsyncEventArgs);
                }
            }

            var sessions = _connectSocketDic.Values.ToArray();
            LogHelper.Instance.Debug($"CheckConnectedClient:{sessions.Length}");
            foreach (var s in sessions)
            {
                if (DateTime.Now.Subtract(s.LastSessionTime).TotalSeconds > 180)
                {
                    _connectSocketDic.TryRemove(s.SessionID, out _);

                    if (s.AsyncEventArgs != null)
                    {
                        try
                        {
                            //RealseSocketAsyncEventArgs(s.AsyncEventArgs, "CheckConnectedClient2");
                            s.AsyncEventArgs.AcceptSocket?.Disconnect(true);
                        }
                        catch(Exception ex)
                        {
                            LogHelper.Instance.Error("s.AsyncEventArgs.AcceptSocket?.Disconnect", ex);
                            _deleteQueue.Enqueue(s.AsyncEventArgs);
                        }

                    }

                    s.Close("CheckConnectedClient",true);
                }
            }
        }

        #region server

        private void RealseSocketAsyncEventArgs(IOCPSocketAsyncEventArgs e, string from)
        {
            try
            {
                lock (_realseSocketAsyncEventArgsLocker)
                {
                    LogHelper.Instance.Debug($"RealseSocketAsyncEventArgs,from:{from},id:{e.Id}");

                    //应该要确保连接关闭
                    e.ClearBuffer();

                    e.Completed -= SocketAsyncEventArgs_Completed;
                    _bufferpoll.RealseBuffer(e.BufferIndex);

                    //e.AcceptSocket.Disconnect(true);
                    try
                    {
                        if (e.AcceptSocket != null)
                        {
                            if (e.AcceptSocket.Connected)
                            {
                                //e.AcceptSocket.Disconnect(true);
                                e.AcceptSocket.Shutdown(SocketShutdown.Both);
                                //e.AcceptSocket.Close();
                                e.AcceptSocket.Close();
                            }
                            e.AcceptSocket = null;
                        }
                    }
                    catch(Exception ex)
                    {
                        LogHelper.Instance.Error("e.AcceptSocket.Shutdown", ex);
                        e.AcceptSocket = null;
                    }

                    _iocpQueue.Enqueue(e);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error("RealseSocketAsyncEventArgs 出错", ex);
                throw;
            }
        }

        private void SetBuffer(IOCPSocketAsyncEventArgs e,int offset,int len)
        {
            if (offset > 0)
            {
                offset = (e.BufferIndex == -1 ? 0 : _bufferpoll.GetOffset(e.BufferIndex)) + offset;
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
                        e.BufferIndex = newindex;
                    }
                }
            }
            else
            {
                if (e.BufferIndex > 0)
                {
                    _bufferpoll.RealseBuffer(e.BufferIndex);
                    e.BufferIndex = -1;
                }
                buf = new byte[len];
            }

            if (buf != null)
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
                bool isNull = args.AcceptSocket == null;
                LogHelper.Instance.Debug("复用IOCPSocketAsyncEventArgs,id=" + args.Id + ",AcceptSocket is null:" + isNull);
            }
            else
            {
                args = new IOCPSocketAsyncEventArgs();
                args.DisconnectReuseSocket = true;
                args.Completed += Args_Completed;

                LogHelper.Instance.Debug($"创建第{IOCPSocketAsyncEventArgs.InstanceCount}个IOCPSocketAsyncEventArgs");
            }

            return args;
        }

        void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            Listening();

            e.Completed -= Args_Completed;

            Socket socket = e.AcceptSocket;

            if (socket == null)
            {
                LogHelper.Instance.Error("Args_Completed socket为空");
            }

            var endPoint = socket.RemoteEndPoint;
            if (endPoint == null || !(endPoint is IPEndPoint ipEndPoint))
            {
                RealseSocketAsyncEventArgs(e as IOCPSocketAsyncEventArgs, "Args_Completed");
                LogHelper.Instance.Error("Args_Completed endPoint为空");
                return;
            }

            var socketAsyncEventArgs = e as IOCPSocketAsyncEventArgs;

            socket.NoDelay = true;
            Session appSocket = new Session();
            appSocket.IPAddress = ipEndPoint.Address.ToString();
            appSocket.IsValid = true;
            appSocket.SessionID = SocketApplicationComm.GetSeqNum();
            appSocket.Socket = socket;
            appSocket.Port = ipEndPoint.Port;
            appSocket.ConnectTime = DateTime.Now;

            socketAsyncEventArgs.AcceptSocket = socket;
            socketAsyncEventArgs.Completed += SocketAsyncEventArgs_Completed;
            socketAsyncEventArgs.UserToken = appSocket.SessionID;
            appSocket.AsyncEventArgs = socketAsyncEventArgs;

            //byte[] buffer = new byte[4];
            //socketAsyncEventArgs.SetBuffer(buffer, 0, 4);
            SetBuffer(socketAsyncEventArgs, 0, 4);
            //socketAsyncEventArgs.SetBuffer(_bufferpoll.Buffer, _bufferpoll.GetOffset(socketAsyncEventArgs.BufferIndex), 4);

            _connectSocketDic.TryAdd(appSocket.SessionID, appSocket);

            try
            {
                if (!socket.ReceiveAsync(socketAsyncEventArgs))
                {
                    //Session old
                    //_connectSocketDic.TryRemove(appSocket.SessionID, out old);
                    //RealseSocketAsyncEventArgs(socketAsyncEventArgs);

                    LogHelper.Instance.Debug(socket.Handle + "同步完成，手动处理");

                    SocketAsyncEventArgs_Completed(null, e);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error(socket.Handle + "同步异常完成", ex);
                SocketAsyncEventArgs_Completed(null, e);
            }
        }

        private void Listening()
        {
            try
            {
                socketServer.AcceptAsync(GetSocketAsyncEventArgs());
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error("socketServer.AcceptAsync error:" + ex.Message, ex);
                throw;
            }
        }

        void RemoveSession(IOCPSocketAsyncEventArgs args)
        {
            if (args.UserToken != null)
            {
                //用户断开了
                _connectSocketDic.TryRemove(args.UserToken.ToString(), out _);
            }

            try
            {
                RealseSocketAsyncEventArgs(args, "RemoveSession");
            }
            catch
            {
                _deleteQueue.Enqueue(args);
            }

        }

        void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= SocketAsyncEventArgs_Completed;

            var args = e as IOCPSocketAsyncEventArgs;
            if (args.BytesTransferred == 0 || args.SocketError != SocketError.Success)
            {
                if (SocketApplicationEnvironment.TraceSocketDataBag)
                {
                    LogHelper.Instance.Debug(e.AcceptSocket.Handle + "异常断开:" + args.SocketError);
                }

                RemoveSession(args);
                return;
            }
            else
            {
                bool hasdataerror = false;
                try
                {
                    #region 数据逻辑
                    if (!args.IsReadPackLen)
                    {
                        if (args.BytesTransferred != 4)
                        {
                            throw new Exception("读取长度失败");
                        }
                        var offset = args.BufferIndex == -1 ? 0 : _bufferpoll.GetOffset(args.BufferIndex);

                        var dataLen = BitConverter.ToInt32(e.Buffer, offset);

                        if (SocketApplicationEnvironment.TraceSocketDataBag)
                        {
                            LogHelper.Instance.Debug(e.AcceptSocket.Handle + "准备接收数据:长度" + dataLen, null);
                        }

                        if (dataLen > MaxPackageLength || dataLen <= 0)
                        {
                            throw new SocketSessionDataException(string.Format("数据异常，长度:" + dataLen));
                        }
                        else
                        {
                            args.IsReadPackLen = true;
                            //byte[] readbuffer = new byte[dataLen];
                            args.BufferLen = dataLen;
                            args.BufferRev = 0;
                            //args.SetBuffer(readbuffer, 0, dataLen);


                            SetBuffer(args, 0, dataLen);
                        }
                    }
                    else
                    {
                        if (SocketApplication.SocketApplicationEnvironment.TraceSocketDataBag)
                        {
                            var offset1 = (args.BufferLen == args.Buffer.Length) ? 0 : _bufferpoll.GetOffset(args.BufferIndex);
                            var bytes = args.Buffer.Skip(offset1 + args.BufferRev).Take(args.BytesTransferred).ToArray();
                            //if (args.BytesTransferred < args.BufferLen)
                            {
                                LogManager.LogHelper.Instance.Debug(string.Format(e.AcceptSocket.Handle + "接收数据{0}/{1}/{2},{3}", args.BufferLen, args.BufferRev, args.BytesTransferred, Convert.ToBase64String(bytes)), null);
                            }
                        }

                        args.BufferRev += args.BytesTransferred;

                        Exception messageError = null;
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
                                    Session connSession=null;
                                    try
                                    {
                                        Message message = null;

                                        if (_connectSocketDic.TryGetValue(args.UserToken.ToString(), out connSession))
                                        {
                                            if (!string.IsNullOrWhiteSpace(connSession.EncryKey))
                                            {
                                                try
                                                {
                                                    bt = AesEncryHelper.AesDecrypt(bt, connSession.EncryKey);
                                                }
                                                catch(Exception ex)
                                                {
                                                    throw new SocketApplicationException("解密失败",ex);
                                                }
                                            }

                                            try
                                            {
                                                message = EntityBufCore.DeSerialize<Message>(bt);
                                            }
                                            catch (Exception ex)
                                            {
                                                messageError = ex;
                                            }

                                            connSession.LastSessionTime = DateTime.Now;
                                            connSession.BytesRev += bt.Length;
                                            if (messageError == null)
                                            {
                                                //如果是协商加密的
                                                if (message.IsMessage(MessageType.NEGOTIATIONENCRYR))
                                                {
                                                    var nmsg = message.GetMessageBody<NegotiationEncryMessage>();
                                                    if (string.IsNullOrWhiteSpace(nmsg.PublicKey))
                                                    {
                                                        throw new SocketApplicationException("公钥错误");
                                                    }

                                                    var encrykey = connSession.EncryKey;
                                                    if (string.IsNullOrWhiteSpace(encrykey))
                                                    {
                                                        encrykey = Guid.NewGuid().ToString("N");
                                                        Console.WriteLine("发送加密串:" + encrykey);
                                                        var rep = new Message(MessageType.NEGOTIATIONENCRYR);
                                                        rep.SetMessageBody(nmsg);
                                                        nmsg.EncryKey = Convert.ToBase64String(RsaEncryHelper.RsaEncrypt(Encoding.ASCII.GetBytes(encrykey), nmsg.PublicKey)); ;
                                                        connSession.SendMessage(rep);

                                                        connSession.EncryKey = encrykey;
                                                    }
                                                    else
                                                    {
                                                        throw new SocketApplicationException("不允许多次协商密钥");
                                                    }

                                                }
                                                else
                                                {
                                                    FromApp(message, connSession);
                                                }
                                            }
                                            else
                                            {
                                                OnError(messageError);
                                            }
                                        }
                                        else
                                        {
                                            OnError(new Exception("取会话失败,args.UserToken=" + args.UserToken));
                                        }
                                    }
                                    catch (SocketApplicationException ex)
                                    {
                                        if (connSession != null)
                                        {
                                            connSession.Close(ex.Message,true);
                                        }
                                        ex.Data.Add("SessionID", connSession.SessionID);
                                        OnError(ex);
                                    }
                                }), bt);
                            }
                            else
                            {
                                messageError = new Exception("检查校验码出错");
                                messageError.Data.Add("crc32", crc32);
                                messageError.Data.Add("calcrc32", calcrc32);
                                messageError.Data.Add("data", bt == null ? "" : Convert.ToBase64String(bt));

                                //LogManager.LogHelper.Instance.Error("接收数据出错", messageError);

                                Session connSession;
                                if (_connectSocketDic.TryGetValue(args.UserToken.ToString(), out connSession))
                                {
                                    connSession.LastSessionTime = DateTime.Now;
                                    connSession.BytesRev += args.BufferRev;
                                    OnError(messageError);
                                }
                                else
                                {
                                    OnError(new Exception("取会话失败,args.UserToken=" + args.UserToken));
                                }
                            }

                            args.IsReadPackLen = false;
                            //args.SetBuffer(_bufferpoll.Buffer, _bufferpoll.GetOffset(args.BufferIndex), 4);
                            SetBuffer(args, 0, 4);
                        }
                        else
                        {
                            SetBuffer(args, args.BufferRev, args.BufferLen - args.BufferRev);
                            LogManager.LogHelper.Instance.Debug("e.SetBuffer:" + (args.BufferRev) + ",len:" + (args.BufferLen - args.BufferRev), null);
                        }
                    }
                    #endregion
                }
                catch (SocketSessionDataException ex)
                {
                    RemoveSession(args);
                    hasdataerror = true;
                    OnError(ex);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
                finally
                {
                    if (!hasdataerror)
                    {
                        e.Completed += SocketAsyncEventArgs_Completed;
                        try
                        {
                            if (!e.AcceptSocket.ReceiveAsync(e))
                            {
                                LogManager.LogHelper.Instance.Debug(e.AcceptSocket.Handle + "同步完成，手动处理", null);
                                SocketAsyncEventArgs_Completed(null, e);
                            }
                        }
                        catch(Exception ex)
                        {
                            RemoveSession(args);
                            OnError(ex);
                        }
                    }
                }
            }
        }

        protected virtual void FromApp(Message message, Session session)
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
                try
                {
                    socketServer.Shutdown(SocketShutdown.Both);
                }
                catch
                {

                }
                socketServer.Close();
            }

            if (_worktimer != null)
            {
                _worktimer.Stop();
                _worktimer.Close();
            }
        }
    }
}
