using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    public class CallBackRequest
    {
        public string SparkTaskId
        {
            get;
            set;
        }

        public bool Success
        {
            get;
            set;
        }

        public string StdOutPut
        {
            get;
            set;
        }

        public string ErrorOutPut
        {
            get;
            set;
        }

        public DateTime StartTime
        {
            get;
            set;
        }

        public DateTime EndTime
        {
            get;
            set;
        }
    }
}
