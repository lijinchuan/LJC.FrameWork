using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace LJC.FrameWork.ConfigurationSectionHandler
{
    [Serializable]
    [XmlRoot("ThreadPoolConfig")]
    public class ThreadPoolConfig: IConfigurationSectionHandler
    {
        [XmlAttribute]
        public int MaxWorkerThreads
        {
            get;
            set;
        }

        [XmlAttribute]
        public int MinWorkerThreads
        {
            get;
            set;
        }

        [XmlAttribute]
        public int MaxCompletionPortThreads
        {
            get;
            set;
        }

        [XmlAttribute]
        public int MinCompletionPortThreads
        {
            get;
            set;
        }

        public object Create(object parent, object configContext, XmlNode section)
        {
            if(section==null||section.OuterXml==null)
            {
                return default(ThreadPoolConfig);
            }
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(ThreadPoolConfig));
            using (System.IO.StringReader ms = new System.IO.StringReader(section.OuterXml))
            {
                var cfg= (ThreadPoolConfig)ser.Deserialize(ms);

                cfg.MaxCompletionPortThreads = Math.Max(cfg.MaxCompletionPortThreads, 100);
                cfg.MaxWorkerThreads = Math.Max(cfg.MaxWorkerThreads, 100);
                return cfg;
            }
        }
    }
}
