using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public abstract class SortTool<T> where T:IComparable<T>
    {
        protected T[] sortarray=null;

        public int CompareCount = 0;
        public int ExchangeCount = 0;

        protected int Compare(T t1, T t2)
        {
            CompareCount++;
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
            var item1 = sortarray[i];

            sortarray[i] = sortarray[j];
            sortarray[j] = item1;

            ExchangeCount++;
        }

        public virtual IEnumerable<T> Sort()
        {
            return this.sortarray;
        }
    }
}
