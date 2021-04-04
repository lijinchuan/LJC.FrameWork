using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class Print:SysFun
    {
        public override string SignName => "print";

        public override int Params => 1;

        protected override CalResult SingOperate()
        {
            Console.WriteLine(GetPara(0));

            return null;
        }

        protected override CalResult CollectOperate()
        {
            return SingOperate();
        }
    }
}
