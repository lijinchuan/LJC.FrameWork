using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    [Serializable]
    public class CachItem
    {
        public DateTime CachTime;
        public object CachObj;
    }
}
