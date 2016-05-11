using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class AndSign : DualCalSign
    {

        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.andOr;
            }
        }

        public override string Sign
        {
            get
            {
                return "And";
            }
        }

        public override string SignName
        {
            get
            {
                return "AND";
            }
        }

        protected override object DoSingleOperate(object lVal, object rVal)
        {
            return (bool)lVal && (bool)rVal;
        }
    }
}
