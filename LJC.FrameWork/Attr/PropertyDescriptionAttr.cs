using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Attr
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Enum)]
    /// <summary>
    /// 描述一个属性信息
    /// </summary>
    public class PropertyDescriptionAttribute : Attribute
    {
        private string _desc = string.Empty;
        public string Desc
        {
            get
            {
                return _desc;
            }
        }

        public PropertyDescriptionAttribute(string desc)
        {
            _desc = desc;
        }
    }
}
