using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP
{
    public class UDPRevResultMessage
    {
        public long BagId
        {
            get;
            set;
        }

        public int[] Miss
        {
            get;
            set;
        }

        public bool IsReved
        {
            get;
            set;
        }
    }
}
