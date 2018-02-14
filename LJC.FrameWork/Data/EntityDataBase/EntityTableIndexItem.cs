using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.Data.EntityDataBase
{
    public class EntityTableIndexItem
    {
        public string Key
        {
            get;
            set;
        }

        public long Offset
        {
            get;
            set;
        }

        public int len
        {
            get;
            set;
        }

        public bool Del
        {
            get;
            set;
        }
    }
}
