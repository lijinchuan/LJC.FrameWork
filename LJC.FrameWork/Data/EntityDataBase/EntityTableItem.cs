using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.Data.EntityDataBase
{
    public class EntityTableItem<T> where T : new()
    {
        public byte Flag
        {
            get;
            set;
        }

        public EntityTableItem()
        {

        }

        public EntityTableItem(T item)
        {
            Data=item;
            Flag = (byte)EntityTableItemFlag.Ok;
        }

        public T Data
        {
            get;
            set;
        }
    }
}
