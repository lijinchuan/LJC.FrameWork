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
            this.sortarray = data.ToArray();
        }

        public override IEnumerable<T> Sort()
        {
            if (sortarray == null)
            {
                return sortarray;
            }

            for (var i = 0; i < sortarray.Count() - 1; i++)
            {
                for (var j = 1; j < sortarray.Count(); j++)
                {
                    var itemi = sortarray.ElementAt(j - 1);
                    var itemj = sortarray.ElementAt(j);

                    var compare = Compare(itemi, itemj);
                    if (compare > 0)
                    {
                        Exchange(j - 1, j);
                    }
                }
            }

            return sortarray;
        }
    }
}
