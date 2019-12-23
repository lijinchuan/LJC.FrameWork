using LJC.FrameWork.Comm;
using LJC.FrameWork.EntityBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi.EntityBuf
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
                StringBuilder insb = new StringBuilder("<table cellpadding=0 cellspacing=0 border=\"0\">");
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

                    if (tp.Item1.EntityType == EntityType.COMPLEX)
                    {
                        //这里加上属性
                        StringBuilder insb = new StringBuilder("<table cellpadding=0 cellspacing=0 border=\"0\">");
                        insb.AppendFormat("<tr><th>参数名</th><th>参数类型</th><th>备注</th></tr>");
                        //GetPropToTable(bufType.ClassType, insb);
                        DelComplex(isTypeArray2, tp.Item1, insb);
                        insb.Append("</table>");

                        sb.AppendFormat("<tr><td>{0}</td><td>+{1}<br/>{2}</td><td>{3}</td></tr>",
                            tp.Item1.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(tp.Item1.Property.PropertyInfo),
                            tp.Item1.ValueType.Name,
                            insb.ToString(),
                            tp.Item1.Property == null ? string.Empty : ReflectionHelper.GetObjectDescription(tp.Item1.Property));
                    }
                    else
                    {
                        DelSimple(isTypeArray2, tp.Item1, sb);
                    }
                }
            }
        }

        private static void GetComplexInvokeHtml(bool isArray, EntityBufType bufType, StringBuilder sb)
        {
            if (isArray)
            {
                //StringBuilder insb = new StringBuilder("<table cellpadding=0 _obj=\"\"  cellspacing=0 border=\"0\">");
                StringBuilder insb = new StringBuilder();
                GetInvokeHtml(bufType.ClassType, true, insb);
                //insb.Append("</table>");
                sb.Append(insb.ToString());
            }
            else
            {
                bool isTypeArray2;
                var entitybuftypelist = GetTypeEntityBufType(bufType.ClassType);
                foreach (var tp in entitybuftypelist)
                {
                    isTypeArray2 = tp.Item2;

                    if (tp.Item1.EntityType == EntityType.COMPLEX)
                    {
                        var ppname = tp.Item1.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(tp.Item1.Property.PropertyInfo);
                        //这里加上属性
                        StringBuilder insb = new StringBuilder(string.Format("<table cellpadding=0 _obj=\"1\" cellspacing=0 border=\"0\" {0} _call=\"attach,{1},{2}\">", isTypeArray2 ? "class=\"arraytable\"" : "", ppname, isTypeArray2));

                        GetComplexInvokeHtml(isTypeArray2, tp.Item1, insb);
                        insb.Append("</table>");

                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", ppname,
                            insb.ToString());
                    }
                    else
                    {
                        GetSimpleInvokeHtml(isTypeArray2, tp.Item1, sb);
                    }
                }
            }
        }

        private static void DelSimple(bool isArray, EntityBufType bufType, StringBuilder sb)
        {
            if (bufType.EntityType == EntityType.COMPLEX)
            {
                throw new Exception("无法处理复杂类型");
            }

            switch (bufType.EntityType)
            {
                case EntityType.SBYTE:
                case EntityType.BYTE:

                case EntityType.STRING:

                case EntityType.SHORT:
                case EntityType.INT16:

                case EntityType.INT32:

                case EntityType.DECIMAL:
                case EntityType.FLOAT:
                case EntityType.DOUBLE:

                case EntityType.INT64:

                case EntityType.DATETIME:

                case EntityType.BOOL:
                    string jsonpropname = bufType.Property != null ? ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo) : string.Empty;
                    if (!string.IsNullOrWhiteSpace(jsonpropname))
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), bufType.EntityType.ToString().ToLower() + (isArray ? "[]" : ""), ReflectionHelper.GetObjectDescription(bufType.Property));
                    }
                    else
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", "data", bufType.EntityType.ToString().ToLower() + (isArray ? "[]" : ""), string.Empty);
                    }
                    break;
                case EntityType.ENUM:
                    if (isArray)
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}[]</td><td>{2}</td></tr>", bufType.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), "enum[]", ReflectionHelper.GetEnumDesc(bufType.ValueType));
                    }
                    else
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", bufType.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), "enum", ReflectionHelper.GetEnumDesc(bufType.ValueType));
                    }
                    break;
                case EntityType.DICTIONARY:
                    if (isArray)
                    {
                        sb.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), "Dictionary[]", ReflectionHelper.GetEnumDesc(bufType.Property.PropertyInfo.PropertyType));
                    }
                    else
                    {
                        sb.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), "Dictionary", ReflectionHelper.GetEnumDesc(bufType.Property.PropertyInfo.PropertyType));
                    }
                    break;
                case EntityType.LIST:
                    if (isArray)
                    {

                    }
                    else
                    {
                        var listvaluetype = GetListValueType(bufType.ValueType);
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", bufType.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo), string.Format("List&lt;{0}&gt;", listvaluetype.Name), bufType.Property == null ? string.Empty : ReflectionHelper.GetObjectDescription(bufType.Property));

                        bool isarray = false;
                        if (MapBufType(listvaluetype, out isarray).EntityType == EntityType.COMPLEX)
                        {
                            StringBuilder sbinner = new StringBuilder("<table  cellpadding=0 cellspacing=0 border=\"0\">");
                            sbinner.AppendFormat("<tr><th>参数名</th><th>参数类型</th><th>备注</th></tr>");
                            GetPropToTable(listvaluetype, sbinner);
                            sbinner.Append("</table>");
                            sb.AppendFormat("<tr><td colspan=\"3\">+{0}<br/>{1}</td></tr>", listvaluetype.Name, sbinner.ToString());
                        }
                        else if (MapBufType(listvaluetype, out isarray).EntityType == EntityType.ENUM)
                        {
                            StringBuilder sbinner = new StringBuilder("<table  cellpadding=0 cellspacing=0 border=\"0\">");
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

        private static void GetSimpleInvokeHtml(bool isArray, EntityBufType bufType, StringBuilder sb)
        {
            if (bufType.EntityType == EntityType.COMPLEX)
            {
                throw new Exception("无法处理复杂类型");
            }

            switch (bufType.EntityType)
            {
                case EntityType.SBYTE:
                case EntityType.BYTE:
                case EntityType.SHORT:
                case EntityType.INT16:

                case EntityType.INT32:

                case EntityType.DECIMAL:
                case EntityType.FLOAT:
                case EntityType.DOUBLE:

                case EntityType.INT64:
                    {
                        string jsonpropname = bufType.Property != null ? ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo) : string.Empty;
                        if (!string.IsNullOrWhiteSpace(jsonpropname))
                        {
                            sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo),
                                string.Format("<input _call=\"set,{0}\" type='text' value=''>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo)));
                        }
                        else
                        {
                            sb.AppendFormat("<tr><td>{0} {1}</td></tr>", "<input _call=\"set,data\" type='text' value=''>", isArray ? "<span class=\"spanbutton\" onclick=\"copyrow(this)\">+</span><span class=\"spanbutton\" onclick=\"remrow(this)\">-</span>" : string.Empty);
                        }
                        break;
                    }
                case EntityType.STRING:
                case EntityType.DATETIME:
                    {
                        string jsonpropname = bufType.Property != null ? ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo) : string.Empty;
                        if (!string.IsNullOrWhiteSpace(jsonpropname))
                        {
                            sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo),
                                string.Format("<input _call=\"settext,{0}\" type='text' value=''>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo)));
                        }
                        else
                        {
                            sb.AppendFormat("<tr><td>{0} {1}</td></tr>", "<input _call=\"settext,data\" type='text' value=''>", isArray ? "<span class=\"spanbutton\" onclick=\"copyrow(this)\">+</span><span class=\"spanbutton\" onclick=\"remrow(this)\">-</span>" : string.Empty);
                        }
                        break;
                    }
                case EntityType.BOOL:
                    {
                        string jsonpropname = bufType.Property != null ? ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo) : string.Empty;
                        if (!string.IsNullOrWhiteSpace(jsonpropname))
                        {
                            sb.AppendFormat("<tr><td>{0}</td><td><input _call=\"setbool,{0}\" type='checkbox'/></td></tr>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo));
                        }
                        else
                        {
                            sb.AppendFormat("<tr><td>{0} {1}</td></tr>", "<input _call=\"settext,data\" type='checkbox'/>", isArray ? "<span class=\"spanbutton\" onclick=\"copyrow(this)\">+</span><span class=\"spanbutton\" onclick=\"remrow(this)\">-</span>" : string.Empty);
                        }
                        break;
                    }
                case EntityType.ENUM:
                    {
                        sb.AppendFormat("<tr>{0}<td {1}><select _call=\"settext,{2}\">", bufType.Property == null ? string.Empty : ("<td>" + ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo) + "</td>"), isArray ? "class=\"arraytd\"" : "",
                            bufType.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo));
                        foreach (var item in Enum.GetNames(bufType.ValueType))
                        {
                            sb.AppendFormat("<option value =\"{0}\">{0}</option>", item);
                        }
                        sb.AppendFormat("</select> {0}</td></tr>", isArray ? "<span class=\"spanbutton\" onclick=\"copyrow(this)\">+</span><span class=\"spanbutton\" onclick=\"remrow(this)\">-</span>" : string.Empty);
                    }
                    break;
                case EntityType.DICTIONARY:
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo));
                        sb.Append(string.Format("<table obj=\"\" _call=\"attach,{0},false\">", ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo)));
                        sb.Append("<tr>");
                        sb.AppendFormat("<td><input type='text'/>:<input _call=\"setdic\" type='text'/></td><td><span class=\"spanbutton\" onclick=\"copyrow(this)\">+</span><span class=\"spanbutton\" onclick=\"remrow(this)\">-</span></td>");
                        sb.Append("</tr>");
                        sb.Append("</table>");
                        sb.Append("</td></tr>");
                        break;
                    }
                case EntityType.LIST:
                    {
                        var ppname = ReflectionHelper.GetJsonPropertyName(bufType.Property.PropertyInfo);

                        var listvaluetype = GetListValueType(bufType.ValueType);

                        bool isarray = false;
                        if (MapBufType(listvaluetype, out isarray).EntityType == EntityType.COMPLEX)
                        {
                            if (!string.IsNullOrEmpty(ppname))
                            {
                                sb.AppendFormat("<tr><td>{0}</td><td><table obj=\"\" _call=\"attach,{0},True\">", ppname);
                            }
                            StringBuilder sbinner = new StringBuilder();
                            GetInvokeHtml(listvaluetype, true, sbinner);
                            //sbinner.Append("</table>");
                            sb.Append(sbinner.ToString());
                            if (!string.IsNullOrEmpty(ppname))
                            {
                                sb.Append("</table></td></tr>");
                            }
                        }
                        else
                        {
                            if (isarray)
                            {

                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(ppname))
                                {
                                    sb.AppendFormat("<tr><td>{0}</td><td><table obj=\"\" _call=\"attach,{0},True\">", ppname);
                                }
                                StringBuilder sbinner = new StringBuilder();
                                bool subisarray = false;
                                GetSimpleInvokeHtml(true, new EntityBufType
                                {
                                    ClassType = listvaluetype,
                                    ValueType = listvaluetype,
                                    EntityType = MapBufType(listvaluetype, out subisarray).EntityType,
                                    Property = null
                                }, sbinner);
                                sb.Append(sbinner.ToString());
                                if (!string.IsNullOrEmpty(ppname))
                                {
                                    sb.Append("</table></td></tr>");
                                }
                            }
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

            if (touple.Item1.EntityType != EntityType.COMPLEX)
            {
                DelSimple(touple.Item2, touple.Item1, sb);
            }
            else
            {
                DelComplex(touple.Item2, touple.Item1, sb);
            }
        }

        public static void GetInvokeHtml(Type type, bool array, StringBuilder sb)
        {
            Tuple<EntityBufType, bool> tuple = GetTypeBufType(type);
            StringBuilder innsersb = new StringBuilder();
            // touple.Item1
            innsersb.AppendFormat("<table cellpadding=0 cellspacing=0 _obj=\"1\" border=\"0\" {0} _call=\"attach,{1},{2}\">", tuple.Item2 ? "class=\"arraytable\"" : "",
                tuple.Item1.Property == null ? string.Empty : ReflectionHelper.GetJsonPropertyName(tuple.Item1.Property.PropertyInfo), tuple.Item2);
            if (array)
            {
                innsersb.AppendFormat("<tr><td>+{0}</td><td><span class=\"spanbutton\" onclick=\"copytable(this)\">+</span><span class=\"spanbutton\" onclick=\"remtable(this)\">-</span></td></tr>", type.Name);
            }
            if (tuple.Item1.EntityType != EntityType.COMPLEX)
            {
                GetSimpleInvokeHtml(tuple.Item2, tuple.Item1, innsersb);
            }
            else
            {
                GetComplexInvokeHtml(tuple.Item2, tuple.Item1, innsersb);
            }
            innsersb.Append("</table>");
            sb.AppendFormat("<tr><td colspan=\"2\">{0}</td></tr>", innsersb.ToString());
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
                    ebtype.EntityType = EntityType.SBYTE;
                    ebtype.DefaultValue = default(sbyte);
                    break;
                case "Short":
                    ebtype.EntityType = EntityType.SHORT;
                    ebtype.DefaultValue = default(short);
                    break;
                case "Ushort":
                    ebtype.EntityType = EntityType.USHORT;
                    ebtype.DefaultValue = default(ushort);
                    break;
                case "Int16":
                    ebtype.EntityType = EntityType.INT16;
                    ebtype.DefaultValue = default(Int16);
                    break;
                case "Int32":
                    ebtype.EntityType = EntityType.INT32;
                    ebtype.DefaultValue = default(Int16);
                    break;
                case "Long":
                case "Int64":
                    ebtype.EntityType = EntityType.INT64;
                    ebtype.DefaultValue = default(Int64);
                    break;
                case "Byte":
                    ebtype.EntityType = EntityType.BYTE;
                    ebtype.DefaultValue = default(byte);
                    break;
                case "Char":
                    ebtype.EntityType = EntityType.CHAR;
                    ebtype.DefaultValue = default(char);
                    break;
                case "Double":
                    ebtype.EntityType = EntityType.DOUBLE;
                    ebtype.DefaultValue = default(double);
                    break;
                case "Single":
                case "Float":
                    ebtype.EntityType = EntityType.FLOAT;
                    ebtype.DefaultValue = default(float);
                    break;
                case "String":
                    ebtype.EntityType = EntityType.STRING;
                    ebtype.DefaultValue = default(string);
                    break;
                case "DateTime":
                    ebtype.EntityType = EntityType.DATETIME;
                    break;
                case "Decimal":
                    ebtype.EntityType = EntityType.DECIMAL;
                    ebtype.DefaultValue = default(decimal);
                    break;
                case "Boolean":
                    ebtype.EntityType = EntityType.BOOL;
                    ebtype.DefaultValue = default(bool);
                    break;
                case "Dictionary`2":
                    ebtype.EntityType = EntityType.DICTIONARY;
                    break;
                case "List`1":
                    ebtype.EntityType = EntityType.LIST;
                    break;
                default:
                    //if(isArray)
                    //{
                    //    ebtype.EntityType = EntityType.ARRAY;
                    //}
                    //else 
                    if (ebtype.ClassType.IsEnum)
                    {
                        ebtype.EntityType = EntityType.ENUM;
                    }
                    else if (ebtype.ClassType.IsClass)
                    {
                        ebtype.EntityType = EntityType.COMPLEX;
                    }
                    break;
            }

            return ebtype;
        }
    }
}
