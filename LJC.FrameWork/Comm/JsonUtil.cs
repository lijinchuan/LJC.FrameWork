using LJC.FrameWork.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class JsonUtil<T> where T : class
    {
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="fromObj"></param>
        /// <returns></returns>
        public static string Serialize(T fromObj, bool prettify=false)
        {
            string result = string.Empty;

            if (prettify)
            {
                result = JsonConvert.SerializeObject(fromObj, new JsonSerializerSettings() { Formatting = Formatting.Indented });
            }
            else
            {
                result = JsonConvert.SerializeObject(fromObj);
            }

            return result;
        }

        /// <summary>
        /// 对象反序列化
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static T Deserialize(string jsonStr, JsonSerializerSettings settings = null)
        {
            T result = default(T);
            if (null == settings)
            {
                result = JsonConvert.DeserializeObject<T>(jsonStr);
            }
            else
            {
                result = JsonConvert.DeserializeObject<T>(jsonStr, settings);
            }
            return result;
        }

        public static object Deserialize(string jsonStr,Type destType, JsonSerializerSettings settings = null)
        {
            object result = null;
            if (null == settings)
            {
                result = JsonConvert.DeserializeObject(jsonStr,destType);
            }
            else
            {
                result = JsonConvert.DeserializeObject(jsonStr,destType, settings);
            }
            return result;
        }
    }
}
