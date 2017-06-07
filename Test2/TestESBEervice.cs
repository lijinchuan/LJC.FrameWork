using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test2
{
    public class TestESBEervice:LJC.FrameWork.SOA.ESBService
    {
        public TestESBEervice()
            : base("127.0.0.1", 8888, 1)
        {
        }
    }
}
