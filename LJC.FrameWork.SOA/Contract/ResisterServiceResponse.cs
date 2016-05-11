using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    public class ResisterServiceResponse
    {
        public bool IsSuccess
        {
            get;
            set;
        }

        public string ErrMsg
        {
            get;
            set;
        }
    }
}
