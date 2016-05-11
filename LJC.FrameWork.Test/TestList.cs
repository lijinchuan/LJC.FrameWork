using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Test
{
    [ProtoContract]
    public class TestList
    {
        [ProtoMember(1)]
        public string xxxx
        {
            get;
            set;
        }

        [ProtoMember(221)]
        public int de
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public int tr
        {
            get;
            set;
        }
    }
}
