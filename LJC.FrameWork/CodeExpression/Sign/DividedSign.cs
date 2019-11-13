using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class DividedSign : DualCalSign
    {
        public override string Sign
        {
            get { return "/"; }
        }

        public override int Priority
        {
            get { return (int)SignPriorityEnum.multDividMod; }
        }

        public override string SignName
        {
            get
            {
                return "商";
            }
        }

        protected override object DoSingleOperate(object lVal, object rVal)
        {
            return lVal.ToDouble() / rVal.ToDouble();
        }
    }
}
