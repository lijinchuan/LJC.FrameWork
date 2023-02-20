using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class WebMapper
    {
        /// <summary>
        /// 虚拟节点，简单路由
        /// </summary>
        public string VirRoot
        {
            get;
            set;
        }

        /// <summary>
        /// 正则路径，优先度高与VirRoot
        /// </summary>
        public string RegexRoute
        {
            get;
            set;
        }

        /// <summary>
        /// 本地访问host，不填写填写localhost
        /// </summary>
        public string LocalHost
        {
            get;
            set;
        }


    }
}
