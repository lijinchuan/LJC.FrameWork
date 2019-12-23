using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;
using System.Threading;
using LJC.FrameWork.Attr;

namespace LJC.FrameWork.Comm
{
    public static class ReflectionHelper
    {
        private static ReaderWriterLockSlim getValueMethedPoolLock = new ReaderWriterLockSlim();
        private static Dictionary<int, Func<object, object>> getValueMethedPool = new Dictionary<int, Func<object, object>>();
        private static ReaderWriterLockSlim setValueMethedPoolLock = new ReaderWriterLockSlim();
        private static Dictionary<int, Action<object, object>> setValueMethedPool = new Dictionary<int, Action<object, object>>();

        private static Func<object, object> GetMethedFuncPoolCach(int key,
            Func<Func<object, object>> newFunc)
        {
            Func<object, object> fun;


            if (getValueMethedPool.ContainsKey(key))
            {
                fun = getValueMethedPool[key];
                return fun;
            }

            try
            {
                getValueMethedPoolLock.EnterWriteLock();
                fun = newFunc();
                if (fun != null)
                {
                    getValueMethedPool.Add(key, fun);
                }
            }
            finally
            {
                getValueMethedPoolLock.ExitWriteLock();
            }
            return fun;
        }

        private static Action<object, object> GetSetValueFuncCach(int key,
            Func<Action<object, object>> newFunc)
        {
            Action<object, object> fun;
            if (setValueMethedPool.ContainsKey(key))
            {
                fun = setValueMethedPool[key];
                return fun;
            }

            try
            {
                setValueMethedPoolLock.EnterWriteLock();
                fun = newFunc();
                if (fun != null)
                {
                    setValueMethedPool.Add(key, fun);
                }
            }
            finally
            {
                setValueMethedPoolLock.ExitWriteLock();
            }
            return fun;
        }

        //public static object EvalDrect(this object o, PropertyInfo prop)
        //{
        //    if (o == null)
        //        return null;
        //    if (prop == null)
        //        return null;
        //    return prop.GetValue(o, null);
        //}

        public static Func<object, object> GetGetValueFunc(Type type, PropertyInfoEx propertyInfo)
        {
            ParameterExpression instance = Expression.Parameter(
            typeof(object), "instance");
            Expression instanceCast = Expression.Convert(
            instance, propertyInfo.PropertyInfo.ReflectedType);
            Expression propertyAccess = Expression.Property(
            instanceCast, propertyInfo.PropertyInfo);
            UnaryExpression castPropertyValue = Expression.Convert(
            propertyAccess, typeof(object));

            Expression<Func<object, object>> lamexpress =
            Expression.Lambda<Func<object, object>>(
                castPropertyValue, instance);

            return lamexpress.Compile();
        }

        public static object Eval(this object o, PropertyInfoEx propertyInfo)
        {
            if (o == null)
                return null;
            if (propertyInfo == null)
                return null;

            if (propertyInfo.IsSetGetValueMethed)
            {
                return propertyInfo.GetValueMethed(o);
            }

            ParameterExpression instance = Expression.Parameter(
            typeof(object), "instance");
            Expression instanceCast = Expression.Convert(
            instance, propertyInfo.PropertyInfo.ReflectedType);
            Expression propertyAccess = Expression.Property(
            instanceCast, propertyInfo.PropertyInfo);
            UnaryExpression castPropertyValue = Expression.Convert(
            propertyAccess, typeof(object));

            Expression<Func<object, object>> lamexpress =
            Expression.Lambda<Func<object, object>>(
                castPropertyValue, instance);

            propertyInfo.GetValueMethed = lamexpress.Compile();

            return propertyInfo.GetValueMethed(o);
        }

        public static object Eval(this object o, PropertyInfo propertyInfo)
        {
            if (o == null)
                return null;
            if (propertyInfo == null)
                return null;

            //string key = "get$"+o.GetType().Name + "$" + propertyInfo.Name;
            int key = propertyInfo.GetHashCode();
            var evalmethed = GetMethedFuncPoolCach(key, () =>
                {
                    ParameterExpression instance = Expression.Parameter(
                    typeof(object), "instance");
                    Expression instanceCast = Expression.Convert(
                    instance, propertyInfo.ReflectedType);
                    Expression propertyAccess = Expression.Property(
                    instanceCast, propertyInfo);
                    UnaryExpression castPropertyValue = Expression.Convert(
                    propertyAccess, typeof(object));

                    Expression<Func<object, object>> lamexpress =
                    Expression.Lambda<Func<object, object>>(
                        castPropertyValue, instance);

                    return lamexpress.Compile();
                });
            if (evalmethed != null)
                return evalmethed(o);
            return null;
        }

        /// <summary>
        /// 反射取值
        /// </summary>
        /// <param name="o"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object Eval(this object o, string property)
        {
            try
            {
                if (o == null)
                    return null;

                var tp = o.GetType();
                var propertyInfo = GetProperty(tp,property);

                if (propertyInfo == null)
                {
                    throw new Exception(string.Format("类型\"{0}\"不在名为\"{1}\"的属性", tp.FullName, property));
                }

                return o.Eval(propertyInfo);
            }
            catch
            {
                return null;
            }
        }

        public static void SetValueDrect(this object o, PropertyInfo property, object val)
        {
            if (o == null)
                return;
            if (property == null)
                return;
            property.SetValue(o, val, null);
        }

        public static Action<object, object> GetSetValueFunc(Type type, PropertyInfoEx property)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression valParameter = Expression.Parameter(typeof(object), "val");

            //转换为真实类型
            var instanceCast = Expression.Convert(instance, type);
            var valParameterCast = Expression.Convert(valParameter, property.PropertyInfo.PropertyType);


            var body = Expression.Call(instanceCast, property.PropertyInfo.GetSetMethod(), valParameterCast);
            var lamexpress = Expression.Lambda<Action<object, object>>(body, instance, valParameter).Compile();

            //property.SetValueMethed = lamexpress;

            return lamexpress;
        }

        public static void SetValue(this object o, PropertyInfoEx property, object val)
        {
            if (property.IsSetSetValueMethed)
            {
                try
                {
                    property.SetValueMethed(o, val);
                }
                catch
                {
                    try
                    {
                        property.SetValueMethed(o, Convert.ChangeType(val, property.PropertyInfo.PropertyType));
                    }
                    catch
                    {
                        throw new Exception(string.Format("字符串转换失败，无法从{0}转到{1}:{2}", val.GetType().Name, property.PropertyInfo.PropertyType.Name, val));
                    }
                }
                return;
            }

            var tp = o.GetType();
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression valParameter = Expression.Parameter(typeof(object), "val");

            //转换为真实类型
            var instanceCast = Expression.Convert(instance, tp);
            var valParameterCast = Expression.Convert(valParameter, property.PropertyInfo.PropertyType);


            var body = Expression.Call(instanceCast, property.PropertyInfo.GetSetMethod(), valParameterCast);
            var lamexpress = Expression.Lambda<Action<object, object>>(body, instance, valParameter).Compile();

            property.SetValueMethed = lamexpress;
            try
            {
                lamexpress(o, val);
            }
            catch
            {
                try
                {
                    lamexpress(o, Convert.ChangeType(val, property.PropertyInfo.PropertyType));
                }
                catch
                {
                    throw new Exception(string.Format("字符串转换失败，无法从{0}转到{1}:{2}", val.GetType().Name, property.PropertyInfo.PropertyType.Name, val));
                }
            }
        }

        public static void SetValue(this object o, PropertyInfo property, object val)
        {
            //string key = "set$" + tp.Name + "$" + property.Name;
            int key = property.GetHashCode();

            var setMethed = GetSetValueFuncCach(key, () =>
                {
                    var tp = o.GetType();
                    ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
                    ParameterExpression valParameter = Expression.Parameter(typeof(object), "val");

                    //转换为真实类型
                    var instanceCast = Expression.Convert(instance, tp);
                    var valParameterCast = Expression.Convert(valParameter, property.PropertyType);


                    var body = Expression.Call(instanceCast, property.GetSetMethod(), valParameterCast);
                    var lamexpress = Expression.Lambda<Action<object, object>>(body, instance, valParameter).Compile();
                    return lamexpress;
                });

            setMethed(o, val);
        }

        public static string GetObjectDescription(PropertyInfoEx prop)
        {
            if (prop == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(prop.GetDesc()))
            {
                return prop.GetDesc();
            }
            var descs = prop.PropertyInfo.GetCustomAttributes(typeof(PropertyDescriptionAttribute), true);
            if (descs == null || descs.Length == 0)
                return string.Empty;

            string tname = string.Empty;
            try
            {
                if (prop.PropertyInfo.ReflectedType.IsGenericType)
                {
                    //tname = prop.ReflectedType.GetGenericArguments().First().ReflectedType.Name;
                    tname = ((Type[])prop.PropertyInfo.ReflectedType.Eval("GenericTypeArguments")).First().Name;
                }
                return (descs[0] as PropertyDescriptionAttribute).Desc.Replace("<T>", tname);

            }
            catch
            {
                return (descs[0] as PropertyDescriptionAttribute).Desc.Replace("<T>", "对象");
            }
        }

        public static string GetObjectDescription(FieldInfo field)
        {
            var descs = field.GetCustomAttributes(typeof(PropertyDescriptionAttribute), true);
            if (descs == null || descs.Length == 0)
                return string.Empty;

            string tname = string.Empty;
            try
            {
                if (field.ReflectedType.IsGenericType)
                {
                    //tname = prop.ReflectedType.GetGenericArguments().First().ReflectedType.Name;
                    tname = ((Type[])field.ReflectedType.Eval("GenericTypeArguments")).First().Name;
                }
                return (descs[0] as PropertyDescriptionAttribute).Desc.Replace("<T>", tname);

            }
            catch
            {
                return (descs[0] as PropertyDescriptionAttribute).Desc.Replace("<T>", "对象");
            }
        }

        public static string GetJsonPropertyName(PropertyInfo prop)
        {
            if (prop == null)
                return string.Empty;

            foreach (var p in prop.GetCustomAttributesData())
            {
                foreach (var arg in p.NamedArguments)
                {
                    if (arg.MemberInfo.DeclaringType == typeof(Newtonsoft.Json.JsonPropertyAttribute))
                    {
                        if (!string.IsNullOrWhiteSpace((string)arg.TypedValue.Value))
                            return arg.TypedValue.Value.ToString();
                        else
                            return prop.Name;
                    }
                }

                if (p.Constructor.ReflectedType == typeof(Newtonsoft.Json.JsonPropertyAttribute))
                {
                    if (p.ConstructorArguments.Count > 0 && !string.IsNullOrWhiteSpace((string)p.ConstructorArguments[0].Value))
                    {
                        return p.ConstructorArguments[0].Value.ToString();
                    }
                    else
                    {
                        return prop.Name;
                    }
                }

            }

            return prop.Name;
        }

        public static string GetEnumDesc(Type enumtype)
        {
            if (!enumtype.IsEnum)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            var tp = Enum.GetUnderlyingType(enumtype);
            foreach (var item in Enum.GetValues(enumtype))
            {
                var val = Convert.ChangeType(item, tp);
                sb.AppendFormat("{0}({1}):{2}<br>", item.ToString(), val, ReflectionHelper.GetEnumAnnotation(enumtype, item.ToString()) ?? GetObjectDescription(enumtype.GetField(item.ToString())));
            }

            return sb.ToString();
        }

        public static PropertyInfo GetProperty(Type type, string property)
        {
            var pps = type.GetProperties();
            if (pps == null || pps.Length == 0)
            {
                return null;
            }

            foreach (var p in pps)
            {
                if (p.Name.Equals(property))
                {
                    return p;
                }
            }

            return null;
        }

        public static DocXml GetAssemblyXml(string modelname)
        {
            if (string.IsNullOrWhiteSpace(modelname))
            {
                return null;
            }
            var key = $"GetAssemblyXml_{modelname}";
            return LocalCacheManager<DocXml>.Find(key, () =>
            {
                string xmlfile = string.Empty;
                if (modelname.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                    || modelname.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    xmlfile = $"{AppDomain.CurrentDomain.BaseDirectory}\\{modelname.Substring(0, modelname.IndexOf('.'))}.xml";
                }
                else
                {
                    xmlfile = $"{AppDomain.CurrentDomain.BaseDirectory}\\{modelname}";
                }

                var doc = SerializerHelper.DeSerializerFile<DocXml>(xmlfile);
                if (doc != null)
                {
                    return doc;
                }
                return null;
            });
        }

        public static string GetMethodAnnotation(MethodInfo method)
        {
            if (method == null)
            {
                return string.Empty;
            }
            string _desc = null;
            var doc = ReflectionHelper.GetAssemblyXml(method.Module.Name);
            if (doc != null)
            {
                var key = $"M:{method.DeclaringType.FullName.Replace('+', '.')}.{method.Name}";
                if (method.GetParameters().Length > 0)
                {
                    key += $"({string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName.Replace('+', '.')))})";
                }
                var m = doc.MemberList.Member.FirstOrDefault(p => p.Name == key);
                _desc = m?.Summary;
            }

            _desc = (_desc ?? string.Empty).Trim('\r', '\n', ' ');

            return _desc;
        }

        public static string GetMethodParamAnnotation(MethodInfo method, string paramname)
        {
            if (method == null)
            {
                return string.Empty;
            }
            string _desc = null;
            var doc = ReflectionHelper.GetAssemblyXml(method.Module.Name);
            if (doc != null)
            {
                var key = $"M:{method.DeclaringType.FullName.Replace('+', '.')}.{method.Name}";
                if (method.GetParameters().Length > 0)
                {
                    key += $"({string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName.Replace('+', '.')))})";
                }
                var m = doc.MemberList.Member.FirstOrDefault(p => p.Name == key);
                _desc = m?.Params?.FirstOrDefault(p => paramname.Equals(p.Name, StringComparison.OrdinalIgnoreCase))?.Txt;
            }

            _desc = (_desc ?? string.Empty).Trim('\r', '\n', ' ');

            return _desc;
        }

        public static string GetEnumAnnotation(Type enumtype, string @enum)
        {
            string _desc = null;
            var doc = ReflectionHelper.GetAssemblyXml(enumtype.Module.Name);
            if (doc != null)
            {
                var key = $"F:{enumtype.FullName.Replace('+', '.')}.{@enum.ToString()}";
                var m = doc.MemberList.Member.FirstOrDefault(p => p.Name == key);
                _desc = m?.Summary?.Trim('\r', '\n', ' ');
            }

            return _desc;
        }
    }
}
