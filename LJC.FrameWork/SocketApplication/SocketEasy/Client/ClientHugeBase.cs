using LJC.FrameWork.Comm;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketEasy.Client
{
    public class ClientHugeBase:SocketBase
    {
        protected Socket socketClient;

        protected bool isStartClient = false;
        protected bool errorResume = true;
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

        protected string serverIp;
        protected int ipPort;
        private DateTime lastReStartClientTime;
        /// <summary>
        /// 断线重连时间间隔
        /// </summary>
        private int reConnectClientTimeInterval = 5000;
        private IOCPSocketAsyncEventArgs socketAsyncEvent;

        /// <summary>
        /// 对象清理之前的事件
        /// </summary>
        public event Action BeforRelease;

        private int _maxPackageLength = 10 * 1024 * 1024 * 8;

        private AutoResetEvent _startSign=new AutoResetEvent(false);

        public event Action OnClientReset;

        private byte[] _lenbyte = new byte[4];

        private byte[] _readbytes = new byte[4096];

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
                if(value<=0)
                {
                    return;
                }
                _maxPackageLength = value;
            }
        }

        protected Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverip"></param>
        /// <param name="serverport"></param>
        /// <param name="stop">如果为true,不会断开自动重连</param>
        public ClientHugeBase(string serverip, int serverport, bool errorResume = true, bool security=false)
        {
            this.serverIp = serverip;
            this.ipPort = serverport;
            this.errorResume = errorResume;
            this.isSecurity = security;
        }

        public ClientHugeBase()
        {

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
            lock (this)
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
                        IPAddress connectip;
                        if (!string.IsNullOrEmpty(serverIp))
                            connectip = IPAddress.Parse(serverIp);
                        else
                            connectip = IPAddress.Any;
                        socketAsyncEvent = new IOCPSocketAsyncEventArgs();
                        socketAsyncEvent.Completed += socketAsyncEvent_Completed;
                        socketAsyncEvent.RemoteEndPoint = new IPEndPoint(connectip, ipPort);

                        _startSign.Reset();
                        socketClient.ConnectAsync(socketAsyncEvent);
                        _startSign.WaitOne(30 * 1000);

                        if (!socketClient.Connected)
                        {
                            throw new Exception("连接超时");
                        }

                        if (isSecurity)
                        {
                            RsaEncryHelper.GenPair(out rsaPubKey, out rsaRrivateKey);
                            var msg = new Message(MessageType.NEGOTIATIONENCRYR);
                            encryKey = null;
                            _startSign.Reset();
                            msg.SetMessageBody(new NegotiationEncryMessage
                            {
                                PublicKey = rsaPubKey
                            });
                            socketClient.SendMessage(msg, string.Empty);
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
                            serverIp, ipPort, e.Message, e.SocketErrorCode));
                        throw ne;

                    }
                    catch (Exception e)
                    {
                        lastReStartClientTime = DateTime.Now;
                        throw e;
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
        }

        private void SetBuffer(IOCPSocketAsyncEventArgs e, int offset, int len)
        {
            if(offset>0)
            {
                e.SetBuffer(offset, len);
                return;
            }

            byte[] buf = null;
            if (len > _readbytes.Length)
            {
                buf = new byte[len];
                e.SetBuffer(buf, offset, len);
            }
            else
            {
                e.SetBuffer(_readbytes, offset, len);
            }
        }

        void socketAsyncEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= socketAsyncEvent_Completed;

            var args = e as IOCPSocketAsyncEventArgs;
            if (e.LastOperation == SocketAsyncOperation.Connect)
            {
                if (e.SocketError == SocketError.Success)
                {
                    socketClient = e.ConnectSocket;
                    _startSign.Set();

                    //e.SetBuffer(_lenbyte, 0, 4);
                    SetBuffer(args, 0, 4);
                }
                else
                {
                    //throw new Exception("连接失败:" + e.SocketError);
                }
            }
            else
            {
                if (args.BytesTransferred == 0 || args.SocketError != SocketError.Success)
                {
                    Dispose();
                    return;
                }
                else
                {
                    if (!args.IsReadPackLen)
                    {
                        for(int i=0;i<_lenbyte.Length;i++)
                        {
                            _lenbyte[i] = e.Buffer[i];
                        }
                        int dataLen = BitConverter.ToInt32(_lenbyte, 0);
                        if (dataLen > MaxPackageLength)
                        {
                            Dispose();
                            return;
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
                        args.BufferRev += args.BytesTransferred;
                        if (args.BufferRev == args.BufferLen)
                        {
                            //检验
                            byte[] bt4 = e.Buffer.Take(4).ToArray();
                            var crc32 = BitConverter.ToInt32(bt4, 0);

                            byte[] bt = new byte[args.BufferLen-4];
                            for (int i = 0; i < bt.Length; i++)
                            {
                                bt[i] = e.Buffer[i + 4];
                            }
                            var calcrc32 = LJC.FrameWork.Comm.HashEncrypt.GetCRC32(bt, 0);
                            if (calcrc32 != crc32)
                            {
                                var ex = new Exception("数据校验错误");
                                ex.Data.Add("calcrc32", calcrc32);
                                ex.Data.Add("crc32", crc32);
                                ex.Data.Add("data", Convert.ToBase64String(bt));
                                OnError(ex);
                            }
                            else
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), bt);
                            }
                            args.IsReadPackLen = false;
                            //args.SetBuffer(_lenbyte, 0, 4);
                            SetBuffer(args, 0, 4);
                        }
                        else
                        {
                            e.SetBuffer(args.BufferRev, args.BufferLen - args.BufferRev);
                        }
                    }
                }
            }

            e.Completed += socketAsyncEvent_Completed;

            if (e.SocketError == SocketError.Success)
            {
                socketClient.ReceiveAsync(e);
                //if (!socketClient.ReceiveAsync(e))
                //{
                //    e.Completed -= socketAsyncEvent_Completed;
                //}
            }
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


        public void Dispose()
        {
            if (BeforRelease != null)
            {
                BeforRelease();
            }

            if (socketClient != null)
            {
                socketClient.Close();
            }

            isStartClient = false;
        }
    }
}
