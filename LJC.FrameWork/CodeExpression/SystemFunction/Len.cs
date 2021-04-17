using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.SystemFunction
{
    internal class Len:SysFun
    {
        public override string SignName => "Len";

        public override int Params => 1;

        public override CalResult Operate()
        {
            if (param1 is object[])
            {
                return new CalResult
                {
                    Result = ((object[])param1).Length
                };
            }
            else if (param1 is string)
            {
                return new CalResult
                {
                    Result = ((string)param1).Length
                };
            }
            else if(param1==null)
            {
                return new CalResult
                {
                    Result = 0
                };
            }
            else
            {
                throw new NotImplementedException("长度函数只支持数组和字符串");
            }
        }
    }
}
