using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// 连接时间
        /// </summary>
        internal DateTime ConnectTime
        {
            get;
            set;
        }

        /// <summary>
        /// 上次心跳时间
        /// </summary>
        internal DateTime LastSessionTime
        {
            get;
            set;
        }

        /// <summary>
        /// 心跳间隔
        /// </summary>
        internal int HeadBeatInterVal
        {
            get;
            set;
        }

        /// <summary>
        /// 会话超时
        /// </summary>
        internal int SessionTimeOut
        {
            get;
            set;
        }

        internal Session()
        {
            HeadBeatInterVal = 10000;
            SessionTimeOut = 30000;
        }

        internal bool IsTimeOut()
        {
            return ((int)DateTime.Now.Subtract(LastSessionTime).TotalMilliseconds) > SessionTimeOut;
        }
    }
}
