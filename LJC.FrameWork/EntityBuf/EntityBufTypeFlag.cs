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

    [Flags]
    internal enum StringTypeFlag : byte
    {
        DEFAULT=0,
        NULL=1,
        Empty = 2,
        ByteLen=4,
        ShortLen=8,
        IntLen=16,
        LongLen=32,
        AssciiEncoding = 64,
        UTF8Encoding = 128,
    }

    [Flags]
    public enum IntTypeFlag : byte
    {
        DEFAULT = 0,
        NULL=1,
        BYTE=2,
        SHORT=4,
        INT=8,
        Zero=64,
        Minus = 128,
    }

    [Flags]
    public enum ArrayTypeFlag : byte
    {
        DEFAULT=0,
        NULL=1,
        Empty=2,
        ByteLen=4,
        ShortLen=8,
        IntLen=16,
        Zero=64,
        Compress=128, //压缩
    }

    [Flags]
    public enum DoubleTypeFlag : byte
    {
        DEFAULT=0,
        ByteVal=1,
        ShortVal=2,
        IntVal=4,
        FloatVal=8,
        Minus = 128,
    }

    [Flags]
    public enum DecimalTypeFlag : byte
    {
        DEFAULT=0,
        ByteVal=1,
        ShortVal=2,
        IntVal=4,
        FloatVal=8,
        DoubleVal=16,
        Int64Val=32,
        Zero=64,
        Minus=128, 
    }

    [Flags]
    public enum LongTypeEnum : byte
    {
        DEFAULT=0,
        Zero=1,
        ByteVal=2,
        ShortVal=4,
        IntVal=8,
        Minus=128,
    }

    [Flags]
    public enum ShortTypeEnum : byte
    {
        DEFAULT=0,
        Zero=1,
        ByteVal=2,
        Minus=128,
    }

    public enum UShrotTypeEnum : byte
    {
        DEFAULT=0,
        Zero = 1,
        ByteVal = 2,
    }
}
