using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    public static class Ext
    {
        public static T ConvertTo<T>(this object value, T defaultValue = default)
        {
            if (value == null)
            {
                return defaultValue;
            }

            try
            {
                Type underlyingType = Nullable.GetUnderlyingType(typeof(T));
                if (underlyingType != null)
                {
                    return (T)Convert.ChangeType(value, underlyingType);
                }

                if (typeof(System.Enum).IsAssignableFrom(typeof(T)))
                {
                    return (T)Enum.Parse(typeof(T), value.ToString());
                }

                if (typeof(T) == typeof(Guid))
                {
                    return (T)(object)Guid.Parse(value.ToString());
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
