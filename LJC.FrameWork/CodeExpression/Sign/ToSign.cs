using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.Sign
{
    internal class ToSign : CalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.select;
            }
        }

        public override int Params
        {
            get
            {
                return 2;
            }
        }

        public override CalResult Operate()
        {
            return RightVal;
        }
    }
}
