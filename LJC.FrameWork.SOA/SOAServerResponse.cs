using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class SOAServerEnvironmentResponse
    {
        public string MachineName { get; set; }

        public string OSVersion { get; set; }

        public int ProcessorCount { get; set; }
    }
}
