using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    public class WebResponse
    {
        public string Url
        {
            get;
            set;
        }

        public int ResponseCode
        {
            get;
            set;
        }

        public string ContentType
        {
            get;
            set;
        }

        public Dictionary<string, string> Headers
        {
            get;
            set;
        }

        public Dictionary<string, string> Cookies
        {
            get;
            set;
        }

        public byte[] ResponseData
        {
            get;
            set;
        }
    }
}
