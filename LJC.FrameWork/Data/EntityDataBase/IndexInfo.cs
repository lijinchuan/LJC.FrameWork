using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.EntityDataBase
{
    public class IndexInfo
    {
        public string IndexName
        {
            get;
            set;
        }

        public IndexItem[] Indexs
        {
            get;
            set;
        }
    }
}
