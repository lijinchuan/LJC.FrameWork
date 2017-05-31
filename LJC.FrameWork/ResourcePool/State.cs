using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.ResourcePool
{
    public class Status
    {
        // Properties
        public bool HasException 
        { 
            get; 
            set; 
        }
        public bool IsActive 
        { 
            get;
            set; 
        }
        public bool IsUsing 
        { 
            get;
            set;
        }
    }

}
