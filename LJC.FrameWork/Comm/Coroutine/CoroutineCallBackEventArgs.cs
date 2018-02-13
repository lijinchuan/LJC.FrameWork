using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.Comm.Coroutine
{
    public class CoroutineCallBackEventArgs
    {
        public bool HasError
        {
            get;
            internal set;
        }

        public Exception Error
        {
            get;
            internal set;
        }

        public ICoroutineUnit CoroutineUnit
        {
            get;
            internal set;
        }
    }
}
