using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class SortedList<T> where T : IComparable<T>
    {
        internal T Tag
        {
            get;
            set;
        }

        private List<T> ListSort = null;

        public SortedList(List<T> souce, T tag)
        {
            this.ListSort = souce;
            this.Tag = tag;
        }

        public SortedList()
        {
            this.ListSort = new List<T>();
        }

        public void Add(T value)
        {
            if (ListSort.Count == 0)
            {
                ListSort.Add(value);
            }
            else
            {
                int md = 0;
                var pos = Find(value, ref md);
                if (pos != -1)
                {
                    ListSort.Insert(pos + 1, value);
                }
                else
                {
                    md += 1;
                    ListSort.Insert(md, value);
                    //ListSort.Insert(0, value);
                }
            }
        }

        public int Find(T key, ref int mid)
        {
            mid = 0;

            if (ListSort.Count == 0)
            {
                mid = -1;
                return -1;
            }

            if (ListSort.Count == 1)
            {
                var compare = ListSort[0].CompareTo(key);
                if (compare > 0)
                {
                    mid = -1;
                }
                return compare == 0 ? 0 : -1;
            }

            int lt = 0, rt = ListSort.Count - 1;
            var comparelt = ListSort[lt].CompareTo(key);
            var comparert = ListSort[rt].CompareTo(key);

            if (comparelt == 0)
            {
                return lt;
            }

            if (comparert == 0)
            {
                return rt;
            }

            if (comparelt > 0)
            {
                mid = -1;
                return -1;
            }

            if (comparert < 0)
            {
                mid = rt;
                return -1;
            }

            int md = (lt + (rt - lt) / 2);
            while (lt < md && md < rt)
            {
                var compare = ListSort[md].CompareTo(key);
                if (compare == 0)
                {
                    return md;
                }
                else if (compare > 0)
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

        public IEnumerable<T> FindAll(T key)
        {
            if (this.ListSort.Count > 0)
            {
                int mid = -1;
                var pos = this.Find(key, ref mid);
                var pospre = pos;
                var poslast = pos;
                var compare = -1;
                if (pos > -1)
                {
                    compare = 0;
                    while (pospre >= 0)
                    {
                        compare = this.ListSort[pospre].CompareTo(key);
                        if (compare < 0 || pospre == 0)
                        {
                            break;
                        }
                        pospre--;
                    }
                    if (compare != 0)
                    {
                        pospre++;
                    }
                }
                else
                {
                    pospre = mid + 1;
                    poslast = pospre;
                }
                while (poslast < this.ListSort.Count)
                {
                    compare = this.ListSort[poslast].CompareTo(key);
                    if (compare > 0)
                    {
                        break;
                    }
                    if (poslast == this.ListSort.Count - 1)
                    {
                        break;
                    }
                    poslast++;
                }
                if (compare != 0)
                {
                    poslast--;
                }
                for (; pospre <= poslast; pospre++)
                {
                    yield return this.ListSort[pospre];
                }
            }
        }

        public List<T> GetList()
        {
            return ListSort;
        }

        public int Length()
        {
            return ListSort.Count;
        }
    }
}
