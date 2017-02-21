using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm.TextReaderWriter
{
    [Serializable]
    public class LocalFileQueueCfg
    {
        public long LastPos
        {
            get;
            set;
        }

        [System.Xml.Serialization.XmlIgnore]
        public long LastChageTime
        {
            get;
            set;
        }

        public string QueueFile
        {
            get;
            set;
        }
    }
}
