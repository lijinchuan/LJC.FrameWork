using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public abstract class SortTool<T> where T:IComparable<T>
    {
        protected IEnumerable<T> sortlist=null;

        protected int Compare(T t1, T t2)
        {
            var boo1 = object.Equals(t1, null);
            var boo2 = object.Equals(t2, null);
            if (boo1 && boo2)
            {
                return 0;
            }

            if (boo1)
            {
                return -1;
            }

            if (boo2)
            {
                return 1;
            }

            return t1.CompareTo(t2);
        }

        protected void Exchange(int i, int j)
        {
            var item1 = sortlist.ElementAt(i);

            if (sortlist is IList<T>)
            {
                (sortlist as IList<T>)[i] = sortlist.ElementAt(j);
                (sortlist as IList<T>)[j] = item1;
            }
            else if (sortlist is Array)
            {
                var array = (sortlist as T[]);
                array[i] = array[j];
                array[j] = item1;
            }
            else
            {
                throw new Exception("交换失败");
            }
        }

        public virtual IEnumerable<T> Sort()
        {
            return this.sortlist;
        }
    }
}
