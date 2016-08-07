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
            string filename = @"D:\GitHub\LJC.FrameWork\Test\bin\Debug\testrwobj.bin";
            using (LJC.FrameWork.Comm.ObjTextReader reader = LJC.FrameWork.Comm.ObjTextReader.CreateReader(filename))
            {
                int i = 0;

                Man man = null;
                while ((man=reader.ReadObject<Man>())!=null)
                {
                    //var man = reader.ReadObject<Man>();
                    Console.WriteLine(man.Name);
                    Thread.Sleep(1);
                }
            }
        }

        static void TryRead2()
        {
            string filename = @"D:\GitHub\LJC.FrameWork\Test\bin\Debug\testrwobj.bin";
            using (LJC.FrameWork.Comm.ObjTextReader reader = LJC.FrameWork.Comm.ObjTextReader.CreateReader(filename))
            {
                foreach(var item in reader.ReadObjectWating<Man>())
                {
                    Console.WriteLine(item.Name);
                }
            }
        }

        static void Main(string[] args)
        {
            TryRead2();

            //LJC.FrameWork.SocketApplication.SessionClient client = new LJC.FrameWork.SocketApplication.SessionClient("127.0.0.1", 5555, true);
            //client.LoginSuccess += client_LoginSuccess;
            //client.Error += client_Error;
            //client.Login("", "");
            Console.Read();
        }

        static void client_Error(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }

        static void client_LoginSuccess()
        {
            Console.WriteLine("登录成功");
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
