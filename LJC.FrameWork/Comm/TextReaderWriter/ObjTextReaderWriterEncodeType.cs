using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public enum ObjTextReaderWriterEncodeType : byte
    {
        jsongzip = 1,
        json,
        protobuf,
        //扩展的功能，支持回读
        protobufex,
    }
}
