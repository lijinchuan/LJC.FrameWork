using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class SOAException:Exception
    {
        public SOAException(string message)
            : base(message)
        {

        }

        public SOAException(string message,Exception innerException)
            :base(message,innerException)
        {
            
        }
    }
}
