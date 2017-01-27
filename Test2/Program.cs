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

        static void Main(string[] args)
        {
            //TryRead1();

            //LJC.FrameWork.SocketApplication.SessionClient client = new LJC.FrameWork.SocketApplication.SessionClient("127.0.0.1", 5555, true);
            //client.LoginSuccess += client_LoginSuccess;
            //client.Error += client_Error;
            //client.Login("", "");
            //Console.Read();


            //LJC.FrameWork.MSMQ.MsmqClient mc = new LJC.FrameWork.MSMQ.MsmqClient(".\\private$\\ljctest111",false,false);
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

            LJC.FrameWork.MSMQ.MsmqClient mc = new LJC.FrameWork.MSMQ.MsmqClient(".\\private$\\ljctest111", false, true);
            mc.CreateIfNotExis();
            int nowticks = Environment.TickCount;
            int i = 0;
            try
            {
                while (true)
                {
                    if (i == 10000)
                    {
                        break;
                    }
                    i++;

                    mc.SendQueue("你好:" + i, false);
                    Console.WriteLine("发送第" + i + "条消息");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("出错" + i, ex.Message);
            }
            Console.WriteLine("完成ms:" + (Environment.TickCount - nowticks));

            //var nowticks = Environment.TickCount;
            //int cnt = 0;
            //LJC.FrameWork.MSMQ.MsmqClient mc = new LJC.FrameWork.MSMQ.MsmqClient(".\\private$\\ljctest111", false, true);
            //foreach (var msg in mc.ReadQueue(1))
            //{
            //    cnt++;
            //    //Console.WriteLine(msg.Body.ToString());
            //}
            //Console.WriteLine("完成ms:" + cnt + "条,用时" + (Environment.TickCount - nowticks));

            Console.Read();
        }

        static void client_Error(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }

        static void client_LoginSuccess()
        {
            Console.WriteLine("登录成功");
            var resp = LJC.FrameWork.SOA.ESBClient.DoSOARequest<LJC.FrameWork.SOA.SOAServerEchoResponse>(0, 1, null);

            var ms = Environment.TickCount;

            for(int i=0;i<100000;i++)
            {
                resp = LJC.FrameWork.SOA.ESBClient.DoSOARequest<LJC.FrameWork.SOA.SOAServerEchoResponse>(0, 1, null);
            }

            Console.WriteLine("用时ms:" + (Environment.TickCount - ms));
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
