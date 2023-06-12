using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class SubSearchResultItem
    {
        public int StartPos
        {
            get;
            set;
        }

        public int Len
        {
            get;
            set;
        }

        public string SubWord
        {
            get;
            set;
        }

        public int WordStartPos
        {
            get;
            set;
        }
    }
}
