using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test2
{
    [ProtoContract]
    public class Man
    {
        [ProtoMember(1)]
        public string IDCard
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public string Name
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public int Sex
        {
            get;
            set;
        }

        private string _addr="addr";
        [ProtoMember(4)]
        public string Addr
        {
            get
            {
                return _addr;
            }
            set
            {
                _addr = value;
            }
        }
    }
}
