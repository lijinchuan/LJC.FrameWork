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
    public class ClientBase:SocketBase
    {
        protected Socket socketClient;

        protected bool isStartClient = false;
        protected bool errorResume = true;
        protected string serverIp;
        protected int ipPort;
        private DateTime lastReStartClientTime;
        /// <summary>
        /// 断线重连时间间隔
        /// </summary>
        private int reConnectClientTimeInterval = 5000;

        /// <summary>
        /// 对象清理之前的事件
        /// </summary>
        public event Action BeforRelease;

        private int _maxPackageLength = 10 * 1024 * 1024 * 8;
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

        private byte[] _reciveBuffer = new byte[1024];

        protected Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverip"></param>
        /// <param name="serverport"></param>
        /// <param name="stop">如果为true,不会断开自动重连</param>
        public ClientBase(string serverip, int serverport, bool errorResume = true)
        {
            this.serverIp = serverip;
            this.ipPort = serverport;
            this.errorResume = errorResume;
        }

        public ClientBase()
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
                    if (!string.IsNullOrEmpty(serverIp))
                        socketClient.Connect(IPAddress.Parse(serverIp), ipPort);
                    else
                        socketClient.Connect(IPAddress.Any, ipPort);
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

        private void Receiving()
        {
            while (!stop/* && socketClient.Connected*/)
            {
                try
                {
                    byte[] buff4 = new byte[4];
                    int count = socketClient.Receive(buff4);
                    if (count != 4)
                        break;

                    int dataLen = BitConverter.ToInt32(buff4, 0);

                    if(dataLen>MaxPackageLength)
                    {
                        throw new Exception("超过了最大字节数：" + MaxPackageLength);
                    }

                    MemoryStream ms = new MemoryStream();
                    int readLen = 0;

                    while (readLen < dataLen)
                    {
                        count = socketClient.Receive(_reciveBuffer, Math.Min(dataLen - readLen, _reciveBuffer.Length), SocketFlags.None);
                        readLen += count;
                        ms.Write(_reciveBuffer, 0, count);
                    }
                    var buffer = ms.ToArray();
                    ms.Close();

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
                Message message = EntityBufCore.DeSerialize<Message>(data);
                OnMessage(message);
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
                return socketClient.SendMessage(message, string.Empty).SendCount > 0;
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
