using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    public class GetRegisterServiceInfoResponse
    {
        public int ServiceNo
        {
            get;
            set;
        }

        public RegisterServiceInfo[] Infos
        {
            get;
            set;
        }
    }
}
