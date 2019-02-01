using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    public class SOANoticeClientMessage
    {
        public int ServiceNo
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
    }
}
