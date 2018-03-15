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


        private int _loadFactor = 1;
        /// <summary>
        /// 加载因子
        /// </summary>
        public int LoadFactor
        {
            get
            {
                return _loadFactor;
            }
            set
            {
                _loadFactor = value;
            }
        }
    }
}
