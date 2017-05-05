using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public enum MongoQueryCodition
    {
        EQ,
        NE,
        LT,
        LTE,
        GT,
        GTE,
        In,
        NotIn,
        All,
        Exists,
        Size,
        SizeGreaterThan,
        SizeGreaterThanOrEqual,
        SizeLessThan,
        SizeLessThanOrEqual
    }
}
