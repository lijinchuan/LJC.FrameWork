using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    public class WebRequest
    {
        public string Url
        {
            get;
            set;
        }

        public int TimeOut
        {
            get;
            set;
        } = 180 * 1000;

        public string Method
        {
            get;
            set;
        }

        public string QueryString
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

        public byte[] InputData
        {
            get;
            set;
        }
    }
}
