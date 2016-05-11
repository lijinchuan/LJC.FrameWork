using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.EntityBuf
{
    [Flags]
    internal enum EntityBufTypeFlag : byte
    {
        Empty = 0,
        /// <summary>
        /// 是否是数组
        /// </summary>
        ArrayFlag = 1,
        ListFlag = 2,
        /// <summary>
        /// 值为null
        /// </summary>
        VlaueNull=4,
    }
}
