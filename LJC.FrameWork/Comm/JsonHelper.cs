using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using System.Linq.Expressions;

namespace LJC.FrameWork.Comm
{
    public class DynamicJsonObject : DynamicObject
    {
        private IDictionary<string, object> Dictionary { get; set; }

        public DynamicJsonObject(IDictionary<string, object> dictionary)
        {
            this.Dictionary = dictionary;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this.Dictionary[binder.Name];

            if (result is IDictionary<string, object>)
            {
                result = new DynamicJsonObject(result as IDictionary<string, object>);
            }
            else if (result is ArrayList && (result as ArrayList) is IDictionary<string, object>)
            {
                result = new List<DynamicJsonObject>((result as ArrayList).ToArray().Select(x => new DynamicJsonObject(x as IDictionary<string, object>)));
            }
            else if (result is ArrayList)
            {
                result = new List<object>((result as ArrayList).ToArray());
            }

            return this.Dictionary.ContainsKey(binder.Name);
        }
    }

    public class DynamicJsonConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (type == typeof(object))
            {
                return new DynamicJsonObject(dictionary);
            }

            return null;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(object) })); }
        }
    }

    public static class JsonHelper
    {
        public static dynamic DynamicJson(string json)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = int.MaxValue;
            jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });
            dynamic dy = jss.Deserialize(json, typeof(object)) as dynamic;
            return dy;
        }

        public static object FormJson(this string jsonnString)
        {
            try
            {
                JsonSerializer jss = new JsonSerializer();
                JsonTextReader jwt = new JsonTextReader(new StringReader(jsonnString));
                return jss.Deserialize(jwt);
            }
            catch
            {
                return null;
            }
        }

        public static T JsonToEntity<T>(this string JsonString)
        {
            if (string.IsNullOrEmpty(JsonString))
                return default(T);

            JsonSerializer Jss = new JsonSerializer();

            JsonTextReader Jtr = new JsonTextReader(new StringReader(JsonString));

            return Jss.Deserialize<T>(Jtr);
        }

        public static string ToJson(this object obj)
        {
            if (obj == null)
                return string.Empty;
            JsonSerializer jss = new JsonSerializer();
            StringBuilder sb = new StringBuilder();
            JsonTextWriter jwt = new JsonTextWriter(new StringWriter(sb));
            jss.Serialize(jwt, obj);
            return sb.ToString();
        }

        public static string EvalJson(this object obj, string name)
        {
            if (!(obj is Newtonsoft.Json.Linq.JToken))
                return null;
            foreach (Newtonsoft.Json.Linq.JProperty prop in (obj as Newtonsoft.Json.Linq.JToken))
            {
                if (string.Equals(name, prop.Name))
                {
                    return prop.Value.ToString();
                }
            }
            return null;
        }

        public static IEnumerable<object> EnumJsonArray(this object arrObj)
        {
            if (!(arrObj is Newtonsoft.Json.Linq.JToken))
            {
                yield return null;
            }

            var jtoken = arrObj as Newtonsoft.Json.Linq.JToken;
            var ret = jtoken.FirstOrDefault();
            while (ret != null)
            {
                yield return ret;
                ret = ret.Next;
            }
        }

        public static string GetJsonTag<T>(Expression<Func<T,object>> predicate)
        {
            string mn = string.Empty;

            return GetJsonTag(predicate, out mn);
        }

        public static string GetJsonTag<T>(Expression<Func<T, object>> predicate,out string membername)
        {
            MemberExpression expression = null;
            if (predicate.Body is UnaryExpression)
            {
                expression = ((predicate.Body as UnaryExpression).Operand as MemberExpression);
            }
            else if (predicate.Body is MemberExpression)
            {
                expression = predicate.Body as MemberExpression;
            }
            else
            {
                throw new NotSupportedException();
            }
            
            var jsonprop = (JsonPropertyAttribute)expression.Member.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault();
            membername = expression.Member.Name;
            if (jsonprop != null)
            {
                return jsonprop.PropertyName;
            }
            else
            {
                return membername;
            }
        }

    }
}
