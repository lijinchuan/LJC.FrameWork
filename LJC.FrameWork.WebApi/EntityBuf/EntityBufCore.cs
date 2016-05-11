using LJC.FrameWork;
using LJC.FrameWork.Comm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.WebApi
{
    public class EntityBufCore
    {
        private static ReaderWriterLockSlim EntityBufTypeDicLockSlim = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim TypeBufTypeDicLockSlim = new ReaderWriterLockSlim();
        /// <summary>
        /// 类型缓存
        /// </summary>
        private static Dictionary<int, List<Tuple<EntityBufType, bool>>> EntityBufTypeDic = new Dictionary<int, List<Tuple<EntityBufType, bool>>>();
        private static Dictionary<int, Tuple<EntityBufType, bool>> TypeBufTypeDic = new Dictionary<int, Tuple<EntityBufType, bool>>();

        private static int listTypeIndex = typeof(List<int>).AssemblyQualifiedName.IndexOf("[[");
        private static Type GetListValueType(Type listType)
        {
            Type ret = null;
            //System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            //int firstdoubleleftbracket = listType.AssemblyQualifiedName.IndexOf("[[");
            int firstdoubleleftbracket = listTypeIndex;
            int bracketCount = 1;
            string valuetypename = "";
            int i = firstdoubleleftbracket + 2;
            var qualifiedNameChars = listType.AssemblyQualifiedName.ToCharArray();
            for (; i < qualifiedNameChars.Length; i++)
            {
                if (qualifiedNameChars[i] == '[')
                {
                    bracketCount++;
                }
                else if (qualifiedNameChars[i] == ']')
                {
                    bracketCount--;
                }
                if (bracketCount == 0)
                {
                    valuetypename = listType.AssemblyQualifiedName.Substring(firstdoubleleftbracket + 2, i - firstdoubleleftbracket - 2);
                    ret = Type.GetType(valuetypename);
                    i++;
                    break;
                }
            }

            return ret;
        }

        private static void DelComplex(bool isArray, EntityBufType bufType, StringBuilder sb)
        {
            if (isArray)
            {
                //sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", bufType.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), bufType.ValueType.Name, bufType.Property == null ? string.Empty : ReflectionHelper.GetObjectDescription(bufType.Property.PropertyInfo));
                StringBuilder insb = new StringBuilder("<table style=\"border:solid 1px yellow;\" border=\"1\">");
                insb.AppendFormat("<tr><th>参数名</th><th>参数类型</th><th>备注</th></tr>");
                GetPropToTable(bufType.ClassType, insb);
                insb.Append("</table>");
                sb.AppendFormat("<tr><td colspan=\"3\">+{0}<br/>{1}</td></tr>", bufType.ClassType.Name, insb.ToString());
            }
            else
            {
                bool isTypeArray2;
                var entitybuftypelist = GetTypeEntityBufType(bufType.ClassType);
                foreach (var tp in entitybuftypelist)
                {
                    isTypeArray2 = tp.Item2;

                    if (tp.Item1.EntityType == LJC.FrameWork.EntityBuf.EntityType.COMPLEX)
                    {
                        //这里加上属性
                        StringBuilder insb = new StringBuilder("<table style=\"border:solid 1px yellow;\" border=\"1\">");
                        insb.AppendFormat("<tr><th>参数名</th><th>参数类型</th><th>备注</th></tr>");
                        //GetPropToTable(bufType.ClassType, insb);
                        DelComplex(isTypeArray2, tp.Item1, insb);
                        insb.Append("</table>");

                        sb.AppendFormat("<tr><td>{0}</td><td>+{1}<br/>{2}</td><td>{3}</td></tr>",
                            tp.Item1.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(tp.Item1.Property.PropertyInfo),
                            tp.Item1.ValueType.Name,
                            insb.ToString(),
                            tp.Item1.Property == null ? string.Empty : ReflectionHelper.GetObjectDescription(tp.Item1.Property.PropertyInfo));
                    }
                    else
                    {
                        DelSimple(isTypeArray2, tp.Item1, sb);
                    }
                }
            }
        }

        private static void DelSimple(bool isArray, EntityBufType bufType, StringBuilder sb)
        {
            if (bufType.EntityType == LJC.FrameWork.EntityBuf.EntityType.COMPLEX)
            {
                throw new Exception("无法处理复杂类型");
            }

            switch (bufType.EntityType)
            {
                case LJC.FrameWork.EntityBuf.EntityType.SBYTE:
                case LJC.FrameWork.EntityBuf.EntityType.BYTE:

                case LJC.FrameWork.EntityBuf.EntityType.STRING:

                case LJC.FrameWork.EntityBuf.EntityType.SHORT:
                case LJC.FrameWork.EntityBuf.EntityType.INT16:

                case LJC.FrameWork.EntityBuf.EntityType.INT32:

                case LJC.FrameWork.EntityBuf.EntityType.DECIMAL:
                case LJC.FrameWork.EntityBuf.EntityType.FLOAT:
                case LJC.FrameWork.EntityBuf.EntityType.DOUBLE:

                case LJC.FrameWork.EntityBuf.EntityType.INT64:

                case LJC.FrameWork.EntityBuf.EntityType.DATETIME:

                case LJC.FrameWork.EntityBuf.EntityType.BOOL:
                    string jsonpropname = bufType.Property != null ? ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo) : string.Empty;
                    if (!string.IsNullOrWhiteSpace(jsonpropname))
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), bufType.EntityType.ToString().ToLower() + (isArray ? "[]" : ""), ReflectionHelper.GetObjectDescription(bufType.Property.PropertyInfo));
                    }
                    else
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", "data", bufType.EntityType.ToString().ToLower() + (isArray ? "[]" : ""), string.Empty);
                    }
                    break;
                case LJC.FrameWork.EntityBuf.EntityType.ENUM:
                    if (isArray)
                    {
                        //sb.AppendFormat("<tr><td>{0}</td><td>{1}[]</td><td>{2}</td></tr>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), "enum", string.Empty);
                    }
                    else
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), "enum", string.Empty);
                    }
                    break;
                case LJC.FrameWork.EntityBuf.EntityType.DICTIONARY:
                    if (isArray)
                    {

                    }
                    else
                    {
                        //Test(bufType.)
                    }
                    break;
                case LJC.FrameWork.EntityBuf.EntityType.LIST:
                    if (isArray)
                    {

                    }
                    else
                    {
                        var listvaluetype = GetListValueType(bufType.ValueType);
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", bufType.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), string.Format("List&lt;{0}&gt;", listvaluetype.Name), bufType.Property == null ? string.Empty : ReflectionHelper.GetObjectDescription(bufType.Property.PropertyInfo));

                        bool isarray = false;
                        if (MapBufType(listvaluetype, out isarray).EntityType == LJC.FrameWork.EntityBuf.EntityType.COMPLEX)
                        {
                            StringBuilder sbinner = new StringBuilder("<table  style=\"border:solid 1px yellow;\" border=\"1\">");
                            sbinner.AppendFormat("<tr><th>参数名</th><th>参数类型</th><th>备注</th></tr>");
                            GetPropToTable(listvaluetype, sbinner);
                            sbinner.Append("</table>");
                            sb.AppendFormat("<tr><td colspan=\"3\">+{0}<br/>{1}</td></tr>", listvaluetype.Name, sbinner.ToString());
                        }
                    }
                    break;
                default:
                    throw new Exception("错误");
            }
        }



        public static void GetPropToTable(Type type, StringBuilder sb)
        {
            Tuple<EntityBufType, bool> touple = GetTypeBufType(type);

            if (touple.Item1.EntityType != LJC.FrameWork.EntityBuf.EntityType.COMPLEX)
            {
                DelSimple(touple.Item2, touple.Item1, sb);
            }
            else
            {
                DelComplex(touple.Item2, touple.Item1, sb);
            }
        }

        private static List<Tuple<EntityBufType, bool>> GetTypeEntityBufType(Type tp)
        {
            if (tp == null)
                return null;

            int key = tp.GetHashCode();
            try
            {
                //EntityBufTypeDicLockSlim.EnterUpgradeableReadLock();
                List<Tuple<EntityBufType, bool>> val;
                if (EntityBufTypeDic.TryGetValue(key, out val))
                {
                    if (val != null)
                    {
                        return val;
                    }
                }

                try
                {
                    EntityBufTypeDicLockSlim.EnterWriteLock();

                    List<Tuple<EntityBufType, bool>> list = new List<Tuple<EntityBufType, bool>>();

                    PropertyInfo[] props = tp.GetProperties();
                    bool isArray = false;
                    foreach (PropertyInfo prop in props)
                    {
                        EntityBufType buftype = MapBufType(prop.PropertyType, out isArray);
                        buftype.Property = new PropertyInfoEx(prop);
                        list.Add(new Tuple<EntityBufType, bool>(buftype, isArray));
                    }
                    //再次检查下
                    try
                    {
                        EntityBufTypeDic.Add(key, list);
                    }
                    catch
                    {
                    }

                    return list;
                }
                finally
                {
                    EntityBufTypeDicLockSlim.ExitWriteLock();
                }
            }
            finally
            {
                //EntityBufTypeDicLockSlim.ExitUpgradeableReadLock();
            }
        }

        private static Tuple<EntityBufType, bool> GetTypeBufType(Type tp)
        {
            if (tp == null)
            {
                return null;
            }
            int key = tp.GetHashCode();
            if (TypeBufTypeDic.ContainsKey(key))
            {
                return TypeBufTypeDic[key];
            }
            else
            {
                try
                {
                    TypeBufTypeDicLockSlim.EnterWriteLock();
                    bool isArray;
                    EntityBufType objType = MapBufType(tp, out isArray);
                    Tuple<EntityBufType, bool> touple = new Tuple<EntityBufType, bool>(objType, isArray);
                    try
                    {
                        TypeBufTypeDic.Add(key, touple);
                    }
                    catch
                    {

                    }
                    return touple;
                }
                finally
                {
                    TypeBufTypeDicLockSlim.ExitWriteLock();
                }
            }
        }

        internal static EntityBufType MapBufType(Type type, out bool isArray)
        {
            EntityBufType ebtype = new EntityBufType();
            ebtype.ValueType = type;

            if (type.IsArray)
            //if (type.Name.EndsWith("[]"))
            {
                isArray = true;
                string typefullname = string.Format("{0}, {1}", type.FullName.Substring(0, type.FullName.LastIndexOf('[')),
                                type.Assembly.FullName);
                ebtype.ClassType = Type.GetType(typefullname);
            }
            else
            {
                isArray = false;
                ebtype.ClassType = type;
            }

            string typename = ebtype.ClassType.Name;
            switch (typename)
            {
                case "SByte":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.SBYTE;
                    ebtype.DefaultValue = default(sbyte);
                    break;
                case "Short":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.SHORT;
                    ebtype.DefaultValue = default(short);
                    break;
                case "Ushort":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.USHORT;
                    ebtype.DefaultValue = default(ushort);
                    break;
                case "Int16":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.INT16;
                    ebtype.DefaultValue = default(Int16);
                    break;
                case "Int32":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.INT32;
                    ebtype.DefaultValue = default(Int16);
                    break;
                case "Long":
                case "Int64":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.INT64;
                    ebtype.DefaultValue = default(Int64);
                    break;
                case "Byte":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.BYTE;
                    ebtype.DefaultValue = default(byte);
                    break;
                case "Char":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.CHAR;
                    ebtype.DefaultValue = default(char);
                    break;
                case "Double":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.DOUBLE;
                    ebtype.DefaultValue = default(double);
                    break;
                case "Single":
                case "Float":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.FLOAT;
                    ebtype.DefaultValue = default(float);
                    break;
                case "String":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.STRING;
                    ebtype.DefaultValue = default(string);
                    break;
                case "DateTime":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.DATETIME;
                    break;
                case "Decimal":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.DECIMAL;
                    ebtype.DefaultValue = default(decimal);
                    break;
                case "Boolean":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.BOOL;
                    ebtype.DefaultValue = default(bool);
                    break;
                case "Dictionary`2":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.DICTIONARY;
                    break;
                case "List`1":
                    ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.LIST;
                    break;
                default:
                    if (ebtype.ClassType.IsEnum)
                    {
                        ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.ENUM;
                    }
                    else if (ebtype.ClassType.IsClass)
                    {
                        ebtype.EntityType = LJC.FrameWork.EntityBuf.EntityType.COMPLEX;
                    }
                    break;
            }

            return ebtype;
        }
    }
}
