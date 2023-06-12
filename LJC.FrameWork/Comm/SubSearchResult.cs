using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    /// <summary>
    /// 子搜索结果
    /// </summary>
    public class SubSearchResult
    {
        public int StartPos
        {
            get;
            set;
        }

        public List<SubSearchResultItem> SubSearchResultItems
        {
            get;
            set;
        }

        public int EndLeftLen
        {
            get;
            set;
        }

        internal object Tag
        {
            get;
            set;
        }
    }
}
