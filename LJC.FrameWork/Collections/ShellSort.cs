using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class ShellSortTool<T> : SortTool<T> where T : IComparable<T>
    {
        public ShellSortTool(IEnumerable<T> data)
        {
            this.sortarray = data.ToArray();
        }

        public override IEnumerable<T> Sort()
        {
            int inner, outer;
            T temp;
            int h = 1;
            while (h <= sortarray.Length / 3)
            {
                h = h * 3 + 1;
            }
            //外循环控制排序趟数
            while (h > 0)
            {
                //控制每个增量的循环
                for (outer = h; outer < sortarray.Length; outer++)
                {
                    temp = sortarray[outer];
                    inner = outer;

                    while (inner > h - 1 && Compare(sortarray[inner - h], temp) > 0)
                    {
                        sortarray[inner] = sortarray[inner - h];
                        inner = inner - h;
                    }
                    sortarray[inner] = temp;
                }
                h = (h - 1) / 3;
            }
            return this.sortarray;
        }
    }
}
