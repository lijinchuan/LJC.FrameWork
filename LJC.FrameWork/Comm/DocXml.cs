using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LJC.FrameWork.Comm
{
    [Serializable]
    [XmlRoot("doc")]
    public class DocXml
    {
        [Serializable]
        public class AssemblyName
        {
            [XmlElement("name")]
            public string Name
            {
                get;
                set;
            }
        }

        [Serializable]
        public class MemberParam
        {
            [XmlAttribute("name")]
            public string Name
            {
                get;
                set;
            }

            [XmlText()]
            public string Txt
            {
                get;
                set;
            }
        }

        [Serializable]
        public class MemberInfo
        {
            [XmlAttribute("name")]
            public string Name
            {
                get;
                set;
            }

            [XmlElement("summary")]
            public string Summary
            {
                get;
                set;
            }

            [XmlElement("param")]
            public MemberParam[] Params
            {
                get;
                set;
            }
        }

        [Serializable]
        public class Members
        {
            [XmlElement("member")]
            public MemberInfo[] Member
            {
                get;
                set;
            }
        }

        [XmlElement("assembly")]
        public AssemblyName Assembly
        {
            get;
            set;
        }

        [XmlElement("members")]
        public Members MemberList
        {
            get;
            set;
        }
    }
}
