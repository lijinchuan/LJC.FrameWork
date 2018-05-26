using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class BubbleSortTool<T>:SortTool<T> where T:IComparable<T>
    {
        public BubbleSortTool(IEnumerable<T> data)
        {
            this.sortlist = data;
        }

        public override IEnumerable<T> Sort()
        {
            if (sortlist == null)
            {
                return sortlist;
            }

            for (var i = 0; i < sortlist.Count() - 1; i++)
            {
                for (var j = 1; j < sortlist.Count(); j++)
                {
                    var itemi = sortlist.ElementAt(j - 1);
                    var itemj = sortlist.ElementAt(j);

                    var compare = Compare(itemi, itemj);
                    if (compare > 0)
                    {
                        Exchange(j - 1, j);
                    }
                }
            }

            return sortlist;
        }
    }
}
