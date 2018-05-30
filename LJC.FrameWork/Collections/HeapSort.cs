using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class HeapSortTool<T> : SortTool<T> where T : IComparable<T>
    {
        public HeapSortTool(IEnumerable<T> data)
        {
            this.sortarray = data.ToArray();
        }

        public override IEnumerable<T> Sort()
        {
            //1.构建大顶堆
            for (int i = sortarray.Length / 2 - 1; i >= 0; i--)
            {
                //从第一个非叶子结点从下至上，从右至左调整结构
                AdjustHeap(i, sortarray.Length);
            }
            //2.调整堆结构+交换堆顶元素与末尾元素
            for (int j = sortarray.Length - 1; j > 0; j--)
            {
                Exchange(0, j);
                //Swap(arr, 0, j);//将堆顶元素与末尾元素进行交换
                AdjustHeap(0, j);//重新对堆进行调整
            }

            return this.sortarray;
        }

        /**
     * 调整大顶堆（仅是调整过程，建立在大顶堆已构建的基础上）
     * @param arr
     * @param i
     * @param length
     */
        public void AdjustHeap(int i, int length)
        {
            T temp = sortarray[i];//先取出当前元素i
            for (int k = i * 2 + 1; k < length; k = k * 2 + 1)
            {//从i结点的左子结点开始，也就是2i+1处开始
                if (k + 1 < length && Compare(sortarray[k], sortarray[k + 1]) < 0)
                {//如果左子结点小于右子结点，k指向右子结点
                    k++;
                }
                if (Compare(sortarray[k], temp) > 0)
                {//如果子节点大于父节点，将子节点值赋给父节点（不用进行交换）
                    sortarray[i] = sortarray[k];
                    i = k;
                }
                else
                {
                    break;
                }
            }
            sortarray[i] = temp;//将temp值放到最终的位置
        }
    }
}
