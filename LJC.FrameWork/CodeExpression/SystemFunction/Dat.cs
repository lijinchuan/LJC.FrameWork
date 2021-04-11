using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.SystemFunction
{
    internal class Dat: SysFun
    {
        public override string SignName => "Dat";

        public override int Params => 1;

        protected override CalResult SingOperate()
        {
            var param1 = GetPara(0);

            return new CalResult
            {
                Result=Convert.ToDateTime(param1).Date,
                ResultType=typeof(DateTime)
            };
        }

        protected override CalResult CollectOperate()
        {
            var param1 = GetPara(0);

            var arr = param1.ToArr();

            return new CalResult
            {
                Results = arr.Select(p => (object)Convert.ToDateTime(p).Date).ToArray(),
                ResultType = typeof(DateTime[])
            };
        }
    }
}
