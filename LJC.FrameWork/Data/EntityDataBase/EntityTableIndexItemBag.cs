using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.EntityDataBase
{
    public class EntityTableIndexItemBag
    {
        private ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>> _dics = new ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>>();
        public ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>> Dics
        {
            get
            {
                return _dics;
            }
        }

        public DateTime LastUsed
        {
            get;
            set;
        }

        public long LastOffset
        {
            get;
            set;
        }
    }
}
