using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test2
{
    public class SparkLauncherContext
    {
        public String SparkHome
        {
            get;
            set;
        }

        public String AppResource
        {
            get;
            set;
        }

        public String MainClass
        {
            get;
            set;
        }

        public String Master
        {
            get;
            set;
        }

        public String DeployMode
        {
            get;
            set;
        }

        public String DriverMem
        {
            get;
            set;
        }

        public String ExecuteMem
        {
            get;
            set;
        }

        public int ExecuteCores
        {
            get;
            set;
        }
    }
}
