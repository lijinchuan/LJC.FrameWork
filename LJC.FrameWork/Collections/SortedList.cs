using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class SortedList<T> where T : IComparable
    {
        private List<T> ListSort = new List<T>();

        public void Add(T value)
        {
            if (ListSort.Count == 0)
            {
                ListSort.Add(value);
            }
            else
            {
                int md = 0;
                if (Find(value, ref md) != -1)
                {
                    ListSort.Insert(0, value);
                }
                else
                {
                    if (ListSort[md].CompareTo(value)<0)
                    {
                        md = (UInt16)(md + 1);
                    }
                    //ListSort.Insert(md, value);
                    ListSort.Insert(0, value);
                }
            }
        }

        public int Find(T key, ref int mid)
        {
            mid = 0;

            if (ListSort.Count == 0)
                return -1;

            if (ListSort.Count == 1)
            {
                return ListSort[0].CompareTo(key)==0 ? 0 : -1;
            }

            int lt = 0, rt = ListSort.Count - 1;

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
                mid = lt;
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

        public List<T> GetList()
        {
            return ListSort;
        }
    }
}
