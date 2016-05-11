using LJC.FrameWork.SOA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Test
{
    public class SOAServicesTest:ESBService
    {

        public SOAServicesTest(int serviceno)
            : base("192.168.22.11", 9981, serviceno)
            //:base("2.5.129.118",9981)
        {

        }

        public override object DoResponse(int funcId, byte[] Param)
        {
            Console.WriteLine("调用了我");
            if (funcId == 1)
            {
                return "h";
            }
            return base.DoResponse(funcId, Param);
        }
    }
}
