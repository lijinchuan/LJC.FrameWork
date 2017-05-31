using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.ResourcePool
{
    public class PoolConfig
    {

        // Properties
        public int? MaxPoolSize
        {
            get;
            set;
        }

        public int? PoolTimeout
        {
            get;
            set;
        }
    }
}
