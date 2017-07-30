using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP
{
    public class SendFileCheckResponseMessage
    {
        public string FileName
        {
            get;
            set;
        }

        public long FileLength
        {
            get;
            set;
        }
    }
}
