using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    class Fun3 : IFun
    {

        static void ReadDayQuote()
        {
            Console.WriteLine("输入股票代码");
            var code = Console.ReadLine();
            

            Console.WriteLine("开始时间");
            var begin = DateTime.Parse(Console.ReadLine());

            Console.WriteLine("结束时间");
            var end = DateTime.Parse(Console.ReadLine());

            DateTime time = DateTime.Now;
            var quotes = EMStockService.GetStockDayQuote(code, begin.Date, end.Date).ToList();
            Console.WriteLine("完成用时:" + (DateTime.Now.Subtract(time).TotalMilliseconds + "ms"));

            foreach (var item in quotes)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", item.InnerCode, item.Time.ToString("yyyy-MM-dd"), item.Open, item.High, item.Low, item.Close));
            }
        }

        public void Start()
        {
            Console.WriteLine(@"选择操作 1-取股票日行情");

            var cmd = Console.ReadLine();

            if (cmd == "1")
            {
                ReadDayQuote();
            }
        }
    }
}
