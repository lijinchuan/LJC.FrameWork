using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.Sign
{
    public class LoopLeftSign:CalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.forleft;
            }
        }

        public override string Sign
        {
            get
            {
                return "loopleft";
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
            return LeftVal;
        }
    }
}
