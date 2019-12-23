using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    [Serializable]
    public class WebSiteConfig
    {
        public string[] Host
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }
    }
}
