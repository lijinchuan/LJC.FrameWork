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
        private const int minGZIPCompressLenth = 21;
        private static ReaderWriterLockSlim EntityBufTypeDicLockSlim = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim TypeBufTypeDicLockSlim = new ReaderWriterLockSlim();

        /// <summary>
        /// 类型缓存
        /// </summary>
        private static Dictionary<int, List<Tuple<EntityBufType, bool>>> EntityBufTypeDic = new Dictionary<int, List<Tuple<EntityBufType, bool>>>();
        private static Dictionary<int, Tuple<EntityBufType, bool>> TypeBufTypeDic = new Dictionary<int, Tuple<EntityBufType, bool>>();

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
            switch (typename)
            {
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
                    if(isArray)
                    {
                        ebtype.EntityType = EntityType.ARRAY;
                    }
                    else if (ebtype.ClassType.IsEnum)
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

        private static List<Tuple<EntityBufType,bool>> GetTypeEntityBufType(Type tp)
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
                        buftype.Property =new PropertyInfoEx(prop);
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
                case EntityType.SHORT:
                case EntityType.INT16:
                    if (isArray)
                    {
                        msWriter.WriteInt16Array((Int16[])val);
                    }
                    else
                    {
                        msWriter.WriteInt16((Int16)val);
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
                case EntityType.ARRAY:
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
                        var arr = (Array)val;
                        msWriter.WriteInt32(arr.Length);
                        foreach (var item in arr)
                        {
                            Serialize(item, msWriter);
                        }
                    }
                    break;
                default:
                    throw new Exception("序列化错误");
            }
        }

        public static byte[] Serialize(object o, bool compress=true)
        {
            MemoryStream ms = new MemoryStream();
            MemoryStreamWriter writer = new MemoryStreamWriter(ms);
            Serialize(o, writer);
            var bytes = writer.GetBytes();

            //if (compress&&bytes.Length>minGZIPCompressLenth)
            //{
            //    var compressBytes = GZip.Compress(bytes);
            //    return compressBytes;
            //}
            //else
            //{
            //    return bytes;
            //}

            return bytes;
        }

        public static void Serialize(object o, BufferPollManager poolmanager, ref int bufferindex, ref long size, ref byte[] serbyte)
        {
            MemoryStreamWriter writer = new MemoryStreamWriter(poolmanager);

            Serialize(o, writer);
            bufferindex = writer.Bufferindex;
            size = writer.GetDataLen();

            if (bufferindex == -1)
            {
                serbyte = writer.GetBytes();
            }
        }

        public static void Serialize(object o, string file, bool compress = true)
        {
            byte[] results= Serialize(o, compress);
            using (FileStream fs=new FileStream(file,FileMode.Create))
            {
                int len = results.Length;
                byte[] lenByte = BitConverter.GetBytes(len);
                fs.Write(lenByte, 0, lenByte.Length);
                fs.Write(results, 0, len);
            }
        }

        private static void Serialize(object o, MemoryStreamWriter msWriter)
        {
            if (o == null)
            {
                EntityBufTypeFlag flag = EntityBufTypeFlag.VlaueNull;
                msWriter.WriteByte((byte)flag);
                return;
            }
            else
            {
                EntityBufTypeFlag flag = EntityBufTypeFlag.Empty;
                msWriter.WriteByte((byte)flag);
            }

            Tuple<EntityBufType, bool> touple = GetTypeBufType(o.GetType());

            if (touple.Item1.EntityType != EntityType.COMPLEX)
            {
                SerializeSimple(o, touple.Item2, touple.Item1, msWriter);
                return;
            }

            bool isArray;
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

        private static int dirctionaryIndex = typeof(Dictionary<int, int>).AssemblyQualifiedName.IndexOf("[[");
        private static Type[] GetDirctionaryKeyValueType(Type iDicType)
        {
            Type[] ret=new Type[2];
            //System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            string keytypename = "",valtypename="";
            //int firstdoubleleftbracket = iDicType.AssemblyQualifiedName.IndexOf("[[");
            int firstdoubleleftbracket = dirctionaryIndex;
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
                case EntityType.SHORT:
                case EntityType.INT16:
                    if (isArray)
                    {
                        return msReader.ReadInt16Array();
                    }
                    else
                    {
                        return msReader.ReadInt16();
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
                        var listvaluetype=GetListValueType(buftype.ValueType);
                        for (int i = 0; i < listlen; i++)
                        {
                            list.Add(DeSerialize(listvaluetype, msReader));
                        }
                        return list;
                    }
                case EntityType.ARRAY:
                    if (isArray)
                    {
                        var listarrlen = msReader.ReadInt32();
                        if (listarrlen == -1)
                            return null;
                        var listArray = (Array)Activator.CreateInstance(buftype.ValueType, listarrlen);
                        for (int i = 0; i < listarrlen; i++)
                        {
                            listArray.SetValue(DeSerialize(buftype.ClassType, msReader), i);
                        }
                        return listArray;
                    }
                    else
                    {
                        var arrlen = msReader.ReadInt32();
                        if (arrlen == -1)
                            return null;
                        var arr = (Array)Activator.CreateInstance(buftype.ValueType,arrlen);
                        var listvaluetype = GetListValueType(buftype.ValueType);
                        for (int i = 0; i < arrlen; i++)
                        {
                            arr.SetValue(DeSerialize(listvaluetype, msReader), i);
                        }
                        return arr;
                    }
                default:
                    throw new Exception("反序列化错误");
            }
        }

        public static object DeSerialize(Type DestType, byte[] bytes, bool compress = true)
        {
            //var decompressBytes=bytes;
            //if (compress && bytes != null && bytes.Length > minGZIPCompressLenth)
            //{
            //    decompressBytes = GZip.Decompress(bytes);
            //}
            //var ms = new MemoryStream(decompressBytes);

            var ms = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(ms);
            MemoryStreamReader rd = new MemoryStreamReader(reader);
            var obj = DeSerialize(DestType, rd);
            return obj;
        }

        public static T DeSerialize<T>(byte[] bytes, bool compress = true)
        {
            var obj = DeSerialize(typeof(T), bytes, compress);
            return (T)obj;
        }

        private static object DeSerialize(Type DestType, MemoryStreamReader msReader)
        {
            var firstByte= (EntityBufTypeFlag)msReader.ReadByte();
            if ((firstByte & EntityBufTypeFlag.VlaueNull) == EntityBufTypeFlag.VlaueNull)
            {
                return null;
            }

            //EntityBufType destTypeBufType = MapBufType(DestType, out isArray);
            Tuple<EntityBufType, bool> touple = GetTypeBufType(DestType);
            if (touple.Item1.EntityType != EntityBuf.EntityType.COMPLEX)
            {
                return DeserializeSimple(touple.Item1, touple.Item2, msReader);
            }

            bool isArray;
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
                            //ret.SetValue(buftype.Item1.Property, null);
                            //ret.SetValueDrect(buftype.Item1.Property, null);
                            continue;
                        }
                        else
                        {
                            object[] objs = (object[])System.Activator.CreateInstance(buftype.Item1.Property.PropertyInfo.PropertyType, len);
                            
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
                            if (!object.Equals(objs, buftype.Item1.DefaultValue))
                            {
                                ret.SetValue(buftype.Item1.Property, objs);
                            }
                        }
                    }
                    else
                    {
                        //读下标志
                        EntityBufTypeFlag flag=(EntityBufTypeFlag)msReader.ReadByte();
                        if ((flag & EntityBufTypeFlag.VlaueNull)==EntityBufTypeFlag.VlaueNull)
                        {
                            //ret.SetValue(buftype.Item1.Property, null);
                            continue;
                        }
                        else
                        {
                            object val = DeSerialize(buftype.Item1.Property.PropertyInfo.PropertyType, msReader);
                            if (!object.Equals(val, buftype.Item1.DefaultValue))
                            {
                                ret.SetValue(buftype.Item1.Property, val);
                            }
                        }
                    }
                }
                else
                {
                    object val = DeserializeSimple(buftype.Item1, isArray, msReader);
                    if (!object.Equals(val,buftype.Item1.DefaultValue))
                    {
                        ret.SetValue(buftype.Item1.Property, val);
                    }
                }
            }

            return ret;
        }

        public static object DeSerialize(Type DestType, string file, bool compress = true)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                byte[] btLen=new byte[4];
                fs.Read(btLen, 0, btLen.Length);
                int size = BitConverter.ToInt32(btLen, 0);

                byte[] buffer = new byte[size];
                fs.Read(buffer, 0, size);

                return DeSerialize(DestType, buffer, compress);
            }
        }
    }
}
