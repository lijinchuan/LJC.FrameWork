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

        [XmlIgnore]
        public PropertyInfoEx KeyProperty
        {
            get;
            set;
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
    }
}
