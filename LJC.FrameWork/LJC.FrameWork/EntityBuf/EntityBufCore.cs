using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Cache;
using System.Reflection;
using System.IO;
using System.Collections;
using LJC.FrameWork.Comm;
using System.Threading;

namespace LJC.FrameWork.EntityBuf
{
    public class EntityBufCore
    {
        private static ReaderWriterLockSlim EntityBufTypeDicLockSlim = new ReaderWriterLockSlim();
        /// <summary>
        /// 类型缓存
        /// </summary>
        private static Dictionary<string, List<Tuple<EntityBufType, bool>>> EntityBufTypeDic = new Dictionary<string, List<Tuple<EntityBufType, bool>>>();

        private static EntityBufType MapBufType(Type type, out bool isArray)
        {
            EntityBufType ebtype = new EntityBufType();
            ebtype.ValueType = type;

            if(type.IsArray)
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
            switch (typename.ToLower())
            {
                case "short":
                    ebtype.EntityType = EntityType.SHORT;
                    break;
                case "ushort":
                    ebtype.EntityType = EntityType.USHORT;
                    break;
                case "int16":
                    ebtype.EntityType = EntityType.INT16;
                    break;
                case "int32":
                    ebtype.EntityType = EntityType.INT32;
                    break;
                case "long":
                case "int64":
                    ebtype.EntityType = EntityType.INT64;
                    break;
                case "byte":
                    ebtype.EntityType = EntityType.BYTE;
                    break;
                case "char":
                    ebtype.EntityType = EntityType.CHAR;
                    break;
                case "double":
                    ebtype.EntityType = EntityType.DOUBLE;
                    break;
                case "float":
                    ebtype.EntityType = EntityType.FLOAT;
                    break;
                case "string":
                    ebtype.EntityType = EntityType.STRING;
                    break;
                case "datetime":
                    ebtype.EntityType = EntityType.DATETIME;
                    break;
                case "decimal":
                    ebtype.EntityType = EntityType.DECIMAL;
                    break;
                case "boolean":
                    ebtype.EntityType = EntityType.BOOL;
                    break;
                case "dictionary`2":
                    ebtype.EntityType = EntityType.DICTIONARY;
                    break;
                case "list`1":
                    ebtype.EntityType = EntityType.LIST;
                    break;
                default:
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

        private static List<Tuple<EntityBufType,bool>> GetTypeEntityBufType(Type tp)
        {
            if (tp == null)
                return null;

            string key = tp.Name;
            try
            {
                EntityBufTypeDicLockSlim.EnterUpgradeableReadLock();
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
                        buftype.Property = prop;
                        list.Add(new Tuple<EntityBufType, bool>(buftype, isArray));
                    }
                    EntityBufTypeDic.Add(key, list);
                    return list;
                }
                finally
                {
                    EntityBufTypeDicLockSlim.ExitWriteLock();
                }
            }
            finally
            {
                EntityBufTypeDicLockSlim.ExitUpgradeableReadLock();
            }
        }

        private static void SerializeComplex(object val,bool isArray, EntityBufType bufType, MemoryStreamWriter msWriter)
        {
            if (isArray)
            {
                var vals = (object[])val;
                int len = -1;
                if (vals != null)
                {
                    len = vals.Length;
                }
                msWriter.WriteInt32(len);
                if (len > 0)
                {
                    foreach (object v in vals)
                    {
                        //写入标志
                        if (v != null)
                        {
                            EntityBufTypeFlag flag = EntityBufTypeFlag.Empty;
                            msWriter.WriteByte((byte)flag);
                            Serialize(v, msWriter);
                        }
                        else
                        {
                            EntityBufTypeFlag flag = EntityBufTypeFlag.VlaueNull;
                            msWriter.WriteByte((byte)flag);
                        }
                    }
                }

            }
            else
            {
                if (val != null)
                {
                    EntityBufTypeFlag flag = EntityBufTypeFlag.Empty;
                    msWriter.WriteByte((byte)flag);
                    Serialize(val, msWriter);
                }
                else
                {
                    EntityBufTypeFlag flag = EntityBufTypeFlag.VlaueNull;
                    msWriter.WriteByte((byte)flag);
                }
            }
        }

        private static void SerializeSimple(object val, bool isArray, EntityBufType bufType, MemoryStreamWriter msWriter)
        {
            if (bufType.EntityType == EntityType.COMPLEX)
            {
                throw new Exception("无法序列化复杂类型");
            }

            switch (bufType.EntityType)
            {
                case EntityType.BYTE:
                    if (isArray)
                    {
                        msWriter.WriteByteArray((byte[])val);
                    }
                    else
                    {
                        msWriter.WriteByte((byte)val);
                    }
                    break;
                case EntityType.STRING:
                    if (isArray)
                    {
                        msWriter.WriteStringArray((string[])val);
                    }
                    else
                    {
                        msWriter.WriteString((string)val);
                    }
                    break;
                case EntityType.INT32:
                    if (isArray)
                    {
                        msWriter.WriteInt32Array((Int32[])val);
                    }
                    else
                    {
                        msWriter.WriteInt32((Int32)val);
                    }
                    break;
                case EntityType.DECIMAL:
                    if (isArray)
                    {
                        msWriter.WriteDeciamlArray((decimal[])val);
                    }
                    else
                    {
                        msWriter.WriteDecimal((decimal)val);
                    }
                    break;
                case EntityType.DOUBLE:
                    if (isArray)
                    {
                        msWriter.WriteDoubleArray((double[])val);
                    }
                    else
                    {
                        msWriter.WriteDouble((double)val);
                    }
                    break;
                case EntityType.INT64:
                    if (isArray)
                    {
                        msWriter.WriteInt64Array((Int64[])val);
                    }
                    else
                    {
                        msWriter.WriteInt64((Int64)val);
                    }
                    break;
                case EntityType.DATETIME:
                    if (isArray)
                    {
                        msWriter.WriteDateTimeArray((DateTime[])val);
                    }
                    else
                    {
                        msWriter.WriteDateTime((DateTime)val);
                    }
                    break;
                case EntityType.BOOL:
                    if (isArray)
                    {
                        msWriter.WriteBoolArray((bool[])val);
                    }
                    else
                    {
                        msWriter.WriteBool((bool)val);
                    }
                    break;
                case EntityType.ENUM:
                    if (isArray)
                    {
                        Array arr = (Array)val;
                        string[] strarr = new string[arr.Length];
                        for (int i = 0; i < arr.Length; i++)
                        {
                            strarr[i] = arr.GetValue(i).ToString();
                        }
                        msWriter.WriteStringArray(strarr);
                    }
                    else
                    {
                        msWriter.WriteString(val.ToString());
                    }
                    break;
                case EntityType.DICTIONARY:
                    if (isArray)
                    {
                        if (val == null)
                        {
                            msWriter.WriteInt32(-1);
                            break;
                        }
                        var dicArray = (Array)val;
                        msWriter.WriteInt32(dicArray.Length);
                        for (int i = 0; i < dicArray.Length; i++)
                        {
                            Serialize(dicArray.GetValue(i), msWriter);
                        }
                    }
                    else
                    {
                        if (val == null)
                        {
                            msWriter.WriteInt32(-1);
                            break;
                        }
                        //
                        IDictionary idic = (IDictionary)val;
                        //写入长度
                        msWriter.WriteInt32(idic.Count);
                        int i = 0;
                        foreach (var kv in idic)
                        {
                            object k=kv.Eval("Key");
                            object v = kv.Eval("Value");

                            Serialize(k, msWriter);
                            Serialize(v, msWriter);
                            i++;
                        }
                    }
                    break;
                case EntityType.LIST:
                    if (isArray)
                    {
                        if (val == null)
                        {
                            msWriter.WriteInt32(-1);
                            break;
                        }
                        var listarr = (Array)val;
                        msWriter.WriteInt32(listarr.Length);
                        for (int i = 0; i < listarr.Length; i++)
                        {
                            Serialize(listarr.GetValue(i), msWriter);
                        }
                    }
                    else
                    {
                        if (val == null)
                        {
                            msWriter.WriteInt32(-1);
                            break;
                        }
                        var list = (IList)val;
                        msWriter.WriteInt32(list.Count);
                        foreach (var item in list)
                        {
                            Serialize(item, msWriter);
                        }
                    }
                    break;
                default:
                    throw new Exception("序列化错误");
            }
        }

        public static byte[] Serialize(object o)
        {
            MemoryStream ms = new MemoryStream();
            MemoryStreamWriter writer = new MemoryStreamWriter(ms);
            Serialize(o, writer);
            return writer.GetBytes();
        }

        private static void Serialize(object o, MemoryStreamWriter msWriter)
        {
            bool isArray;
            EntityBufType objType = MapBufType(o.GetType(), out isArray);
            if (objType.EntityType != EntityType.COMPLEX)
            {
                SerializeSimple(o, isArray, objType, msWriter);
                return;
            }

            //PropertyInfo[] props = o.GetType().GetProperties();
            var entitybuftypelist = GetTypeEntityBufType(o.GetType());
            foreach (var tp in entitybuftypelist)
            {
                //EntityBufType buftype = MapBufType(prop.PropertyType, out isArray);
                isArray = tp.Item2;
                object val = o.Eval(tp.Item1.Property);
                if (tp.Item1.EntityType == EntityType.COMPLEX)
                {
                    SerializeComplex(val, isArray, tp.Item1, msWriter);
                }
                else
                {
                    SerializeSimple(val, isArray, tp.Item1, msWriter);
                }
            }
        }

        private static Type GetListValueType(Type listType)
        {
            Type ret = null;
            //System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            int firstdoubleleftbracket = listType.AssemblyQualifiedName.IndexOf("[[");
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

        private static Type[] GetDirctionaryKeyValueType(Type iDicType)
        {
            Type[] ret=new Type[2];
            //System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            string keytypename = "",valtypename="";
            int firstdoubleleftbracket = iDicType.AssemblyQualifiedName.IndexOf("[[");
            int bracketCount = 1;
            int i = firstdoubleleftbracket+2;
            //int len=iDicType.AssemblyQualifiedName.Length;
            var qualifiedNameChars = iDicType.AssemblyQualifiedName.ToCharArray();
            int len = qualifiedNameChars.Length;
            for (; i <len ; i++)
            {
                //char indxChar=iDicType.AssemblyQualifiedName[i];
                if (qualifiedNameChars[i]== '[')
                {
                    bracketCount++;
                }
                else if (qualifiedNameChars[i] == ']')
                {
                    bracketCount--;
                }
                if (bracketCount == 0)
                {
                    keytypename = iDicType.AssemblyQualifiedName.Substring(firstdoubleleftbracket + 2, i - firstdoubleleftbracket - 2);
                    ret[0] = Type.GetType(keytypename);
                    i++;
                    break;
                }
            }

            bracketCount = -1;
            for (; i < len; i++)
            {
                if (qualifiedNameChars[i] == '[')
                {
                    if (bracketCount == -1)
                    {
                        bracketCount = 1;
                        firstdoubleleftbracket=i;
                    }
                    else
                    {
                        bracketCount++;
                    }
                }
                else if (qualifiedNameChars[i] == ']')
                {
                    bracketCount--;
                }

                if (bracketCount == 0)
                {
                    valtypename = iDicType.AssemblyQualifiedName.Substring(firstdoubleleftbracket + 1, i - firstdoubleleftbracket - 1);
                    ret[1] = Type.GetType(valtypename);
                    break;
                }
            }
            
            return ret;
        }


        private static object DeserializeSimple(EntityBufType buftype, bool isArray, MemoryStreamReader msReader)
        {
            if (buftype.EntityType == EntityType.COMPLEX)
            {
                throw new Exception("无法反序列化复杂类型");
            }

            if (buftype.EntityType == EntityType.UNKNOWN)
            {
                throw new Exception("无法反序列化未知类型");
            }

            switch (buftype.EntityType)
            {
                case EntityType.BYTE:
                    if (isArray)
                    {
                        return msReader.ReadByteArray();
                    }
                    else
                    {
                        return msReader.ReadByte();
                    }
                case EntityType.STRING:
                    if (isArray)
                    {
                        return msReader.ReadStringArray();
                    }
                    else
                    {
                        return msReader.ReadString();
                    }
                case EntityType.INT32:
                    if (isArray)
                    {
                        return msReader.ReadInt32Array();
                    }
                    else
                    {
                        return msReader.ReadInt32();
                    }
                    
                case EntityType.INT64:
                    if (isArray)
                    {
                        return msReader.ReadInt64Array();
                    }
                    else
                    {
                        return msReader.ReadInt64();
                    }
                case EntityType.DOUBLE:
                    if (isArray)
                    {
                        return msReader.ReadDoubleArray();
                    }
                    else
                    {
                        return msReader.ReadDouble();
                    }
                case EntityType.DECIMAL:
                    if (isArray)
                    {
                        return msReader.ReadDeciamlArray();
                    }
                    else
                    {
                        return msReader.ReadDecimal();
                    }
                case EntityType.DATETIME:
                    if (isArray)
                    {
                        return msReader.ReadDateTimeArray();
                    }
                    else
                    {
                        return msReader.ReadDateTime();
                    }
                case EntityType.BOOL:
                    if (isArray)
                    {
                        return msReader.ReadBoolArray();
                    }
                    else
                    {
                        return msReader.ReadBool();
                    }
                case EntityType.ENUM:
                    if (isArray)
                    {
                        string[] strarray = msReader.ReadStringArray();
                        Array arr=  (Array)Activator.CreateInstance(buftype.ValueType, strarray.Length);
                        for (int i = 0; i < strarray.Length; i++)
                        {
                            arr.SetValue(Enum.Parse(buftype.ClassType, strarray[i]), i);
                        }
                        return arr;
                    }
                    else
                    {
                        return Enum.Parse(buftype.ClassType, msReader.ReadString());
                    }
                case EntityType.DICTIONARY:
                    if (isArray)
                    {
                        int arrlen = msReader.ReadInt32();
                        if (arrlen == -1)
                            return null;

                        var dicarr = (Array)Activator.CreateInstance(buftype.ValueType,arrlen);
                        for (int i = 0; i < arrlen; i++)
                        {
                            dicarr.SetValue(DeSerialize(buftype.ClassType,msReader), i);
                        }

                        return dicarr;
                    }
                    else
                    {
                        int dicLen=msReader.ReadInt32();
                        if (dicLen == -1)
                        {
                            return null;
                        }
                        
                        IDictionary idic=(IDictionary)Activator.CreateInstance(buftype.ValueType);
                        var keyvaluetype = GetDirctionaryKeyValueType(buftype.ValueType);

                        for (int i = 0; i < dicLen; i++)
                        {
                            idic.Add(DeSerialize(keyvaluetype[0], msReader), DeSerialize(keyvaluetype[1], msReader));
                        }

                        return idic;
                    }
                case EntityType.LIST:
                    if (isArray)
                    {
                        var listarrlen = msReader.ReadInt32();
                        if (listarrlen == -1)
                            return null;
                        var listArray = (Array)Activator.CreateInstance(buftype.ValueType, listarrlen);
                        for (int i = 0; i < listarrlen; i++)
                        {
                            listArray.SetValue(DeSerialize(buftype.ClassType,msReader),i);
                        }
                        return listArray;
                    }
                    else
                    {
                        var listlen = msReader.ReadInt32();
                        if (listlen == -1)
                            return null;
                        var list = (IList)Activator.CreateInstance(buftype.ValueType);
                        for (int i = 0; i < listlen; i++)
                        {
                            list.Add(DeSerialize(GetListValueType(buftype.ValueType),msReader));
                        }
                        return list;
                    }
                default:
                    throw new Exception("反序列化错误");
            }
        }

        public static object DeSerialize(Type DestType,byte[] bytes)
        {
            var ms = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(ms);
            MemoryStreamReader rd = new MemoryStreamReader(reader);
            var obj = DeSerialize(DestType, rd);
            return obj;
        }

        public static T DeSerialize<T>(byte[] bytes)
        {
            var obj = DeSerialize(typeof(T), bytes);
            return (T)obj;
        }

        private static object DeSerialize(Type DestType, MemoryStreamReader msReader)
        {
            bool isArray;
            EntityBufType destTypeBufType = MapBufType(DestType, out isArray);
            if (destTypeBufType.EntityType != EntityBuf.EntityType.COMPLEX)
            {
                return DeserializeSimple(destTypeBufType, isArray, msReader);
            }

            object ret = System.Activator.CreateInstance(DestType);
            //PropertyInfo[] props = DestType.GetProperties();
            var buftypelist= GetTypeEntityBufType(DestType);
            foreach (var buftype in buftypelist)
            {
                //EntityBufType buftype = MapBufType(prop.PropertyType, out isArray);
                isArray=buftype.Item2;
                if (buftype.Item1.EntityType == EntityType.COMPLEX)
                {
                    if (isArray)
                    {
                        int len = msReader.ReadInt32();
                        if (len == -1)
                        {
                            ret.SetValue(buftype.Item1.Property, null);
                        }
                        else
                        {
                            object[] objs = (object[])System.Activator.CreateInstance(buftype.Item1.Property.PropertyType, len);
                            
                            for (int i = 0; i < len; i++)
                            {
                                //读下标志
                                EntityBufTypeFlag flag=(EntityBufTypeFlag)msReader.ReadByte();
                                if ((flag & EntityBufTypeFlag.VlaueNull) == EntityBufTypeFlag.VlaueNull)
                                {
                                    objs[i] = null;
                                }
                                else
                                {
                                    //string typefullname = string.Format("{0}, {1}", buftype.Item1.Property.PropertyType.FullName.Substring(0, buftype.Item1.Property.PropertyType.FullName.LastIndexOf('[')),
                                    //buftype.Item1.Property.PropertyType.Assembly.FullName);
                                    //objs[i] = DeSerialize(Type.GetType(typefullname, false, true), msReader);
                                    objs[i] = DeSerialize(buftype.Item1.ClassType , msReader);
                                }
                                
                            }
                            ret.SetValue(buftype.Item1.Property, objs);
                        }
                    }
                    else
                    {
                        //读下标志
                        EntityBufTypeFlag flag=(EntityBufTypeFlag)msReader.ReadByte();
                        if ((flag & EntityBufTypeFlag.VlaueNull)==EntityBufTypeFlag.VlaueNull)
                        {
                            ret.SetValue(buftype.Item1.Property, null);
                        }
                        else
                        {
                            object val = DeSerialize(buftype.Item1.Property.PropertyType, msReader);
                            ret.SetValue(buftype.Item1.Property, val);
                        }
                    }
                }
                else
                {
                    object val = DeserializeSimple(buftype.Item1, isArray, msReader);
                    ret.SetValue(buftype.Item1.Property, val);
                }
            }

            return ret;
        }
    }
}
