using Ljc.Com.NewsService.Entity;
using LJC.Com.StockService.Contract;
using LJC.FrameWork.Comm;
using LJC.FrameWork.Comm.TextReaderWriter;
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
            service.SessionResume += new Action(() => { service.RegisterService(); Console.WriteLine("恢复重注册"); });
            service.Error += service_Error;
            service.OnClientReset += new Action(() => { service.RegisterService(); Console.WriteLine("重置重注册"); });
            service.Login(null, null);
        }

        static void service_Error(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }

        static LJC.FrameWork.SocketApplication.SocketSTD.SessionClient client = null;
        static void Main(string[] args)
        {
            //while (true)
            //{
            //    try
            //    {
            //        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //        sw.Start();
            //        //var result11 = ESBClient.DoSOARequest2<TradeStatusInfo>(1, 9009, DateTime.Now);
            //        var list = ESBClient.DoSOARequest2<List<StockBaseInfo>>(LJC.Com.StockService.Contract.Consts.ServiceNo, LJC.Com.StockService.Contract.Consts.FunID_GetAllStockBaseInfo, null);
            //        sw.Stop();
            //        Console.WriteLine(sw.ElapsedMilliseconds);

            //        //Thread.Sleep(10);
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}
            //Console.Read();
            //return;

            //TestEsbservices();
            //Console.Read();
            //return;

            //ClientBase2 clientbase =new ClientBase2("192.168.0.100", 50000);
            SessionClient clientbase = new SessionClient("127.0.0.1", 19000);
            //ClientBase2 clientbase = new ClientBase2("2.5.176.91", 50000);
            //LJC.FrameWork.SocketEasyUDP.Client.ClientBase clientbase = new LJC.FrameWork.SocketEasyUDP.Client.ClientBase("172.31.56.129", 50000);
            //SessionClient clientbase = new SessionClient("106.14.193.150", 19000);
            clientbase.StartClient();
            if (clientbase.ClearTempData())
            {
                Console.WriteLine("清理成功");
            }
            //if (clientbase.SetMTU(1272))
            //{
            //    Console.WriteLine("mtu设置成功");
            //}
            //else
            //{
            //    Console.WriteLine("mtu设置失败");
            //}

            clientbase.Login(string.Empty, string.Empty);

            clientbase.LoginSuccess += () =>
                {
                    System.Diagnostics.Stopwatch sw20 = new System.Diagnostics.Stopwatch();
                    sw20.Start();
                    if (clientbase.SendFile(@"E:\Work\learn\Git\LJC.FrameWork\Test2\bin\Release\Framework.Logging.V2.API.dll",null, 1024 * 1000, (q) => Console.WriteLine("上传进度:" + q * 100 + "%")))
                    {
                        sw20.Stop();
                        Console.Write("上传成功:" + sw20.ElapsedMilliseconds);
                    }
            
                };
            
           

            Console.Read();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 30000; i++)
            {
                sb.Append(i.ToString());
            }
            var testmsg = new Message
                {
                    MessageHeader = new MessageHeader() { MessageType = (int)MessageType.LOGIN }
                };
            testmsg.SetMessageBody(sb.ToString());
            

            int sendcnt = 0;

            System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch();
            for (int i = 0; i < 5; i++)
            {
                sw2.Restart();
                clientbase.SendMessage(testmsg, null);
                sw2.Stop();
                Console.WriteLine("成功:" + ++sendcnt + "," + sw2.ElapsedMilliseconds + "ms");

            }
            
            Console.Read();
            return;

            var buffer0 = Convert.FromBase64String("AAAAAgUBAAAAAAAAAAACAwABAQ==");
            var crc320 = LJC.FrameWork.Comm.HashEncrypt.GetCRC32(buffer0, 0);

            var bytesnum = new byte[6];
            var numb = BitConverter.GetBytes(123);
            for (int i = 0; i < 4; i++)
            {
                bytesnum[i + 1] = numb[i];
            }
            int crc32 = LJC.FrameWork.Comm.HashEncrypt.GetCRC32(bytesnum, 0);
            var num = BitConverter.ToInt32(bytesnum, 1);

            string newsID = "100";
            string encryNewsId = Convert.ToBase64String(Encoding.ASCII.GetBytes(new LJC.FrameWork.Comm.EncryHelper().Encrypto(newsID)));
            string realnewsid = new LJC.FrameWork.Comm.EncryHelper().Decrypto(Encoding.ASCII.GetString(Convert.FromBase64String(encryNewsId)));

            var ids = "601901.SH,600543.SH,300596.SZ,600730.SH,002508.SZ,601318.SH,000333.SZ,000820.SZ,000858.SZ,603833.SH,600421.SH,603883.SH,600507.SH,002202.SZ,002450.SZ,002509.SZ,600518.SH,600601.SH,600031.SH,600109.SH,000876.SZ,002405.SZ,600015.SH,000528.SZ,000651.SZ,300628.SZ,000937.SZ,000818.SZ,600197.SH,601229.SH,300070.SZ,000826.SZ,002594.SZ,601336.SH,600663.SH,600004.SH,002555.SZ,000046.SZ,002780.SZ,000788.SZ,300348.SZ,002701.SZ,300338.SZ,002573.SZ,000061.SZ,600612.SH,000856.SZ,002736.SZ,000663.SZ,300282.SZ".Split(',');
            var result = LJC.FrameWork.SOA.ESBClient.DoSOARequest<List<LJC.Com.StockService.Contract.StockSimpleInfo>>(1, 10021, ids);
            var rightbuffer = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(result);
            var rightbufferbase64 = Convert.ToBase64String(rightbuffer);

            string bytes = "dQ8AAAAAAAJlhCVhMWMyZDYwNTIyNzU0ZTMzYjMwM2QxOGU4ZGViYmVkN181MzgzAAAAAAAAAAAEPg8AAQAAAAAAAAAAAQQwDwACMgCEHuaWueato+ivgeWIuOiCoeS7veaciemZkOWFrOWPuIQM5pa55q2j6K+B5Yi4hAY2MDE5MDGECTYwMTkwMS5TSIQEZnp6cQCEKueUmOiCg+iOq+mrmOWunuS4muWPkeWxleiCoeS7veaciemZkOWFrOWPuIQM6I6r6auY6IKh5Lu9hAY2MDA1NDOECTYwMDU0My5TSIQEbWdnZgCEKuWkqea0peWIqeWuiemahuaWsOadkOaWmeiCoeS7veaciemZkOWFrOWPuIQJ5Yip5a6J6ZqGhAYzMDA1OTaECTMwMDU5Ni5TWoQDTEFMAIQk5Lit5Zu96auY56eR6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuK3lm73pq5jnp5GEBjYwMDczMIQJNjAwNzMwLlNIhAR6Z2drAIQk5p2t5bee6ICB5p2/55S15Zmo6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzogIHmnb/nlLXlmaiEBjAwMjUwOIQJMDAyNTA4LlNahARMQkRRAIQw5Lit5Zu95bmz5a6J5L+d6Zmp77yI6ZuG5Zui77yJ6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuK3lm73lubPlromEBjYwMTMxOIQJNjAxMzE4LlNIhAR6Z3BhAIQe576O55qE6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAznvo7nmoTpm4blm6KEBjAwMDMzM4QJMDAwMzMzLlNahARNREpUAIQe56We6Zu+6IqC6IO96IKh5Lu95pyJ6ZmQ5YWs5Y+4hAznpZ7pm77oioLog72EBjAwMDgyMIQJMDAwODIwLlNahARTV0pOAIQh5a6c5a6+5LqU57Ku5ray6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnkupTnsq7mtrKEBjAwMDg1OIQJMDAwODU4LlNahANXTFkAhCTmrKfmtL7lrrblsYXpm4blm6LogqHku73mnInpmZDlhazlj7iEDOasp+a0vuWutuWxhYQGNjAzODMzhAk2MDM4MzMuU0iEBG9wamoAhCTmuZbljJfku7DluIbmjqfogqHogqHku73mnInpmZDlhazlj7iEDOS7sOW4huaOp+iCoYQGNjAwNDIxhAk2MDA0MjEuU0iEBHlma2cAhCrogIHnmb7lp5PlpKfoja/miL/ov57plIHogqHku73mnInpmZDlhazlj7iECeiAgeeZvuWnk4QGNjAzODgzhAk2MDM4ODMuU0iEA2xieACEJOaWueWkp+eJuemSouenkeaKgOiCoeS7veaciemZkOWFrOWPuIQM5pa55aSn54m56ZKihAY2MDA1MDeECTYwMDUwNy5TSIQEZmR0ZwCEJOaWsOeWhumHkemjjuenkeaKgOiCoeS7veaciemZkOWFrOWPuIQM6YeR6aOO56eR5oqAhAYwMDIyMDKECTAwMjIwMi5TWoQESkZLSgCELeW6t+W+l+aWsOWkjeWQiOadkOaWmembhuWbouiCoeS7veaciemZkOWFrOWPuIQJ5bq35b6X5pawhAYwMDI0NTCECTAwMjQ1MC5TWoQDS0RYAIQe5aSp5bm/5Lit6IyC6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlpKnlub/kuK3ojIKEBjAwMjUwOYQJMDAyNTA5LlNahARUR1pNAIQe5bq3576O6I2v5Lia6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlurfnvo7oja/kuJqEBjYwMDUxOIQJNjAwNTE4LlNIhARrbXl5AIQk5pa55q2j56eR5oqA6ZuG5Zui6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmlrnmraPnp5HmioCEBjYwMDYwMYQJNjAwNjAxLlNIhARmemtqAIQe5LiJ5LiA6YeN5bel6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkuInkuIDph43lt6WEBjYwMDAzMYQJNjAwMDMxLlNIhARzeXpnAIQe5Zu96YeR6K+B5Yi46IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlm73ph5Hor4HliLiEBjYwMDEwOYQJNjAwMTA5LlNIhARnanpxAIQh5paw5biM5pyb5YWt5ZKM6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnmlrDluIzmnJuEBjAwMDg3NoQJMDAwODc2LlNahANYWFcAhCrljJfkuqzlm5vnu7Tlm77mlrDnp5HmioDogqHku73mnInpmZDlhazlj7iEDOWbm+e7tOWbvuaWsIQGMDAyNDA1hAkwMDI0MDUuU1qEBFNXVFgAhB7ljY7lpI/pk7booYzogqHku73mnInpmZDlhazlj7iEDOWNjuWkj+mTtuihjIQGNjAwMDE1hAk2MDAwMTUuU0iEBGh4eWgAhCTlub/opb/mn7Plt6XmnLrmorDogqHku73mnInpmZDlhazlj7iEBuafs+W3pYQGMDAwNTI4hAkwMDA1MjguU1qEAkxHAIQk54+g5rW35qC85Yqb55S15Zmo6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmoLzlipvnlLXlmaiEBjAwMDY1MYQJMDAwNjUxLlNahARHTERRAIQq5Y6m6Zeo5Lq/6IGU572R57uc5oqA5pyv6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzkur/ogZTnvZHnu5yEBjMwMDYyOIQJMzAwNjI4LlNahARZTFdMAIQe5YaA5Lit6IO95rqQ6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzlhoDkuK3og73mupCEBjAwMDkzN4QJMDAwOTM3LlNahARKWk5ZAIQq5pa55aSn6ZSm5YyW5YyW5bel56eR5oqA6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmlrnlpKfljJblt6WEBjAwMDgxOIQJMDAwODE4LlNahARGREhHAIQn5paw55aG5LyK5Yqb54m55a6e5Lia6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnkvIrlipvnibmEBjYwMDE5N4QJNjAwMTk3LlNIhAN5bHQAhB7kuIrmtbfpk7booYzogqHku73mnInpmZDlhazlj7iEDOS4iua1t+mTtuihjIQGNjAxMjI5hAk2MDEyMjkuU0iEBHNoeWgAhCfljJfkuqznoqfmsLTmupDnp5HmioDogqHku73mnInpmZDlhazlj7iECeeip+awtOa6kIQGMzAwMDcwhAkzMDAwNzAuU1qEA0JTWQCEKuWQr+i/quahkeW+t+eOr+Wig+i1hOa6kOiCoeS7veaciemZkOWFrOWPuIQM5ZCv6L+q5qGR5b63hAYwMDA4MjaECTAwMDgyNi5TWoQEUURTRACEG+avlOS6mui/quiCoeS7veaciemZkOWFrOWPuIQJ5q+U5Lqa6L+qhAYwMDI1OTSECTAwMjU5NC5TWoQDQllEAIQk5paw5Y2O5Lq65a+/5L+d6Zmp6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAzmlrDljY7kv53pmamEBjYwMTMzNoQJNjAxMzM2LlNIhAR4aGJ4AIQ25LiK5rW36ZmG5a625Zi06YeR6J6N6LS45piT5Yy65byA5Y+R6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnpmYblrrblmLSEBjYwMDY2M4QJNjAwNjYzLlNIhANsanoAhCrlub/lt57nmb3kupHlm73pmYXmnLrlnLrogqHku73mnInpmZDlhazlj7iEDOeZveS6keacuuWcuoQGNjAwhazlj7iEDOS4ieS4g+S6kuWosYQGMDAyNTU1hAkwMDI1NTUuU1qEBFNRSFkAhB7ms5vmtbfmjqfogqHogqHku73mnInpmZDlhazlj7iEDOazm+a1t+aOp+iCoYQGMDAwMDQ2hAkwMDAwNDYuU1qEBEZIS0cAhCrljJfkuqzkuInlpKvmiLflpJbnlKjlk4HogqHku73mnInpmZDlhazlj7iEDOS4ieWkq+aIt+WkloQGMDAyNzgwhAkwMDI3ODAuU1qEBFNGSFcAhB7ljJflpKfljLvoja/ogqHku73mnInpmZDlhazlj7iEDOWMl+Wkp+WMu+iNr4QGMDAwNzg4hAkwMDA3ODguU1qEBEJEWVkAhCfmt7HlnLPluILplb/kuq7np5HmioDogqHku73mnInpmZDlhazlj7iEDOmVv+S6ruenkeaKgIQGMzAwMzQ4hAkzMDAzNDguU1qEBENMS0oAhCHlpaXnkZ7ph5HljIXoo4XogqHku73mnInpmZDlhazlj7iECeWlpeeRnumHkYQGMDAyNzAxhAkwMDI3MDEuU1qEA0FSSgCEJOmVv+aymeW8gOWFg+S7quWZqOiCoeS7veaciemZkOWFrOWPuIQM5byA5YWD6IKh5Lu9hAYzMDAzMziECTMwMDMzOC5TWoQES1lHRgCEKuWMl+S6rOa4heaWsOeOr+Wig+aKgOacr+iCoeS7veaciemZkOWFrOWPuIQM5riF5paw546v5aKDhAYwMDI1NzOECTAwMjU3My5TWoQEUVhISgCEJOa3seWcs+W4guWGnOS6p+WTgeiCoeS7veaciemZkOWFrOWPuIQJ5Yac5Lqn5ZOBhAYwMDAwNjGECTAwMDA2MS5TWoQDTkNQAIQb6ICB5Yek56Wl6IKh5Lu95pyJ6ZmQ5YWs5Y+4hAnogIHlh6TnpaWEBjYwMDYxMoQJNjAwNjEyLlNIhANsZngAhCrllJDlsbHlhoDkuJzoo4XlpIflt6XnqIvogqHku73mnInpmZDlhazlj7iEDOWGgOS4nOijheWkh4QGMDAwODU2hAkwMDA4NTYuU1qEBEpEWkIAhB7lm73kv6Hor4HliLjogqHku73mnInpmZDlhazlj7iEDOWbveS/oeivgeWIuIQGMDAyNzM2hAkwMDI3MzYuU1qEBEdYWlEAhC/npo/lu7rnnIHmsLjlronmnpfkuJoo6ZuG5ZuiKeiCoeS7veaciemZkOWFrOWPuIQM5rC45a6J5p6X5LiahAYwMDA2NjOECTAwMDY2My5TWoQEWUFMWQCEJ+WMl+S6rOaxh+WGoOaWsOaKgOacr+iCoeS7veaciemZkOWFrOWPuIQM5rGH5Yag6IKh5Lu9hAYzMDAyODKECTMwMDI4Mi5TWoQESEdHRgCEIemHkeiwt+a6kOaOp+iCoeiCoeS7veaciemZkOWFrOWPuIQJKlNU6YeR5rqQhAYwMDA0MDiECTAwMDQwOC5TWoQFKlNUSg==";
            var buffermsg = Convert.FromBase64String(bytes);
            var demsg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(buffermsg.Skip(4).ToArray(), true);
            var soaresp = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<LJC.FrameWork.SOA.SOAResponse>(demsg.MessageBuffer);

            var respdata = Convert.ToBase64String(soaresp.Result);
            var realdata = "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";

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
                var list = ESBClient.DoSOARequest<List<CategoryLevel2Entity>>(LJC.Com.StockService.Contract.Consts.ServiceNo, LJC.Com.StockService.Contract.Consts.FunID_GetCategoryLevel2List, null)
                    .ToDictionary(q => q.CategoryCode);
                sw.Stop();
                Thread.Sleep(10);
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
            client = new LJC.FrameWork.SocketApplication.SocketSTD.SessionClient("127.0.0.1", 5555, true);
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
