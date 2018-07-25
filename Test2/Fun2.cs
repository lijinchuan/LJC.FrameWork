using LJC.FrameWork.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    class Fun2 : IFun
    {
        public static void TestInt()
        {
            SortedList<int> sl = new SortedList<int>();
            sl.Add(2);
            sl.Add(4);
            sl.Add(-2);
            sl.Add(78);
            sl.Add(2);
            sl.Add(7);
            sl.Add(8);
            Console.WriteLine("---------------");
            foreach (var l in sl.GetList())
            {
                Console.WriteLine(l);
            }
        }

        public static void TestString()
        {
            List<string> list = new List<string>();
            var first = Guid.NewGuid().ToString("N");
            for (int i = 0; i < 2000000; i++)
            {
                list.Add(Guid.NewGuid().ToString("N"));
                //list.Add(first+i);
            }

            Console.WriteLine("开始排序112");
            DateTime now = DateTime.Now;

            SortArrayList<string> sl = new SortArrayList<string>();
            int j = 0;
            foreach (var ite in list)
            {
                try
                {
                    sl.Add(ite);
                    j++;
                }
                catch (Exception ce)
                {
                    Console.WriteLine(ce.ToString());
                }
            }

            var ms = DateTime.Now.Subtract(now).TotalMilliseconds;
            var orderlist = sl.GetList().ToList();
            Console.WriteLine("排序完成，用时:" + ms + "ms," + orderlist.Count());

            now = DateTime.Now;
            list.Sort();
            Console.WriteLine("原生排序完成:" + DateTime.Now.Subtract(now).TotalMilliseconds + "ms");

            bool issame = true;
            if (list.Count() == orderlist.Count())
            {
                int i = 0;
                foreach (var item in list)
                {
                    if (item != orderlist[i++])
                    {
                        issame = false;
                        Console.WriteLine("不同:" + i);
                        break;
                    }
                }
            }
            else
            {
                issame = false;

            }
            if (issame)
            {
                Console.WriteLine("排序相同");
            }
            else
            {
                Console.WriteLine("排序不同");
            }
        }

        private void TestFind()
        {
            List<string> list = new List<string>();
            var first = Guid.NewGuid().ToString("N");
            for (int i = 0; i < 2000000; i++)
            {
                list.Add(Guid.NewGuid().ToString("N"));
                //list.Add(first+i);
            }

            Console.WriteLine("开始排序112");
            DateTime now = DateTime.Now;

            SortArrayList<string> sl = new SortArrayList<string>();
            int j = 0;
            foreach (var ite in list)
            {
                try
                {
                    sl.Add(ite);
                    j++;
                }
                catch (Exception ce)
                {
                    Console.WriteLine(ce.ToString());
                }
            }

            var ms = DateTime.Now.Subtract(now).TotalMilliseconds;
            var orderlist = sl.GetList().ToList();
            Console.WriteLine("排序完成，用时:" + ms + "ms," + orderlist.Count());

            list.Add(Guid.NewGuid().ToString());
            Console.WriteLine("测试查找");
            now = DateTime.Now;
            int fund = 0;
            foreach (var item in list)
            {
                try
                {
                    var s = sl.Find(item);
                    if (s == null)
                    {
                        Console.WriteLine("查找失败:" + item);
                    }
                    else
                    {
                        fund++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("查找出错：" + item + ":" + ex.ToString());
                    break;
                }
            }
            Console.WriteLine("查找完成，查找到:" + fund + "个,用时:" + DateTime.Now.Subtract(now).TotalMilliseconds + "ms");
        }

        public void Start()
        {
            //TestString();
            TestFind();
        }
    }
}
