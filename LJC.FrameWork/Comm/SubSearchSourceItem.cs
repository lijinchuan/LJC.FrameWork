using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class SubSearchSourceItem
    {
        public SubSearchSourceItem(string source, object tag)
        {
            Source = source;
            Tag = tag;
        }

        public string Source
        {
            get;
            set;
        }

        public object Tag
        {
            get;
            set;
        }

    }
}
