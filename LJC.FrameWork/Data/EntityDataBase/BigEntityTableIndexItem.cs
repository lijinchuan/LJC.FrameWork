using LJC.FrameWork.EntityBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.EntityDataBase
{
    public class BigEntityTableIndexItem : IEntityBufObject, IComparable<BigEntityTableIndexItem>
    {
        public object[] Key
        {
            get;
            set;
        }

        public IndexInfo Index
        {
            get;
            set;
        }

        public long Offset
        {
            get;
            set;
        }

        public int len
        {
            get;
            set;
        }

        public bool Del
        {
            get;
            set;
        }

        public long KeyOffset
        {
            get;
            set;
        }

        private BigEntityTableIndexItem DeSerializeSimple(BinaryReader br,BigEntityTableIndexItem ret)
        {
            object[] objs = new object[this.Index.Indexs.Length];

            for(int i = 0; i < this.Index.Indexs.Length; i++)
            {
                var fieldtype = this.Index.Indexs[i].FieldType;
                switch (fieldtype)
                {
                    case EntityType.STRING:
                        {
                            var len = br.ReadInt16() & 65535;
                            objs[i] = Encoding.UTF8.GetString(br.ReadBytes(len));
                            break;
                        }
                    case EntityType.INT16:
                    case EntityType.SHORT:
                        {
                            objs[i] = br.ReadInt16();
                            break;
                        }
                    case EntityType.CHAR:
                        {
                            objs[i] = br.ReadChar();
                            break;
                        }
                    case EntityType.BOOL:
                        {
                            objs[i] = br.ReadBoolean();
                            break;
                        }
                    case EntityType.BYTE:
                        {
                            objs[i] = br.ReadByte();
                            break;
                        }
                    case EntityType.DATETIME:
                        {
                            objs[i] = DateTime.FromOADate(br.ReadDouble());
                            break;
                        }
                    case EntityType.DOUBLE:
                        {
                            objs[i] = br.ReadDouble();
                            break;
                        }
                    case EntityType.FLOAT:
                        {
                            objs[i] = br.ReadSingle();
                            break;
                        }
                    case EntityType.INT64:
                        {
                            objs[i] = br.ReadInt64();
                            break;
                        }
                    default:
                        {
                            throw new NotSupportedException(this.Index.Indexs[i].ToString());
                        }
                }
            }

            ret.Key = objs;

            return ret;
        }

        public IEntityBufObject DeSerialize(byte[] bytes)
        {
            var ret = new BigEntityTableIndexItem();

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
            {
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
                {
                    ret.KeyOffset = br.ReadInt64();
                    ret.len = br.ReadInt32();
                    ret.Offset = br.ReadInt64();
                    ret.Del = br.ReadBoolean();
                    DeSerializeSimple(br, ret);
                }
            }

            return ret;
        }

        private void SerializeKeySimple(System.IO.MemoryStream ms)
        {
            for(int i = 0; i < this.Index.Indexs.Length; i++)
            {
                object o = this.Key[i];
                switch (this.Index.Indexs[i].FieldType)
                {
                    case EntityType.STRING:
                        {
                            var keybytes = Encoding.UTF8.GetBytes(o.ToString());
                            if (keybytes.Length > 65000)
                            {
                                throw new Exception("字符串长度不能超过65000");
                            }
                            ms.Write(BitConverter.GetBytes((Int16)keybytes.Length),0,2);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.INT16:
                    case EntityType.SHORT:
                        {
                            var keybytes = BitConverter.GetBytes((Int16)o);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.CHAR:
                        {
                            var keybytes = BitConverter.GetBytes((char)o);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.BOOL:
                        {
                            var keybytes = BitConverter.GetBytes((bool)o);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.BYTE:
                        {
                            var keybytes = BitConverter.GetBytes((byte)o);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.DATETIME:
                        {
                            var keybytes = BitConverter.GetBytes(((DateTime)o).ToOADate());
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.DOUBLE:
                        {
                            var keybytes = BitConverter.GetBytes((double)o);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.FLOAT:
                        {
                            var keybytes = BitConverter.GetBytes((float)o);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.INT64:
                        {
                            var keybytes = BitConverter.GetBytes((long)o);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    default:
                        {
                            throw new NotSupportedException(this.Index.Indexs[i].FieldType.ToString());
                        }
                }
            }
        }

        public byte[] Serialize()
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(this.KeyOffset), 0, 8);
                ms.Write(BitConverter.GetBytes(this.len), 0, 4);
                ms.Write(BitConverter.GetBytes(this.Offset), 0, 8);
                ms.Write(BitConverter.GetBytes(this.Del), 0, 1);

                SerializeKeySimple(ms);

                return ms.ToArray();
            }
        }

        public int CompareTo(BigEntityTableIndexItem other)
        {
            int compare = -1;
            if (this.Index.IndexName != other.Index.IndexName)
            {
                throw new Exception("无法比较，索引名称不相同");
            }
            for (int j = 0; j < this.Index.Indexs.Length; j++)
            {
                switch (this.Index.Indexs[j].FieldType)
                {
                    case EntityType.STRING:
                        {
                            if (this.Key[j] != null && other.Key[j] != null)
                            {
                                var b = other.Key[j].ToString();
                                var lenb = b.Length;
                                int i = 0, diff = 0;

                                foreach (var a in this.Key[j].ToString())
                                {
                                    if (i > lenb - 1)
                                    {
                                        return 1;
                                    }
                                    diff = a - b[i];
                                    if (diff > 0)
                                    {
                                        return 1;
                                    }
                                    else if (diff < 0)
                                    {
                                        return -1;
                                    }
                                    i++;
                                }
                                compare = i == lenb ? 0 : -1;
                            }
                            else if (this.Key[j] == null)
                            {
                                compare = -1;
                            }
                            else
                            {
                                compare = 1;
                            }
                            break;
                        }
                    case EntityType.INT16:
                    case EntityType.SHORT:
                        {
                            compare = ((Int16)Key[j]).CompareTo((Int16)other.Key[j]);
                            break;
                        }
                    case EntityType.CHAR:
                        {
                            compare = ((char)Key[j]).CompareTo((char)other.Key[j]);
                            break;
                        }
                    case EntityType.BOOL:
                        {
                            compare = ((bool)Key[j]).CompareTo((bool)other.Key[j]);
                            break;
                        }
                    case EntityType.BYTE:
                        {
                            compare = ((byte)Key[j]).CompareTo((byte)other.Key[j]);
                            break;
                        }
                    case EntityType.DATETIME:
                        {
                            compare = ((DateTime)Key[j]).CompareTo((DateTime)other.Key[j]);
                            break;
                        }
                    case EntityType.DOUBLE:
                        {
                            compare = ((double)Key[j]).CompareTo((double)other.Key[j]);
                            break;
                        }
                    case EntityType.FLOAT:
                        {
                            compare = ((float)Key[j]).CompareTo((float)other.Key[j]);
                            break;
                        }
                    case EntityType.INT64:
                        {
                            compare = ((long)Key[j]).CompareTo((long)other.Key[j]);
                            break;
                        }
                    default:
                        {
                            throw new NotSupportedException(this.Index.Indexs[j].FieldType.ToString());
                        }
                }
                if (compare != 0)
                {
                    if (this.Index.Indexs[j].Direction == -1)
                    {
                        compare = -compare;
                    }
                    break;
                }
            }

            if (compare == 0 && other.Offset > 0 && Offset > 0)
            {
                return Offset.CompareTo(other.Offset);
            }

            return compare;
        }
    }
}
