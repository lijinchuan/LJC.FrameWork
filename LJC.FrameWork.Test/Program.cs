using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.Data.QuickDataBase;
using LJC.FrameWork.Comm;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Reflection;
using LJC.FrameWork.EntityBuf;
using System.IO;
using System.Collections;
using System.Threading;
using System.Diagnostics;
//using ATrade.Data;
using LJC.FrameWork.CodeExpression;
using LJC.FrameWork.Test.Entity;


namespace LJC.FrameWork.Test
{

    [Flags]
    public enum EntityBufTypeFlag : int
    {
        /// <summary>
        /// 是否是数组
        /// </summary>
        ArrayFlag = 1,
        TestFlag = 2,
        ThreeFlag = 4,
    }

    public class Test2Class
    {
        public int xx
        {
            get;
            set;
        }

        public bool Boo
        {
            get;
            set;
        }
    }

    public class TestClass
    {
        public List<Test2Class> List
        {
            get;
            set;
        }

        public int col1
        {
            get;
            set;
        }

        public int[] IntArray
        {
            get;
            set;
        }

        public bool[] EmptyBoolArray
        {
            get;
            set;
        }

        public EntityBufTypeFlag flag
        {
            get;
            set;
        }

        public EntityBufTypeFlag[] flags
        {
            get;
            set;
        }


        public long LongCol
        {
            get;
            set;
        }

        public long[] longArray
        {
            get;
            set;
        }

        public DateTime DateCol
        {
            get;
            set;
        }

        public DateTime[] DateColArray
        {
            get;
            set;
        }

        public bool[] BoolArray
        {
            get;
            set;
        }

        public Test2Class Test2
        {
            get;
            set;
        }

        public Test2Class[] ListTest2
        {
            get;
            set;
        }

        public Dictionary<int, string> Dic
        {
            get;
            set;
        }

        public Dictionary<int, Test2Class> Dic2
        {
            get;
            set;
        }

        public Dictionary<int, Dictionary<string, Test2Class>> Dic3
        {
            get;
            set;
        }

        public Dictionary<int, string>[] DicArray
        {
            get;
            set;
        }
    }

    class Program
    {
        public static string TT(Expression<Func<TestClass, object>> pd)
        {
            Regex varnameRg = new Regex("\\.([_a-zA-Z][_a-zA-Z0-9]*)");
            return varnameRg.Match(pd.Body.ToString()).Groups[1].Value;
        }

        static void Main0(string[] args)
        {
            for (int i = 0; i < 1000000; i++)
            {
                List<StockCmd> stockCmds = DataContextMoudelFactory<StockCmd>.GetDataContext()
                    //.WhereSmallerEq("CreateTime", DateTime.Now)
               .WhereBigerEq("EffDate", DateTime.Now.Date)
               .ExecuteList();
                Console.WriteLine(i);
                Thread.Sleep(2);
            }
            Console.Read();
        }

        static bool IsEqual1(string s1)
        {
            return string.Equals(s1, "fun", StringComparison.OrdinalIgnoreCase);
        }

        static bool IsEqual3(string s1)
        {
            return s1.Equals("fun", StringComparison.OrdinalIgnoreCase);
        }

        static bool IsEqual2(string s1)
        {
            switch (s1.ToLower())
            {
                case "fun":
                    return true;
                default:
                    return false;
            }
        }

        static Regex rg = new Regex(@"^\d{1,}(\.\d{1,})?$");
        static bool IsDecimal(string s)
        {
            return rg.IsMatch(s); 
        }
        static decimal ParseDecimal(string s,int point)
        {
            if(IsDecimal(s))
            {
                return Decimal.Parse(decimal.Parse(s).ToString("f2"));
            }
            return default(decimal);
        }

        public static decimal ToDecimal(object o, int poit = 2)
        {
            if (o == null)
                return 0.00M;

            decimal d;
            //if (o is RunintimeValue)
            //{
            //    var ro = ((RunintimeValue)o).Invoke();
            //    if (decimal.TryParse(ro.ToString(), out d))
            //    {
            //        return decimal.Parse(d.ToString("f" + poit));
            //    }
            //}
            //else
            //{
            if (decimal.TryParse(o.ToString(), out d))
            {
                return decimal.Parse(d.ToString("f" + poit));
            }
            //}

            return 0M;
        }

        public static decimal ToDecimal2(object o, int point = 2)
        {
            if (o == null)
                return 0.00M;

            var arr = o.ToString().ToArray();
            var arr2 = new char[arr.Length];
            int pointNum = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                char ch = arr[i];
                if (ch >= '0' && ch <= '9' && pointNum < point)
                {
                    arr2[i] = ch;
                    if (pointNum >= 0)
                    {
                        pointNum++;
                        if (pointNum == point)
                            break;
                    }
                }
                else if (ch == '.' && i > 0 && pointNum == -1)
                {
                    arr2[i] = ch;
                    pointNum = 0;
                }
                else
                {
                    return default(decimal);
                }
            }

            return decimal.Parse(new string(arr2));
            //return decimal.Parse(decimal.Parse(o.ToString()).ToString("f2"));
        }

        static void Main00(string[] args)
        {
            //
            var et= DataContextMoudelFactory<NewsEntity>.GetDataContext().WhereEq(p => p.Id, 16751).ExecuteEntity();
            et.Mdate = DateTime.Now;
            et.Keywords = "修改了2233";
            et.Content = string.Empty;
            bool boo = DataContextMoudelFactory<NewsEntity>.GetDataContext(et).NotUpdate(p => p.Content, p => p.Mdate);

            return;
            var str=LJC.FrameWork.Comm.StringHelper.ChineseCap("工商银行");

            //var bytes = new byte[] { 0x01};
            //var bb = LJC.FrameWork.Comm.GZip.Compress(bytes);

            //bool boo = LJC.FrameWork.Comm.StringHelper.IsUserAccount("李大",2,20);
            LJC.FrameWork.SOA.ESBConfig.WriteConfig("ljcserver", 9990,true);
            var config = LJC.FrameWork.SOA.ESBConfig.ReadConfig();

            return;

            SOAServicesTest soasevice = new SOAServicesTest(1);
            soasevice.StartClient();
            
            soasevice.LoginFail += () =>
                {
                    Console.WriteLine(soasevice.ServiceNo);
                };

            soasevice.LoginSuccess += () =>
                {
                    //soasevice.Logout();
                };

            soasevice.Login("1", "1");
            
            Console.Read();

            soasevice.Logout();
        }


        static void Main(string[] args)
        {

            Stopwatch sw = new Stopwatch();

            //sw.Restart();
            //for (int i = 0; i < 100000000; i++)
            //{
            //    IsEqual2("FUN");
            //}
            //sw.Stop();
            //Console.WriteLine("IsEqual2:" + sw.ElapsedMilliseconds);

            //sw.Restart();
            //for (int i = 0; i < 100000000; i++)
            //{
            //    IsEqual1("FUN");
            //}
            //sw.Stop();
            //Console.WriteLine("IsEqual1:"+sw.ElapsedMilliseconds);

            //sw.Restart();
            //for (int i = 0; i < 100000000; i++)
            //{
            //    IsEqual3("FUN");
            //}
            //sw.Stop();
            //Console.WriteLine("IsEqual3:" + sw.ElapsedMilliseconds);

            //Console.Read();



//            string code = @"DIF:EMA(CLOSE,12)-EMA(CLOSE,26);
//DEA:EMA(DIF,9);
//MACD:(DIF-DEA)*2;
//VAR1:(H+L)/2;
//UP:REF(SMA(VAR1,5,1),3);
//TEETH:REF(SMA(VAR1,8,1),5);
//DOWN:REF(SMA(VAR1,13,1),8);
//MFI:(H-L)*10000000/V;
//MA1:MA((HIGH+LOW)/2,5);
//MA2:MA((HIGH+LOW)/2,21);
//AO:MA1-MA2;
//AC:AO-MA( AO,5);
//B0:IF H>HF OR ((AO<=0 AND REF(AO,1)<REF(AO,2) AND MFI<REF(MFI,1)*0.9 AND V>=REF(V,1)*1.1)) THEN TRUE ELSE FALSE END;
//S0:IF (REF(LF,1)>0 AND LF=0) OR ((REF(UP,1)<UP AND AO>0 AND REF(AO,1)>REF(AO,2) AND (MFI<=REF(MFI,1)*0.9 AND V>=REF(V,1)*1.1))) THEN TRUE ELSE F END;
//IF Profit<-10 AND Profit>-30 THEN SELL('sl') ELSE IF B0 AND NOT S0 THEN BUY('buy') ELSE IF NOT B0 AND S0 THEN SELL('sell') END END END;";
            //CalculateModel.StockDataCalPool pool = new StockDataCalPool(null ,null,null,code,false);
            //pool.CallResult();

            string code = "y:'wyyy'";
            //var code = "x:if c<=6.5 then if c<6.4 then 6.4 else 6.5 end else if c<=7 then 7 else if c<=7.5 then 8 else 9 end end end";
            //var stk = ATrade.Server.StockServer.GetStock("000952.sz");
            //var quotes = ATrade.Server.StockServer.GetHisDayQuote("000952.sz").ToArray();
            //StockDataCalPool pool = new StockDataCalPool(null, stk, quotes, code);
            //pool.CallResult();

            //var code = "x:if c<=8.5 then if ref(c,1)<7.1 then 7.0 else 7.1 end end";
            //var stk = ATrade.Server.StockServer.GetStock("000952.sz");
            //var quotes = ATrade.Server.StockServer.GetHisDayQuote("000952.sz").ToArray();
            //StockDataCalPool pool = new StockDataCalPool(null, stk, quotes, code);
            //pool.CallResult();

            //var code = "x:if c>7 then if sum(sum(c,2))>100 then true else false end end";
            //var stk = ATrade.Server.StockServer.GetStock("000952.sz");
            //var quotes = ATrade.Server.StockServer.GetHisDayQuote("000952.sz").ToArray();
            //StockDataCalPool pool = new StockDataCalPool(new ATrade.TradeBusiness.TestBusiness(), stk, quotes, code);
            ExpressCode xcode = new ExpressCode(code);
            //xcode.AnalyseExpress();
            //xcode.CallResult();
            
            sw.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                //LJC.FrameWork.Expression.ExpressHelper.ResolveCalStep(code, null);
                //pool.VarDataPool.Clear();
                //pool.CallResult();

                xcode.CallResult();

                

                //var xxx= ParseDecimal("45558773.1548995",2);
                //var xxx = ToDecimal("45558773.1548995", 2);
                //var xxx=ToDecimal2("45558773.1548995", 2);

                
            }
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.WriteLine(LJC.FrameWork.CodeExpression.Comm.CallCount_ToDecimal);
            Console.Read();
        }

        static void Main6(string[] args)
        {
            

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            //var list = LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<O_PkgOrderEntity>.GetDataContext().WhereBigerEq(p => p.Amount, 1000).Top(100000).ExecuteList();
            //sw.Stop();
            //Console.WriteLine("总共费时:" + sw.ElapsedMilliseconds);
            //sw.Restart();
            //list = LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<O_PkgOrderEntity>.GetDataContext().Top(100000).ExecuteList();
            //sw.Stop();
            //Console.WriteLine("总共费时:" + sw.ElapsedMilliseconds);
            sw.Restart();
            var list = LJC.FrameWork.Data.QuickDataBase.DataContextMoudelFactory<O_PkgOrderEntity>.GetDataContext().Top(100000).ExecuteList();
            //var list = LJC.FrameWork.Comm.SerializerHelper.DeSerializerFile<List<O_PkgOrderEntity>>(LJC.FrameWork.Comm.CommFun.GetRuningPath() + "test.xml")
            //    .Take(100000).ToList();
            sw.Stop();
            Console.WriteLine("总共费时:" + sw.ElapsedMilliseconds);

            //sw.Restart();
            //string jsonstr = string.Empty;
            //for (int i = 0; i < 1; i++)
            //{
            //    jsonstr = JsonHelper.ToJson(list);
            //}
            //sw.Stop();
            //Console.WriteLine("Json序列化总共用时：" + sw.ElapsedMilliseconds);
            //sw.Restart();
            //var jdlist = JsonHelper.DynamicJson(jsonstr);
            //sw.Stop();
            //Console.WriteLine("Json反序列化动态对象总共用时：" + sw.ElapsedMilliseconds);
            //sw.Restart();
            //var jlist = JsonHelper.FormJson(jsonstr);
            //sw.Stop();
            //Console.WriteLine("Json反序列化对象总共用时：" + sw.ElapsedMilliseconds);


            //sw.Restart();
            //string xml = string.Empty;
            //for (int i = 0; i < 1; i++)
            //{
            //    xml = LJC.FrameWork.Comm.SerializerHelper.SerializerToXML(list);
            //}
            //sw.Stop();
            //Console.WriteLine("xml序列化总共用时：" + sw.ElapsedMilliseconds);
            //sw.Restart();
            //for (int i = 0; i < 1; i++)
            //{
            //    var xmllist = LJC.FrameWork.Comm.SerializerHelper.DeserializerXML<List<O_PkgOrderEntity>>(xml);
            //}
            //sw.Stop();
            //Console.WriteLine("xml反序列化总共用时：" + sw.ElapsedMilliseconds);

            O_PkgOrderEntityList itemList = new O_PkgOrderEntityList
            {
                ItemList = list
            };
            sw.Restart();
            byte[] probufbytes = null;
            for (int i = 0; i < 1; i++)
            {
                System.IO.MemoryStream ms = new MemoryStream();
                ProtoBuf.Serializer.Serialize(ms, itemList);
                probufbytes = ms.ToArray();
                ms.Close();
            }
            sw.Stop();
            Console.WriteLine("protobuf序列化总共用时：" + sw.ElapsedMilliseconds);
            var bb = LJC.FrameWork.Comm.GZip.Compress(probufbytes);
            sw.Restart();
            for (int i = 0; i < 1; i++)
            {
                System.IO.MemoryStream ms = new MemoryStream(probufbytes);
                var pobj = ProtoBuf.Serializer.Deserialize<O_PkgOrderEntityList>(ms);
                ms.Close();
            }
            sw.Stop();
            Console.WriteLine("protobuf反序列化总共用时：" + sw.ElapsedMilliseconds);


            sw.Restart();
            byte[] bytes = null;
            for (int i = 0; i < 1; i++)
            {
                bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(list, false);
            }
            sw.Stop();
            Console.WriteLine("序列化总共用时：" + sw.ElapsedMilliseconds);
            sw.Restart();
            List<O_PkgOrderEntity> dlist;
            for (int i = 0; i < 1; i++)
            {
                dlist = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<List<O_PkgOrderEntity>>(bytes, false);
            }
            sw.Stop();
            Console.WriteLine("反序列化总共用时：" + sw.ElapsedMilliseconds);

            Console.Read();
        }

        static void Main1(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            var testlist = new TestList();
            testlist.xxxx = "ljcsadfasdfasdfasdddddddddddddddddddddddddddddd";
            testlist.de = 1;
            testlist.tr = 1;
            var ddd = BitConverter.GetBytes(testlist.de);


            sw.Restart();
            byte[] probufbytes = null;
            for (int i = 0; i < 100000; i++)
            {
                System.IO.MemoryStream ms = new MemoryStream();
                ProtoBuf.Serializer.Serialize(ms, testlist);
                probufbytes = ms.ToArray();
                ms.Close();
            }
            sw.Stop();
            Console.WriteLine("protobuf序列化总共用时：" + sw.ElapsedMilliseconds);
            var bb = LJC.FrameWork.Comm.GZip.Compress(probufbytes);
            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                System.IO.MemoryStream ms = new MemoryStream(probufbytes);
                var pobj = ProtoBuf.Serializer.Deserialize<TestList>(ms);
                ms.Close();
            }
            sw.Stop();
            Console.WriteLine("protobuf反序列化总共用时：" + sw.ElapsedMilliseconds);


            sw.Restart();
            byte[] bytes = null;
            for (int i = 0; i < 100000; i++)
            {
                bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(testlist, false);
            }
            sw.Stop();
            Console.WriteLine("序列化总共用时：" + sw.ElapsedMilliseconds);
            sw.Restart();
            TestList dlist;
            for (int i = 0; i < 10000; i++)
            {
                dlist = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<TestList>(bytes, false);
            }
            sw.Stop();
            Console.WriteLine("反序列化总共用时：" + sw.ElapsedMilliseconds);

            Console.Read();
        }

        static void Main2(string[] args)
        {
            var request = new LJC.FrameWork.Comm.HttpRequestEx();
            request.SupportCompression = true;
            var response = request.DoRequest("http://jingzhi.funds.hexun.com/jz/", null);

            List<string> list = new List<string>();
            var tpp = list.GetType();

            Dictionary<int, string> dics = new Dictionary<int, string>();
            dics.Add(1, "ljc");
            dics.Add(2, "cc");

            Type tp = dics.GetType();

            TestClass test = new TestClass();
            test.col1 = 1234;
            test.DateCol = DateTime.Now;
            test.LongCol = 234456788;
            test.flag = EntityBufTypeFlag.TestFlag | EntityBufTypeFlag.ArrayFlag;
            test.flags = new EntityBufTypeFlag[]{
                EntityBufTypeFlag.ArrayFlag,
                EntityBufTypeFlag.TestFlag,
                EntityBufTypeFlag.ThreeFlag,
            };

            test.IntArray = new int[] { 1000, 200, 122 };

            test.List = new List<Test2Class>();
            test.List.Add(new Test2Class
            {
                Boo = false,
                xx = 1,
            });
            test.List.Add(new Test2Class
            {
                Boo = true,
                xx = 22,
            });
            test.List.Add(new Test2Class
            {
                Boo = false,
                xx = 1200,
            });

            test.Dic = new Dictionary<int, string>();
            test.Dic.Add(1, "xxxx");
            test.Dic.Add(2, "haha");

            test.Dic2 = new Dictionary<int, Test2Class>();
            test.Dic2.Add(1, new Test2Class
            {
                Boo = true,
                xx = 2
            });
            test.Dic2.Add(2, new Test2Class
            {
                Boo = true,
                xx = 3
            });

            var dic3item = new Dictionary<string, Test2Class>();
            dic3item.Add("1", new Test2Class
            {
                Boo = true,
                xx = 10998
            });
            dic3item.Add("2", new Test2Class
            {
                Boo = false,
                xx = 2558,
            });
            test.Dic3 = new Dictionary<int, Dictionary<string, Test2Class>>();
            test.Dic3.Add(1, dic3item);
            test.Dic3.Add(2, dic3item);

            test.DateColArray = new DateTime[]{
                new DateTime(1990,1,1,0,1,1),
                new DateTime(1991,1,1,0,1,1),
                new DateTime(1992,1,1,0,1,1),
                new DateTime(1993,1,1,0,1,1),
            };
            test.BoolArray = new bool[]{
                false,
                false,
                true,
                true,
                false,
                true
            };
            test.Test2 = new Test2Class
            {
                xx = 1111,
                Boo = true,
            };

            test.longArray = new long[] { 1, 2, 3, 45 };

            test.ListTest2 = new Test2Class[]{
                new Test2Class{
                    xx=1223,
                    Boo=true,
                },
                new Test2Class{
                    xx=998,
                    Boo=false,
                },
                new Test2Class{
                    xx=998,
                    Boo=false,
                },
                new Test2Class{
                    xx=998,
                    Boo=false,
                },
                new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }, new Test2Class{
                    xx=998,
                    Boo=false,
                }
                ,new Test2Class{
                    xx=998,
                    Boo=false,
                }
            };

            var innerDic = new Dictionary<int, string>();
            innerDic.Add(1, "ljc");
            innerDic.Add(2, "李大大象");
            test.DicArray = new Dictionary<int, string>[]{
                innerDic
            };

            System.Diagnostics.Stopwatch sp = new System.Diagnostics.Stopwatch();
            sp.Restart();

            long m1 = GC.GetTotalMemory(true);

            for (int i = 0; i < 1; i++)
            {
                string jsonstr = test.ToJson();
                if (i == 0)
                {
                    int len = Encoding.UTF8.GetBytes(jsonstr).Length;
                    Console.WriteLine("JSON序列化包的大小:" + len + "字节");
                }
                var fromjson = (TestClass)LJC.FrameWork.Comm.JsonHelper.JsonToEntity<TestClass>(jsonstr);
            }

            long m2 = GC.GetTotalMemory(true);

            sp.Stop();
            Console.WriteLine("JSON序列化反序列化总共费时:" + sp.ElapsedMilliseconds);
            Console.WriteLine("共用内存:" + (m2 - m1).ToString());

            sp.Restart();
            long m3 = GC.GetTotalMemory(true);
            for (int i = 0; i < 100; i++)
            {

                var data = EntityBufCore.Serialize(test);

                if (i == 0)
                {
                    Console.WriteLine("二进制序列化包的大小:" + data.Length + "字节");
                }

                //MemoryStream ms2 = new MemoryStream(ms.GetBuffer());
                //LJC.FrameWork.EntityBuf.MemoryStreamReader reader = new MemoryStreamReader(new BinaryReader(ms2));
                object o = EntityBufCore.DeSerialize<TestClass>(data);

            }
            long m4 = GC.GetTotalMemory(true);
            sp.Stop();
            Console.WriteLine("二进制序列化反序列化总共费时:" + sp.ElapsedMilliseconds);
            Console.WriteLine("共用内存:" + (m4 - m3).ToString());

            sp.Restart();
            long m5 = GC.GetTotalMemory(true);
            for (int i = 0; i < 0; i++)
            {
                string xml = LJC.FrameWork.Comm.SerializerHelper.SerializerToXML(test);
                if (i == 0)
                {
                    int len = Encoding.UTF8.GetBytes(xml).Length;
                    Console.WriteLine("xml序列化包大小:" + len + "字节");
                }
                var xmlOjb = LJC.FrameWork.Comm.SerializerHelper.DeserializerXML<TestClass>(xml);
            }
            long m6 = GC.GetTotalMemory(true);
            sp.Stop();
            Console.WriteLine("xml序列化反序列化总共费时:" + sp.ElapsedMilliseconds);
            Console.WriteLine("共用内存:" + (m6 - m5).ToString());

            //byte bt = (byte)EntityBufTypeFlag.ThreeFlag;

            //PropertyInfo[] props = typeof(TestClass).GetProperties();
            //foreach (PropertyInfo prop in props)
            //{
            //    string s = prop.ReflectedType.Name;
            //}

            //var result= LJC.FrameWork.Comm.CommFun.GetQuickDataBaseAttr<TestClass>();

            //string filename=@"E:\程序\投资机器人（广发证券真实交易版）\ATrade_gx\ATUI2.0\ATradeUI2.0\bin\Debug\gfaccount\x.png";
            //LJC.FrameWork.Comm.ImageHelper.SaveImage(null, ref filename, false);

            //string s=TT(p => p.col1);

            //var josn1 = "{\"error_info\":\"验证码输入不正确!\",\"success\":false}";
            //var ss= JsonHelper.FormJson(josn1).EvalJson("error_info");

            //var josn2 = "{\"total\":1,\"data\":[{\"pre_interest_tax\":\"0\",\"pre_fine\":\"0\",\"money_type\":\"0\",\"fetch_balance\":\"28562.30\",\"current_balance\":\"28562.30\",\"real_sell_balance\":\"0\",\"integral_balance\":\"1066933.58\",\"market_value\":\"5751.00\",\"correct_balance\":\"0\",\"money_type_dict\":\"人民币\",\"foregift_balance\":\"0\",\"fetch_balance_old\":\"0\",\"mortgage_balance\":\"0\",\"frozen_balance\":\"0\",\"pre_interest\":\"10.37\",\"interest\":\"0\",\"asset_balance\":\"34313.30\",\"enable_balance\":\"28562.30\",\"unfrozen_balance\":\"0\",\"fine_integral\":\"0\",\"fetch_cash\":\"28562.30\",\"opfund_market_value\":\"0\",\"real_buy_balance\":\"0\",\"entrust_buy_balance\":\"0\",\"rate_kind\":\"0\",\"fund_balance\":\"28562.30\",\"begin_balance\":\"28562.30\"}],\"success\":true}";
            //var dj=JsonHelper.DynamicJson(josn2);
            //int total = dj.total;
            //var current_balance = dj.data[0]["current_balance"];

            //var josn3 = "[{name:\"ljc\",age:29},{name:\"chenghong\",age:22}]";
            //var dj3 = JsonHelper.DynamicJson(josn3);
            //var age2 = dj3[1]["age"];

        }
    }
}
