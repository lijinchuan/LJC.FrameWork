using Ljc.Com.NewsService.Entity;
using LJC.Com.StockService.Contract;
using LJC.FrameWork.Comm;
using LJC.FrameWork.Comm.TextReaderWriter;
using LJC.FrameWork.Data.EntityDataBase;
using LJC.FrameWork.Data.Mongo;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.SOA;
using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Test2
{
    class Program
    {
        public class Person
        {
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

            public DateTime Birth
            {
                get;
                set;
            }

            public byte[] Info
            {
                get;
                set;
            }
        }

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
                foreach(var item in reader.ReadObjectsWating<Man>())
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

        static void TestUdpClient()
        {
            var udpclient = new LJC.FrameWork.SocketApplication.SocketEasyUDP.Client.SessionClient("127.0.0.1", 19000);
            udpclient.StartClient();
            DateTime now = DateTime.Now;
            for (int i = 0; i < 100000; i++)
            {
                if (udpclient.SetMTU(10240))
                {
                    //Console.WriteLine("设置mtu成功");
                }
            }

            Console.WriteLine("用时:"+(DateTime.Now.Subtract(now).TotalMilliseconds));

            Console.Read();
        }

        static string PrintCmd()
        {
            Console.WriteLine("1-测试BigEntityTable");
            Console.WriteLine("2-测试二分法排序");
            Console.WriteLine("3-测试股票");
            Console.WriteLine("4-运行BigEntityTable测试用户");
            Console.WriteLine("0-退出");
            return Console.ReadLine();
        }

        static void TestUdpClient2()
        {
            LJC.FrameWork.SocketApplication.SocketEasyUDP.Client.SessionClient[] udpclients=new LJC.FrameWork.SocketApplication.SocketEasyUDP.Client.SessionClient[3];
            for (int i = 0; i < udpclients.Length; i++)
            {
                udpclients[i] = new LJC.FrameWork.SocketApplication.SocketEasyUDP.Client.SessionClient("127.0.0.1", 19000);
                udpclients[i].StartClient();
            }

            var now = DateTime.Now;

            List<int> list = new List<int>();
            for (int i = 0; i < 100000; i++)
            {
                list.Add(i);
            }

            //LJC.FrameWork.Comm.TaskHelper.RunTask2<int>(list, 3, (i) =>
            //{
            //    udpclients[i%udpclients.Length].SetMTU(10240);
            //});

            Console.WriteLine("多线程用时:" + (DateTime.Now.Subtract(now).TotalMilliseconds));

            Console.Read();
        }

        static void TestNio()
        {
            var resp = ESBClient.DoSOARequest<CallBackRequest>(100,
                               1, new SubmitSparkRequest()
                               {
                                   Context = new SparkLauncherContext
                                   {
                                       AppResource = "AppResource",
                                       MainClass = "MainClass",
                                       Master = "Master",
                                       SparkHome = "SparkHome",

                                   },
                                   TaskId = "test1"
                               });
        }

        static void TestEntityTable()
        {
            EntityTableEngine engine = new EntityTableEngine(null);
            engine.CreateTable("testperson", "Name", typeof(Person));
            engine.Insert("testperson", new Person
            {
                Name = "ljc1",
                Age = 20,
                Birth = DateTime.Now.AddYears(-20)
            });

            engine.Insert("testperson", new Person
            {
                Name = "ljc2",
                Age = 21,
                Birth = DateTime.Now.AddYears(-21)
            });

            engine.Insert("testperson", new Person
            {
                Name = "ljc3",
                Age = 21,
                Birth = DateTime.Now.AddYears(-21)
            });
        }

        static void TestEntityTableRead()
        {
            EntityTableEngine engine = new EntityTableEngine(null);
            DateTime st = DateTime.Now;
            //for (int i = 0; i < 10000; i++)
            {
                foreach (var item in engine.Find<Person>("testperson", "ljc2"))
                {
                    Console.WriteLine(item.Name + "," + item.Age);
                }
            }

            Console.WriteLine("用时:" + (DateTime.Now.Subtract(st).TotalMilliseconds));
        }

        static void TestEntityTableDel()
        {
            EntityTableEngine engine = new EntityTableEngine(null);
            engine.Delete("testperson", "ljc1");
        }

        static void TestEntityTableUpdate()
        {
            EntityTableEngine engine = new EntityTableEngine(null);
            Console.WriteLine("修改前:" + engine.Find<Person>("testperson", "ljc2").First().Age);
            engine.Update<Person>("testperson", new Person
            {
                Name = "ljc2",
                Age = 30
            });
            Console.WriteLine("修改后:" + engine.Find<Person>("testperson", "ljc2").First().Age);
        }

        static bool SimpleObjectsEq(object[] obj1, object[] obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }
            if (obj1 == null || obj2 == null)
            {
                return false;
            }
            if (obj1.Length != obj2.Length)
            {
                return false;
            }
            for (int i = 0; i < obj1.Length; i++)
            {
                if (!obj1[i].Equals(obj2[i]))
                {
                    return false;
                }
            }

            return true;
        }


        public static int ShortestSubarray(int[] a, int k)
        {
            var now = DateTime.Now;
            long sum = 0, sum2 = 0;
            int len = 0, minlen = int.MaxValue;
            int p = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == k)
                {
                    return 1;
                }
                if (sum < k)
                {
                    if (len == 0 && a[i] <= 0)
                    {
                        continue;
                    }
                    sum += a[i];
                    
                    len++;
                    if (sum >= k)
                    {
                        p = i;
                        var more = sum - k;
                        var tempsum = sum - a[i];

                        for (int j = i - 1; j > i - len; j--)
                        {
                            if (tempsum <= more)
                            {
                                len -= (j + 1 - (i + 1 - len));
                                sum -= tempsum;
                                break;
                            }
                            tempsum -= a[j];
                        }

                        more = sum - k;
                        tempsum = 0;

                        for (int j = p;j > p - len; j--)
                        {
                            tempsum += a[j];
                            if (tempsum <= more)
                            {
                                sum -= a[j];
                                sum2 += a[j];
                                len--;
                                p--;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (minlen > len)
                        {
                            minlen = len;
                        }
                    }
                }
                else
                {
                    if (i - p > len)
                    {
                        sum = 0;
                        len = 0;
                        sum2 = 0;
                        i = p + 1;
                    }
                    else
                    {
                        sum2 += a[i];
                        var tempsum = sum;
                        var more = sum - k;
                        bool flag = false;

                        for (var j = p; j > p - len; j--)
                        {
                            tempsum -= a[j];
                            if (sum2 >= tempsum - more && (i - j + 1) <= len)
                            {
                                len = i - j + 1;
                                flag = true;
                                sum += sum2 - tempsum;
                                sum2 = 0;
                                break;
                            }

                        }
                        if (flag)
                        {
                            p = i;
                            if (len < minlen)
                            {
                                minlen = len;
                            }
                        }

                    }
                }
            }
            if (minlen == int.MaxValue)
            {
                minlen = -1;
            }
            Console.WriteLine("结果:"+minlen+",用时:"+(DateTime.Now.Subtract(now).TotalMilliseconds+"ms"));
            return minlen;
        }


 public class ListNode
        {
      public int val;
      public ListNode next;
      public ListNode(int x) { val = x; }
  }

            public static ListNode ReverseBetween(ListNode head, int m, int n)
            {
                ListNode ret = null;
                ListNode h1 = null;
                ListNode t1 = null;
                var h = head;
                int i = 1;
                while (h != null)
                {
                    var node = new ListNode(h.val);
                    if (i < m)
                    {
                        if (t1 == null)
                        {
                            t1 = node;
                            ret = t1;
                        }
                        else
                        {
                            t1.next = node;
                            t1 = node;
                        }
                    }
                    else if (i >= m && i <= n)
                    {
                        if (h1 == null)
                        {
                            h1 = node;
                        }
                        else
                        {
                            node.next = h1;
                            h1 = node;
                        }
                    }
                    else if (i == n + 1)
                    {
                        if (t1 == null)
                        {
                            t1 = h1;
                            ret = h1;
                        }
                        else
                        {
                            t1.next = h1;
                        }
                        while (t1.next != null)
                        {
                            t1 = t1.next;
                        }
                        t1.next = node;
                        t1 = node;
                    }
                    else
                    {
                        t1.next = node;
                        t1 = node;
                    }


                    h = h.next;
                    i++;
                }

                if (h1 != null && t1 != null)
                {
                    t1.next = h1;
                }


                return ret;
            }
        

        static LJC.FrameWork.SocketApplication.SocketSTD.SessionClient client = null;
        static LJC.FrameWork.SocketEasy.Client.SessionClient sc = null;
        static void Main(string[] args)
        {
            var code = "for x:0 step 2 to 10 begin if 1=1 then true else false end end;";
            LJC.FrameWork.CodeExpression.ExpressCode ec = new LJC.FrameWork.CodeExpression.ExpressCode(code);
            //var dt = Convert.ToDateTime("2021-04-10");
            var rslt = ec.CallResult();
            return;

            var head = new ListNode(3);
            head.next = new ListNode(5);
            var ret=ReverseBetween(head, 1, 1);

            while (true)
            {
                Console.WriteLine("输入要计算的文件");
                string file = null;
                while (!string.IsNullOrWhiteSpace(file = Console.ReadLine()))
                {
                    break;
                }
                if (file == "exit")
                {
                    break;
                }
                var lines = System.IO.File.ReadAllLines(file);
                ShortestSubarray(lines[0].Split(',').Select(p => int.Parse(p)).ToArray(), int.Parse(lines[1]));
            }
            return;
            var cmd = PrintCmd();
            IFun funx = null;
            while (cmd != "0")
            {
                switch (cmd)
                {
                    case "1":
                        {
                            funx = new Fun1();
                            break;
                        }
                    case "2":
                        {
                            funx = new Fun2();
                            break;
                        }
                    case "3":
                        {
                            funx = new Fun3();
                            break;
                        }
                    case "4":
                        {
                            funx = new LocalDUnitBTest();

                            break;
                        }
                }

                if (funx != null)
                {
                    try
                    {
                        funx.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                cmd = PrintCmd();
            }


            return;
        }

        static void sc_Error(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }

        static void sc_LoginSuccess()
        {
            Console.WriteLine("登录成功");

            while (true)
            {
                Thread.Sleep(new Random().Next(1, 10) * 1000);
                var msg = new Message(10240);
                
                msg.MessageHeader.TransactionID = Guid.NewGuid().ToString();
                var re = sc.SendMessageAnsy<string>(msg);
                Console.WriteLine("收到回复:" + re);
            }
        }

        static void udpclient_Error(Exception obj)
        {
            Console.WriteLine(obj.ToString());
        }

        static void TestRedisSpeed()
        {
            //Host_Redis
            
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

            string bytestring = "AAIyAIQe5Zu95peF6IGU5ZCI6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlm73ml4XogZTlkIiEBjYwMDM1OIQJNjAwMzU4LlNIhARnbGxoAIQe5Lic5pa56YeR6ZKw6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuJzmlrnph5HpkrCEBjYwMDA4NoQJNjAwMDg2LlNIhARkZmpqAIQe5Lit6Iiq6aOe5py66IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuK3oiKrpo57mnLqEBjAwMDc2OIQJMDAwNzY4LlNahARaSEZKAIQk5LiK5rW35Lya55WF6YCa6K6v6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkvJrnlYXpgJrorq+EBjMwMDU3OIQJMzAwNTc4LlNahARIQ1RUAIQn5rWZ5rGf6YeR5Yip5Y2O55S15rCU6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzph5HliKnljY7nlLWEBjMwMDA2OYQJMzAwMDY5LlNahARKTEhEAIQe5Y2a5rex5bel5YW36IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzljZrmt7Hlt6XlhbeEBjAwMjI4MoQJMDAyMjgyLlNahARCU0dKAIQe5a6M576O5LiW55WM6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlroznvo7kuJbnlYyEBjAwMjYyNIQJMDAyNjI0LlNahARXTVNKAIQk5rWZ5rGf5YGl55ub6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlgaXnm5vpm4blm6KEBjYwMzU1OIQJNjAzNTU4LlNIhARqc2p0AIQw5rKI6Ziz5paw5p2+5py65Zmo5Lq66Ieq5Yqo5YyW6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnmnLrlmajkurqEBjMwMDAyNIQJMzAwMDI0LlNahANKUVIAhDPmt7HlnLPluILku4rlpKnlm73pmYXnianmtYHmioDmnK/ogqHku73mnInpmZDlhazlj7iEDOS7iuWkqeWbvemZhYQGMzAwNTMyhAkzMDA1MzIuU1qEBEpUR0oAhB7kuK3lm73pk7booYzogqHku73mnInpmZDlhazlj7iEDOS4reWbvemTtuihjIQGNjAxOTg4hAk2MDE5ODguU0iEBHpneWgAhB7kuK3mnZDoioLog73ogqHku73mnInpmZDlhazlj7iEDOS4readkOiKguiDvYQGNjAzMTI2hAk2MDMxMjYuU0iEBHpjam4AhCTkuK3lm73lu7rorr7pk7booYzogqHku73mnInpmZDlhazlj7iEDOW7uuiuvumTtuihjIQGNjAxOTM5hAk2MDE5MzkuU0iEBGpzeWgAhCTmt7HlnLPkuJbnuqrmmJ/mupDogqHku73mnInpmZDlhazlj7iEDOS4lue6quaYn+a6kIQGMDAwMDA1hAkwMDAwMDUuU1qEBFNKWFkAhB7kuqTpgJrpk7booYzogqHku73mnInpmZDlhazlj7iEDOS6pOmAmumTtuihjIQGNjAxMzI4hAk2MDEzMjguU0iEBGp0eWgAhCTkuK3lm73lhpzkuJrpk7booYzogqHku73mnInpmZDlhazlj7iEDOWGnOS4mumTtuihjIQGNjAxMjg4hAk2MDEyODguU0iEBG55eWgAhCTljqbpl6jml6XkuIrpm4blm6LogqHku73mnInpmZDlhazlj7iEDOaXpeS4iumbhuWbooQGMDAyNTkzhAkwMDI1OTMuU1qEBFJTSlQAhCTlronlvr3mlrDlipvph5Hono3ogqHku73mnInpmZDlhazlj7iEDOaWsOWKm+mHkeiejYQGNjAwMzE4hAk2MDAzMTguU0iEBHhsanIAhCfkuJznnabmlrDmnZDmlpnpm4blm6LogqHku73mnInpmZDlhazlj7iEDOS4nOedpuiCoeS7vYQGNjAwMTE0hAk2MDAxMTQuU0iEBGRtZ2YAhCTkuK3lpKnln47mipXpm4blm6LogqHku73mnInpmZDlhazlj7iEDOS4reWkqeWfjuaKlYQGMDAwNTQwhAkwMDA1NDAuU1qEBFpUQ1QAhCTlm73ms7DlkJvlronor4HliLjogqHku73mnInpmZDlhazlj7iEDOWbveazsOWQm+WuiYQGNjAxMjExhAk2MDEyMTEuU0iEBGd0amEAhCTkuK3lm73msJHnlJ/pk7booYzogqHku73mnInpmZDlhazlj7iEDOawkeeUn+mTtuihjIQGNjAwMDE2hAk2MDAwMTYuU0iEBG1zeWgAhB7ljY7lpI/pk7booYzogqHku73mnInpmZDlhazlj7iEDOWNjuWkj+mTtuihjIQGNjAwMDE1hAk2MDAwMTUuU0iEBGh4eWgAhCTkuK3lm73lt6XllYbpk7booYzogqHku73m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";
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

            var list = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<List<LJC.Com.StockService.Contract.StockSimpleInfo>>(bytes);
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

        protected static byte[] MargeBag(byte[] bag)
        {
            string str = "CwgAAAAAAABFAAAASAAAANgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            bag = Convert.FromBase64String(str);
            int packageno = 0, packagelen = 0;
            Dictionary<string, byte[][]> TempBagDic = new Dictionary<string, byte[][]>();
            try
            {
                packageno = BitConverter.ToInt32(bag, 8);
                packagelen = BitConverter.ToInt32(bag, 12);

                if (packagelen > 1)
                {
                    long bagid = BitConverter.ToInt64(bag, 16);
                    string key = "gg";
                    byte[][] bags = null;
                    if (!TempBagDic.TryGetValue(key, out bags))
                    {
                        lock (TempBagDic)
                        {
                            if (!TempBagDic.TryGetValue(key, out bags))
                            {
                                bags = new byte[packagelen][];
                                TempBagDic.Add(key, bags);

                            }
                        }
                    }

                    lock (bags)
                    {
                        var index = packageno - 1;
                        if (bags[index] == null)
                        {
                            bags[index] = bag;
                        }
                    }

                    for (var i = 0; i < bags.Length; i++)
                    {
                        if (bags[i] == null)
                        {
                            return null;
                        }
                    }

                    lock (TempBagDic)
                    {
                        //TempBagDic.Remove(key);
                        TempBagDic[key] = new byte[0][];
                    }

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        for (int i = 0; i < bags.Length; i++)
                        {
                            var offset = 24;
                            ms.Write(bags[i], offset, bags[i].Length - offset);
                        }

                        return ms.ToArray();
                    }
                }
                else
                {
                    return bag.Skip(24).ToArray();
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("packageno", packageno);
                ex.Data.Add("packagelen", packagelen);
                ex.Data.Add("bag", Convert.ToBase64String(bag));

                //LogManager.LogHelper.Instance.Error("MargeBag error", ex);

                throw ex;
            }

        }
    }
}
