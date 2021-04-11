using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class Bigersign : DualCalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.compare;
            }
        }

        public override string Sign
        {
            get
            {
                return ">";
            }
        }

        public override string SignName
        {
            get
            {
                return "比较";
            }
        }

        protected override object DoSingleOperate(object lVal, object rVal)
        {
            //return lVal.ToDouble() > rVal.ToDouble();
            return CompareHelper.Biger(lVal, rVal);
        }

    }
}
