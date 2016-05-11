using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class ModSign : DualCalSign
    {
        public override string Sign
        {
            get { return "Mod"; }
        }

        public override int Priority
        {
            get { return (int)SignPriorityEnum.multDividMod; }
        }

        public override string SignName
        {
            get
            {
                return "模";
            }
        }

        protected override object DoSingleOperate(object lVal, object rVal)
        {
            return (decimal)lVal % (decimal)rVal;
        }
    }
}
