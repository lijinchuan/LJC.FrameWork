using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP
{
    public class UDPException:Exception
    {
        public UDPException(string message)
            : base(message)
        {

        }
    }
}
