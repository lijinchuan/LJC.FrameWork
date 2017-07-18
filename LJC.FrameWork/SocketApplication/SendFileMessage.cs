using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    public class SendFileMessage
    {
        public string FileName
        {
            get;
            set;
        }

        public long FieSize
        {
            get;
            set;
        }

        public bool IsFinished
        {
            get;
            set;
        }

        public byte[] FileBytes
        {
            get;
            set;
        }
    }
}
