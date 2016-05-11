using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.EntityBuf
{
    public enum EntityType : byte
    {
        ENUM = 1,
        BYTE,
        BOOL,
        CHAR,
        USHORT,
        SBYTE,
        SHORT,
        /// <summary>
        /// 短整形
        /// </summary>
        INT16,
        /// <summary>
        /// 32位整形
        /// </summary>
        INT32,
        /// <summary>
        /// 64位整形
        /// </summary>
        INT64,
        /// 浮点数
        /// </summary>
        FLOAT,
        /// <summary>
        /// decimal
        /// </summary>
        DECIMAL,
        /// <summary>
        /// 时间类型
        /// </summary>
        DATETIME,
        /// <summary>
        /// 双精度
        /// </summary>
        DOUBLE,
        /// <summary>
        /// 字符串
        /// </summary>
        STRING,
        /// <summary>
        /// 字典类型
        /// </summary>
        DICTIONARY,
        /// <summary>
        /// 列表类型
        /// </summary>
        LIST,
        /// <summary>
        /// 复杂类型
        /// </summary>
        COMPLEX,
        /// <summary>
        /// 未知
        /// </summary>
        UNKNOWN,
    }
}
