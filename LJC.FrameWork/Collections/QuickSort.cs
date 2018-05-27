using LJC.FrameWork.Comm;
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
            this.sortarray = data.ToArray();
        }

        void Partition(int left, int right)
        {
            if (left > right)
            {
                return;
            }
            int i = left, j = right;
            int temp = left;

            #region 写法1
            //while (i < j)
            //{
            //    if (temp == i)
            //    {
            //        if (Compare(sortarray[temp], sortarray[j]) > 0)
            //        {
            //            Exchange(temp, j);
            //            temp = j;
            //            i++;
            //        }
            //        else
            //        {
            //            j--;
            //        }
            //    }
            //    else
            //    {
            //        if (Compare(sortarray[temp], sortarray[i]) < 0)
            //        {
            //            Exchange(temp, i);
            //            temp = i;
            //            j--;
            //        }
            //        else
            //        {
            //            i++;
            //        }
            //    }
            //}
            //Partition(left, temp - 1);
            //Partition(temp + 1, right);
            #endregion

            #region 写法2
            while (i != j)
            {
                while (Compare(sortarray[j], sortarray[temp]) >= 0 && i < j)
                {
                    j--;
                }

                while (Compare(sortarray[i], sortarray[temp]) <= 0 && i < j)
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
            #endregion
        }

        public override IEnumerable<T> Sort()
        {
            Partition(0, this.sortarray.Length - 1);

            return this.sortarray;
        }
    }
}
