using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.Sign
{
    public class LoopSign:CalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.forloop;
            }
        }

        public override string Sign
        {
            get
            {
                return "loop";
            }
        }


        public override int Params
        {
            get
            {
                return 2;
            }
        }
    }
}
