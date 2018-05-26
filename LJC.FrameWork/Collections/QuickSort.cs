using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class QuickSortTool<T> : SortTool<T> where T : IComparable<T>
    {
        public QuickSortTool(IEnumerable<T> data)
        {
            this.sortlist = data;
        }

        void Partition(int left, int right)
        {
            if (left > right)
            {
                return;
            }
            int i = left, j = right;
            int temp = left;
            while (i != j)
            {
                while (Compare(sortlist.ElementAt(j), sortlist.ElementAt(temp)) >= 0&&i<j)
                {
                    j--;
                }

                while (Compare(sortlist.ElementAt(i), sortlist.ElementAt(temp)) <= 0 && i < j)
                {
                    i++;
                }

                if (i < j)
                {
                    Exchange(i, j);
                }

            }
            
            Exchange(left, i);

            Partition(left, i - 1);
            Partition(i + 1, right);
        }

        public override IEnumerable<T> Sort()
        {
            Partition(0, this.sortlist.Count() - 1);

            return this.sortlist;
        }
    }
}
