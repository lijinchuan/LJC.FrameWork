using LJC.FrameWork.Comm;
using LJC.FrameWork.Data.EntityDataBase;
using LJC.FrameWork.Data.QuickDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    public class Fun1:IFun
    {
        static void TestBigLocaldb()
        {
            int insertcount = 3210053;
            //Man 
            BigEntityTableEngine.LocalEngine.CreateTable("Man", "Name", typeof(Man));
            //EntityTableEngine.LocalEngine.CreateTable("Man", "Name", typeof(Man));
            DateTime time = DateTime.Now;
            List<Man> list = new List<Man>();
            for (int i = 0; i < insertcount; i++)
            {

                var man = new Man
                {
                    Addr = "addr" + Guid.NewGuid().ToString(),
                    IDCard = "id" + i,
                    Name = "name" + i,
                    Sex = new Random(Guid.NewGuid().GetHashCode()).Next(2)
                };
                list.Add(man);
                if (list.Count > 10000)
                {
                    BigEntityTableEngine.LocalEngine.InsertBatch("Man", list);
                    list.Clear();
                }
            }
            if (list.Count > 0)
            {
                BigEntityTableEngine.LocalEngine.InsertBatch("Man", list);
                list.Clear();
            }
            Console.WriteLine("写入完成:" + DateTime.Now.Subtract(time).TotalMilliseconds);
            // Console.Read();
        }

        static void TestBigLocaldbFindBatch()
        {
            //Console.WriteLine("cnt:"+BigEntityTableEngine.LocalEngine.Count("Man"));
            //Console.Read();
            int insertcount = 1210001;
            Console.WriteLine("要先统计所有的数据吗？Y/N");
            var input = Console.ReadLine();
            if (input.Equals("Y"))
            {
                //int insertcount = 10000000;
                int ccount = 0;
                var start = DateTime.Now;
                foreach (var item in BigEntityTableEngine.LocalEngine.List<Man>("Man", 1, insertcount))
                {
                    //Console.WriteLine(item.Name);
                    ccount++;
                }
                Console.WriteLine("ccount:" + ccount + ",用时:" + (DateTime.Now.Subtract(start).TotalSeconds) + "秒");
            }

            for (int c = 0; c < 1; c++)
            {
                var time = DateTime.Now;
                int cnt = 0;
                int readcnt = 0;

                var findkeylist = new List<string>();
                for (int i = 0; i < insertcount; i++)
                {
                    var key = "name" + i;

                    findkeylist.Add(key);

                    if (findkeylist.Count >= 10000)
                    {
                        DateTime timenow = DateTime.Now;
                        var findvallist = BigEntityTableEngine.LocalEngine.FindBatch<Man>("Man", findkeylist).ToList();

                        int nullcount = findvallist.Where(p => p == null).Count();
                        cnt += nullcount;
                        if (nullcount > 0)
                        {
                            for (int ii = 0; ii < findvallist.Count; ii++)
                            {
                                if (findvallist[ii] == null)
                                {
                                    Console.WriteLine("key->" + findkeylist[ii]);
                                }
                            }
                            Console.Read();
                        }
                        readcnt += findkeylist.Count;
                        Console.WriteLine(cnt + "用时:" + DateTime.Now.Subtract(timenow).TotalMilliseconds + "，未找到记录数:" + cnt);
                        findkeylist.Clear();
                    }

                }

                if (findkeylist.Count > 0)
                {
                    DateTime timenow = DateTime.Now;
                    var findvallist = BigEntityTableEngine.LocalEngine.FindBatch<Man>("Man", findkeylist).ToList();
                    int nullcount = findvallist.Where(p => p == null).Count();
                    cnt += nullcount;
                    if (nullcount > 0)
                    {
                        for (int i = 0; i < findvallist.Count; i++)
                        {
                            if (findvallist[i] == null)
                            {
                                Console.WriteLine("key->" + findkeylist[i]);
                            }
                        }
                        Console.Read();
                    }
                    readcnt += findkeylist.Count;
                    Console.WriteLine(cnt + "用时:" + DateTime.Now.Subtract(timenow).TotalMilliseconds + "，未找到记录数:" + cnt);
                    findkeylist.Clear();
                }
                Console.WriteLine("读取完成:" + readcnt + "条,用时:" + DateTime.Now.Subtract(time).TotalMilliseconds);

            }
            Console.Read();
        }

        public void TestDel()
        {
            Console.WriteLine("删除要删除的主键");
            string key = Console.ReadLine();
            var item = BigEntityTableEngine.LocalEngine.Find<Man>("Man", key);
            if (item == null)
            {
                Console.WriteLine("找不到数据");
            }
            else
            {
                Console.WriteLine("查找到数据:" + JsonUtil<object>.Serialize(item));
                if (BigEntityTableEngine.LocalEngine.Delete<Man>("Man", key))
                {
                    Console.WriteLine("调用删除方法成功");

                    item = BigEntityTableEngine.LocalEngine.Find<Man>("Man", key);
                    if (item == null)
                    {
                        Console.WriteLine("查询不存在了，建议整理索引后再检查");
                    }
                    else
                    {
                        Console.WriteLine("仍然存在:" + JsonUtil<object>.Serialize(item));
                    }
                }
                else
                {
                    Console.WriteLine("调用删除方法不成功");
                }
            }
        }

        public void TestFind()
        {
            Console.WriteLine("输入key");
            var key = Console.ReadLine();

            var item = BigEntityTableEngine.LocalEngine.Find<Man>("Man", key);
            if (item == null)
            {
                Console.WriteLine("查询不存在");
            }
            else
            {
                Console.WriteLine("存在:" + JsonUtil<object>.Serialize(item));
            }
        }

        public void TestUpdate()
        {
            Console.WriteLine("输入key");
            var key = Console.ReadLine();

            var item = BigEntityTableEngine.LocalEngine.Find<Man>("Man",key);
            if (item == null)
            {
                Console.WriteLine("查询不存在");
            }
            else
            {
                Console.WriteLine("查找到数据：" + JsonUtil<object>.Serialize(item));
                Console.WriteLine("1-修改性别 2-修改身份证 3-修改地址");
                var typestr = Console.ReadLine();
                switch (typestr)
                {
                    case "1":
                    case "2":
                    case "3":
                        {
                            Console.WriteLine("输入" + new[] { "性别", "身份证", "地址" }[int.Parse(typestr) - 1] + "新值");
                            var val = Console.ReadLine();
                            switch (typestr)
                            {
                                case "1":
                                    item.Sex = int.Parse(val);
                                    break;
                                case "2":
                                    {
                                        item.IDCard = val;
                                        break;
                                    }
                                case "3":
                                    {
                                        item.Addr = val;
                                        break;
                                    }
                            }
                            if(BigEntityTableEngine.LocalEngine.Update<Man>("Man", item))
                            {
                                Console.WriteLine("修改成功");

                                var newman = BigEntityTableEngine.LocalEngine.Find<Man>("Man", item.Name);
                                Console.WriteLine("修改后:"+JsonUtil<object>.Serialize(newman));
                            }
                            else
                            {
                                Console.WriteLine("修改失败");
                            }
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("选择错误");
                            break;
                        }
                }
            }
        }

        public static void testIndex()
        {
            //foreach (var item in BigEntityTableEngine.LocalEngine.Find<Man>("Man", "IDCard", "id15590"))
            //{
            //    Console.WriteLine(JsonUtil<object>.Serialize(item));
            //}

            int count = 0;
            foreach (var item in BigEntityTableEngine.LocalEngine.Find<Man>("Man", "Sex", new[] { "0" }))
            {
                //Console.WriteLine(item.Name+" "+item.Sex);
                count++;
            }

            Console.WriteLine("sex=0," + count + "条");

            count = 0;
            foreach (var item in BigEntityTableEngine.LocalEngine.Find<Man>("Man", "Sex",new[] { "1" }))
            {
                //Console.WriteLine(item.Name + " " + item.Sex);
                count++;
            }

            Console.WriteLine("sex=1," + count + "条");
        }

        public static void ContinueWrite()
        {
            Console.WriteLine("开始编号");
            long start = long.Parse(Console.ReadLine());
            Console.WriteLine("结束编号");
            long end = long.Parse(Console.ReadLine());
            List<Man> list = new List<Man>();
            for (; start < end; start++)
            {
                var man = new Man
                {
                    Addr = "addr" + Guid.NewGuid().ToString(),
                    IDCard = "id" + start,
                    Name = "name" + start,
                    Sex = new Random(Guid.NewGuid().GetHashCode()).Next(2)
                };
                list.Add(man);

            }

            DateTime now = DateTime.Now;
            foreach (var m in list)
            {
                BigEntityTableEngine.LocalEngine.Insert("Man", m);
            }
            Console.WriteLine("写入完成，用时" + (DateTime.Now.Subtract(now).TotalMilliseconds) + "ms");
        }

        private void TestKeyWord()
        {
            BigEntityTableEngine.LocalEngine.CreateTable("NewsKeysEntity", "NewsKeysID", typeof(NewsKeysEntity),new IndexInfo[]{
                new IndexInfo
                {
                    IndexName="NewsID",
                    Indexs=new IndexItem[]
                    {
                        new IndexItem
                        {
                            Direction=1,
                            Field="NewsID",
                            FieldType=LJC.FrameWork.EntityBuf.EntityType.INT64
                        }
                    }
                },new IndexInfo
                {
                    IndexName="Keys",
                    Indexs=new IndexItem[]
                    {
                        new IndexItem
                        {
                            Direction=1,
                            Field="Keys",
                            FieldType=LJC.FrameWork.EntityBuf.EntityType.STRING
                        }
                    }
                }
                });
            long nid = 0;
            List<NewsKeysEntity> list = null;
            Console.WriteLine("开始读数据");
            while ((list = DataContextMoudelFactory<NewsKeysEntity>.GetDataContext("ConndbDB$CjzfDB").WhereBiger(p => p.NewsKeysID, nid).Top(10000).OrderBy(p => p.NewsKeysID).ExecuteList()).Count > 0)
            {
                nid = list.Last().NewsKeysID;
                Console.WriteLine("nid:" + nid);
                DateTime now = DateTime.Now;
                BigEntityTableEngine.LocalEngine.InsertBatch("NewsKeysEntity", list);
                Console.WriteLine("写入完成:"+(DateTime.Now.Subtract(now).TotalMilliseconds+"ms"));
            }
            Console.WriteLine("读数据完成");

            
        }

        private void TestFindKeyWord()
        {
            Console.WriteLine("输入key");
            string key = Console.ReadLine();
            var now = DateTime.Now;
            ProcessTraceUtil.StartTrace();
            ProcessTraceUtil.Trace("start find");
            var count = BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity", "Keys",new[] { key }).Count();

            var detail = ProcessTraceUtil.PrintTrace();
            Console.WriteLine(detail);
            LJC.FrameWork.LogManager.LogHelper.Instance.Debug(detail);
        }

        private void TestDelKeyWord()
        {
            try
            {
                Console.WriteLine("要删除的NewsKeysID");
                var key = Console.ReadLine();
                var item = BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity", key);
                if (item == null)
                {
                    Console.WriteLine("删除失败，数据不存在");
                    return;
                }
                Console.WriteLine("数据为:" + JsonUtil<object>.Serialize(item));

                Console.WriteLine("是否要删除?Y/N");
                var yesno = Console.ReadLine();
                if (yesno != "Y")
                {
                    Console.WriteLine("测试取消");
                    return;
                }

                DateTime now = DateTime.Now;

                if (BigEntityTableEngine.LocalEngine.Delete<NewsKeysEntity>("NewsKeysEntity", key))
                {
                    Console.WriteLine("删除成功");
                }
                else
                {
                    Console.WriteLine("删除失败");
                }
                Console.WriteLine("删除完成用时:" + DateTime.Now.Subtract(now).TotalMilliseconds + "ms");

                var newitem = BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity", key);
                if (newitem != null)
                {
                    Console.WriteLine("删除失败，数据依然存在:" + JsonUtil<object>.Serialize(newitem));
                    return;
                }

                foreach (var it in BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity", "NewsID",new object[] { item.NewsID }))
                {
                    if (it.NewsKeysID == item.NewsKeysID)
                    {
                        Console.WriteLine("索引仍存在:NewsID");
                    }
                }

                foreach (var it in BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity", "Keys",new object[] { item.Keys }))
                {
                    if (it.NewsKeysID == item.NewsKeysID)
                    {
                        Console.WriteLine("索引仍存在:Keys");
                    }
                }

                Console.WriteLine("测试完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void TestUpdateKewWord()
        {
            Console.WriteLine("输入key");
            string key = Console.ReadLine();
            NewsKeysEntity item = BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity", key);
            if (item == null)
            {
                Console.WriteLine("数据不存在");
                return;
            }
            string oldkey = item.Keys;
            Console.WriteLine("查找到了数据:" + JsonUtil<object>.Serialize(item, false));
            Console.WriteLine("修改内容:1-title 2-Keys");
            string innercmd = Console.ReadLine();
            if (innercmd != null)
            {
                if (!(innercmd == "1"))
                {
                    if (innercmd == "2")
                    {
                        item.Keys = Console.ReadLine();
                        goto Label_00A5;
                    }
                }
                else
                {
                    item.Title = Console.ReadLine();
                    goto Label_00A5;
                }
            }
            Console.WriteLine("取消操作");
            return;
        Label_00A5:
            try
            {
                BigEntityTableEngine.LocalEngine.Update<NewsKeysEntity>("NewsKeysEntity", item);
                NewsKeysEntity newitem = BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity", key);
                if (newitem == null)
                {
                    Console.WriteLine("数据不存在");
                }
                else
                {
                    Console.WriteLine("查找后的数据:" + JsonUtil<object>.Serialize(newitem, false));
                    foreach (NewsKeysEntity it in BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity", "Keys",new object[] { oldkey }))
                    {
                        if (it.NewsKeysID == item.NewsKeysID)
                        {
                            Console.WriteLine("查索引找到了:" + it.Keys);
                        }
                    }
                    if (item.Keys != oldkey)
                    {
                        bool find2 = false;
                        foreach (NewsKeysEntity it in BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity", "Keys",new object[] { item.Keys }))
                        {
                            if (it.NewsKeysID == item.NewsKeysID)
                            {
                                Console.WriteLine("查索引找到了:" + it.Title);
                                find2 = true;
                            }
                        }
                        if (!find2)
                        {
                            Console.WriteLine("查找索引找不到:" + item.Keys);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("修改失败:" + ex.ToString());
            }
        }

        public void textCount()
        {
            Console.WriteLine("输入key");
            string key = Console.ReadLine();
            var now = DateTime.Now;
            ProcessTraceUtil.StartTrace();
            var total = BigEntityTableEngine.LocalEngine.Count("NewsKeysEntity");
            ProcessTraceUtil.Trace("total:"+total);
            ProcessTraceUtil.Trace("start find");
            var count = BigEntityTableEngine.LocalEngine.Count("NewsKeysEntity", "Keys",new object[] { key });

            Console.WriteLine("完成，用时:" + (DateTime.Now.Subtract(now).TotalMilliseconds + "ms,条数:" + count + "," + ProcessTraceUtil.PrintTrace()));
        }

        public void Fun95()
        {
            BigEntityTableEngine.LocalEngine.CreateTable("NewsKeysEntity", "NewsKeysID", typeof(NewsKeysEntity), new IndexInfo[]{
                new IndexInfo
                {
                    IndexName="NewsID",
                    Indexs=new IndexItem[]
                    {
                        new IndexItem
                        {
                            Direction=1,
                            Field="NewsID",
                            FieldType=LJC.FrameWork.EntityBuf.EntityType.INT64
                        }
                    }
                },new IndexInfo
                {
                    IndexName="Keys",
                    Indexs=new IndexItem[]
                    {
                        new IndexItem
                        {
                            Direction=1,
                            Field="Keys",
                            FieldType=LJC.FrameWork.EntityBuf.EntityType.STRING
                        }
                    }
                }
                });
            DateTime now = DateTime.Now;
            int count = 0;
            List<NewsKeysEntity> list = new List<NewsKeysEntity>();
            foreach (var item in BigEntityTableEngine.LocalEngine.Find<NewsKeysEntity>("NewsKeysEntity_temp", (p) => true))
            {
                count++;
                list.Add(item);

                if (list.Count > 10000)
                {
                    BigEntityTableEngine.LocalEngine.InsertBatch<NewsKeysEntity>("NewsKeysEntity", list);
                    list.Clear();
                }

                //if (count > 101000)
                //{
                //    break;
                //}
            }
            if (list.Count > 0)
            {
                BigEntityTableEngine.LocalEngine.InsertBatch<NewsKeysEntity>("NewsKeysEntity", list);
                list.Clear();
            }

            Console.WriteLine("遍历完成，用时:"+(DateTime.Now.Subtract(now).TotalMilliseconds+"ms,条数:"+count));
        }

        public void Fun96()
        {
            Console.WriteLine("开始");
            var now = DateTime.Now;
            BigEntityTableEngine.LocalEngine.AssertFindKeyMem("NewsKeysEntity");
            Console.WriteLine("结束用时:"+(DateTime.Now.Subtract(now)).TotalMilliseconds+"ms");
        }

        public void Fun97()
        {
            Console.WriteLine("开始");
            var now = DateTime.Now;
            BigEntityTableEngine.LocalEngine.AssertFindEqual<NewsKeysEntity>("NewsKeysEntity");
            Console.WriteLine("结束用时:" + (DateTime.Now.Subtract(now)).TotalMilliseconds + "ms");
        }

        public void Fun98()
        {
            BigEntityTableEngine.LocalEngine.CreateTable("GubaBandResultEntity", "ID", typeof(GubaBandResultEntity), new IndexInfo[]{
                new IndexInfo
                {
                    IndexName="GubaCode_Uid_Recount",
                    Indexs=new IndexItem[]
                    {
                        new IndexItem
                        {
                            Direction=1,
                            Field="GubaCode",
                            FieldType=LJC.FrameWork.EntityBuf.EntityType.STRING
                        },
                         new IndexItem
                        {
                            Direction=1,
                            Field="Uid",
                            FieldType=LJC.FrameWork.EntityBuf.EntityType.STRING
                        },
                         new IndexItem
                        {
                            Direction=-1,
                            Field="Recount",
                            FieldType=LJC.FrameWork.EntityBuf.EntityType.INT32
                        }
                    }
                }
            });
            long nid = 0;
            List<GubaBandResultEntity> list = null;
            Console.WriteLine("开始读数据");
            while ((list = DataContextMoudelFactory<GubaBandResultEntity>.GetDataContext("ConndbDB$GubaData").WhereBiger(p => p.ID, nid).Top(10000).OrderBy(p => p.ID).ExecuteList()).Count > 0)
            {
                nid = list.Last().ID;
                Console.WriteLine("nid:" + nid);
                DateTime now = DateTime.Now;
                BigEntityTableEngine.LocalEngine.InsertBatch("GubaBandResultEntity", list);
                Console.WriteLine("写入完成:" + (DateTime.Now.Subtract(now).TotalMilliseconds + "ms"));
            }
            Console.WriteLine("读数据完成");
        }

        public void Fun99()
        {
            long nid = 0;
            List<GubaBandResultEntity> list = null;
            Console.WriteLine("开始读数据");
            while ((list = DataContextMoudelFactory<GubaBandResultEntity>.GetDataContext("ConndbDB$GubaData").WhereBiger(p => p.ID, nid).Top(10000).OrderBy(p => p.ID).ExecuteList()).Count > 0)
            {
                nid = list.Last().ID;
                int findcount = 0;
                DateTime now = DateTime.Now;
                ProcessTraceUtil.StartTrace();
                foreach (var item in list)
                {
                    
                    var re = BigEntityTableEngine.LocalEngine.Find<GubaBandResultEntity>("GubaBandResultEntity", "GubaCode_Uid_Recount", new object[] { item.GubaCode, item.Uid, item.Recount });
                    if (re.Count() == 0)
                    {
                        Console.WriteLine("找不到数据:" + item.GubaCode + "," + item.Uid + "," + item.Recount);
                        Console.Read();
                    }
                    else
                    {
                        findcount++;
                    }
                }
                LJC.FrameWork.LogManager.LogHelper.Instance.Debug("查找：" + ProcessTraceUtil.PrintTrace());
                Console.WriteLine("找到数:" + findcount + ",用时" + (DateTime.Now.Subtract(now).TotalMilliseconds) + "ms");

            }
            Console.WriteLine("读数据完成");
        }

        public void Fun100()
        {
            long total = 0;
            var list = BigEntityTableEngine.LocalEngine.Scan<GubaBandResultEntity>("GubaBandResultEntity", "GubaCode_Uid_Recount", new object[] { "000001", "", 0 }, new object[] { "000002", "", 0 },266,13,ref total).ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                //sb.Append(JsonUtil<object>.Serialize(item));
                //sb.AppendLine();
                Console.WriteLine(JsonUtil<object>.Serialize(item));
            }
            //LJC.FrameWork.LogManager.LogHelper.Instance.Info(sb.ToString());
            //Console.WriteLine("000001总条数:" + list.Count);
        }

        public void Fun101()
        {
            var cnt = BigEntityTableEngine.LocalEngine.Count("Man");
            Console.WriteLine("man表count:" + cnt);
        }


        public void Start()
        {
            Console.WriteLine(@"选择操作 1-写库 11-继续写库 2-读库 3-整理索引 4-测试删除 5-查找 51-遍历查找key 6-修改 7-scan 8-测试索引 
9-写入关键字 91-查询关键字 92-测试删除关键字 93-测试修改关键字 94-count统计 95-遍历关键字 96-测试关键字 97-测试关键字查找数量 98-写入GubaBandResult测试数据 99- 101-man的count");
            var cmd = Console.ReadLine();

            if (cmd == "1")
            {
                //TestSorteArray();
                //Console.Read();
                //return;
                TestBigLocaldb();
            }
            if (cmd == "11")
            {
                ContinueWrite();
            }
            if (cmd == "2")
            {
                //TestBigLocaldbDel();
                //TestBigLocalUpdate();
                //BigEntityTableEngine.LocalEngine.MergeIndex("Man","Name");
                TestBigLocaldbFindBatch();
            }

            if (cmd == "3")
            {
                BigEntityTableEngine.LocalEngine.MergeIndex("Man", "Name");
            }

            if (cmd == "4")
            {
                TestDel();
            }

            if (cmd == "5")
            {
                TestFind();
            }

            if (cmd == "6")
            {
                TestUpdate();
            }

            if (cmd == "8")
            {
                testIndex();
            }

            if (cmd == "9")
            {
                TestKeyWord();
            }

            if (cmd == "91")
            {
                TestFindKeyWord();
            }

            if (cmd == "92")
            {
                TestDelKeyWord();
            }

            if (cmd == "93")
            {
                this.TestUpdateKewWord();
            }

            if (cmd == "94")
            {
                this.textCount();
            }

            if (cmd == "95")
            {
                Fun95();
            }

            if (cmd == "96")
            {
                Fun96();
            }

            if (cmd == "97")
            {
                Fun97();
            }

            if (cmd == "98")
            {
                Fun98();
            }

            if (cmd == "99")
            {
                Fun99();
            }

            if (cmd == "100")
            {
                Fun100();
            }

            if (cmd == "101")
            {
                Fun101();
            }
        }
    }
}
