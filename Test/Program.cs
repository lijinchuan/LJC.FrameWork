﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork;
using LJC.FrameWork.Comm;
using LJC.FrameWork.Data.QuickDataBase;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Data.SqlClient;
using LJC.FrameWork.Data;
using System.Linq.Expressions;
using System.Threading;

namespace Test
{
    public enum TestEnum
    {
        stud=1,
        learn=2,
        book=4,
        man=8,
        tree=16
    }



    class Program
    {
        public static void TestGZIP()
        {
            BufferPollManager poll=new BufferPollManager(100,1024*1000);

            string str = "我是中国人的，我常常爱着我的祖asfdgefaserfaTimeout 时间已到。";

            RunConfig rc = new RunConfig();
            rc.CmdPhoneNumber = str;
            rc.DefaultEmailAccount = str;
            rc.DefaultEmailPwd = str;
            rc.DefaultReciveEmailAccount = str;
            rc.ID = 12456;
            rc.StopEarn = 12.0;
            rc.CmdPhoneNumber = str;
            byte[] bytes = null;

            var utf8bytes = Encoding.UTF8.GetBytes(str);
            var cputf8bytes = GZip.Compress(utf8bytes);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(rc);
            sw.Restart();

            int bufferindex=-1;
            long size = 0;
            for (int i = 0; i < 100000; i++)
            {
                //string str1 = JsonUtil<object>.Serialize(rc);
                //var obj = JsonUtil<RunConfig>.Deserialize(str1);
                LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(rc,poll,ref bufferindex,ref size,ref bytes);// Encoding.UTF8.GetBytes(str);
                //LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(rc);
                if (bytes == null)
                {

                }
                else
                {
                    poll.RealseBuffer(bufferindex);
                }
            }
            sw.Stop();
            Console.WriteLine("序列化用时" + sw.ElapsedMilliseconds);

            Console.WriteLine("原始长度:" + bytes.Length);

            return;

            byte[] commpressbytes = null, decompressbytes = null;
            sw.Restart();

            for (int i = 0; i < 10000; i++)
            {
                commpressbytes = GZip.Compress(bytes);
            }

            sw.Stop();

            Console.WriteLine("压缩用时" + sw.ElapsedMilliseconds + "，压缩后长度:" + commpressbytes.Length);


            sw.Restart();

            for (int i = 0; i < 10000; i++)
            {
                decompressbytes = GZip.Decompress(commpressbytes);
            }

            sw.Stop();

            Console.WriteLine("解压缩用时" + sw.ElapsedMilliseconds + ",解压缩后长度" + decompressbytes.Length);

            Console.Read();
        }

        static void TestMs()
        {
            byte[] buffer = new byte[512];

            string s = "你好啊，今天怎么样啊";
            int x = 256;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer, x, 256))
            {
                var bts = System.Text.Encoding.UTF8.GetBytes(s);

                ms.Write(bts, 0, bts.Length);

                string r = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }

            s = "今天天气不太好";
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer, x, 256))
            {
                var bts = System.Text.Encoding.UTF8.GetBytes(s);

                ms.Write(bts, 0, bts.Length);

                string r = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        static void Main(string[] args)
        {
            Console.Read();

            LJC.FrameWork.SocketEasyUDP.Server.ServerBase serverbase = new LJC.FrameWork.SocketEasyUDP.Server.ServerBase(50000);
            serverbase.StartServer();

            Console.WriteLine("服务启动...");

            Console.Read();

            var list = LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<RunConfig>.GetDataContext().ExecuteList();

            var newrunconfig = list.FirstOrDefault();
            newrunconfig.ID = 0;
            LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<RunConfig>.GetDataContext(newrunconfig).Add();
            var list2 = LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<RunConfig>.GetDataContext().ExecuteList();

            newrunconfig.UserID = "123456";
            LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<RunConfig>.GetDataContext(newrunconfig).Update();
            list2 = LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<RunConfig>.GetDataContext().ExecuteList();

            LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<RunConfig>.GetDataContext(new RunConfig { ID = list2.Last().ID}).Del();
            list2 = LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<RunConfig>.GetDataContext().ExecuteList();
            Console.Read();
            return;

            ThreadPool.SetMinThreads(100, 100);

            MySession server = new MySession();
            server.StartServer();

            Console.Read();

            //TestGZIP();
            //TestMs();

            //Console.Read();
            //return;

            var client = LJC.FrameWork.Redis.RedisManager.GetClient("Host_Redis");
            client.Set("name", "ljc123456asdfasdfdsaf");

            int i = 0;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Restart();
            while (i++ < 100000)
            {
                
                var s = client.Get<string>("name");
                
            }

            sw.Stop();

            Console.WriteLine("取到数据,用时ms:" + sw.ElapsedMilliseconds);

            Console.Read();
        }

        static void server_OnAppMessage(LJC.FrameWork.SocketApplication.Session arg1, LJC.FrameWork.SocketApplication.Message arg2)
        {
            Console.WriteLine(arg2.MessageHeader.TransactionID);
        }

        static void Main1(string[] args)
        {

            LJC.FrameWork.SocketApplication.MessageApp appServer = new LJC.FrameWork.SocketApplication.MessageApp();
            appServer.EnableBroadCast = true;
            appServer.EnableMultiCast = true;
            var msg = new LJC.FrameWork.SocketApplication.Message
            {
                MessageHeader = new LJC.FrameWork.SocketApplication.MessageHeader
                {
                    MessageTime = DateTime.Now,
                    TransactionID = "1122",
                    MessageType = 2000
                },
            };
            msg.SetMessageBody(@"在进行UDP编程的时候,我们最容易想到的问题就是,一次发送多少bytes好?
当然,这个没有唯一答案，相对于不同的系统,不同的要求,其得到的答案是不一样的,我这里仅对
像ICQ一类的发送聊天消息的情况作分析，对于其他情况，你或许也能得到一点帮助:
首先,我们知道,TCP/IP通常被认为是一个四层协议系统,包括链路层,网络层,运输层,应用层.
UDP属于运输层,下面我们由下至上一步一步来看:
");
            appServer.BroadCast(msg);
            appServer.MultiCast(msg);
        }


        static void TestRWObj()
        {
            int i = 0;
            string filename="testrwobj.bin";

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            using(LJC.FrameWork.Comm.ObjTextWriter writer = ObjTextWriter.CreateWriter(filename, ObjTextReaderWriterEncodeType.protobuf))
            {
                while ((i++) < 1000000)
                {
                    writer.AppendObject<Man>(new Man
                    {
                        Name = "李金川"+i,
                        IDCard = "421182198612301310",
                        Addr = "湖北省武穴市",
                        Sex = 1
                    });

                    //writer.Flush();

                    if (i % 10000 == 0)
                        Console.WriteLine("写入成功到" + i);

                    //Thread.Sleep(1000);
                }
            }
            sw.Stop();
            Console.WriteLine("共用时："+sw.Elapsed.TotalSeconds+"秒");
        }

        static void TestRWObj1()
        {
            int i = 0;
            string filename = "testrwobj1.bin";

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            using (LJC.FrameWork.Comm.ObjTextWriter writer = ObjTextWriter.CreateWriter(filename, ObjTextReaderWriterEncodeType.protobufex))
            {
                while ((i++) < 1000000)
                {
                    writer.AppendObject<Man>(new Man
                    {
                        Name = "李金川" + i,
                        IDCard = "421182198612301310",
                        Addr = "湖北省武穴市",
                        Sex = 1
                    });

                    //writer.Flush();

                    if (i % 10000 == 0)
                        Console.WriteLine("写入成功到" + i);

                    //Thread.Sleep(1000);
                }
            }
            sw.Stop();
            Console.WriteLine("共用时：" + sw.Elapsed.TotalSeconds + "秒");
        }


        static void TestRWObjEx()
        {
            int i = 0;
            string filename = "testrwobjex.bin";
            using (LJC.FrameWork.Comm.ObjTextWriter writer = ObjTextWriter.CreateWriter(filename, ObjTextReaderWriterEncodeType.protobufex))
            {
                while ((i++) < 100)
                {
                    writer.AppendObject<Man>(new Man
                    {
                        Name = "李金川" + i,
                        IDCard = "421182198612301310",
                        Addr = "湖北省武穴市",
                        Sex = 1
                    });

                    //writer.Flush();

                    Console.WriteLine("写入成功一条！" + i);

                    Thread.Sleep(10);
                }
            }
        }

        static void Main0(string[] args)
        {
            TestRWObj1();

            //var xx=DataContextMoudelFactory<RunConfig>.GetDataContext()
            //   .ExecuteList().FirstOrDefault() ?? new RunConfig();

            //var list = DataContextMoudelFactory<UserPrestigeBand_CompareEntity>.GetDataContext()
            //    .WhereEq(p => p.用户ID, "1").ExecuteEntity();

            //var list = new List<UserActive>();
            //for (int i = 0; i < 10000; i++)
            //{
            //    list.Add(new UserActive
            //    {
            //        ActiveTime = DateTime.Now,
            //        ActiveType = new Random().Next(0, 9),
            //        GubaCode = "000998",
            //        ID = 0,
            //        ToPostId = 0,
            //        ToUid = string.Empty,
            //        Uid = i.ToString(),

            //    });
            //}

            //var cachtb = new CachDataContextMoudel<UserActive>();
            //foreach (var a in list)
            //{
            //    //Dictionary<string,object> dic=new Dictionary<string,object>();
            //    //var sqlparams = new DbParameter[]
            //    //    {
            //    //       new SqlParameter("@ID",System.Data.SqlDbType.BigInt, 18),
            //    //       new SqlParameter("@Uid",a.Uid),
            //    //       new SqlParameter("@ActiveType",a.ActiveType),
            //    //       new SqlParameter("@GubaCode",a.GubaCode),
            //    //       new SqlParameter( "@ToUid",a.ToUid),
            //    //       new SqlParameter("@ToPostId",a.ToPostId),
            //    //       new SqlParameter("@ActiveTime",a.ActiveTime),
            //    //    };
            //    //sqlparams[0].Direction = System.Data.ParameterDirection.Output;
            //    //DataContextMoudelFactory<UserActive>.GetDataContext().ExecuteProc("PrestigeV2TestDB_sp3_UserActive_i",
            //    //   sqlparams, ref dic);
            //    a.ActiveTime = a.ActiveTime.Date;
            //    cachtb.Add(a);
            //}
            //cachtb.FlushToTable();

           // int x = 3;
           // TestEnum te = (TestEnum)x;

           //bool b= Enum.IsDefined(typeof(TestEnum), x);

           //var cc= Enum.Parse(typeof(TestEnum), x.ToString());
            
            //var cc= (TestEnum)Convert.ChangeType(x, typeof(TestEnum));
        }
    }
}
