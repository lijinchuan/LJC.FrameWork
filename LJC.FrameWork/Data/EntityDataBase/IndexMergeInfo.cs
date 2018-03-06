using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.EntityDataBase
{
    [Serializable]
    public class IndexMergeInfo
    {
        public string IndexName
        {
            get;
            set;
        }

        public long IndexMergePos
        {
            get;
            set;
        }
    }
}
