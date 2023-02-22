using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LJC.FrameWork.SOA
{
    [Serializable]
    public class ESBServerConfigItem
    {
        public string ESBServer
        {
            get;
            set;
        }

        public int ESBPort
        {
            get;
            set;
        }

        public bool IsSecurity
        {
            get;
            set;
        }

        public bool AutoStart
        {
            get;
            set;
        }

        public int MaxClientCount
        {
            get;
            set;
        }

        /// <summary>
        /// 注册的服务
        /// </summary>
        [XmlIgnore]
        public List<RegisterServiceInfo> RegisterServiceInfos
        {
            get;
            set;
        }
    }
}
