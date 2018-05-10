using Ljc.Com.NewsService.Entity;
using LJC.Com.StockService.Contract;
using LJC.FrameWork.Collections;
using LJC.FrameWork.Comm;
using LJC.FrameWork.Comm.Coroutine;
using LJC.FrameWork.Comm.TextReaderWriter;
using LJC.FrameWork.Data.EntityDataBase;
using LJC.FrameWork.Data.Mongo;
using LJC.FrameWork.SOA;
using LJC.FrameWork.SocketApplication;
using LJC.FrameWork.SocketApplication.SocketEasyUDP.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Test2
{
    class Program
    {
        static void TryRead()
        {
            //string filename = @"E:\Work\learn\Git\LJC.FrameWork\Test\bin\Debug\testrwobjex.bin";
            string filename = @"E:\Work\learn\Git\LJC.FrameWork\Test\bin\Release\testrwobj.bin";
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int i = 0;
            using (LJC.FrameWork.Comm.ObjTextReader reader = LJC.FrameWork.Comm.ObjTextReader.CreateReader(filename))
            {
                Man man = null;
                while ((man=reader.ReadObject<Man>())!=null)
                {
                    i++;
                    //var man = reader.ReadObject<Man>();
                    //Console.WriteLine(man.Name);
                    //Thread.Sleep(1);
                }
            }
            sw.Stop();

            Console.WriteLine("读取完毕，共用时" + sw.Elapsed.TotalSeconds + "秒，共" + i + "条");
        }

        static void TryRead1()
        {
            //string filename = @"E:\Work\learn\Git\LJC.FrameWork\Test\bin\Debug\testrwobjex.bin";
            string filename = @"E:\Work\learn\Git\LJC.FrameWork\Test\bin\Release\testrwobj1.bin";
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int i = 0;
            using (LJC.FrameWork.Comm.ObjTextReader reader = LJC.FrameWork.Comm.ObjTextReader.CreateReader(filename))
            {
                Man man = null;
                //while ((man = reader.ReadObjectFromBack<Man>()) != null)
                {
                    i++;
                    //var man = reader.ReadObject<Man>();
                    //Console.WriteLine(man.Name);
                    //Thread.Sleep(1);
                }
            }
            sw.Stop();

            Console.WriteLine("读取完毕，共用时" + sw.Elapsed.TotalSeconds + "秒，共" + i + "条");
        }

        static void TryRead2()
        {
            string filename = @"E:\Work\learn\Git\LJC.FrameWork\Test\bin\Debug\testrwobjex.bin";
            using (LJC.FrameWork.Comm.ObjTextReader reader = LJC.FrameWork.Comm.ObjTextReader.CreateReader(filename))
            {
                foreach(var item in reader.ReadObjectWating<Man>())
                {
                    Console.WriteLine(item.Name);
                }
            }
        }

        static void TryRead3()
        {
            string filename = @"E:\Work\learn\Git\LJC.FrameWork\Test\bin\Debug\testrwobjex.bin";
            using (LJC.FrameWork.Comm.ObjTextReader reader = LJC.FrameWork.Comm.ObjTextReader.CreateReader(filename))
            {
                //var man = reader.ReadObjectFromBack<Man>();
                //Console.WriteLine(man.Name);
                Man man = null;
                //while((man=reader.ReadObjectFromBack<Man>())!=null)
                {
                    Console.WriteLine(man.Name);
                }
            }
        }

        static void TestLogQueue()
        {
            LocalFileQueue<NewsEntity> tempqueue = new LocalFileQueue<NewsEntity>("testnews2", "newsqueuefile");
            tempqueue.OnProcessQueue += tempqueue_OnProcessQueue;
            string html = new System.Net.WebClient().DownloadString("http://china.huanqiu.com/article/2017-04/10547796.html");
            System.Threading.Tasks.Parallel.For(0, 100, (i) =>
            {
                for (int m = 0; m < 30; m++)
                {
                    tempqueue.Enqueue(new NewsEntity
                    {
                        Title = "title_" + i + "_" + m,
                        Cdate = DateTime.Now,
                        Mdate = DateTime.Now,
                        Class = "news" + i,
                        Clicktime = m,
                        Content = Guid.NewGuid().ToString() + html,
                        Formurl = Guid.NewGuid().ToString()
                    });

                }
            });
        }

        static bool tempqueue_OnProcessQueue(NewsEntity arg)
        {
            int newcount = System.Threading.Interlocked.Increment(ref count);
            if (newcount == 3000)
                Console.WriteLine("条数" + newcount + "," + arg.Title);
            return true;
        }

        static void TestNews()
        {
            var localqueue = new LJC.FrameWork.Comm.TextReaderWriter.LocalFileQueue<Ljc.Com.NewsService.Entity.NewsEntity>("test", @"C:\Users\Administrator\Desktop\queuefile\news1 - 副本.queue");
            localqueue.OnProcessQueue += localqueue_OnProcessQueue;
            localqueue.OnProcessError += localqueue_OnProcessError;

        }

        static void localqueue_OnProcessError(Ljc.Com.NewsService.Entity.NewsEntity arg1, Exception arg2)
        {
            //Console.WriteLine(arg2.ToString());
        }

        static int count = 0;
        static bool localqueue_OnProcessQueue(Ljc.Com.NewsService.Entity.NewsEntity arg)
        {
            int newcount = System.Threading.Interlocked.Increment(ref count);

            if (newcount == 3000)
                Console.WriteLine("条数：" + arg.Title);
            

            return true;
        }

        static void TestEsbservices()
        {
            TestESBEervice service = new TestESBEervice();
            service.LoginSuccess += new Action(() =>
            {
                service.RegisterService();
                Console.WriteLine("注册成功");
            });
            service.Error += service_Error;
            service.Login(null, null);
        }

        static void service_Error(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }

        static void TestSorteArray()
        {
            List<BigEntityTableIndexItem> indexarray = new List<BigEntityTableIndexItem>();
            for (int i = 0; i < 10; i++)
            {
                indexarray.Add(new BigEntityTableIndexItem
                {
                    Key=((i*2)+1).ToString(),
                    Del=false,
                    KeyOffset=0,
                    len=100,
                    Offset=0
                });
            }
            SorteArray<LJC.FrameWork.Data.EntityDataBase.BigEntityTableIndexItem> sa = new SorteArray<BigEntityTableIndexItem>(indexarray.OrderBy(p=>p.Key).ToArray());
            int mid=-100;
            int fnd = 0;

            var array = sa.GetArray().ToArray();

            for (int i = 0; i < 30; i++)
            {
                if (i == 22)
                {
                    i = 99;
                }
                fnd = sa.Find(new BigEntityTableIndexItem
                {
                    Del=false,
                    Key=i.ToString()
                }, ref mid);

                if (mid == array.Length - 1)
                {
                    Console.WriteLine(string.Format("查找:{0},fnd:{1},mid:{2},real:{3},midread:{4},midreadnext:{5}", i, fnd, mid, fnd > -1 ? array[fnd].Key : "", array[mid].Key, "最右边"));
                }
                else if (mid == -1)
                {
                    Console.WriteLine(string.Format("查找:{0},fnd:{1},mid:{2},real:{3},midread:{4},midreadnext:{5}", i, fnd, mid, fnd > -1 ? array[fnd].Key : "", "最左边", array[mid + 1].Key));
                }
                else
                {
                    Console.WriteLine(string.Format("查找:{0},fnd:{1},mid:{2},real:{3},midread:{4},midreadnext:{5}", i, fnd, mid, fnd > -1 ? array[fnd].Key : "", array[mid].Key, array[mid + 1].Key));
                }
            }
        }

        static void TestLocaldb()
        {
            //Man 
            EntityTableEngine.LocalEngine.CreateTable("Man", "Name", typeof(Man), new[] { "IDCard", "Sex" });
            //EntityTableEngine.LocalEngine.CreateTable("Man", "Name", typeof(Man));
            DateTime time = DateTime.Now;
            for(int i=0;i<100000;i++){
                EntityTableEngine.LocalEngine.Insert("Man", new Man
                {
                    Addr="addr"+Guid.NewGuid().ToString(),
                    IDCard="id"+i,
                    Name="name"+i,
                    Sex=new Random(Guid.NewGuid().GetHashCode()).Next(2)
                });
            }

            Console.WriteLine("写入完成:"+DateTime.Now.Subtract(time).TotalMilliseconds);
            Console.Read();
        }

        static void TestBigLocaldb()
        {
            //Man 
            BigEntityTableEngine.LocalEngine.CreateTable("Man", "Name", typeof(Man));
            //EntityTableEngine.LocalEngine.CreateTable("Man", "Name", typeof(Man));
            DateTime time = DateTime.Now;
            List<Man> list = new List<Man>();
            for (int i = 0; i < 1000000; i++)
            {
                
                var man=new Man
                {
                    Addr = "addr" + Guid.NewGuid().ToString(),
                    IDCard = "id" + i,
                    Name = "name" + i,
                    Sex = new Random(Guid.NewGuid().GetHashCode()).Next(2)
                };
                list.Add(man);
                if (list.Count > 100)
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

        static void TestLocaldb2()
        {
            //Man 
            EntityTableEngine.LocalEngine.CreateTable("Man", "Name", typeof(Man), new[] { "IDCard", "Sex" });
            //EntityTableEngine.LocalEngine.CreateTable("Man", "Name", typeof(Man));
            DateTime time = DateTime.Now;
            for (int i = 100000; i < 200000; i++)
            {
                EntityTableEngine.LocalEngine.Insert("Man", new Man
                {
                    Addr = "addr" + Guid.NewGuid().ToString(),
                    IDCard = "id" + i,
                    Name = "name" + i,
                    Sex = new Random(Guid.NewGuid().GetHashCode()).Next(2)
                });
            }

            Console.WriteLine("写入完成:" + DateTime.Now.Subtract(time).TotalMilliseconds);
            Console.Read();
        }

        static void TestOrder()
        {
            BigEntityTableEngine.LocalEngine.MergeIndex("Man", "Name");
            using (ObjTextReader reader = ObjTextReader.CreateReader(@"D:\GitHub\LJC.FrameWork\Test2\bin\Release\localdb\Man.id"))
            {
                foreach(var item in  reader.ReadObjectsWating<EntityTableIndexItem>(1))
                {
                    Console.WriteLine(item.Key + "->" + item.Offset);
                }
            }

            Console.Read();
        }

        static void TestLocaldbFind()
        {
            for (int i = 0; i < 10; i++)
            {
                var time = DateTime.Now;
                int cnt = 0;
                //foreach (var m in EntityTableEngine.LocalEngine.Find<Man>("Man", "Sex", "1"))
                //{
                //    //Console.WriteLine(m.Name + " " + m.Sex);
                //    cnt++;
                //}
                foreach (var m in EntityTableEngine.LocalEngine.Find<Man>("Man", "name8651"))
                {
                    Console.WriteLine(m.Name + " " + m.Addr);
                    cnt++;
                }
                Console.WriteLine("读取完成:" + cnt + "条,用时:" + DateTime.Now.Subtract(time).TotalMilliseconds);
            }
            Console.Read();
        }

        static void TestBigLocaldbFind()
        {
            //Console.WriteLine("cnt:"+BigEntityTableEngine.LocalEngine.Count("Man"));
            //Console.Read();

            int ccount = 0;
            foreach (var item in BigEntityTableEngine.LocalEngine.List<Man>("Man", 1, 100000))
            {
                //Console.WriteLine(item.Name);
                ccount++;
            }
            Console.WriteLine("ccount:" + ccount);

            for (int c = 0; c < 3; c++)
            {
                var time = DateTime.Now;
                int cnt = 0;
                int readcnt = 0;
                for (int i = 0; i < 1000000; i++)
                {

                    var m = BigEntityTableEngine.LocalEngine.Find<Man>("Man", "name" + i);
                    if (m == null)
                    {
                        cnt++;
                        Console.WriteLine("找不到用户:" + ("name" + i));
                        Console.Read();
                    }
                    readcnt++;
                    if (readcnt % 10000 == 0)
                    {
                        Console.WriteLine(cnt + "用时:" + DateTime.Now.Subtract(time).TotalMilliseconds);
                    }

                }
                Console.WriteLine("读取完成:" + cnt + "条,用时:" + DateTime.Now.Subtract(time).TotalMilliseconds);

            }
            Console.Read();
        }

        static void TestBigLocalUpdate()
        {
            var man1920=BigEntityTableEngine.LocalEngine.FindMem<Man>("Man","name1920");
            Console.Write("修改前:"+man1920.Addr);
            man1920.Addr = "龙坪镇新村李陈1026";
            BigEntityTableEngine.LocalEngine.Update<Man>("Man", man1920);
            var man1920_u = BigEntityTableEngine.LocalEngine.FindMem<Man>("Man", "name1920");
            Console.Write("修改后:" + man1920_u.Addr);

            man1920_u.Addr = Guid.NewGuid().ToString() + "asfasdfasdfase_addr";
            BigEntityTableEngine.LocalEngine.Update<Man>("Man", man1920_u);

            var man1920_uu = BigEntityTableEngine.LocalEngine.FindMem<Man>("Man", "name1920");
            Console.Write("修改后:" + man1920_uu.Addr);
        }

        static void TestBigLocaldbDel()
        {
            var boo = BigEntityTableEngine.LocalEngine.DeleteMem("Man", "name3991");
            Console.WriteLine("删除:" + boo);

            var items = BigEntityTableEngine.LocalEngine.FindMem<Man>("Man", "name3991");
            if (items!=null)
            {
                Console.WriteLine("查找name3991:" + items.Name);
            }
            else
            {
                Console.WriteLine("查找name3991:不存在");
            }

            items = BigEntityTableEngine.LocalEngine.FindMem<Man>("Man", "name3992");
            if (items!=null)
            {
                Console.WriteLine("查找name3992:" + items.Name);
            }
            else
            {
                Console.WriteLine("查找name3992:不存在");
            }
        }

        class coroutineTest : LJC.FrameWork.Comm.Coroutine.ICoroutineUnit
        {
            DateTime time = DateTime.Now;
            public bool IsSuccess()
            {
                return false;
            }

            public bool IsDone()
            {
                return DateTime.Now.Subtract(time).TotalSeconds > 10;
            }

            public bool IsTimeOut()
            {
                return false;
            }

            public void Exceute()
            {
                
            }

            public object GetResult()
            {
                return null;
            }

            public void CallBack(CoroutineCallBackEventArgs args)
            {
                Console.WriteLine("计时完成");
            }
        }

        static LJC.FrameWork.SocketApplication.SocketSTD.SessionClient client = null;
        static void Main(string[] args)
        {
            Console.WriteLine("选择操作 1-写库 2-读库");
            var cmd = Console.ReadLine();

            if (cmd == "1")
            {
                //TestSorteArray();
                //Console.Read();
                //return;
                TestBigLocaldb();
            }

            if (cmd == "2")
            {
                //TestBigLocaldbDel();
                //TestBigLocalUpdate();
                //BigEntityTableEngine.LocalEngine.MergeIndex("Man","Name");
                TestBigLocaldbFind();
            }
            Console.Read();
            return;

            TestLocaldb();
            //TestLocaldb2();
            //TestLocaldbFind();
            TestOrder();
            Console.Read();

            CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new coroutineTest());

            Thread.Sleep(15000);
            CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new coroutineTest());

            Thread.Sleep(30000);
            CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new coroutineTest());

            

            Console.Read();
            return;

            ESBUdpClient client = new ESBUdpClient("192.168.0.100", 2998);
            client.StartClient();
            client.LoginFail += () =>
                {
                    Console.WriteLine("登录失败");
                    client.Dispose();
                };
            client.Login(null, null);

            Console.Read();

            Message msg = new Message();
            var bytes= LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);

            var newmsg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(bytes);


            TestEsbservices();
            Console.Read();
        }

        static void client_Error(Exception obj)
        {
            Console.WriteLine(obj.ToString());
        }

        static void Test10021()
        {
            try
            {
                System.Threading.Tasks.Parallel.For(0, 1, (n) =>
                    {
                        long start = Environment.TickCount & Int32.MaxValue;
                        for (int i = 0; i < 100; i++)
                        {
                            try
                            {
                                start = Environment.TickCount & Int32.MaxValue;
                                var ids = "600358.SH,600086.SH,000768.SZ,300578.SZ,002624.SZ,603558.SH,300024.SZ,300532.SZ,601939.SH,601988.SH,002593.SZ,600318.SH,000540.SZ,601211.SH,000783.SZ,002847.SZ,603444.SH,600015.SH,002145.SZ,601288.SH,002131.SZ,600067.SH,300058.SZ,600628.SH,300242.SZ,600016.SH,600208.SH,601377.SH,000967.SZ,002712.SZ,300392.SZ,601888.SH,603099.SH,002143.SZ,300071.SZ,600276.SH,601398.SH,300513.SZ,600768.SH,603588.SH,603993.SH,000430.SZ,603729.SH,603881.SH,000630.SZ,000839.SZ,300144.SZ,600418.SH,600594.SH,603069.SH".Split(',');
                                //var ids = "600358.SH,600086.SH".Split(',');
                                var result = LJC.FrameWork.SOA.ESBClient.DoSOARequest2<List<LJC.Com.StockService.Contract.StockSimpleInfo>>(1, 10021, ids);
                                long ms = Environment.TickCount & Int32.MaxValue - start;
                                Console.WriteLine(i + " " + ms);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                throw ex;
                            }
                        }
                    });

                Console.WriteLine("完成");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        static void TestEntityBuf()
        {
            byte[] bt4 = new byte[100];
            using(System.IO.MemoryStream ms=new System.IO.MemoryStream(bt4))
            {
                var bts = ms.ToArray();

                var btstr = Convert.ToBase64String(bts);
            }

            string bytestring = "AAIyAIQe5Zu95peF6IGU5ZCI6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlm73ml4XogZTlkIiEBjYwMDM1OIQJNjAwMzU4LlNIhARnbGxoAIQe5Lic5pa56YeR6ZKw6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuJzmlrnph5HpkrCEBjYwMDA4NoQJNjAwMDg2LlNIhARkZmpqAIQe5Lit6Iiq6aOe5py66IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuK3oiKrpo57mnLqEBjAwMDc2OIQJMDAwNzY4LlNahARaSEZKAIQk5LiK5rW35Lya55WF6YCa6K6v6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkvJrnlYXpgJrorq+EBjMwMDU3OIQJMzAwNTc4LlNahARIQ1RUAIQn5rWZ5rGf6YeR5Yip5Y2O55S15rCU6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzph5HliKnljY7nlLWEBjMwMDA2OYQJMzAwMDY5LlNahARKTEhEAIQe5Y2a5rex5bel5YW36IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzljZrmt7Hlt6XlhbeEBjAwMjI4MoQJMDAyMjgyLlNahARCU0dKAIQe5a6M576O5LiW55WM6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlroznvo7kuJbnlYyEBjAwMjYyNIQJMDAyNjI0LlNahARXTVNKAIQk5rWZ5rGf5YGl55ub6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlgaXnm5vpm4blm6KEBjYwMzU1OIQJNjAzNTU4LlNIhARqc2p0AIQw5rKI6Ziz5paw5p2+5py65Zmo5Lq66Ieq5Yqo5YyW6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnmnLrlmajkurqEBjMwMDAyNIQJMzAwMDI0LlNahANKUVIAhDPmt7HlnLPluILku4rlpKnlm73pmYXnianmtYHmioDmnK/ogqHku73mnInpmZDlhazlj7iEDOS7iuWkqeWbvemZhYQGMzAwNTMyhAkzMDA1MzIuU1qEBEpUR0oAhB7kuK3lm73pk7booYzogqHku73mnInpmZDlhazlj7iEDOS4reWbvemTtuihjIQGNjAxOTg4hAk2MDE5ODguU0iEBHpneWgAhB7kuK3mnZDoioLog73ogqHku73mnInpmZDlhazlj7iEDOS4readkOiKguiDvYQGNjAzMTI2hAk2MDMxMjYuU0iEBHpjam4AhCTkuK3lm73lu7rorr7pk7booYzogqHku73mnInpmZDlhazlj7iEDOW7uuiuvumTtuihjIQGNjAxOTM5hAk2MDE5MzkuU0iEBGpzeWgAhCTmt7HlnLPkuJbnuqrmmJ/mupDogqHku73mnInpmZDlhazlj7iEDOS4lue6quaYn+a6kIQGMDAwMDA1hAkwMDAwMDUuU1qEBFNKWFkAhB7kuqTpgJrpk7booYzogqHku73mnInpmZDlhazlj7iEDOS6pOmAmumTtuihjIQGNjAxMzI4hAk2MDEzMjguU0iEBGp0eWgAhCTkuK3lm73lhpzkuJrpk7booYzogqHku73mnInpmZDlhazlj7iEDOWGnOS4mumTtuihjIQGNjAxMjg4hAk2MDEyODguU0iEBG55eWgAhCTljqbpl6jml6XkuIrpm4blm6LogqHku73mnInpmZDlhazlj7iEDOaXpeS4iumbhuWbooQGMDAyNTkzhAkwMDI1OTMuU1qEBFJTSlQAhCTlronlvr3mlrDlipvph5Hono3ogqHku73mnInpmZDlhazlj7iEDOaWsOWKm+mHkeiejYQGNjAwMzE4hAk2MDAzMTguU0iEBHhsanIAhCfkuJznnabmlrDmnZDmlpnpm4blm6LogqHku73mnInpmZDlhazlj7iEDOS4nOedpuiCoeS7vYQGNjAwMTE0hAk2MDAxMTQuU0iEBGRtZ2YAhCTkuK3lpKnln47mipXpm4blm6LogqHku73mnInpmZDlhazlj7iEDOS4reWkqeWfjuaKlYQGMDAwNTQwhAkwMDA1NDAuU1qEBFpUQ1QAhCTlm73ms7DlkJvlronor4HliLjogqHku73mnInpmZDlhazlj7iEDOWbveazsOWQm+WuiYQGNjAxMjExhAk2MDEyMTEuU0iEBGd0amEAhCTkuK3lm73msJHnlJ/pk7booYzogqHku73mnInpmZDlhazlj7iEDOawkeeUn+mTtuihjIQGNjAwMDE2hAk2MDAwMTYuU0iEBG1zeWgAhB7ljY7lpI/pk7booYzogqHku73mnInpmZDlhazlj7iEDOWNjuWkj+mTtuihjIQGNjAwMDE1hAk2MDAwMTUuU0iEBGh4eWgAhCTkuK3lm73lt6XllYbpk7booYzogqHku73mnInpmZDlhazlj7iEDOW3peWVhumTtuihjIQGNjAxMzk4hAk2MDEzOTguU0iEBGdzeWgAhB7plb/msZ/or4HliLjogqHku73mnInpmZDlhazlj7iEDOmVv+axn+ivgeWIuIQGMDAwNzgzhAkwMDA3ODMuU1qEBENKWlEAhB7liKnmrKfpm4blm6LogqHku73mnInpmZDlhazlj7iEDOWIqeasp+iCoeS7vYQGMDAyMTMxhAkwMDIxMzEuU1qEBExPR0YAhC3mtZnmsZ/nnIHlm7Tmtbflu7rorr7pm4blm6LogqHku73mnInpmZDlhazlj7iEDOWbtOa1t+iCoeS7vYQGMDAyNTg2hAkwMDI1ODYuU1qEBFdIR0YAhB7lhqDln47lpKfpgJrogqHku73mnInpmZDlhazlj7iEDOWGoOWfjuWkp+mAmoQGNjAwMDY3hAk2MDAwNjcuU0iEBGdjZHQAhB7mi5vllYbor4HliLjogqHku73mnInpmZDlhazlj7iEDOaLm+WVhuivgeWIuIQGNjAwOTk5hAk2MDA5OTkuU0iEBHpzenEAhDbljJfkuqzok53oibLlhYnmoIflk4HniYznrqHnkIbpob7pl67ogqHku73mnInpmZDlhazlj7iEDOiTneiJsuWFieagh4QGMzAwMDU4hAkzMDAwNTguU1qEBExTR0IAhB7mi5vllYbpk7booYzogqHku73mnInpmZDlhazlj7iEDOaLm+WVhumTtuihjIQGNjAwMDM2hAk2MDAwMzYuU0iEBHpzeWgAhCHkuIrmtbfmlrDkuJbnlYzogqHku73mnInpmZDlhazlj7iECeaWsOS4lueVjIQGNjAwNjI4hAk2MDA2MjguU0iEA3hzagCELeWOpumXqOWQieavlOeJuee9kee7nOaKgOacr+iCoeS7veaciemZkOWFrOWPuIQJ5ZCJ5q+U54m5hAY2MDM0NDSECTYwMzQ0NC5TSIQDamJ0AIQw5bm/5Lic5piO5a626IGU5ZCI56e75Yqo56eR5oqA6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmmI7lrrbogZTlkIiEBjMwMDI0MoQJMzAwMjQyLlNahARNSkxIAIQ25YyX5Lqs6IW+5L+h5Yib5paw572R57uc6JCl6ZSA5oqA5pyv6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzohb7kv6HogqHku72EBjMwMDM5MoQJMzAwMzkyLlNahARUWEdGAIQk5Y2O5aSP5bm456aP5Z+65Lia6IKh5Lu95pyJ6Zm05Lia6K+B5Yi4hAY2MDEzNzeECTYwMTM3Ny5TSIQEeHl6cQCEKea3seWcs+W4guWkqeWcsCjpm4blm6Ip6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmt7HlpKnlnLDvvKGEBjAwMDAyM4QJMDAwMDIzLlNahARTVERBAIQe5paw5rmW5Lit5a6d6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmlrDmuZbkuK3lrp2EBjYwMDIwOIQJNjAwMjA4LlNIhAR4aHpiAIQe5Lit5Zu95Zu95peF6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuK3lm73lm73ml4WEBjYwMTg4OIQJNjAxODg4LlNIhAR6Z2dsAIQe5a6B5rOi6ZO26KGM6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlroHms6Lpk7booYyEBjAwMjE0MoQJMDAyMTQyLlNahAROQllIAIQk5Y2w57qq5aix5LmQ5Lyg5aqS6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzljbDnuqrkvKDlqpKEBjAwMjE0M4QJMDAyMTQzLlNahARZSkNNAIQe5oCd576O5Lyg5aqS6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmgJ3nvo7kvKDlqpKEBjAwMjcxMoQJMDAyNzEyLlNahARTTUNNAIQ85YyX5Lqs5Y2O6LCK5ZiJ5L+h5pW05ZCI6JCl6ZSA6aG+6Zeu6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzljY7osIrlmInkv6GEBjMwMDA3MYQJMzAwMDcxLlNahARIWUpYAIQq5peg6ZSh5YWI5a+85pm66IO96KOF5aSH6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlhYjlr7zmmbrog72EBjMwMDQ1MIQJMzAwNDUwLlNahARYRFpOAIQq5YyX5Lqs5oGS5rOw5a6e6L6+56eR5oqA6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmgZLms7Dlrp7ovr6EBjMwMDUxM4QJMzAwNTEzLlNahARIVFNEAIQq5a6B5rOi5a+M6YKm57K+5Lia6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlroHms6Llr4zpgqaEBjYwMDc2OIQJNjAwNzY4LlNIhARuYmZiAIQt5Y6m6Zeo5ZCJ5q+U54m5572R57uc5oqA5pyv6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnlkInmr5TnibmEBjYwMzQ0NIQJNjAzNDQ0LlNIhANqYnQAhB7pnZLlspvlj4zmmJ/ogqHku73mnInpmZDlhazlj7iEDOmdkuWym+WPjOaYn4QGMDAwNTk5hAkwMDA1OTkuU1qEBFFEU1gAhCrnm4jls7Dnjq/looPnp5HmioDpm4blm6LogqHku73mnInpmZDlhazlj7iEDOebiOWzsOeOr+Wig4QGMDAwOTY3hAkwMDA5NjcuU1qEBFlGSEoAhDDoi4/lt57mlrDljLrpq5jmlrDmioDmnK/kuqfkuJrogqHku73mnInpmZDlhazlj7iEDOiLj+W3numrmOaWsIQGNjAwNzM2hAk2MDA3MzYuU0iEBHN6Z3g=";
            string oldertring = "AAIyAIQe5Zu95peF6IGU5ZCI6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlm73ml4XogZTlkIiEBjYwMDM1OIQJNjAwMzU4LlNIhARnbGxoAIQe5Lic5pa56YeR6ZKw6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuJzmlrnph5HpkrCEBjYwMDA4NoQJNjAwMDg2LlNIhARkZmpqAIQe5Lit6Iiq6aOe5py66IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuK3oiKrpo57mnLqEBjAwMDc2OIQJMDAwNzY4LlNahARaSEZKAIQk5LiK5rW35Lya55WF6YCa6K6v6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkvJrnlYXpgJrorq+EBjMwMDU3OIQJMzAwNTc4LlNahARIQ1RUAIQn5rWZ5rGf6YeR5Yip5Y2O55S15rCU6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzph5HliKnljY7nlLWEBjMwMDA2OYQJMzAwMDY5LlNahARKTEhEAIQe5Y2a5rex5bel5YW36IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzljZrmt7Hlt6XlhbeEBjAwMjI4MoQJMDAyMjgyLlNahARCU0dKAIQe5a6M576O5LiW55WM6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlroznvo7kuJbnlYyEBjAwMjYyNIQJMDAyNjI0LlNahARXTVNKAIQk5rWZ5rGf5YGl55ub6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlgaXnm5vpm4blm6KEBjYwMzU1OIQJNjAzNTU4LlNIhARqc2p0AIQw5rKI6Ziz5paw5p2+5py65Zmo5Lq66Ieq5Yqo5YyW6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnmnLrlmajkurqEBjMwMDAyNIQJMzAwMDI0LlNahANKUVIAhDPmt7HlnLPluILku4rlpKnlm73pmYXnianmtYHmioDmnK/ogqHku73mnInpmZDlhazlj7iEDOS7iuWkqeWbvemZhYQGMzAwNTMyhAkzMDA1MzIuU1qEBEpUR0oAhB7kuK3lm73pk7booYzogqHku73mnInpmZDlhazlj7iEDOS4reWbvemTtuihjIQGNjAxOTg4hAk2MDE5ODguU0iEBHpneWgAhB7kuK3mnZDoioLog73ogqHku73mnInpmZDlhazlj7iEDOS4readkOiKguiDvYQGNjAzMTI2hAk2MDMxMjYuU0iEBHpjam4AhCTkuK3lm73lu7rorr7pk7booYzogqHku73mnInpmZDlhazlj7iEDOW7uuiuvumTtuihjIQGNjAxOTM5hAk2MDE5MzkuU0iEBGpzeWgAhCTmt7HlnLPkuJbnuqrmmJ/mupDogqHku73mnInpmZDlhazlj7iEDOS4lue6quaYn+a6kIQGMDAwMDA1hAkwMDAwMDUuU1qEBFNKWFkAhB7kuqTpgJrpk7booYzogqHku73mnInpmZDlhazlj7iEDOS6pOmAmumTtuihjIQGNjAxMzI4hAk2MDEzMjguU0iEBGp0eWgAhCTkuK3lm73lhpzkuJrpk7booYzogqHku73mnInpmZDlhazlj7iEDOWGnOS4mumTtuihjIQGNjAxMjg4hAk2MDEyODguU0iEBG55eWgAhCTljqbpl6jml6XkuIrpm4blm6LogqHku73mnInpmZDlhazlj7iEDOaXpeS4iumbhuWbooQGMDAyNTkzhAkwMDI1OTMuU1qEBFJTSlQAhCTlronlvr3mlrDlipvph5Hono3ogqHku73mnInpmZDlhazlj7iEDOaWsOWKm+mHkeiejYQGNjAwMzE4hAk2MDAzMTguU0iEBHhsanIAhCfkuJznnabmlrDmnZDmlpnpm4blm6LogqHku73mnInpmZDlhazlj7iEDOS4nOedpuiCoeS7vYQGNjAwMTE0hAk2MDAxMTQuU0iEBGRtZ2YAhCTkuK3lpKnln47mipXpm4blm6LogqHku73mnInpmZDlhazlj7iEDOS4reWkqeWfjuaKlYQGMDAwNTQwhAkwMDA1NDAuU1qEBFpUQ1QAhCTlm73ms7DlkJvlronor4HliLjogqHku73mnInpmZDlhazlj7iEDOWbveazsOWQm+WuiYQGNjAxMjExhAk2MDEyMTEuU0iEBGd0amEAhCTkuK3lm73msJHnlJ/pk7booYzogqHku73mnInpmZDlhazlj7iEDOawkeeUn+mTtuihjIQGNjAwMDE2hAk2MDAwMTYuU0iEBG1zeWgAhB7ljY7lpI/pk7booYzogqHku73mnInpmZDlhazlj7iEDOWNjuWkj+mTtuihjIQGNjAwMDE1hAk2MDAwMTUuU0iEBGh4eWgAhCTkuK3lm73lt6XllYbpk7booYzogqHku73m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";
            string esbbytes =   "AAIyAIQe5Zu95peF6IGU5ZCI6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlm73ml4XogZTlkIiEBjYwMDM1OIQJNjAwMzU4LlNIhARnbGxoAIQe5Lic5pa56YeR6ZKw6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuJzmlrnph5HpkrCEBjYwMDA4NoQJNjAwMDg2LlNIhARkZmpqAIQe5Lit6Iiq6aOe5py66IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuK3oiKrpo57mnLqEBjAwMDc2OIQJMDAwNzY4LlNahARaSEZKAIQk5LiK5rW35Lya55WF6YCa6K6v6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkvJrnlYXpgJrorq+EBjMwMDU3OIQJMzAwNTc4LlNahARIQ1RUAIQn5rWZ5rGf6YeR5Yip5Y2O55S15rCU6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzph5HliKnljY7nlLWEBjMwMDA2OYQJMzAwMDY5LlNahARKTEhEAIQe5Y2a5rex5bel5YW36IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzljZrmt7Hlt6XlhbeEBjAwMjI4MoQJMDAyMjgyLlNahARCU0dKAIQe5a6M576O5LiW55WM6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlroznvo7kuJbnlYyEBjAwMjYyNIQJMDAyNjI0LlNahARXTVNKAIQk5rWZ5rGf5YGl55ub6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlgaXnm5vpm4blm6KEBjYwMzU1OIQJNjAzNTU4LlNIhARqc2p0AIQw5rKI6Ziz5paw5p2+5py65Zmo5Lq66Ieq5Yqo5YyW6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnmnLrlmajkurqEBjMwMDAyNIQJMzAwMDI0LlNahANKUVIAhDPmt7HlnLPluILku4rlpKnlm73pmYXnianmtYHmioDmnK/ogqHku73mnInpmZDlhazlj7iEDOS7iuWkqeWbvemZhYQGMzAwNTMyhAkzMDA1MzIuU1qEBEpUR0oAhB7kuK3lm73pk7booYzogqHku73mnInpmZDlhazlj7iEDOS4reWbvemTtuihjIQGNjAxOTg4hAk2MDE5ODguU0iEBHpneWgAhB7kuK3mnZDoioLog73ogqHku73mnInpmZDlhazlj7iEDOS4readkOiKguiDvYQGNjAzMTI2hAk2MDMxMjYuU0iEBHpjam4AhCTkuK3lm73lu7rorr7pk7booYzogqHku73mnInpmZDlhazlj7iEDOW7uuiuvumTtuihjIQGNjAxOTM5hAk2MDE5MzkuU0iEBGpzeWgAhCTmt7HlnLPkuJbnuqrmmJ/mupDogqHku73mnInpmZDlhazlj7iEDOS4lue6quaYn+a6kIQGMDAwMDA1hAkwMDAwMDUuU1qEBFNKWFkAhB7kuqTpgJrpk7booYzogqHku73mnInpmZDlhazlj7iEDOS6pOmAmumTtuihjIQGNjAxMzI4hAk2MDEzMjguU0iEBGp0eWgAhCTkuK3lm73lhpzkuJrpk7booYzogqHku73mnInpmZDlhazlj7iEDOWGnOS4mumTtuihjIQGNjAxMjg4hAk2MDEyODguU0iEBG55eWgAhCTljqbpl6jml6XkuIrpm4blm6LogqHku73mnInpmZDlhazlj7iEDOaXpeS4iumbhuWbooQGMDAyNTkzhAkwMDI1OTMuU1qEBFJTSlQAhCTlronlvr3mlrDlipvph5Hono3ogqHku73mnInpmZDlhazlj7iEDOaWsOWKm+mHkeiejYQGNjAwMzE4hAk2MDAzMTguU0iEBHhsanIAhCfkuJznnabmlrDmnZDmlpnpm4blm6LogqHku73mnInpmZDlhazlj7iEDOS4nOedpuiCoeS7vYQGNjAwMTE0hAk2MDAxMTQuU0iEBGRtZ2YAhCTkuK3lpKnln47mipXpm4blm6LogqHku73mnInpmZDlhazlj7iEDOS4reWkqeWfjuaKlYQGMDAwNTQwhAkwMDA1NDAuU1qEBFpUQ1QAhCTlm73ms7DlkJvlronor4HliLjogqHku73mnInpmZDlhazlj7iEDOWbveazsOWQm+WuiYQGNjAxMjExhAk2MDEyMTEuU0iEBGd0amEAhCTkuK3lm73msJHnlJ/pk7booYzogqHku73mnInpmZDlhazlj7iEDOawkeeUn+mTtuihjIQGNjAwMDE2hAk2MDAwMTYuU0iEBG1zeWgAhB7ljY7lpI/pk7booYzogqHku73mnInpmZDlhazlj7iEDOWNjuWkj+mTtuihjIQGNjAwMDE1hAk2MDAwMTUuU0iEBGh4eWgAhCTkuK3lm73lt6XllYbpk7booYzogqHku73m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";

            bool boo = bytestring == oldertring;

            int diff = 0, same = 0;
            for(int i=0;i<bytestring.Length;i++)
            {
                if (bytestring[i] == oldertring[i])
                {
                    same++;
                }
                else
                {
                    diff++;
                }
            }

            var bytes = Convert.FromBase64String(bytestring);

            var list = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<List<LJC.Com.StockService.Contract.StockSimpleInfo>>(bytes, true);
        }

        static void client_LoginSuccess()
        {
            Console.WriteLine("登录成功");

            System.Threading.Tasks.Parallel.For(0, 20, (no) =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var msg = new LJC.FrameWork.SocketApplication.Message
                        {
                            MessageHeader = new LJC.FrameWork.SocketApplication.MessageHeader
                            {
                                TransactionID = LJC.FrameWork.SocketApplication.SocketApplicationComm.GetSeqNum(),
                                MessageTime = DateTime.Now,
                                MessageType = 10240
                            }
                        };
                        msg.SetMessageBody("hello");
                        Console.WriteLine(no + "发送消息:" + msg.MessageHeader.TransactionID);
                        var str = client.SendMessageAnsy<string>(msg, 5000);
                        Console.WriteLine(no + "收到消息:" + str);
                    }
                });

            

            //var resp = LJC.FrameWork.SOA.ESBClient.DoSOARequest<LJC.FrameWork.SOA.SOAServerEchoResponse>(0, 1, null);

            //var ms = Environment.TickCount;

            //for(int i=0;i<100000;i++)
            //{
            //    resp = LJC.FrameWork.SOA.ESBClient.DoSOARequest<LJC.FrameWork.SOA.SOAServerEchoResponse>(0, 1, null);
            //}

            //Console.WriteLine("用时ms:" + (Environment.TickCount - ms));
        }

        static void Main1(string[] args)
        {
            LJC.FrameWork.SocketApplication.SocketSTD.MessageApp appClient = new LJC.FrameWork.SocketApplication.SocketSTD.MessageApp();
            
            appClient.EnableBroadCast = true;
            appClient.EnableMultiCast = true;
            appClient.OnBroadCast += appClient_OnBroadCast;
            appClient.OnMultiCast += appClient_OnMultiCast;
            Console.Read();
        }

        static void appClient_OnMultiCast(LJC.FrameWork.SocketApplication.Message obj)
        {
            string msg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<string>(obj.MessageBuffer);
            Console.WriteLine("收到组播消息:" + msg);
        }

        static void appClient_OnBroadCast(LJC.FrameWork.SocketApplication.Message obj)
        {
            //throw new NotImplementedException();
            string msg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<string>(obj.MessageBuffer);
            Console.WriteLine("收到广播消息:" + msg);
        }
    }
}
