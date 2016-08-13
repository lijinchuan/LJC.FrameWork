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
