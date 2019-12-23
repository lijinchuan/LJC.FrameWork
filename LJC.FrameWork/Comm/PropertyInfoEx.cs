using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork
{
    public class PropertyInfoEx
    {
        private PropertyInfo _propertyInfo;
        public PropertyInfo PropertyInfo
        {
            get
            {
                return _propertyInfo;
            }
            set
            {
                _propertyInfo=value;
            }
        }

        public PropertyInfoEx(PropertyInfo prop)
        {
            _propertyInfo = prop;

            this.GetValueMethed = ReflectionHelper.GetGetValueFunc(prop.DeclaringType, this);
            this.SetValueMethed = ReflectionHelper.GetSetValueFunc(prop.DeclaringType, this);
        }

        public bool IsSetSetValueMethed
        {
            get;
            private set;
        }

        private Action<object, object> _setvaluemethed;
        public Action<object, object> SetValueMethed
        {
            get
            {
                return _setvaluemethed;
            }
            internal set
            {
                IsSetSetValueMethed = true;
                _setvaluemethed = value;
            }
        }

        public bool IsSetGetValueMethed = false;
        private Func<object, object> _getValueMethed;
        public Func<object, object> GetValueMethed
        {
            get
            {
               return _getValueMethed;
            }
            internal set
            {
                _getValueMethed = value;
                IsSetGetValueMethed = true;
            }
        }

        private string _desc = null;
        public string GetDesc()
        {
            if (_desc != null)
            {
                return _desc;
            }


            var doc = ReflectionHelper.GetAssemblyXml(_propertyInfo.Module.Name);
            if (doc != null)
            {
                var key = $"P:{_propertyInfo.DeclaringType.FullName.Replace('+', '.')}.{_propertyInfo.Name}";

                var m = doc.MemberList.Member.FirstOrDefault(p => p.Name == key);
                _desc = m?.Summary;
            }

            _desc = (_desc ?? string.Empty).Trim('\r', '\n', ' ');

            return _desc;
        }
    }
}
