using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.KeyWordMatch
{
    internal enum KeyWordANFStatus : int
    {
        //等待匹配
        wating = 1,

        accept1,
        accept2,
        //接受匹配，并且继续
        acceptwating,
        //接受匹配
        accept,
        //放弃匹配
        abort,
    }
}
