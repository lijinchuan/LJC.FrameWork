using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class ConditionSign : CalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.condtionOrFor;
            }
        }

        public override string Sign
        {
            get
            {
                return "if";
            }
        }


        public override int Params
        {
            get
            {
                return 1;
            }
        }

        public override CalResult Operate()
        {
            if (RightVal == null)
                return LeftVal;

            return RightVal;
            //return base.Operate();
        }
    }
}
