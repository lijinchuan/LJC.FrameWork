using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

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

        public int TotalCount
        {
            get;
            set;
        }

        private bool _ismergin = false;
        [XmlIgnore]
        public bool IsMergin
        {
            get
            {
                return _ismergin;
            }
            set
            {
                _ismergin = value;
            }
        }
    }
}
