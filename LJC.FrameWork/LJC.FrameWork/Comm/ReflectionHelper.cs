using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    public static class ReflectionHelper
    {
        private static ReaderWriterLockSlim getValueMethedPoolLock = new ReaderWriterLockSlim();
        private static Dictionary<string, Func<object, object>> getValueMethedPool = new Dictionary<string, Func<object, object>>();
        private static ReaderWriterLockSlim setValueMethedPoolLock = new ReaderWriterLockSlim();
        private static Dictionary<string, Action<object, object>> setValueMethedPool = new Dictionary<string, Action<object, object>>();

        private static Func<object, object> GetMethedFuncPoolCach(string key,
            Func<Func<object,object>> newFunc)
        {
            try
            {
                getValueMethedPoolLock.EnterUpgradeableReadLock();
                Func<object,object> fun;
                if (getValueMethedPool.TryGetValue(key,out fun))
                {
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
            finally
            {
                getValueMethedPoolLock.ExitUpgradeableReadLock();
            }
        }

        private static Action<object, object> GetSetValueFuncCach(string key,
            Func<Action<object, object>> newFunc)
        {
            try
            {
                setValueMethedPoolLock.EnterUpgradeableReadLock();
                Action<object, object> fun;
                if (setValueMethedPool.TryGetValue(key, out fun))
                {
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
            finally
            {
                setValueMethedPoolLock.ExitUpgradeableReadLock();
            }
        }

        //public static object EvalDrect(this object o, PropertyInfo prop)
        //{
        //    if (o == null)
        //        return null;
        //    if (prop==null)
        //        return null;
        //    return prop.GetValue(o, null);
        //}

        public static object Eval(this object o, PropertyInfo propertyInfo)
        {
            try
            {
                if (o == null)
                    return null;
                if (propertyInfo == null)
                    return null;

                string key = "get$"+o.GetType().Name + "$" + propertyInfo.Name;
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
            catch
            {
                return null;
            }
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
                var propertyInfo = tp.GetProperty(property);

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

        //public static void SetValueDrect(this object o, PropertyInfo property, object val)
        //{
        //    if (o == null)
        //        return;
        //    if (property == null)
        //        return;
        //    property.SetValue(o, val, null);
        //}


        public static void SetValue(this object o, PropertyInfo property, object val)
        {
            if (o == null || property == null)
            {
                return;
            }

            var tp = o.GetType();
            string key = "set$" + tp.Name + "$" + property.Name;

            var setMethed = GetSetValueFuncCach(key, () =>
                {
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

        //public static void SetValue(this object o, string property, object val)
        //{
        //    var tp = o.GetType();
        //    var propertyInfo = tp.GetProperty(property);
        //    if (propertyInfo == null)
        //    {
        //        throw new Exception(string.Format("类型\"{0}\"不在名为\"{1}\"的属性", tp.FullName, property));
        //    }

        //    o.SetValueDrect(propertyInfo, val);
        //}
    }
}
