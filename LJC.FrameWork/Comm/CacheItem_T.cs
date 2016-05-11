using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    [Serializable]
    public class CacheItem<T>
    {
        /// <summary>
        /// 值
        /// </summary>
        public T Item
        {
            get;
            set;
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime Expired
        {
            get;
            set;
        }

        public Func<T> RefrashFunc
        {
            get;
            set;
        }

        private int _cachMinis = 10;
        public int CachMinis
        {
            get
            {
                return _cachMinis;
            }
            set
            {
                if (value < 1)
                    value = 1;
                _cachMinis = value;
            }
        }
    }
}
