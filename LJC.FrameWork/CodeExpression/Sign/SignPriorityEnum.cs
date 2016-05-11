using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal enum SignPriorityEnum
    {
        setValue = 1,
        bean,

        condition,

        select,//eles

        andOr,
        not,
        compare,
        plusMinus,
        multDividMod,
        fun
    }
}
