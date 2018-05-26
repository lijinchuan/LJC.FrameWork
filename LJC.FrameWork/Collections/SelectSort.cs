using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class SelectSortTool<T> : SortTool<T> where T : IComparable<T>
    {
        public SelectSortTool(IEnumerable<T> data)
        {
            this.sortlist = data;
        }

        public override IEnumerable<T> Sort()
        {
            for (int i = 0; i < this.sortlist.Count(); i++)
            {
                int min = i;
                for (int j = i + 1; j < this.sortlist.Count(); j++)
                {
                    var item = this.sortlist.ElementAt(j);
                    if (Compare(this.sortlist.ElementAt(min), item) > 0)
                    {
                        min = j;
                    }
                }
                if (min != i)
                {
                    Exchange(i, min);
                }
            }

            return this.sortlist;
        }
    }
}
