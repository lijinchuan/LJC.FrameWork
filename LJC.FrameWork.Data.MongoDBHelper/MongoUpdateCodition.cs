using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public enum MongoUpdateCodition
    {
        Set,
        SetOnInsert,
        AddToSet,
        AddToSetEach,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        Max,
        Min,
        Mul,
        UnSet,
        Incr,
        PopFirst,
        PopLast,
        Push
    }
}
