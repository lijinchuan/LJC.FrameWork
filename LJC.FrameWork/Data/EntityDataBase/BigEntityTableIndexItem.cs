using LJC.FrameWork.EntityBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.EntityDataBase
{
    public class BigEntityTableIndexItem : IEntityBufObject, IComparable<BigEntityTableIndexItem>
    {
        public object Key
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

        public EntityType KeyType
        {
            get;
            set;
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
                    ret.KeyType = (EntityType)br.ReadByte();
                    var skip = 22;
                    switch (ret.KeyType)
                    {
                        case EntityType.STRING:
                            {
                                ret.Key = Encoding.UTF8.GetString(br.ReadBytes(bytes.Length - skip));
                                break;
                            }
                        case EntityType.INT16:
                        case EntityType.SHORT:
                            {
                                ret.Key = br.ReadInt16();
                                break;
                            }
                        case EntityType.CHAR:
                            {
                                ret.Key = br.ReadChar();
                                break;
                            }
                        case EntityType.BOOL:
                            {
                                ret.Key = br.ReadBoolean();
                                break;
                            }
                        case EntityType.BYTE:
                            {
                                ret.Key = br.ReadByte();
                                break;
                            }
                        case EntityType.DATETIME:
                            {
                                ret.Key = DateTime.FromOADate(br.ReadDouble());
                                break;
                            }
                        case EntityType.DOUBLE:
                            {
                                ret.Key = br.ReadDouble();
                                break;
                            }
                        case EntityType.FLOAT:
                            {
                                ret.Key = br.ReadSingle();
                                break;
                            }
                        case EntityType.INT64:
                            {
                                ret.Key = br.ReadInt64();
                                break;
                            }
                        default:
                            {
                                throw new NotSupportedException(KeyType.ToString());
                            }
                    }
                }
            }

            return ret;
        }

        public byte[] Serialize()
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(this.KeyOffset), 0, 8);
                ms.Write(BitConverter.GetBytes(this.len), 0, 4);
                ms.Write(BitConverter.GetBytes(this.Offset), 0, 8);
                ms.Write(BitConverter.GetBytes(this.Del), 0, 1);

                ms.WriteByte((byte)this.KeyType);
                switch (this.KeyType)
                {
                    case EntityType.STRING:
                        {
                            var keybytes = Encoding.UTF8.GetBytes(this.Key.ToString());
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.INT16:
                    case EntityType.SHORT:
                        {
                            var keybytes = BitConverter.GetBytes((Int16)Key);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.CHAR:
                        {
                            var keybytes = BitConverter.GetBytes((char)Key);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.BOOL:
                        {
                            var keybytes = BitConverter.GetBytes((bool)Key);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.BYTE:
                        {
                            var keybytes = BitConverter.GetBytes((byte)Key);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.DATETIME:
                        {
                            var keybytes = BitConverter.GetBytes(((DateTime)Key).ToOADate());
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.DOUBLE:
                        {
                            var keybytes = BitConverter.GetBytes((double)Key);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.FLOAT:
                        {
                            var keybytes = BitConverter.GetBytes((float)Key);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    case EntityType.INT64:
                        {
                            var keybytes = BitConverter.GetBytes((long)Key);
                            ms.Write(keybytes, 0, keybytes.Length);
                            break;
                        }
                    default:
                        {
                            throw new NotSupportedException(KeyType.ToString());
                        }
                }


                return ms.ToArray();
            }
        }

        public int CompareTo(BigEntityTableIndexItem other)
        {
            int compare = -1;
            switch (this.KeyType)
            {
                case EntityType.STRING:
                    {
                        if (this.Key != null && other.Key != null)
                        {
                            var b = other.Key.ToString();
                            var lenb = b.Length;
                            int i = 0, diff = 0;

                            foreach (var a in this.Key.ToString())
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
                        else if (this.Key == null)
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
                        compare = ((Int16)Key).CompareTo((Int16)other.Key);
                        break;
                    }
                case EntityType.CHAR:
                    {
                        compare = ((char)Key).CompareTo((char)other.Key);
                        break;
                    }
                case EntityType.BOOL:
                    {
                        compare = ((bool)Key).CompareTo((bool)other.Key);
                        break;
                    }
                case EntityType.BYTE:
                    {
                        compare = ((byte)Key).CompareTo((byte)other.Key);
                        break;
                    }
                case EntityType.DATETIME:
                    {
                        compare = ((DateTime)Key).CompareTo((DateTime)other.Key);
                        break;
                    }
                case EntityType.DOUBLE:
                    {
                        compare = ((double)Key).CompareTo((double)other.Key);
                        break;
                    }
                case EntityType.FLOAT:
                    {
                        compare = ((float)Key).CompareTo((float)other.Key);
                        break;
                    }
                case EntityType.INT64:
                    {
                        compare = ((long)Key).CompareTo((long)other.Key);
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException(KeyType.ToString());
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
