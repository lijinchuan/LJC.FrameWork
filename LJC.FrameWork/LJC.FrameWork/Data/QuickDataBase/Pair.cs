using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public class Pair<F, S>
    {
        public F First
        {
            get;
            set;
        }

        public S Second
        {
            get;
            set;
        }

        public Pair(F f, S s)
        {
            First = f;
            Second = s;
        }
    }
}
