using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;

namespace LJC.FrameWork.SOA
{
    public class ESBServiceInfo
    {
        public int ClientID
        {
            get;
            set;
        }

        public int ServiceNo
        {
            get;
            set;
        }

        /// <summary>
        /// 会话信息
        /// </summary>
        public Session Session
        {
            get;
            set;
        }
    }
}
