using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public static class DataHelper
    {
        public static void Insert<T,V>(this Dictionary<T, V> dic, T key,V val)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = val;
                return;
            }
            dic.Add(key, val);
        }

        public static void Replace<T, V>(this Dictionary<T, V> dic, T key, V val)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = val;
                return;
            }
            dic.Add(key, val);
        }
    }
}
