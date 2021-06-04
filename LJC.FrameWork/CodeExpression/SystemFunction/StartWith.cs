using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.SystemFunction
{
    internal class StartWith : SysFun
    {
        public override string SignName => "StartWith";

        public override int Params => 2;

        public override CalResult Operate()
        {
            if (!(param1 is string))
            {
                throw new ArgumentException("StartWith第一个参数必须是字符串");
            }

            if (!(param2 is string))
            {
                throw new ArgumentException("StartWith第二个参数必须是字符串");
            }


            return new CalResult
            {
                Result = ((string)param1).StartsWith((string)param2, StringComparison.OrdinalIgnoreCase)
            };
        }
    }
}
