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
        protected bool isStartClient = false;
        protected bool stop = false;
        private string ipString;
        private int ipPort;
        private DateTime lastReStartClientTime;
        protected bool isStartServer = false;
        /// <summary>
        /// 断线重连时间间隔
        /// </summary>
        private int reConnectClientTimeInterval = 5000;

        public event Action<Exception> Error;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public MessageApp(string ip, int port)
        {
            this.ipString = ip;
            this.ipPort = port;
        }

        public bool StartServer()
        {
            try
            {
                if (socketServer == null)
                {
                    socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socketServer.Bind(new IPEndPoint(IPAddress.Any, ipPort));
                }

                socketServer.Listen(100);

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

        public bool StartClient()
        {
            try
            {
                if (socketClient != null && socketClient.Connected)
                    return true;

                if (DateTime.Now.Subtract(lastReStartClientTime).TotalMilliseconds <= reConnectClientTimeInterval)
                    return true;

                if (socketClient != null)
                {
                    socketClient.Close();
                }

                socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    if (!string.IsNullOrEmpty(ipString))
                        socketClient.Connect(IPAddress.Parse(ipString), ipPort);
                    else
                        socketClient.Connect(IPAddress.Any, ipPort);
                }
                catch (Exception e)
                {
                    lastReStartClientTime = DateTime.Now;
                    throw e;
                }

                if (!isStartClient)
                {
                    Thread threadClient = new Thread(Receiveing);
                    threadClient.Start();
                }

                isStartClient = true;
                return true;
            }
            catch (Exception e)
            {
                OnError(e);
                return false;
            }
        }

        private void Receiveing()
        {
            while (!stop)
            {
                try
                {
                    byte[] buff4 = new byte[4];
                    int count = socketClient.Receive(buff4);
                    if (count == 0)
                        break;

                    int dataLen = BitConverter.ToInt32(buff4, 0);

                    byte[] buffer = new byte[dataLen];

                    count = socketClient.Receive(buffer);

                    Thread newThread = new Thread(new ParameterizedThreadStart(ProcessMessage));
                    newThread.Start(buffer);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), buffer2);
                }
                catch (SocketException e)
                {
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

        public bool SendMessage(Message message)
        {
            try
            {
                byte[] data = EntityBufCore.Serialize(message);
                byte[] len = BitConverter.GetBytes(data.Length);
                socketClient.Send(len);
                socketClient.Send(data);
                return true;
            }
            catch (Exception e)
            {
                OnError(e);
                return false;
            }
        }

        protected virtual void OnMessage(Message message)
        {

        }

        protected virtual void OnError(Exception e)
        {
            if (stop)
                return;

            if (socketClient != null && !socketClient.Connected)
            {
                StartClient();
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
            IPEndPoint endPoint = (IPEndPoint)socket.RemoteEndPoint;

            Session appSocket = new Session();
            appSocket.IsValid = true;
            appSocket.SessionID = SocketApplicationComm.GetSeqNum();

            while (appSocket.IsValid)
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

                    byte[] buffer = new byte[dataLen];
                    count = socket.Receive(buffer);

                    Message message=EntityBufCore.DeSerialize<Message>(buffer);
                    FormApp(socket, message, appSocket);
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

        protected virtual void FormApp(Socket s, Message message, Session session)
        {

        }

        #endregion

        public void Dispose()
        {
            if (socketServer != null)
            {
                socketServer.Close();
            }

            if (socketClient != null)
            {
                socketClient.Close();
            }
        }
    }
}
