using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class OutputValueSign : CalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.setValue; ;
            }
        }

        public override string Sign
        {
            get
            {
                return ":=";
            }
        }
    }
}
