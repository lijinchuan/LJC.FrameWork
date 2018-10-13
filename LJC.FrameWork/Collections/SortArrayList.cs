using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class SortArrayList<T> where T : IComparable<T>
    {
        private const int CapLargerCount = 10000;
        private SortedList<T>[] ListSort = null;
        int Count = 0;

        public SortArrayList()
        {
        }

        private void BackMove(int start)
        {
            SortedList<T> last = null;
            int i = 0;
            foreach (var item in ListSort)
            {
                if (i < start)
                {
                    i++;
                    continue;
                }
                else if (i == start)
                {
                    last = item;
                    i++;
                    continue;
                }
                else if (i > Count)
                {
                    break;
                }
                else
                {
                    ListSort[i] = last;
                    last = item;
                    i++;
                }
            }
        }

        private void Split(int pos)
        {
            //Console.WriteLine("split:"+pos);
            var list = ListSort[pos].GetList();
            var half = list.Count / 2;
            var list1 = list.Take(half).ToList();
            var list2 = list.Skip(half).ToList();
            var sortlist1 = new SortedList<T>(list1, list1.Last());
            var sortlist2 = new SortedList<T>(list2, list2.Last());

            if (Count == ListSort.Length)
            {
                Expend();
            }

            BackMove(pos);
            ListSort[pos] = sortlist1;
            ListSort[pos + 1] = sortlist2;
            Count++;
        }

        public void Add(T value)
        {
            if (ListSort == null)
            {
                ListSort = new SortedList<T>[CapLargerCount];
            }

            if (Count == 0)
            {
                var sl = new SortedList<T>();
                sl.Tag = value;
                sl.Add(value);
                ListSort[0] = sl;

                Count++;
            }
            else
            {
                int md = 0;
                var pos = Find(value, ref md);
                if (Count < CapLargerCount / 2)
                {
                    if (pos != -1)
                    {
                        //for (int i = Count - 1; i > pos; i--)
                        //{
                        //    ListSort[i + 1] = ListSort[i];
                        //}
                        BackMove(pos);
                        var sl = new SortedList<T>();
                        ListSort[pos] = sl;
                        sl.Tag = value;
                        sl.Add(value);
                    }
                    else
                    {
                        md += 1;

                        //for (int i = Count - 1; i >= md; i--)
                        //{
                        //    ListSort[i + 1] = ListSort[i];
                        //}
                        BackMove(md);
                        var sl = new SortedList<T>();
                        ListSort[md] = sl;
                        sl.Tag = value;
                        sl.Add(value);
                    }
                    Count++;
                }
                else
                {
                    if (pos != -1)
                    {
                        var mdsl = ListSort[pos];
                        mdsl.Add(value);
                        if (mdsl.GetList().Count > 500)
                        {
                            Split(pos);
                        }
                    }
                    else
                    {
                        md += 1;

                        var mdsl = ListSort[md];
                        if (mdsl == null)
                        {
                            var sl = new SortedList<T>();
                            ListSort[md] = sl;
                            sl.Tag = value;
                            sl.Add(value);
                            Count++;
                        }
                        else
                        {
                            mdsl.Add(value);
                            if (mdsl.GetList().Count > 500)
                            {
                                Split(md);
                            }
                        }
                    }
                }
            }

            if (Count == ListSort.Length)
            {
                Expend();
            }
        }

        private void Expend()
        {
            int largerSize = 0;
            if (Count < CapLargerCount)
            {
                largerSize = Count * 2;
            }
            else
            {
                largerSize = (int)(Count * 1.1);
            }


            var newListSort = new SortedList<T>[largerSize];
            int i = 0;
            foreach (var item in ListSort)
            {
                newListSort[i++] = item;
            }
            ListSort = newListSort;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mid">当结果为-1时，mid返回-1:小于最小的 等于长度-1：大于最大的 中间：位置应在mid之后</param>
        /// <returns>-1没找到</returns>
        private int Find(T key, ref int mid)
        {
            mid = 0;

            if (Count == 0)
            {
                mid = -1;
                return -1;
            }

            if (Count == 1)
            {
                var compare = ListSort[0].Tag.CompareTo(key);
                if (compare > 0)
                {
                    mid = -1;
                }
                return compare == 0 ? 0 : -1;
            }

            int lt = 0, rt = Count - 1;
            int comparetoLt = ListSort[lt].Tag.CompareTo(key);

            if (comparetoLt == 0)
            {
                return lt;
            }
            int comparetoRt = ListSort[rt].Tag.CompareTo(key);
            if (comparetoRt == 0)
            {
                return rt;
            }

            if (comparetoLt > 0)
            {
                mid = -1;
                return -1;
            }

            if (comparetoRt < 0)
            {
                mid = rt;
                return -1;
            }

            int md = (lt + (rt - lt) / 2);
            while (lt < md && md < rt)
            {
                var compareTomd = ListSort[md].Tag.CompareTo(key);
                if (compareTomd == 0)
                {
                    return md;
                }
                else if (compareTomd > 0)
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

        public T Find(T key)
        {
            if (Count == 0)
            {
                return default(T);
            }
            int mid = 0;
            int pos = Find(key, ref mid);
            if (pos != -1)
            {
                return ListSort[pos].Tag;
            }
            else
            {
                var sl = ListSort[mid + 1];
                if (sl != null)
                {
                    pos = sl.Find(key, ref mid);
                    if (pos != -1)
                    {
                        return sl.GetList()[pos];
                    }
                }
                return default(T);
            }
        }

        public IEnumerable<T> FindAll(T key)
        {
            if (Count > 0)
            {
                int mid = 0;
                int pos = Find(key, ref mid);
                var posper = pos;
                if (pos != -1)
                {
                    var compare = 0;
                    while (posper >= 0)
                    {
                        compare = ListSort[posper].Tag.CompareTo(key);
                        if (compare < 0 || posper == 0)
                        {
                            break;
                        }
                        posper--;
                    }
                    if (compare != 0)
                    {
                        posper++;
                    }
                }
                else
                {
                    //var sl = ListSort[mid + 1];
                    posper = mid + 1;
                }

                for (; posper <= this.Count - 1; posper++)
                {
                    var sl = ListSort[posper];

                    foreach (var r in sl.FindAll(key))
                    {
                        yield return r;
                    }

                    if (sl.Tag.CompareTo(key) > 0 || posper == this.Count - 1)
                    {
                        break;
                    }
                }
            }
        }

        public IEnumerable<T> Scan(T start,T end)
        {
            if (Count > 0)
            {
                int mid = 0;
                int pos = Find(start, ref mid);
                var posper = pos;
                if (pos != -1)
                {
                    var compare = 0;
                    while (posper >= 0)
                    {
                        compare = ListSort[posper].Tag.CompareTo(start);
                        if (compare < 0 || posper == 0)
                        {
                            break;
                        }
                        posper--;
                    }
                    if (compare != 0)
                    {
                        posper++;
                    }
                }
                else
                {
                    //var sl = ListSort[mid + 1];
                    posper = mid + 1;
                    if (posper == Count)
                    {
                        yield break;
                    }
                }

                int mid2 = 0;
                int pos2 = Find(end, ref mid2);
                var posper2 = pos2;
                if (pos2 != -1)
                {
                    var compare = 0;
                    while (posper2 <= this.Count - 1)
                    {
                        compare = ListSort[posper2].Tag.CompareTo(end);
                        if (compare > 0 || posper2 == this.Count - 1)
                        {
                            break;
                        }
                        posper2++;
                    }
                    //if (compare != 0)
                    //{
                    //    posper2--;
                    //}
                }
                else
                {
                    //var sl = ListSort[mid + 1];
                    posper2 = mid2 + 1;
                }


                for (int i=posper; i <= posper2; i++)
                {
                    var sl = ListSort[i];
                    if (sl == null)
                    {
                        break;
                    }

                    foreach (var r in sl.GetList())
                    {
                        if (i == posper || i == posper2)
                        {
                            if (r.CompareTo(start) >= 0 && r.CompareTo(end) <= 0)
                            {
                                yield return r;
                            }
                        }
                        else
                        {
                            yield return r;
                        }
                    }

                    if (sl.Tag.CompareTo(end) > 0 || posper == this.Count - 1)
                    {
                        break;
                    }
                }
            }
        }

        public IEnumerable<T> GetList()
        {
            for (int i = 0; i < Count; i++)
            {
                foreach (var item in this.ListSort[i].GetList())
                {
                    yield return item;
                }
            }
        }

        public int Length()
        {
            int cnt = 0;
            for (int i = 0; i < Count; i++)
            {
                cnt += this.ListSort[i].Length();
            }
            return cnt;
        }
    }
}
