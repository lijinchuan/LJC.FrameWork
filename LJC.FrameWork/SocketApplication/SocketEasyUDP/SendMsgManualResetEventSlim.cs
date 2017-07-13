using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketApplication
{
    public class SendMsgManualResetEventSlim : ManualResetEventSlim
    {
        public long SegmentId
        {
            get;
            set;
        }
    }
}
