using Ljc.Com.NewsService.Entity;
using LJC.Com.StockService.Contract;
using LJC.FrameWork.Comm;
using LJC.FrameWork.Comm.TextReaderWriter;
using LJC.FrameWork.Data.Mongo;
using LJC.FrameWork.SOA;
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

        static LJC.FrameWork.SocketApplication.SessionClient client = null;
        static void Main(string[] args)
        {
            string mongoname="Mongo";
            string dbname="Test";
            string collname="News";
            //var query = new LJC.FrameWork.Data.Mongo.MongoQueryWarpper<NewsEntity>().EQ(p => p.Cdate, 2);
            //for (int i = 0; i < 100; i++)
            //{
            //    LJC.FrameWork.Data.Mongo.MongoDBHelper.Insert<NewsEntity>(mongoname, dbname, collname, new NewsEntity
            //    {
            //        Cdate = DateTime.Now,
            //        Class = "cjzf",
            //        Clicktime = i * 100,
            //        Conkeywords = "conkeywords",
            //        Content = "content" + i.ToString(),
            //        Formurl = "http://www.sina.com",
            //        Id = i + 1,
            //        IsHtmlMaked = true,
            //        Md5 = Guid.NewGuid().ToString(),
            //        Title = "title" + i,
            //        NewsDate = DateTime.Now.AddMinutes(-i),
            //        Source = "source" + i,
            //        Isvalid = true,
            //        NewsWriter = "writer" + i,
            //        Path = "path" + i
            //    });
            //}

            //LJC.FrameWork.Data.Mongo.MongoDBHelper.Drop(mongoname, dbname, collname);

      
            var lists = LJC.FrameWork.Data.Mongo.MongoDBHelper.FindAll<NewsEntity>(mongoname, dbname, collname, new LJC.FrameWork.Data.Mongo.MongoSortWarpper<NewsEntity>().Desc(p => p.Clicktime));

            //long total = 0;
            //var lists = LJC.FrameWork.Data.Mongo.MongoDBHelper.Find<NewsEntity>(mongoname, dbname, collname, new MongoQueryWarpper<NewsEntity>().EQ(p => p.Title, "title35"), 1, 100, new MongoSortWarpper<NewsEntity>().Asc(p => p.Clicktime), out total);

            return;
            TestLogQueue();

            Console.Read();

            for (int i = 0; i < 100; i++)
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                var list = ESBClient.DoSOARequest<List<CategoryLevel2Entity>>(Consts.ServiceNo, Consts.FunID_GetCategoryLevel2List, null)
                    .ToDictionary(p => p.CategoryCode);
                sw.Stop();
                Console.WriteLine(i + "用时:" + sw.Elapsed.TotalMilliseconds);
            }
            
            Console.Read();

            Test10021();

            Console.Read();

            TestEntityBuf();

            var content1 = @"[
                                {name:'cjt',age:10},
                                {name:'cjt2',age:21},
                                {name:'cjtc',age:16}
                            ]";

            var res1 = JsonHelper.JsonToEntity<dynamic>(content1);
            var ss = JsonHelper.ToJson(res1);
            //TryRead1();

            //ThreadPool.SetMinThreads(100, 100);
            client = new LJC.FrameWork.SocketApplication.SessionClient("127.0.0.1", 5555, true);
            client.LoginSuccess += client_LoginSuccess;
            client.Error += client_Error;
            client.Login("", "");
            Console.Read();


            //LJC.FrameWork.MSMQ.MsmqClient mc = new LJC.FrameWork.MSMQ.MsmqClient(".\\private$\\ljctest111", false);
            //mc.CreateIfNotExis();
            //int i = 0;
            //try
            //{
            //    while (true)
            //    {
            //        i++;

            //        mc.SendQueue("你好:" + i, false);
            //        Console.WriteLine("发送第" + i + "条消息");
            //        if (i > 1000)
            //        {
            //            break;
            //        }

            //        Thread.Sleep(600000);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("出错" + i, ex.Message);
            //}

            //LJC.FrameWork.MSMQ.MsmqClient mc = new LJC.FrameWork.MSMQ.MsmqClient("FormatName:Direct=http://127.0.0.1/msmq/private$/ljctest111", false);
            ////mc.CreateIfNotExis();
            //int nowticks = Environment.TickCount;
            //int i = 0;
            //try
            //{
            //    while (true)
            //    {
            //        if (i == 10000)
            //        {
            //            break;
            //        }
            //        i++;

            //        mc.SendQueue("你好:" + i, false);
            //        //Console.WriteLine("发送第" + i + "条消息");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("出错" + i, ex.Message);
            //}
            //Console.WriteLine("完成ms:" + (Environment.TickCount - nowticks));

            //LJC.FrameWork.MSMQ.MsmqClient mc = new LJC.FrameWork.MSMQ.MsmqClient("172.31.56.129/ljctest111", false);
            ////mc.CreateIfNotExis();
            //int nowticks = Environment.TickCount;
            //int i = 0;
            //try
            //{
            //    while (true)
            //    {
            //        if (i == 10000)
            //        {
            //            break;
            //        }
            //        i++;

            //        mc.SendQueue("你好:" + i, false);
            //        //Console.WriteLine("发送第" + i + "条消息");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("出错" + i, ex.Message);
            //}
            //Console.WriteLine("完成ms:" + (Environment.TickCount - nowticks));

            var nowticks = Environment.TickCount;
            int cnt = 0;
            LJC.FrameWork.MSMQ.MsmqClient mc = new LJC.FrameWork.MSMQ.MsmqClient("./ljctest111", false);

            //LJC.FrameWork.MSMQ.MsmqClient
            foreach (var msg in mc.ReadQueue(1))
            {
                cnt++;
                //Console.WriteLine(msg.Body.ToString());
            }
            Console.WriteLine("完成ms:" + cnt + "条,用时" + (Environment.TickCount - nowticks));

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
                System.Threading.Tasks.Parallel.For(0, 15, (n) =>
                    {
                        long start = Environment.TickCount;
                        for (int i = 0; i < 100; i++)
                        {
                            try
                            {
                                start = Environment.TickCount;
                                var ids = "600358.SH,600086.SH,000768.SZ,300578.SZ,002624.SZ,603558.SH,300024.SZ,300532.SZ,601939.SH,601988.SH,002593.SZ,600318.SH,000540.SZ,601211.SH,000783.SZ,002847.SZ,603444.SH,600015.SH,002145.SZ,601288.SH,002131.SZ,600067.SH,300058.SZ,600628.SH,300242.SZ,600016.SH,600208.SH,601377.SH,000967.SZ,002712.SZ,300392.SZ,601888.SH,603099.SH,002143.SZ,300071.SZ,600276.SH,601398.SH,300513.SZ,600768.SH,603588.SH,603993.SH,000430.SZ,603729.SH,603881.SH,000630.SZ,000839.SZ,300144.SZ,600418.SH,600594.SH,603069.SH".Split(',');
                                //var ids = "600358.SH,600086.SH".Split(',');
                                var result = LJC.FrameWork.SOA.ESBClient.DoSOARequest<List<LJC.Com.StockService.Contract.StockSimpleInfo>>(1, 10021, ids);
                                long ms = Environment.TickCount - start;
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
            LJC.FrameWork.SocketApplication.MessageApp appClient = new LJC.FrameWork.SocketApplication.MessageApp();
            
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
