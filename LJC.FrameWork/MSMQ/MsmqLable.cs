using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.MSMQ
{
    public class MsmqLable
    {
        public string Lable
        {
            get;
            set;
        }

        public string MergeId
        {
            get;
            set;
        }

        public int Split
        {
            get;
            set;
        }

        public int SplitNo
        {
            get;
            set;
        }

        public long MsgSize
        {
            get;
            set;
        }
    }
}
