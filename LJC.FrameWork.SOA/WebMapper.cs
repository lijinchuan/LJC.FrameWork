using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class WebMapper
    {
        /// <summary>
        /// 本地映射端口，0则使用默认端口
        /// </summary>
        public int MappingPort
        {
            get;
            set;
        }

        /// <summary>
        /// 本地映射虚拟目录
        /// </summary>
        public string MappingRoot
        {
            get;
            set;
        }

        /// <summary>
        /// 目标网站
        /// </summary>
        public string TragetWebHost
        {
            get;
            set;
        }

        /// <summary>
        /// 使用代理名称
        /// </summary>
        public string UseProxyName
        {
            get;
            set;
        }

        /// <summary>
        /// 不改写cookie
        /// </summary>
        public bool NoRewirteCookie
        {
            get;
            set;
        }
    }
}
