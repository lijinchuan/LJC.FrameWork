﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    public class ForSign: CalSign
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
                return "for";
            }
        }


        public override int Params
        {
            get
            {
                return 0;
            }
        }
    }
}
