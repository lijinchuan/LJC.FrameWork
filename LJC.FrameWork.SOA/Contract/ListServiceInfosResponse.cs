using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    public class ListServiceInfosResponse
    {
        /// <summary>
        /// 所有的服务
        /// </summary>
        public RegisterServiceInfo[] Services
        {
            get;
            set;
        }
    }
}
