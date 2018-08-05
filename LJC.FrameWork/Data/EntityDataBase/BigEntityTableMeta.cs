using LJC.FrameWork.EntityBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LJC.FrameWork.Data.EntityDataBase
{
    [Serializable]
    public class BigEntityTableMeta
    {
        public string KeyName
        {
            get;
            set;
        }

        public IndexInfo KeyIndexInfo
        {
            get;
            set;
        }

        public IndexInfo[] IndexInfos
        {
            get;
            set;
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

        private Dictionary<string, EntityType> _entityTypeDic = new Dictionary<string, EntityType>();
        [XmlIgnore]
        public Dictionary<string, EntityType> EntityTypeDic
        {
            get
            {
                return _entityTypeDic;
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
        public int NewAddCount
        {
            get;
            set;
        }
    }
}
