using LJC.FrameWork.SocketEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    public class Session
    {
        public string SessionID
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public bool IsValid
        {
            get;
            set;
        }

        public bool IsLogin
        {
            get;
            internal set;
        }

        public Socket Socket
        {
            get;
            internal set;
        }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectTime
        {
            get;
            internal set;
        }

        private DateTime _lastSessionTime = DateTime.Now;
        /// <summary>
        /// 上次心跳时间
        /// </summary>
        public DateTime LastSessionTime
        {
            get
            {
                return _lastSessionTime;
            }
            internal set
            {
                _lastSessionTime = value;
            }
        }

        /// <summary>
        /// 业务时间戳
        /// </summary>
        public DateTime BusinessTimeStamp
        {
            get;
            set;
        }

        public object Tag
        {
            get;
            set;
        }

        /// <summary>
        /// 心跳间隔
        /// </summary>
        public int HeadBeatInterVal
        {
            get;
            internal set;
        }

        public string IPAddress
        {
            get;
            internal set;
        }

        public int Port
        {
            get;
            internal set;
        }

        /// <summary>
        /// 会话超时
        /// </summary>
        public int SessionTimeOut
        {
            get;
            internal set;
        }

        public long BytesRev
        {
            get;
            set;
        }

        public long BytesSend
        {
            get;
            set;
        }

        internal Session()
        {
            HeadBeatInterVal = 10000;
            SessionTimeOut = 30000;
        }

        public bool IsTimeOut()
        {
            return ((int)DateTime.Now.Subtract(LastSessionTime).TotalMilliseconds) > SessionTimeOut * 2;
        }

        public bool Close(string closeReason)
        {
            if (this.Socket != null)
            {
                LogManager.LogHelper.Instance.Info($"{this.SessionID}关闭：{closeReason}");
                try
                {
                    this.Socket.Shutdown(SocketShutdown.Both);
                    this.Socket.Close();
                }
                catch
                {

                }
                
                return true;
            }
            this.IsValid = false;
            return false;
        }

        public virtual bool SendMessage(Message msg)
        {
            if (this.Socket == null)
                throw new Exception("无套接字");

            var sendcount = this.Socket.SendMessage(msg, this.EncryKey);

            if (sendcount > 0)
            {
                this.LastSessionTime = DateTime.Now;
                this.BytesSend += sendcount;
            }

            return sendcount > 0;
        }

        /// <summary>
        /// 对称加密钥
        /// </summary>
        public string EncryKey
        {
            get;
            set;
        }

        public IOCPSocketAsyncEventArgs AsyncEventArgs
        {
            get;
            set;
        }
    }
}
