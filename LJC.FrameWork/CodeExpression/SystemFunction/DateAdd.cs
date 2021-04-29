using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.SystemFunction
{
    internal class DateAdd:SysFun
    {
        public override string SignName => "DateAdd";

        public override int Params => 3;

        protected override CalResult SingOperate()
        {
            if (!(param1 is DateTime))
            {
                throw new ExpressErrorException("DateAdd第一个参数必须是时间格式");
            }

            if (!(param2 is double))
            {
                throw new ExpressErrorException("DateAdd第二个参数必须是数字");
            }

            var intparam2 = (double)param2;
            DateTime ret = GetResult((DateTime)param1);

            return new CalResult
            {
                Result = ret,
                ResultType = typeof(DateTime)
            };
        }

        private DateTime GetResult(DateTime time)
        {
            var intparam2 = (int)(double)param2;
            DateTime ret = Convert.ToDateTime(param1);
            switch (param3.ToString().ToLower())
            {
                case "s":
                case "second":
                    {
                        ret = ret.AddSeconds(intparam2);
                        break;
                    }
                case "min":
                case "minus":
                    {
                        ret = ret.AddMinutes(intparam2);
                        break;
                    }
                case "h":
                case "hour":
                    {
                        ret = ret.AddHours(intparam2);
                        break;
                    }
                case "d":
                case "day":
                    {
                        ret = ret.AddDays(intparam2);
                        break;
                    }
                case "mon":
                    {
                        ret = ret.AddMonths(intparam2);
                        break;
                    }
                case "y":
                case "year":
                    {
                        ret = ret.AddYears(intparam2);
                        break;
                    }
                default:
                    {
                        throw new Exception("第三个是不支持的参数");
                    }
            }
            return ret;
        }

        protected override CalResult CollectOperate()
        {
            var arr = param1.ToArr();
            var intparam2 = (int)param2;

            return new CalResult
            {
                Results = arr.Select(p => (object)GetResult(Convert.ToDateTime(p))).ToArray(),
                ResultType = typeof(DateTime[])
            };
        }
    }
}
