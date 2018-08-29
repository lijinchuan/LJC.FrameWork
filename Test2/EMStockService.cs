using LJC.Com.StockService.Contract;
using LJC.FrameWork.Comm;
using LJC.FrameWork.Data.EntityDataBase;
using LJC.FrameWork.EntityBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    public class EMStockService
    {
        const string TBName = "EMStockDayQuote";
        const string QuoteIndexName = "Code_Time";
        static EMStockService()
        {
            BigEntityTableEngine.LocalEngine.CreateTable(TBName, "Key", typeof(EMStockDayQuote), new IndexInfo[]{
                new IndexInfo{
                     IndexName=QuoteIndexName,
                     Indexs=new IndexItem[]{
                         new IndexItem{
                             Field="InnerCode",
                             FieldType=EntityType.STRING
                         },
                         new IndexItem{
                             Field="Time",
                             FieldType=EntityType.DATETIME
                         }
                     }
                }
            });
        }

        public class EMDayQuoteResponse
        {
            public EMDayQuoteResponse()
            {
                stats = true;
            }

            public string name { get; set; }
            public string code { get; set; }
            public EMQuoteInfo info { get; set; }
            public string[] data { get; set; }

            public bool stats
            {
                get;
                set;
            }
        }

        public class EMQuoteInfo
        {
            public string c { get; set; }
            public string h { get; set; }
            public string l { get; set; }
            public string o { get; set; }
            public string a { get; set; }
            public string v { get; set; }
            public string yc { get; set; }
            public string time { get; set; }
            public string ticks { get; set; }
            public string total { get; set; }
            public string pricedigit { get; set; }
            public string jys { get; set; }
            public string Settlement { get; set; }
            public int mk { get; set; }
            public string sp { get; set; }
            public bool isrzrq { get; set; }
        }

        public class EMStockDayQuote : StockQuote
        {
            public string Key
            {
                get;
                set;
            }
        }

        private static double ConvertPrice(string price)
        {
            if (string.IsNullOrWhiteSpace(price) || price == "-")
            {
                return 0.00;
            }
            price = price.Trim();
            if (price.EndsWith("%"))
            {
                return double.Parse(price.TrimEnd('%'));
            }
            return double.Parse(price);
        }

        public static IEnumerable<StockQuote> GetStockDayQuote(string code,DateTime bein,DateTime end)
        {
            var quotetblist = BigEntityTableEngine.LocalEngine.Scan<EMStockDayQuote>(TBName, QuoteIndexName, new object[] { code, bein }, new object[] { code, end },1,int.MaxValue).ToList();
            DateTime last = DateTime.MinValue;
            if (quotetblist.Count() > 0)
            {
                last = quotetblist.Last().Time;
                if(last.DayOfWeek==DayOfWeek.Friday)
                {
                    last = last.AddDays(2);
                }
            }
            if (last >= end)
            {
                foreach (var item in quotetblist)
                {
                    yield return item;
                }
            }
            else
            {
                if (code.StartsWith("6"))
                {
                    code += 1;
                }
                else
                {
                    code += 2;
                }
                var token = Guid.NewGuid().ToString("N");
                var enurl = "cMXn904uKPCJelRpEAcghwYB+4nSbdyWjKrUHW3qLbz4FB7QUU31MroUMQusDPABRPqEytkH3h3xLQDn6ej4Q5TTKDy4FKH7xE9krSqGDCSNzATn2VqYX6KPuyKt4rpzUY5EKBZ1gsO/CQRe4u8hU799cUo5UIac/jDtZTQQ6t0=";
                string url = string.Format(new LJC.FrameWork.Comm.EncryHelper().Decrypto(enurl), token, code, DateTimeHelper.GetTimeStamp());
                byte[] data = null;
                var respjson = new HttpRequestEx().DoRequest(url, data);
                respjson.ResponseContent = Encoding.UTF8.GetString(respjson.ResponseBytes);
                var resp = JsonUtil<EMDayQuoteResponse>.Deserialize(respjson.ResponseContent.Substring(respjson.ResponseContent.IndexOf('(') + 1).TrimEnd(')'));
                if (resp.stats)
                {
                    List<EMStockDayQuote> insertlist = new List<EMStockDayQuote>();
                    foreach (var s in resp.data)
                    {
                        var arr = s.Split(',');
                        var quote = new EMStockDayQuote
                        {
                            Time = DateTime.Parse(arr[0]).Date,
                            Open = ConvertPrice(arr[1]),
                            Close = ConvertPrice(arr[2]),
                            High = ConvertPrice(arr[3]),
                            Low = ConvertPrice(arr[4]),
                            Volumne = ConvertPrice(arr[5]),
                            Amount = ConvertPrice(arr[6]),
                            ChangeRate = ConvertPrice(arr[7]),
                            InnerCode = resp.code,

                        };
                        quote.Key = resp.code + "_" + quote.Time.ToString("yyyyMMdd");

                        if (quote.Time == DateTime.Now.Date)
                        {
                            continue;
                        }

                        if (quote.Time > last)
                        {
                            insertlist.Add(quote);
                        }

                        if (quote.Time >= bein && quote.Time <= end)
                        {
                            yield return quote;
                        }
                    }

                    if (DateTime.Now.Date >= bein && DateTime.Now.Date <= end && resp.info != null)
                    {
                        yield return new StockQuote
                        {
                            Close = ConvertPrice(resp.info.c),
                            High = ConvertPrice(resp.info.h),
                            Open = ConvertPrice(resp.info.o),
                            Low = ConvertPrice(resp.info.l),
                            Volumne = ConvertPrice(resp.info.v),
                            Amount = ConvertPrice(resp.info.a),
                            Time = DateTime.Parse(resp.info.time),
                            InnerCode=resp.code
                        };
                    }

                    if (insertlist.Count > 0)
                    {
                        BigEntityTableEngine.LocalEngine.InsertBatch(TBName, insertlist);
                    }
                }
            }
        }
    }
}
