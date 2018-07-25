using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    public class SubmitSparkRequest
    {
        public SparkLauncherContext Context
        {
            get;
            set;
        }

        public String TaskId
        {
            get;
            set;
        }
    }
}
