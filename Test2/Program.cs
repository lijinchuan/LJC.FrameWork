using LJC.FrameWork.Comm;
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

        static LJC.FrameWork.SocketApplication.SessionClient client = null;
        static void Main(string[] args)
        {
            TestEntityBuf();

            var content1 = @"[
                                {name:'cjt',age:10},
                                {name:'cjt2',age:21},
                                {name:'cjtc',age:16}
                            ]";

            var res1 = JsonHelper.JsonToEntity<dynamic>(content1);
            var ss = JsonHelper.ToJson(res1);
            //TryRead1();

            ThreadPool.SetMinThreads(100, 100);
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


        static void TestEntityBuf()
        {
            string bytestring = "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";

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
