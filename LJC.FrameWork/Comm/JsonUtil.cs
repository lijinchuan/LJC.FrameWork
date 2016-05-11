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
        public static string Serialize(T fromObj)
        {
            string result = string.Empty;

            result = JsonConvert.SerializeObject(fromObj);

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
    }
}
