using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test2
{
    public class TestESBEervice:LJC.FrameWork.SOA.ESBService
    {
        public TestESBEervice()
            : base(101, true, false)
        {
        }

        public override object DoResponse(int funcId, byte[] Param,string clientid,Dictionary<string,string> header)
        {
            var str = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<string>(Param);
            Console.WriteLine("收到消息:" + str);
            return funcId + ":" + str;
        }
    }
}
