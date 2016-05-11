using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LJC.FrameWork.Comm
{
    public static class EnumUtility
    {
        public static string GetEnumDescription(Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var desc = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return desc.Length > 0 ? desc[0].Description : enumValue.ToString();
        }
    }
}
