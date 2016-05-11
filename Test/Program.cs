using System;
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
        static void Main0(string[] args)
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

        static void Main(string[] args)
        {
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

            int x = 3;
            TestEnum te = (TestEnum)x;

           bool b= Enum.IsDefined(typeof(TestEnum), x);

           var cc= Enum.Parse(typeof(TestEnum), x.ToString());
            
            //var cc= (TestEnum)Convert.ChangeType(x, typeof(TestEnum));
        }
    }
}
