using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    public class SocketSessionDataException:Exception
    {
        public SocketSessionDataException(string message)
            :base(message)
        {

        }
    }
}
