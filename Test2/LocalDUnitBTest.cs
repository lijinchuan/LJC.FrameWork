using LJC.FrameWork.Comm;
using LJC.FrameWork.Data.EntityDataBase;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test2
{
    public class LocalDUnitBTest : IFun
    {
        public class PersonInfo
        {
            public string ID
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public int Age
            {
                get;
                set;
            }

            public int Sex
            {
                get;
                set;
            }

            public string Addr
            {
                get;
                set;
            }
        }

        public static void TestScan(List<PersonInfo> memlist, int psstart = 1, int psend = 100)
        {
            int pi = 1, ps = psstart;
            memlist = memlist.OrderBy(p => p.Name).ToList();
            while (true)
            {
                pi = 1;
                while (true)
                {
                    ProcessTraceUtil.StartTrace();
                    ProcessTraceUtil.TraceMem("第" + pi + "页");
                    long total = 0;
                    var pagelist = memlist.Skip((pi - 1) * ps).Take(ps).ToList();
                    ProcessTraceUtil.TraceMem("内存排序完成");
                    var pagelist2 = BigEntityTableEngine.LocalEngine.Scan<PersonInfo>("PersonInfo", "Name_1", new object[] { string.Empty }, new object[] { "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" },
                        pi, ps, ref total);
                    ProcessTraceUtil.TraceMem("本地库排序完成");
                    if (total != memlist.Count)
                    {
                        throw new Exception("scan返回的总数错误");
                    }

                    if (pagelist.Count != pagelist2.Count)
                    {
                        throw new Exception("测试分页失败,第" + pi + "页数据不同，页面大小:" + ps);
                    }

                    for (int i = 0; i < pagelist.Count; i++)
                    {
                        if (pagelist[i].ID != pagelist2[i].ID)
                        {
                            throw new Exception("测试分页失败,第" + pi + "页数据不同，页面大小:" + ps);
                        }
                    }

                    if (pagelist.Count == 0)
                    {
                        break;
                    }

                    pi++;

                    ProcessTraceUtil.TraceMem("比较完成");

                    var log = ProcessTraceUtil.PrintTrace();
                    Console.WriteLine(pi + "->" + log);
                    if (!string.IsNullOrWhiteSpace(log))
                    {
                        LogHelper.Instance.Debug(log);
                    }
                }
                ps++;

                if (ps > psend)
                {
                    break;
                }
            }

            Console.WriteLine("TestScan测试完成，所有用例都通过");
        }

        /// <summary>
        /// 插入10条数据，并且扫描中间的数据
        /// </summary>
        /// <param name="memlist"></param>
        public static void TestScan2(List<PersonInfo> memlist)
        {
            int count = 0;
            while (count++ < 10)
            {
                var newguid = Guid.NewGuid().ToString();
                var newpersoninfo = new PersonInfo
                {
                    ID = newguid,
                    Age = new Random(newguid.GetHashCode()).Next(1, 121),
                    Addr = "addr" + newguid,
                    Name = "n" + newguid,
                    Sex = new Random(newguid.GetHashCode()).Next(0, 2)
                };

                memlist.Add(newpersoninfo);
                BigEntityTableEngine.LocalEngine.Insert<PersonInfo>("PersonInfo", newpersoninfo);

                var tempmemelist = memlist.OrderBy(p => p.Name).Where(p => p.Name.CompareTo(newpersoninfo.Name) <= 0).ToList();
                int pi = 1, ps = 999;
                while (true)
                {
                    pi = 1;
                    while (true)
                    {
                        long total = 0;
                        var pagelist = tempmemelist.Skip((pi - 1) * ps).Take(ps).ToList();

                        var pagelist2 = BigEntityTableEngine.LocalEngine.Scan<PersonInfo>("PersonInfo", "Name_1", new object[] { string.Empty }, new object[] { newpersoninfo.Name },
                            pi, ps, ref total);

                        if (total != tempmemelist.Count)
                        {
                            throw new Exception("scan返回的总数错误");
                        }

                        if (pagelist.Count != pagelist2.Count)
                        {
                            throw new Exception("测试分页失败,第" + pi + "页数据不同，页面大小:" + ps);
                        }

                        for (int i = 0; i < pagelist.Count; i++)
                        {
                            if (pagelist[i].ID != pagelist2[i].ID)
                            {
                                LogHelper.Instance.Debug("TestScan3不通过，正确列表:" + JsonUtil<object>.Serialize(pagelist) + "，错误为:" + JsonUtil<object>.Serialize(pagelist2));

                                throw new Exception("测试分页失败,第" + pi + "页数据不同，页面大小:" + ps);
                            }
                        }

                        if (pagelist.Count == 0)
                        {
                            break;
                        }

                        pi++;
                    }
                    ps++;

                    if (ps > 13)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine("TestScan2测试完成，所有用例都通过");
        }

        /// <summary>
        /// 插入10条数据，并且扫描中间的数据
        /// </summary>
        /// <param name="memlist"></param>
        public static void TestScan3(List<PersonInfo> memlist)
        {
            int count = 0;
            while (count++ < 10)
            {
                var newguid = Guid.NewGuid().ToString();
                var name = "n" + newguid;

                int pi = 1, ps = 999;
                var tempmemelist = memlist.OrderBy(p => p.Name).Where(p => p.Name.CompareTo(name) <= 0).ToList();
                while (true)
                {
                    pi = 1;
                    while (true)
                    {
                        long total = 0;
                        var pagelist = tempmemelist.Skip((pi - 1) * ps).Take(ps).ToList();

                        var pagelist2 = BigEntityTableEngine.LocalEngine.Scan<PersonInfo>("PersonInfo", "Name_1", new object[] { string.Empty }, new object[] { name },
                            pi, ps, ref total);

                        if (total != tempmemelist.Count)
                        {
                            throw new Exception("scan返回的总数错误");
                        }

                        if (pagelist.Count != pagelist2.Count)
                        {
                            throw new Exception("测试分页失败,第" + pi + "页数据不同，页面大小:" + ps);
                        }

                        for (int i = 0; i < pagelist.Count; i++)
                        {
                            if (pagelist[i].ID != pagelist2[i].ID)
                            {
                                LogHelper.Instance.Debug("TestScan3不通过，正确列表:" + JsonUtil<object>.Serialize(pagelist) + "，错误为:" + JsonUtil<object>.Serialize(pagelist2));
                                throw new Exception("测试分页失败,第" + pi + "页数据不同，页面大小:" + ps + ",正确为:" + pagelist[i] + ",错误为:" + pagelist2[i]);
                            }
                        }

                        if (pagelist.Count == 0)
                        {
                            break;
                        }

                        pi++;
                    }
                    ps++;

                    if (ps > 13)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine("TestScan3测试完成，所有用例都通过");
        }

        public static void TestDel(ref List<PersonInfo> memlist)
        {
            var delcount = 100;
            if (memlist.Count < 100)
            {
                throw new Exception("删除测试数据过少");
            }

            Console.WriteLine("开始删除");

            List<PersonInfo> dellist = new List<PersonInfo>();
            for (int i = 0; i < delcount; i++)
            {
                var cnt = memlist.Count();
                var idx = new Random(Guid.NewGuid().GetHashCode()).Next(0, cnt);
                dellist.Add(memlist[idx]);
                while (true)
                {
                    try
                    {
                        BigEntityTableEngine.LocalEngine.Delete<PersonInfo>("PersonInfo", memlist[idx].ID);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("合并索引") > -1)
                        {
                            Thread.Sleep(10);
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
                memlist.RemoveAt(idx);
            }

            foreach (var item in dellist)
            {
                var findresult = BigEntityTableEngine.LocalEngine.Find<PersonInfo>("PersonInfo", item.ID);
                if (findresult != null)
                {
                    throw new Exception("删除失败:" + findresult.ID);
                }
            }

            TestScan(memlist, 999, 999);

            Console.WriteLine("删除测试完成，用例都通过");
        }

        public static void TestUpdate(ref List<PersonInfo> memlist)
        {

            //Thread.Sleep(10000);
            Console.WriteLine("开始修改");
            var updatecount = 1000;

            List<PersonInfo> uplist = new List<PersonInfo>();
            for (int i = 0; i < updatecount; i++)
            {
                var guid = Guid.NewGuid();
                var idx = new Random(guid.GetHashCode()).Next(0, memlist.Count);
                var item = memlist[idx];
                item.Age = new Random(guid.GetHashCode()).Next(1, 120);
                var boodec = item.Addr.Length > 3 && new Random(guid.GetHashCode()).Next(0, 10) < 6;
                if (boodec)
                {
                    item.Addr = item.Addr.Substring(0, item.Addr.Length - 2);
                }
                else
                {
                    item.Addr += guid.ToString();
                }
                var booupdatename = new Random(guid.GetHashCode()).Next(0, 100) < 50;
                if (booupdatename)
                {
                    item.Name = "n" + guid.ToString();
                }
                var exitem = uplist.Find(p => p.ID == item.ID);
                if (exitem != null)
                {
                    uplist.Remove(exitem);
                }
                uplist.Add(item);

                while (true)
                {
                    try
                    {
                        BigEntityTableEngine.LocalEngine.Update<PersonInfo>("PersonInfo", item);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("合并索引") > -1)
                        {
                            Thread.Sleep(10);
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }

            foreach (var item in uplist)
            {
                var finditem = BigEntityTableEngine.LocalEngine.Find<PersonInfo>("PersonInfo", item.ID);
                if (finditem == null || finditem.Name != item.Name || finditem.Sex != item.Sex || finditem.Addr != item.Addr || finditem.Age != item.Age)
                {
                    throw new Exception("修改后数据不一致:" + item.ID);
                }
            }

            Console.WriteLine("检查列表数据");
            TestScan(memlist, 999, 999);
        }

        public static void Test()
        {
            //添加9999条数据
            var insertcount = 9999;
            List<PersonInfo> memlist = new List<PersonInfo>();
            for (int i = 0; i < insertcount; i++)
            {
                var id = Guid.NewGuid().ToString();
                memlist.Add(new PersonInfo
                {
                    Name = "n" + id,
                    Addr = "addr",
                    Age = new Random(id.GetHashCode()).Next(1, 101),
                    ID = id,
                    Sex = new Random(id.GetHashCode()).Next(0, 2)
                });
            }

            if (System.IO.Directory.Exists("localdb"))
            {
                System.IO.Directory.Delete("localdb", true);
            }

            BigEntityTableEngine.LocalEngine.CreateTable("PersonInfo", "ID", typeof(PersonInfo), new IndexInfo[]
            {
                new IndexInfo
                {
                  IndexName="Name_1",
                  Indexs=new IndexItem[]{
                      new IndexItem{
                          Direction=1,
                          Field="Name",
                          FieldType=EntityType.STRING
                      }
                  }
                }
                ,new IndexInfo
                {
                    IndexName="Age-1",
                    Indexs=new IndexItem[]
                    {
                        new IndexItem
                        {
                            Direction=-1,
                            Field="Age",
                            FieldType=EntityType.INT32
                        }
                    }
                }
            });

            BigEntityTableEngine.LocalEngine.InsertBatch<PersonInfo>("PersonInfo", memlist);
            var insertcount2 = BigEntityTableEngine.LocalEngine.Count("PersonInfo");
            if (insertcount != insertcount2)
            {
                throw new Exception("插入数据后统计数据测试不通过");
            }

            ////TestScan(memlist);

            //再写入1万条
            var insertmore = 1000;
            for (int k = 0; k < 330; k++)
            {

                var templist = new List<PersonInfo>();
                for (int i = 0; i < insertmore; i++)
                {
                    var id = Guid.NewGuid().ToString();
                    templist.Add(new PersonInfo
                    {
                        Name = "n" + id,
                        Addr = "addr",
                        Age = new Random(id.GetHashCode()).Next(1, 101),
                        ID = id,
                        Sex = new Random(id.GetHashCode()).Next(0, 2)
                    });
                }

                ProcessTraceUtil.StartTrace();
                DateTime now = DateTime.Now;

                BigEntityTableEngine.LocalEngine.InsertBatch<PersonInfo>("PersonInfo", templist);
                LogHelper.Instance.Info(ProcessTraceUtil.PrintTrace());
                Console.WriteLine("写入万条数据完成:" + DateTime.Now.Subtract(now).TotalMilliseconds + "ms");
                memlist.AddRange(templist);
                //Thread.Sleep(1000);
            }

            Console.WriteLine("再次测试scan");
            //TestScan(memlist,999,999);
            TestAgeCount(memlist);
            TestTalbeCount(memlist);
            //TestScan2(memlist);
            //TestScan3(memlist);

            //删除
            //Console.WriteLine("测试删除");
            TestDel(ref memlist);
            //TestScan2(memlist);
            //TestScan3(memlist);
            TestAgeCount(memlist);
            TestTalbeCount(memlist);

            List<PersonInfo> newpersonlist = new List<PersonInfo>();
            for (int i = 0; i < 10000; i++)
            {
                var id = Guid.NewGuid().ToString();
                newpersonlist.Add(new PersonInfo
                {
                    Name = "n" + id,
                    Addr = "addr",
                    Age = new Random(id.GetHashCode()).Next(1, 101),
                    ID = id,
                    Sex = new Random(id.GetHashCode()).Next(0, 2)
                });
            }
            memlist.AddRange(newpersonlist);
            BigEntityTableEngine.LocalEngine.InsertBatch<PersonInfo>("PersonInfo", newpersonlist);
            Console.WriteLine("插入一批数据");
            //TestScan(memlist, 999, 999);
            //TestScan2(memlist);
            //TestScan3(memlist);
            TestAgeCount(memlist);
            TestTalbeCount(memlist);

            Console.WriteLine("删除后新增数据测试成功");

            //测试更新
            TestUpdate(ref memlist);

            //TestScan2(memlist);
            //TestScan3(memlist);
            TestAgeCount(memlist);
            TestTalbeCount(memlist);

            BigEntityTableEngine.LocalEngine.UnLoadTable("PersonInfo");
            //TestScan2(memlist);
            //TestScan3(memlist);
            TestAgeCount(memlist);
            TestTalbeCount(memlist);
        }

        public static void TestAgeCount(List<PersonInfo> memlist)
        {
            var gpage = memlist.GroupBy(p => p.Age).ToDictionary(p => p.Key, q => q.Count());

            foreach (var gp in gpage)
            {
                var st = DateTime.Now;
                var count = BigEntityTableEngine.LocalEngine.Count("PersonInfo", "Age-1", new object[] { gp.Key });
                if (count != gp.Value)
                {
                    throw new Exception("年龄统计数量不对:" + gp.Key);
                }
                Console.WriteLine("统计年龄数:" + gp.Key + "->" + gp.Value + ",用户:" + (DateTime.Now.Subtract(st).TotalMilliseconds) + "ms");
            }
            Console.WriteLine("TestAgeCount 测试通过");
        }

        public static void TestTalbeCount(List<PersonInfo> memlist)
        {
            DateTime st = DateTime.Now;
            var totalcount = BigEntityTableEngine.LocalEngine.Count("PersonInfo");
            if (totalcount != memlist.Count)
            {
                throw new Exception("表总记录数错误:"+totalcount+"，实际数量:"+memlist.Count);
            }

            Console.WriteLine("总记录数测试通过,用时:" + (DateTime.Now.Subtract(st).TotalMilliseconds + "ms"));
        }

        public void TestFullLoad()
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("test find begin");
                DateTime now = DateTime.Now;
                var listall = BigEntityTableEngine.LocalEngine.Find<PersonInfo>("PersonInfo", p => p.Age > 90).ToList();
                var end = DateTime.Now;
                Console.WriteLine("查找完成，用时:" + end.Subtract(now).TotalMilliseconds + "ms,数据量:" + listall.Count + "条");
            }
        }

        public void Start()
        {
            Test();
        }
    }
}
