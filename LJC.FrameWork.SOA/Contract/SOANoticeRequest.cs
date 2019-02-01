using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    public class SOANoticeRequest
    {
        /// <summary>
        /// 接收者
        /// </summary>
        public string[] ReciveClients
        {
            get;
            set;
        }

        public int NoticeType
        {
            get;
            set;
        }

        public byte[] NoticeBody
        {
            get;
            set;
        }

        /// <summary>
        /// 是否需要发送结果
        /// </summary>
        public bool NeedResult
        {
            get;
            set;
        }
    }
}
