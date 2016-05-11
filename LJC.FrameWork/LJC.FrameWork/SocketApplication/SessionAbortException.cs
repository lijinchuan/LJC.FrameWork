using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    internal class SessionAbortException : Exception
    {
        public SessionAbortException()
            : base()
        {

        }

        public SessionAbortException(string msg)
            : base(msg)
        {

        }
    }
}
