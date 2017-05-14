using Ljc.Com.NewsService.Entity;
using LJC.Com.StockService.Contract;
using LJC.FrameWork.Comm;
using LJC.FrameWork.Comm.TextReaderWriter;
using LJC.FrameWork.Data.Mongo;
using LJC.FrameWork.SOA;
using LJC.FrameWork.SocketApplication;
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

        static void Test111()
        {
            var innerCodes = "601668.SH,000520.SZ,601008.SH,600773.SH,300045.SZ,000591.SZ,601018.SH,601857.SH,002209.SZ,600028.SH,300324.SZ,601099.SH,600801.SH,002309.SZ,601398.SH,300123.SZ,600072.SH,601628.SH,600716.SH,600030.SH,601939.SH,000018.SZ,000980.SZ,000042.SZ,002564.SZ,600580.SH,002434.SZ,601288.SH,300427.SZ,000582.SZ,300024.SZ,601988.SH,300407.SZ,600005.SH,600019.SH,002061.SZ,600890.SH,600029.SH,600654.SH,600837.SH,601186.SH,601117.SH,002431.SZ,600369.SH,601800.SH,600717.SH,601669.SH,600068.SH,000776.SZ,000166.SZ".Split(',');

            var haserror = false;
            while (!haserror)
            {
                int max = 0, min = int.MaxValue, avg = 0;
                System.Collections.Concurrent.ConcurrentBag<int> timelist = new System.Collections.Concurrent.ConcurrentBag<int>();
                System.Threading.Tasks.Parallel.For(0, 10, (i) =>
                    {
                        var start = Environment.TickCount;
                        while (Environment.TickCount - start < 1000)
                        {
                            try
                            {
                                int timestart = Environment.TickCount;
                                var info = ESBClient.DoSOARequest<List<StockSimpleInfo>>(1, 10021, innerCodes);
                                if (info == null || info[innerCodes.Length - 1].ShortNameA != "申万宏源")
                                {
                                    throw new Exception("数据校验错误");
                                }
                                int timeend = Environment.TickCount;
                                timelist.Add(timeend - timestart);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                haserror = true;
                            }
                        }
                    });

                var list = timelist.OrderBy(p => p).ToList();
                max = list.Max();
                min = list.Min();
                avg = list.Sum() / timelist.Count;
                Console.WriteLine("次数:{0},最大:{1},最小:{2}，平均:{3}", list.Count, max, min, avg);

                list = list.Take((int)(timelist.Count * 0.9)).ToList();
                max = list.Max();
                min = list.Min();
                avg = list.Sum() / timelist.Count;
                Console.WriteLine("次数:{0},90%最大:{1},最小:{2}，平均:{3}", list.Count, max, min, avg);

                list = list.Take((int)(timelist.Count * 0.5)).ToList();
                max = list.Max();
                min = list.Min();
                avg = list.Sum() / timelist.Count;
                Console.WriteLine("次数:{0},50%最大:{1},最小:{2}，平均:{3}", list.Count, max, min, avg);
            }
            Console.Read();
        }

        static LJC.FrameWork.SocketApplication.SessionClient client = null;
        static void Main(string[] args)
        {
            Test111();
            return;
            string apiresult = "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";
            var apimessage = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(Convert.FromBase64String(apiresult).Skip(4).ToArray());
            var apiresponse = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<SOATransferResponse>(apimessage.MessageBuffer);
            var apiinfos = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<List<LJC.Com.StockService.Contract.StockSimpleInfo>>(apiresponse.Result);

            var ids = "601668.SH,000520.SZ,601008.SH,600773.SH,300045.SZ,000591.SZ,601018.SH,601857.SH,002209.SZ,600028.SH,300324.SZ,601099.SH,600801.SH,002309.SZ,601398.SH,300123.SZ,600072.SH,601628.SH,600716.SH,600030.SH,601939.SH,000018.SZ,000980.SZ,000042.SZ,002564.SZ,600580.SH,002434.SZ,601288.SH,300427.SZ,000582.SZ,300024.SZ,601988.SH,300407.SZ,600005.SH,600019.SH,002061.SZ,600890.SH,600029.SH,600654.SH,600837.SH,601186.SH,601117.SH,002431.SZ,600369.SH,601800.SH,600717.SH,601669.SH,600068.SH,000776.SZ,000166.SZ".Split(',');
            var result = LJC.FrameWork.SOA.ESBClient.DoSOARequest<List<LJC.Com.StockService.Contract.StockSimpleInfo>>(1, 10021, ids);
            var rightbuffer = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(result);
            var rightbufferbase64 = Convert.ToBase64String(rightbuffer);

            string bytes = "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";
            var buffermsg = Convert.FromBase64String(bytes);
            var demsg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(buffermsg.Skip(4).ToArray(), true);
            var soaresp = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<LJC.FrameWork.SOA.SOAResponse>(demsg.MessageBuffer);

            var respdata = Convert.ToBase64String(soaresp.Result);
            var realdata = "AAIyAIQe5pa55q2j6K+B5Yi46IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmlrnmraPor4HliLiEBjYwMTkwMYQJNjAxOTAxLlNIhARmenpxAIQq55SY6IKD6I6r6auY5a6e5Lia5Y+R5bGV6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzojqvpq5jogqHku72EBjYwMDU0M4QJNjAwNTQzLlNIhARtZ2dmAIQq5aSp5rSl5Yip5a6J6ZqG5paw5p2Q5paZ6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnliKnlronpmoaEBjMwMDU5NoQJMzAwNTk2LlNahANMQUwAhCTkuK3lm73pq5jnp5Hpm4blm6LogqHku73mnInpmZDlhazlj7iEDOS4reWbvemrmOenkYQGNjAwNzMwhAk2MDA3MzAuU0iEBHpnZ2sAhCTmna3lt57ogIHmnb/nlLXlmajogqHku73mnInpmZDlhazlj7iEDOiAgeadv+eUteWZqIQGMDAyNTA4hAkwMDI1MDguU1qEBExCRFEAhDDkuK3lm73lubPlronkv53pmanvvIjpm4blm6LvvInogqHku73mnInpmZDlhazlj7iEDOS4reWbveW5s+WuiYQGNjAxMzE4hAk2MDEzMTguU0iEBHpncGEAhB7nvo7nmoTpm4blm6LogqHku73mnInpmZDlhazlj7iEDOe+jueahOmbhuWbooQGMDAwMzMzhAkwMDAzMzMuU1qEBE1ESlQAhB7npZ7pm77oioLog73ogqHku73mnInpmZDlhazlj7iEDOelnumbvuiKguiDvYQGMDAwODIwhAkwMDA4MjAuU1qEBFNXSk4AhCHlrpzlrr7kupTnsq7mtrLogqHku73mnInpmZDlhazlj7iECeS6lOeyrua2soQGMDAwODU4hAkwMDA4NTguU1qEA1dMWQCEJOasp+a0vuWutuWxhembhuWbouiCoeS7veaciemZkOWFrOWPuIQM5qyn5rS+5a625bGFhAY2MDM4MzOECTYwMzgzMy5TSIQEb3BqagCEJOa5luWMl+S7sOW4huaOp+iCoeiCoeS7veaciemZkOWFrOWPuIQM5Luw5biG5o6n6IKhhAY2MDA0MjGECTYwMDQyMS5TSIQEeWZrZwCEKuiAgeeZvuWnk+Wkp+iNr+aIv+i/numUgeiCoeS7veaciemZkOWFrOWPuIQJ6ICB55m+5aeThAY2MDM4ODOECTYwMzg4My5TSIQDbGJ4AIQk5pa55aSn54m56ZKi56eR5oqA6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmlrnlpKfnibnpkqKEBjYwMDUwN4QJNjAwNTA3LlNIhARmZHRnAIQk5paw55aG6YeR6aOO56eR5oqA6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzph5Hpo47np5HmioCEBjAwMjIwMoQJMDAyMjAyLlNahARKRktKAIQt5bq35b6X5paw5aSN5ZCI5p2Q5paZ6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnlurflvpfmlrCEBjAwMjQ1MIQJMDAyNDUwLlNahANLRFgAhB7lpKnlub/kuK3ojILogqHku73mnInpmZDlhazlj7iEDOWkqeW5v+S4reiMgoQGMDAyNTA5hAkwMDI1MDkuU1qEBFRHWk0AhB7lurfnvo7oja/kuJrogqHku73mnInpmZDlhazlj7iEDOW6t+e+juiNr+S4moQGNjAwNTE4hAk2MDA1MTguU0iEBGtteXkAhCTmlrnmraPnp5HmioDpm4blm6LogqHku73mnInpmZDlhazlj7iEDOaWueato+enkeaKgIQGNjAwNjAxhAk2MDA2MDEuU0iEBGZ6a2oAhB7kuInkuIDph43lt6XogqHku73mnInpmZDlhazlj7iEDOS4ieS4gOmHjeW3pYQGNjAwMDMxhAk2MDAwMzEuU0iEBHN5emcAhB7lm73ph5Hor4HliLjogqHku73mnInpmZDlhazlj7iEDOWbvemHkeivgeWIuIQGNjAwMTA5hAk2MDAxMDkuU0iEBGdqenEAhCHmlrDluIzmnJvlha3lkozogqHku73mnInpmZDlhazlj7iECeaWsOW4jOacm4QGMDAwODc2hAkwMDA4NzYuU1qEA1hYVwCEKuWMl+S6rOWbm+e7tOWbvuaWsOenkeaKgOiCoeS7veaciemZkOWFrOWPuIQM5Zub57u05Zu+5pawhAYwMDI0MDWECTAwMjQwNS5TWoQEU1dUWACEHuWNjuWkj+mTtuihjOiCoeS7veaciemZkOWFrOWPuIQM5Y2O5aSP6ZO26KGMhAY2MDAwMTWECTYwMDAxNS5TSIQEaHh5aACEJOW5v+ilv+afs+W3peacuuaisOiCoeS7veaciemZkOWFrOWPuIQG5p+z5belhAYwMDA1MjiECTAwMDUyOC5TWoQCTEcAhCTnj6DmtbfmoLzlipvnlLXlmajogqHku73mnInpmZDlhazlj7iEDOagvOWKm+eUteWZqIQGMDAwNjUxhAkwMDA2NTEuU1qEBEdMRFEAhCrljqbpl6jkur/ogZTnvZHnu5zmioDmnK/ogqHku73mnInpmZDlhazlj7iEDOS6v+iBlOe9kee7nIQGMzAwNjI4hAkzMDA2MjguU1qEBFlMV0wAhB7lhoDkuK3og73mupDogqHku73mnInpmZDlhazlj7iEDOWGgOS4reiDvea6kIQGMDAwOTM3hAkwMDA5MzcuU1qEBEpaTlkAhCrmlrnlpKfplKbljJbljJblt6Xnp5HmioDogqHku73mnInpmZDlhazlj7iEDOaWueWkp+WMluW3pYQGMDAwODE4hAkwMDA4MTguU1qEBEZESEcAhCfmlrDnlobkvIrlipvnibnlrp7kuJrogqHku73mnInpmZDlhazlj7iECeS8iuWKm+eJuYQGNjAwMTk3hAk2MDAxOTcuU0iEA3lsdACEHuS4iua1t+mTtuihjOiCoeS7veaciemZkOWFrOWPuIQM5LiK5rW36ZO26KGMhAY2MDEyMjmECTYwMTIyOS5TSIQEc2h5aACEJ+WMl+S6rOeip+awtOa6kOenkeaKgOiCoeS7veaciemZkOWFrOWPuIQJ56Kn5rC05rqQhAYzMDAwNzCECTMwMDA3MC5TWoQDQlNZAIQq5ZCv6L+q5qGR5b63546v5aKD6LWE5rqQ6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlkK/ov6rmoZHlvreEBjAwMDgyNoQJMDAwODI2LlNahARRRFNEAIQb5q+U5Lqa6L+q6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnmr5Tkuprov6qEBjAwMjU5NIQJMDAyNTk0LlNahANCWUQAhCTmlrDljY7kurrlr7/kv53pmanogqHku73mnInpmZDlhazlj7iEDOaWsOWNjuS/nemZqYQGNjAxMzM2hAk2MDEzMzYuU0iEBHhoYngAhDbkuIrmtbfpmYblrrblmLTph5Hono3otLjmmJPljLrlvIDlj5HogqHku73mnInpmZDlhazlj7iECemZhuWutuWYtIQGNjAwNjYzhAk2MDA2NjMuU0iEA2xqegCEKuW5v+W3nueZveS6keWbvemZheacuuWcuuiCoeS7veaciemZkOWFrOWPuIQM55m95LqR5py65Zy6hAY2MDCFrOWPuIQM5LiJ5LiD5LqS5aixhAYwMDI1NTWECTAwMjU1NS5TWoQEU1FIWQCEHuazm+a1t+aOp+iCoeiCoeS7veaciemZkOWFrOWPuIQM5rOb5rW35o6n6IKhhAYwMDAwNDaECTAwMDA0Ni5TWoQERkhLRwCEKuWMl+S6rOS4ieWkq+aIt+WklueUqOWTgeiCoeS7veaciemZkOWFrOWPuIQM5LiJ5aSr5oi35aSWhAYwMDI3ODCECTAwMjc4MC5TWoQEU0ZIVwCEHuWMl+Wkp+WMu+iNr+iCoeS7veaciemZkOWFrOWPuIQM5YyX5aSn5Yy76I2vhAYwMDA3ODiECTAwMDc4OC5TWoQEQkRZWQCEJ+a3seWcs+W4gumVv+S6ruenkeaKgOiCoeS7veaciemZkOWFrOWPuIQM6ZW/5Lqu56eR5oqAhAYzMDAzNDiECTMwMDM0OC5TWoQEQ0xLSgCEIeWlpeeRnumHkeWMheijheiCoeS7veaciemZkOWFrOWPuIQJ5aWl55Ge6YeRhAYwMDI3MDGECTAwMjcwMS5TWoQDQVJKAIQk6ZW/5rKZ5byA5YWD5Luq5Zmo6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlvIDlhYPogqHku72EBjMwMDMzOIQJMzAwMzM4LlNahARLWUdGAIQq5YyX5Lqs5riF5paw546v5aKD5oqA5pyv6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmuIXmlrDnjq/looOEBjAwMjU3M4QJMDAyNTczLlNahARRWEhKAIQk5rex5Zyz5biC5Yac5Lqn5ZOB6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnlhpzkuqflk4GEBjAwMDA2MYQJMDAwMDYxLlNahANOQ1AAhBvogIHlh6TnpaXogqHku73mnInpmZDlhazlj7iECeiAgeWHpOelpYQGNjAwNjEyhAk2MDA2MTIuU0iEA2xmeACEKuWUkOWxseWGgOS4nOijheWkh+W3peeoi+iCoeS7veaciemZkOWFrOWPuIQM5YaA5Lic6KOF5aSHhAYwMDA4NTaECTAwMDg1Ni5TWoQESkRaQgCEHuWbveS/oeivgeWIuOiCoeS7veaciemZkOWFrOWPuIQM5Zu95L+h6K+B5Yi4hAYwMDI3MzaECTAwMjczNi5TWoQER1haUQCEL+emj+W7uuecgeawuOWuieael+S4mijpm4blm6Ip6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmsLjlronmnpfkuJqEBjAwMDY2M4QJMDAwNjYzLlNahARZQUxZAIQn5YyX5Lqs5rGH5Yag5paw5oqA5pyv6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmsYflhqDogqHku72EBjMwMDI4MoQJMzAwMjgyLlNahARIR0dGAIQh6YeR6LC35rqQ5o6n6IKh6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAkqU1Tph5HmupCEBjAwMDQwOIQJMDAwNDA4LlNahAUqU1RK";

            var eq = respdata == realdata;
            var eq2 = respdata == rightbufferbase64;

            int o = 0;
            for (o = 0; o < respdata.Length; o++)
            {
                if (respdata[o] != rightbufferbase64[o])
                {
                    break;
                }
            }

            int p = 0;
            for (p = 0; p < rightbuffer.Length; p++)
            {
                if (rightbuffer[p] != soaresp.Result[p])
                {
                    break;
                }
            }

            var subdata = soaresp.Result.Skip(p - 1).ToArray();
            var subdatalen = BitConverter.ToInt32(subdata.Take(4).ToArray(), 0);

            //var listsimpleinfo = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<List<StockSimpleInfo>>(soaresp.Result);
            return;

            //System.Diagnostics.Stopwatch sw1 = new System.Diagnostics.Stopwatch();
            //while (true)
            //{
                
            //    sw1.Restart();
            //    var stockList = ESBClient.DoSOARequest<List<StockSimpleInfo>>(1, 1004, null).Where(p => !string.IsNullOrEmpty(p.ShortNameA)).ToList();
            //    sw1.Stop();
            //    var str = stockList.ToJson().Length;
            //    Console.WriteLine(sw1.ElapsedMilliseconds);
            //}
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

      
            var lists = LJC.FrameWork.Data.Mongo.MongoDBHelper.FindAll<NewsEntity>(mongoname, dbname, collname, new LJC.FrameWork.Data.Mongo.MongoSortWarpper<NewsEntity>().Desc(q => q.Clicktime));

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
                    .ToDictionary(q => q.CategoryCode);
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
