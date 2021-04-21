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
        condtionOrFor,
        forloop,
        forleft,
        select,//eles
        outputSetVal,
        andOr,
        not,
        compare,
        plusMinus,
        multDividMod,
        fun
    }
}
