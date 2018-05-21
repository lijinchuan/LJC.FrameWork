using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class SorteArray<T> where T : IComparable<T>
    {
        private const UInt16 CapLargerCount = 2;
        private T[] ListSort = null;
        int Count = 0;

        public SorteArray(T[] array)
        {
            if (array != null)
            {
                this.ListSort = array;
                Count = array.Length;
            }
        }

        public void Add(T value)
        {
            if (ListSort == null)
            {
                ListSort = new T[CapLargerCount];
            }

            if (Count == 0)
            {
                ListSort[0] = value;
            }
            else
            {
                int md = 0;
                if (Find(value, ref md) != -1)
                {
                    for (int i = Count - 1; i >= 0; i--)
                    {
                        ListSort[i + 1] = ListSort[i];
                    }

                    ListSort[0]=value;
                }
                else
                {
                    if (ListSort[md].CompareTo(value) < 0)
                    {
                        md = (UInt16)(md + 1);
                    }

                    for (int i = Count - 1; i >= md; i--)
                    {
                        ListSort[i + 1] = ListSort[i];
                    }

                    //ListSort.Insert(md, value);
                    ListSort[md] = value;
                }
            }

            Count++;

            if (Count == ListSort.Length)
            {
                int largerSize = 0;
                if (Count < 1000000)
                {
                    largerSize = Count * 2;
                }
                else
                {
                    largerSize += 1000000;
                }
                

                var newListSort = new T[largerSize];
                for (int i = 0; i < Count; i++)
                {
                    newListSort[i] = ListSort[i];
                }
                ListSort = newListSort;
            }
        }

        public int Find(T key, ref int mid)
        {
            mid = 0;

            if (Count == 0)
            {
                mid = -1;
                return -1;
            }

            if (Count == 1)
            {
                return ListSort[0].CompareTo(key)==0 ? 0 : -1;
            }

            int lt = 0, rt = Count - 1;

            if (ListSort[lt].CompareTo(key)==0)
            {
                return lt;
            }

            if (ListSort[rt].CompareTo(key)==0)
            {
                return rt;
            }

            if (ListSort[lt].CompareTo(key)>0)
            {
                mid = -1;
                return -1;
            }

            if (ListSort[rt].CompareTo(key)<0)
            {
                mid = rt;
                return -1;
            }

            int md = (lt + (rt - lt) / 2);
            while (lt < md && md < rt)
            {
                if (ListSort[md].CompareTo(key)==0)
                {
                    return md;
                }
                else if (ListSort[md].CompareTo(key)>0)
                {
                    rt = md;
                    md = (lt + (rt - lt) / 2);
                }
                else
                {
                    lt = md;
                    md = (lt + (rt - lt) / 2);
                }
            }

            mid = md;
            return -1;

        }

        public IEnumerable<T> GetArray()
        {
            for(int i = 0; i < Count; i++)
            {
                yield return ListSort[i];
            }
        }
    }
}
