using LJC.FrameWork.Comm;
using LJC.FrameWork.EntityBuf2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.EntityBuf
{
    public class EntityBufCoreEx : EntityBufCore
    {
        //private static ReaderWriterLockSlim EntityBufTypeDicLockSlim = new ReaderWriterLockSlim();
        //private static ReaderWriterLockSlim TypeBufTypeDicLockSlim = new ReaderWriterLockSlim();

        private class ValueGen
        {
            public static byte GenByte()
            {
                return (byte)new Random().Next(byte.MinValue, byte.MaxValue);
            }

            public static IEnumerable<byte> GenBytes()
            {
                byte[] bytes = new byte[new Random().Next(1, 100)];
                new Random().NextBytes(bytes);

                return bytes;
            }

            public static string GenString()
            {
                //return Guid.NewGuid().ToString().Replace("-", "");
                return "string";
            }

            public static IEnumerable<string> GenStrings()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenString();
                }
            }

            public static Int16 GenInt16()
            {
                return (Int16)new Random().Next(Int16.MinValue, Int16.MaxValue);
            }

            public static IEnumerable<Int16> GenInt16s()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenInt16();
                }
            }

            public static UInt16 GenUInt16()
            {
                return (UInt16)new Random().Next(UInt16.MinValue, UInt16.MaxValue);
            }

            public static IEnumerable<UInt16> GenUInt16s()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenUInt16();
                }
            }

            public static Int32 GenInt32()
            {
                return new Random().Next(Int32.MinValue, Int32.MaxValue);
            }

            public static IEnumerable<Int32> GenInt32s()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenInt32();
                }
            }

            public static UInt32 GenUInt32()
            {
                return (UInt32)(new Random().Next(int.MinValue, int.MaxValue) & uint.MaxValue);
            }

            public static IEnumerable<UInt32> GenUInt32s()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenUInt32();
                }
            }

            public static char GenChar()
            {
                return (char)new Random().Next('A', 'z');
            }

            public static IEnumerable<char> GenChars()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenChar();
                }
            }

            public static decimal GenDecimal()
            {
                return (decimal)new Random().NextDouble() * 10000;
            }

            public static IEnumerable<decimal> GenDecimals()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenDecimal();
                }
            }

            public static double GenDouble()
            {
                return new Random().Next(-1, 2) * new Random().NextDouble() * double.MaxValue;
            }

            public static IEnumerable<double> GenDoubles()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenDouble();
                }
            }

            public static float GenFloat()
            {
                return (float)(new Random().Next(-1, 2) * new Random().NextDouble() * float.MaxValue);
            }

            public static IEnumerable<float> GenFloats()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenFloat();
                }
            }

            public static long GenLong()
            {
                var sc = long.MaxValue / int.MaxValue;

                var r = new Random().Next(int.MinValue, int.MaxValue);

                return (long)(0 + sc * r * new Random().NextDouble());
            }

            public static IEnumerable<long> GenLongs()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenLong();
                }
            }

            public static DateTime GenDate()
            {
                var diff = DateTime.MaxValue - DateTime.MinValue;
                return DateTime.MinValue.AddMilliseconds(diff.TotalMilliseconds * new Random().NextDouble());
            }

            public static IEnumerable<DateTime> GenDates()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenDate();
                }
            }

            public static bool GenBool()
            {
                return new Random().Next(1, 11) > 5;
            }

            public static IEnumerable<bool> GenBools()
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenBool();
                }
            }

            public static string GenEnum(Type enumtype)
            {
                var en = Enum.GetValues(enumtype);
                return en.GetValue(new Random().Next(en.Length)).ToString();
            }

            public static IEnumerable<string> GenEnums(Type enumtype)
            {
                int count = new Random().Next(1, 3);

                for (int i = 0; i < count; i++)
                {
                    yield return GenEnum(enumtype);
                }
            }
        }

        private static void GenSerializeComplex(bool isArray, EntityBufType bufType, MemoryStreamWriter msWriter)
        {
            if (isArray)
            {
                var len = new Random().Next(1, 3);

                msWriter.WriteInt32(len);
                if (len > 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        EntityBufTypeFlag flag = EntityBufTypeFlag.Empty;
                        msWriter.WriteByte((byte)flag);
                        GenSerialize(bufType.ValueType, msWriter);
                    }
                }

            }
            else
            {
                EntityBufTypeFlag flag = EntityBufTypeFlag.Empty;
                msWriter.WriteByte((byte)flag);
                GenSerialize(bufType.ValueType, msWriter);
            }
        }

        private static void GenSerializeSimple(object instance, bool isArray, EntityBufType bufType, MemoryStreamWriter msWriter)
        {
            //if (bufType.EntityType == EntityType.COMPLEX)
            //{
            //    throw new Exception("无法序列化复杂类型");
            //}

            object defaultvalue = null;

            if (instance != null)
            {
                defaultvalue = instance.Eval(bufType.Property.PropertyInfo);
            }

            switch (bufType.EntityType)
            {
                case EntityType.BYTE:
                    if (isArray)
                    {
                        if (defaultvalue != null)
                        {
                            msWriter.WriteByteArray((byte[])defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteByteArray(ValueGen.GenBytes().ToArray());
                        }
                    }
                    else
                    {
                        msWriter.WriteByte(ValueGen.GenByte());
                    }
                    break;
                case EntityType.STRING:
                    if (isArray)
                    {
                        if (defaultvalue != null)
                        {
                            msWriter.WriteStringArray((String[])defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteStringArray(ValueGen.GenStrings().ToArray());
                        }
                    }
                    else
                    {
                        if (defaultvalue != null)
                        {
                            msWriter.WriteString((string)defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteString(ValueGen.GenString());
                        }
                    }
                    break;
                case EntityType.SHORT:
                case EntityType.INT16:
                    if (isArray)
                    {
                        if (defaultvalue != null)
                        {
                            msWriter.WriteInt16Array((short[])defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteInt16Array(ValueGen.GenInt16s().ToArray());
                        }
                    }
                    else
                    {
                        if (defaultvalue != null && (short)defaultvalue != default(short))
                        {
                            msWriter.WriteInt16((short)defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteInt16(ValueGen.GenInt16());
                        }
                    }
                    break;
                case EntityType.USHORT:
                    if (isArray)
                    {
                        if (defaultvalue != null)
                        {
                            msWriter.WriteUInt16Array((ushort[])defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteUInt16Array(ValueGen.GenUInt16s().ToArray());
                        }
                    }
                    else
                    {
                        if (defaultvalue != null && (ushort)defaultvalue != default(ushort))
                        {
                            msWriter.WriteUInt16((ushort)defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteUInt16(ValueGen.GenUInt16());
                        }
                    }
                    break;
                case EntityType.INT32:
                    if (isArray)
                    {
                        if (defaultvalue != null)
                        {
                            msWriter.WriteInt32Array(ValueGen.GenInt32s().ToArray());
                        }
                        else
                        {
                            msWriter.WriteInt32Array((Int32[])defaultvalue);
                        }
                    }
                    else
                    {
                        if (defaultvalue != null && (int)defaultvalue != default(int))
                        {
                            msWriter.WriteInt32((int)defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteInt32(ValueGen.GenInt32());
                        }
                    }
                    break;
                case EntityType.CHAR:
                    if (isArray)
                    {
                        if (defaultvalue != null)
                        {
                            msWriter.WriteCharArray((char[])defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteCharArray(ValueGen.GenChars().ToArray());
                        }
                    }
                    else
                    {
                        if (defaultvalue != null && (char)defaultvalue != default(char))
                        {
                            msWriter.WriteChar((char)defaultvalue);
                        }
                        else
                        {
                            msWriter.WriteChar(ValueGen.GenChar());
                        }
                    }
                    break;
                case EntityType.DECIMAL:
                    if (isArray)
                    {
                        msWriter.WriteDeciamlArray(ValueGen.GenDecimals().ToArray());
                    }
                    else
                    {
                        msWriter.WriteDecimal(ValueGen.GenDecimal());
                    }
                    break;
                case EntityType.DOUBLE:
                    if (isArray)
                    {
                        msWriter.WriteDoubleArray(ValueGen.GenDoubles().ToArray());
                    }
                    else
                    {
                        msWriter.WriteDouble(ValueGen.GenDouble());
                    }
                    break;
                case EntityType.FLOAT:
                    if (isArray)
                    {
                        msWriter.WriteFloatArray(ValueGen.GenFloats().ToArray());
                    }
                    else
                    {
                        msWriter.WriteFloat(ValueGen.GenFloat());
                    }
                    break;
                case EntityType.INT64:
                    if (isArray)
                    {
                        msWriter.WriteInt64Array(ValueGen.GenLongs().ToArray());
                    }
                    else
                    {
                        msWriter.WriteInt64(ValueGen.GenLong());
                    }
                    break;
                case EntityType.DATETIME:
                    if (isArray)
                    {
                        msWriter.WriteDateTimeArray(ValueGen.GenDates().ToArray());
                    }
                    else
                    {
                        msWriter.WriteDateTime(ValueGen.GenDate());
                    }
                    break;
                case EntityType.BOOL:
                    if (isArray)
                    {
                        msWriter.WriteBoolArray(ValueGen.GenBools().ToArray());
                    }
                    else
                    {
                        msWriter.WriteBool(ValueGen.GenBool());
                    }
                    break;
                case EntityType.ENUM:
                    if (isArray)
                    {
                        msWriter.WriteStringArray(ValueGen.GenEnums(bufType.ValueType).ToArray());
                    }
                    else
                    {
                        msWriter.WriteString(ValueGen.GenEnum(bufType.ValueType));
                    }
                    break;
                case EntityType.DICTIONARY:
                    if (isArray)
                    {
                        var diclen = new Random().Next(1, 3);

                        msWriter.WriteInt32(diclen);
                        for (int i = 0; i < diclen; i++)
                        {
                            GenSerialize(bufType.ValueType, msWriter);
                        }
                    }
                    else
                    {
                        var diccount = 1;
                        //
                        //写入长度
                        msWriter.WriteInt32(diccount);
                        int i = 0;
                        while (i < diccount)
                        {
                            //object k=kv.Eval("Key");
                            //object v = kv.Eval("Value");

                            GenSerialize(bufType.GenerTypes[0], msWriter);
                            GenSerialize(bufType.GenerTypes[1], msWriter);
                            i++;
                        }
                    }
                    break;
                case EntityType.LIST:
                    if (isArray)
                    {
                        var listlen = new Random().Next(1, 3);
                        msWriter.WriteInt32(listlen);
                        for (int i = 0; i < listlen; i++)
                        {
                            GenSerialize(GetListValueType(bufType.ClassType), msWriter);
                        }
                    }
                    else
                    {
                        var listcount = new Random().Next(1, 3);
                        msWriter.WriteInt32(listcount);
                        for (int i = 0; i < listcount; i++)
                        {
                            GenSerialize(GetListValueType(bufType.ClassType), msWriter);
                        }
                    }
                    break;
                case EntityType.ARRAY:
                    if (isArray)
                    {
                        var arrlen = new Random().Next(1, 3);

                        msWriter.WriteInt32(arrlen);
                        for (int i = 0; i < arrlen; i++)
                        {
                            GenSerialize(bufType.ClassType, msWriter);
                        }
                    }
                    else
                    {
                        var arrlen = new Random().Next(1, 3);
                        msWriter.WriteInt32(arrlen);
                        for (int i = 0; i < arrlen; i++)
                        {
                            GenSerialize(bufType.ClassType, msWriter);
                        }
                    }
                    break;
                default:
                    throw new EntityBufException("序列化错误");
            }
        }

        public static byte[] GenSerialize(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (MemoryStreamWriter writer = new MemoryStreamWriter(ms))
                {
                    GenSerialize(type, writer);
                    var bytes = writer.GetBytes();

                    return bytes;
                }
            }
        }

        private static void GenSerialize(Type type, MemoryStreamWriter msWriter)
        {

            EntityBufTypeFlag flag = EntityBufTypeFlag.Empty;
            msWriter.WriteByte((byte)flag);

            Tuple<EntityBufType, bool> tuple = GetTypeBufType(type);
            object instance = null;

            if (tuple.Item1.EntityType != EntityType.COMPLEX)
            {

                if (tuple.Item1.Property != null && tuple.Item1.Property.PropertyInfo.DeclaringType != null)
                {
                    var cor = tuple.Item1.Property.PropertyInfo.DeclaringType.GetConstructor(new Type[0]);
                    if (cor != null)
                    {
                        instance = cor.Invoke(null);
                    }
                }
                GenSerializeSimple(instance, tuple.Item2, tuple.Item1, msWriter);
                return;
            }

            bool isArray;
            //PropertyInfo[] props = o.GetType().GetProperties();
            var entitybuftypelist = GetTypeEntityBufType(type);
            //foreach (var tp in entitybuftypelist)
            Tuple<EntityBufType, bool> tp = null;
            instance = null;

            {
                var cor = type.GetConstructor(new Type[0]);
                if (cor != null)
                {
                    instance = cor.Invoke(null);
                }
            }

            for (int i = 0; i < entitybuftypelist.Count; i++)
            {
                tp = entitybuftypelist[i];
                //EntityBufType buftype = MapBufType(prop.PropertyType, out isArray);
                isArray = tp.Item2;


                if (tp.Item1.EntityType == EntityType.COMPLEX)
                {
                    GenSerializeComplex(isArray, tp.Item1, msWriter);
                }
                else
                {
                    GenSerializeSimple(instance, isArray, tp.Item1, msWriter);
                }
            }
        }

        public static T GenDeSerialize<T>(byte[] bytes)
        {
            var obj = EntityBuf.EntityBufCore.DeSerialize(typeof(T), bytes,false);
            return (T)obj;
        }
    }
}
