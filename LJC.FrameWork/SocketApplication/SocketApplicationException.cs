using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    public class SocketApplicationException:Exception
    {
        public SocketApplicationException(string message)
            : base(message)
        {

        }

        public SocketApplicationException(string message,Exception innerException)
            :base(message,innerException)
        {

        }
    }
}
