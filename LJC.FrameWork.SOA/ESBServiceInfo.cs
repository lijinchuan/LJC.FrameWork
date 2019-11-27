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
        /// 端点名称
        /// </summary>
        public string EndPointName
        {
            get;
            set;
        }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName
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

        public string[] RedirectTcpIps
        {
            get;
            set;
        }

        public int RedirectTcpPort
        {
            get;
            set;
        }

        public string[] RedirectUdpIps
        {
            get;
            set;
        }

        public int RedirectUdpPort
        {
            get;
            set;
        }
    }
}
