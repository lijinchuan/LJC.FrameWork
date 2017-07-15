using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP
{
    public class PipelineManualResetEventSlim: ManualResetEventSlim
    {
        public long BagId
        {
            get;
            set;
        }

        public byte[] MsgBuffer
        {
            get;
            set;
        }

        private bool _isTimeOut = true;
        public bool IsTimeOut
        {
            get
            {
                return _isTimeOut;
            }
            private set
            {
                _isTimeOut = value;
            }
        }

        public new void Reset()
        {
            _isTimeOut = true;
            base.Reset();
        }

        public new void Set()
        {
            IsTimeOut = false;
            base.Set();
        }
    }
}
