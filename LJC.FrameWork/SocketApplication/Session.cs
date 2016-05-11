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

        /// <summary>
        /// 上次心跳时间
        /// </summary>
        public DateTime LastSessionTime
        {
            get;
            internal set;
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

        /// <summary>
        /// 会话超时
        /// </summary>
        public int SessionTimeOut
        {
            get;
            internal set;
        }

        internal Session()
        {
            HeadBeatInterVal = 10000;
            SessionTimeOut = 30000;
        }

        public bool IsTimeOut()
        {
            return ((int)DateTime.Now.Subtract(LastSessionTime).TotalMilliseconds) > SessionTimeOut;
        }

        public void Close()
        {
            if (this.Socket != null && this.Socket.Connected)
                this.Socket.Close();
            this.IsValid = false;
        }

        public bool SendMessage(Message msg)
        {
            if (this.Socket == null)
                throw new Exception("无套接字");

            return this.Socket.SendMessge(msg);
        }
    }
}
