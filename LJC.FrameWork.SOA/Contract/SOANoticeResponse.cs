using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    public class SOANoticeResponse
    {
        /// <summary>
        /// 是否执行
        /// </summary>
        public bool IsDone
        {
            get;
            set;
        }

        public string[] SuccList
        {
            get;
            set;
        }

        public string[] FailList
        {
            get;
            set;
        }
    }
}
