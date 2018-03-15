using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LJC.FrameWork.Data.EntityDataBase
{
    [Serializable]
    public class EntityTableMeta
    {
        public string KeyName
        {
            get;
            set;
        }

        /// <summary>
        /// 其它索引
        /// </summary>
        public string[] Indexs
        {
            get;
            set;
        }

        private bool _keyDuplicate = true;
        [Obsolete]
        public bool KeyDuplicate
        {
            get
            {
                return _keyDuplicate;
            }
            set
            {
                _keyDuplicate = value;
            }
        }

        public DateTime CTime
        {
            get;
            set;
        }

        private Type _ttype = null;
        [XmlIgnore]
        public Type TType
        {
            get
            {
                return _ttype;
            }
            set
            {
                _ttype = value;
                _typestr = value.AssemblyQualifiedName;
            }
        }

        private string _typestr = null;
        public string TypeString
        {
            get
            {
                return _typestr;
            }
            set
            {
                if (value != null)
                {
                    _typestr = value;//TType.AssemblyQualifiedName;
                    _ttype = Type.GetType(value, true);
                }
            }
        }

        private List<IndexMergeInfo> _indexMergeInfos = new List<IndexMergeInfo>();
        public List<IndexMergeInfo> IndexMergeInfos
        {
            get
            {
                return _indexMergeInfos;
            }
            set
            {
                _indexMergeInfos = value;
            }
        }

        [XmlIgnore]
        public PropertyInfoEx KeyProperty
        {
            get;
            set;
        }

        private Dictionary<string, PropertyInfoEx> _indexProperties = new Dictionary<string, PropertyInfoEx>();
        [XmlIgnore]
        public Dictionary<string, PropertyInfoEx> IndexProperties
        {
            get
            {
                return _indexProperties;
            }
        }

        private object _locker = new object();
        [XmlIgnore]
        public object Locker
        {
            get
            {
                return _locker;
            }
        }

        [XmlIgnore]
        public int NewCount
        {
            get;
            set;
        }
    }
}
