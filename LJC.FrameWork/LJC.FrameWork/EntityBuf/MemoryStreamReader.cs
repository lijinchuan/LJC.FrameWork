using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace LJC.FrameWork.EntityBuf
{
    public class MemoryStreamReader:IDisposable
    {
        private BinaryReader _reader;
        public MemoryStreamReader(BinaryReader reader)
        {
            _reader = reader;
        }

        public bool ReadBool()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.BOOL)
                throw new Exception("不是bool类型");
            _reader.ReadByte();
            return _reader.ReadBoolean();
        }

        public bool[] ReadBoolArray()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.BOOL)
                throw new Exception("不是bool类型");
            EntityBufTypeFlag buftypeflag = (EntityBufTypeFlag)_reader.ReadByte();
            if ((buftypeflag & EntityBufTypeFlag.ArrayFlag) != EntityBufTypeFlag.ArrayFlag)
                throw new Exception("不是数组");
            //取长度
            int len = _reader.ReadInt32();
            bool[] ret = new bool[len];
            byte[] byts = _reader.ReadBytes((int)Math.Ceiling(len / 8.0));
            BitArray ba = new BitArray(byts);
            for (int i = 0; i < len; i++)
            {
                ret[i] = ba[i];
            }
            return ret;
        }

        public Int32 ReadInt32()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.INT32)
                throw new Exception("不是32位类型");
            _reader.ReadByte();
            return _reader.ReadInt32();
        }

        public Int32[] ReadInt32Array()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.INT32)
                throw new Exception("不是32位类型");
            EntityBufTypeFlag buftypeflag = (EntityBufTypeFlag)_reader.ReadByte();
            if ((buftypeflag & EntityBufTypeFlag.ArrayFlag) != EntityBufTypeFlag.ArrayFlag)
                throw new Exception("不是数组");
            //取长度
            int len = _reader.ReadInt32();
            Int32[] ret = new int[len];
            for (int i = 0; i < len; i++)
            {
                ret[i] = _reader.ReadInt32();
            }
            return ret;
        }

        public Int64 ReadInt64()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.INT64)
                throw new Exception("不是64位类型");
            _reader.ReadByte();
            return _reader.ReadInt64();
        }

        public Int64[] ReadInt64Array()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.INT64)
                throw new Exception("不是64位类型");
            EntityBufTypeFlag buftypeflag = (EntityBufTypeFlag)_reader.ReadByte();
            if ((buftypeflag & EntityBufTypeFlag.ArrayFlag) != EntityBufTypeFlag.ArrayFlag)
                throw new Exception("不是数组");
            //取长度
            int len = _reader.ReadInt32();
            Int64[] ret = new Int64[len];
            for (int i = 0; i < len; i++)
            {
                ret[i] = _reader.ReadInt64();
            }
            return ret;
        }

        public string ReadAsii()
        {
            int len = _reader.ReadInt32();
            if (len == -1)
                return null;
            if (len == 0)
                return string.Empty;
            byte[] byts = _reader.ReadBytes(len);
            return Encoding.ASCII.GetString(byts);
        }

        public string ReadString()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.STRING)
                throw new Exception("不是string类型");
            _reader.ReadByte();
            int len = _reader.ReadInt32();
            if (len == -1)
                return null;
            if (len == 0)
                return string.Empty;
            byte[] byts = _reader.ReadBytes(len);
            return Encoding.UTF8.GetString(byts);
        }

        public string[] ReadStringArray()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.STRING)
                throw new Exception("不是string类型");
            _reader.ReadByte();
            int len = _reader.ReadInt32();
            if (len == -1)
            {
                return null;
            }

            string[] ret = new string[len];

            for (int i = 0; i < len; i++)
            {
                int strLen = _reader.ReadInt32();
                if (strLen == -1)
                {
                    ret[i] = null;
                }
                else if (strLen == 0)
                {
                    ret[i] = string.Empty;
                }
                else if (strLen > 0)
                {
                    byte[] strByts = _reader.ReadBytes(strLen);
                    ret[i] = Encoding.UTF8.GetString(strByts);
                }
            }
            return ret;
        }

        public DateTime ReadDateTime()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.DATETIME)
                throw new Exception("不是datetime类型");
            _reader.ReadByte();
            double db = _reader.ReadDouble();
            return DateTime.FromOADate(db);
        }

        public DateTime[] ReadDateTimeArray()
        {
            EntityType buftype = (EntityType)_reader.ReadByte();
            if (buftype != EntityType.DATETIME)
                throw new Exception("不是datetime类型");
            _reader.ReadByte();
            int len = _reader.ReadInt32();
            DateTime[] ret = new DateTime[len];
            for (int i = 0; i < len; i++)
            {
                double db = _reader.ReadDouble();
                ret[i] = DateTime.FromOADate(db);
            }
            return ret;
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public byte[] ReadByteArray()
        {
            int len = ReadInt32();
            return this.ReadBytes(len);
        }

        public byte[] ReadBytes(int len)
        {
            return _reader.ReadBytes(len);
        }

        public decimal ReadDecimal()
        {
            return (decimal)_reader.ReadDouble();
        }

        public decimal[] ReadDeciamlArray()
        {
            int len=this.ReadInt32();
            decimal[] arr = new decimal[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = (decimal)_reader.ReadDouble();
            }
            return arr;
        }

        public double[] ReadDoubleArray()
        {
            int len = this.ReadInt32();
            double[] arr = new double[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = _reader.ReadDouble();
            }
            return arr;
        }

        public double ReadDouble()
        {
            return _reader.ReadDouble();
        }

        void IDisposable.Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
            }
        }
    }
}
