using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    internal class SOARequest
    {
        public int ServiceNo
        {
            get;
            set;
        }

        public int FuncId
        {
            get;
            set;
        }

        public DateTime ReqestTime
        {
            get;
            set;
        }

        public byte[] Param
        {
            get;
            set;
        }
    }
}
