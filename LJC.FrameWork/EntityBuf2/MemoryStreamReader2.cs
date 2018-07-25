using LJC.FrameWork.EntityBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.EntityBuf2
{
    public class MemoryStreamReader2 : IDisposable
    {
        private BinaryReader _reader;
        public MemoryStreamReader2(BinaryReader reader)
        {
            _reader = reader;
        }

        public bool ReadBool()
        {
            return _reader.ReadBoolean();
        }

        public bool[] ReadBoolArray()
        {
            ArrayTypeFlag flag = (ArrayTypeFlag)_reader.ReadByte();
            if (flag == ArrayTypeFlag.NULL)
            {
                return null;
            }
            else if (flag == ArrayTypeFlag.Empty)
            {
                return new bool[0];
            }
            //取长度
            uint
                len = _reader.ReadUInt32();
            bool[] ret = new bool[len];
            byte[] byts = _reader.ReadBytes((int)Math.Ceiling(len / 8.0));
            BitArray ba = new BitArray(byts);
            for (int i = 0; i < len; i++)
            {
                ret[i] = ba[i];
            }
            return ret;
        }

        public Int16 ReadInt16()
        {
            var flag = (ShortTypeEnum)_reader.ReadByte();
            if (flag == ShortTypeEnum.Zero)
            {
                return default(Int16);
            }
            Int16 ret = _reader.ReadInt16();

            if ((flag & ShortTypeEnum.Minus) == ShortTypeEnum.Minus)
            {
                return (Int16)(-ret);
            }

            return ret;
        }

        public Int16[] ReadInt16Array()
        {
            int len = ReadInt32();
            if (len == -1)
                return null;
            Int16[] ret = new Int16[len];
            for (int i = 0; i < len; i++)
            {
                ret[i] = ReadInt16();
            }
            return ret;
        }

        public UInt16[] ReadUInt16Array()
        {
            int len = ReadInt32();
            if (len == -1)
                return null;
            UInt16[] ret = new UInt16[len];
            for (int i = 0; i < len; i++)
            {
                ret[i] = ReadUInt16();
            }
            return ret;
        }

        public UInt16 ReadUInt16()
        {
            var flag = (UShrotTypeEnum)_reader.ReadByte();
            if (flag == UShrotTypeEnum.Zero)
            {
                return default(UInt16);
            }
            UInt16 ret = _reader.ReadUInt16();

            return ret;
        }

        public Int32 ReadInt32()
        {
            IntTypeFlag flag = (IntTypeFlag)_reader.ReadByte();
            if (flag == IntTypeFlag.Zero)
            {
                return default(Int32);
            }
            Int32 ret = _reader.ReadInt32();

            bool isMinus = (flag & IntTypeFlag.Minus) == IntTypeFlag.Minus;
            if (isMinus)
            {
                return -ret;
            }
            return ret;
        }

        public Int32[] ReadInt32Array()
        {
            ArrayTypeFlag flag = (ArrayTypeFlag)_reader.ReadByte();
            if (flag == ArrayTypeFlag.NULL)
            {
                return null;
            }
            //取长度
            int len = _reader.ReadInt32();
            Int32[] ret = new int[len];
            bool isCompress = (flag & ArrayTypeFlag.Compress) == ArrayTypeFlag.Compress;
            for (int i = 0; i < len; i++)
            {
                if (isCompress)
                    ret[i] = ReadInt32();
                else
                    ret[i] = _reader.ReadInt32();
            }
            return ret;
        }

        public Int64 ReadInt64()
        {
            LongTypeEnum flag = (LongTypeEnum)_reader.ReadByte();
            if (flag == LongTypeEnum.Zero)
            {
                return default(Int64);
            }

            Int64 ret = _reader.ReadInt64();
            if ((flag & LongTypeEnum.Minus) == LongTypeEnum.Minus)
            {
                return -ret;
            }
            return ret;
        }

        public Int64[] ReadInt64Array()
        {
            ArrayTypeFlag flag = (ArrayTypeFlag)_reader.ReadByte();
            if (flag == ArrayTypeFlag.NULL)
            {
                return null;
            }
            else if (flag == ArrayTypeFlag.Empty)
            {
                return new Int64[0];
            }
            //取长度
            uint len = _reader.ReadUInt32();

            Int64[] ret = new Int64[len];
            for (int i = 0; i < len; i++)
            {
                ret[i] = ReadInt64();
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
            var flag = (StringTypeFlag)_reader.ReadByte();

            if (flag == StringTypeFlag.NULL)
                return null;
            if (flag == StringTypeFlag.Empty)
                return string.Empty;

            Int32 readlen = _reader.ReadInt32();

            Encoding encode = Encoding.UTF8;

            if ((flag & StringTypeFlag.AssciiEncoding) == StringTypeFlag.AssciiEncoding)
            {
                encode = Encoding.ASCII;
            }

            byte[] byts = _reader.ReadBytes(readlen);
            return encode.GetString(byts);
        }

        public string[] ReadStringArray()
        {
            StringTypeFlag flag = (StringTypeFlag)_reader.ReadByte();
            if (flag == StringTypeFlag.NULL)
            {
                return null;
            }
            int len = _reader.ReadInt32();

            string[] ret = new string[len];

            for (int i = 0; i < len; i++)
            {
                ret[i] = ReadString();
            }
            return ret;
        }

        public DateTime ReadDateTime()
        {
            double db = _reader.ReadDouble();
            return DateTime.FromOADate(db);
        }

        public DateTime[] ReadDateTimeArray()
        {
            ArrayTypeFlag flag = (ArrayTypeFlag)_reader.ReadByte();
            if (flag == ArrayTypeFlag.NULL)
            {
                return null;
            }
            else if (flag == ArrayTypeFlag.Empty)
            {
                return new DateTime[0];
            }

            uint len = _reader.ReadUInt32();

            DateTime[] ret = new DateTime[len];
            for (int i = 0; i < len; i++)
            {
                double db = _reader.ReadDouble();
                ret[i] = DateTime.FromOADate(db);
            }
            return ret;
        }

        public int PeekChar()
        {
            return _reader.PeekChar();
        }

        public void Skip(long offset)
        {
            _reader.BaseStream.Position += offset;
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public byte[] ReadByteArray()
        {
            int len = ReadInt32();
            if (len == -1)
                return null;
            return this.ReadBytes(len);
        }

        public byte[] ReadBytes(int len)
        {
            return _reader.ReadBytes(len);
        }

        public decimal ReadDecimal()
        {
            DecimalTypeFlag flag = (DecimalTypeFlag)_reader.ReadByte();
            if (flag == DecimalTypeFlag.Zero)
            {
                return default(decimal);
            }
            bool isMinus = (flag & DecimalTypeFlag.Minus) == DecimalTypeFlag.Minus;

            decimal ret = (decimal)_reader.ReadDouble();
            if (isMinus)
            {
                ret = -ret;
            }
            return ret;
        }

        public decimal[] ReadDeciamlArray()
        {
            int len = this.ReadInt32();
            if (len == -1)
                return null;
            decimal[] arr = new decimal[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = this.ReadDecimal();
            }
            return arr;
        }

        public double[] ReadDoubleArray()
        {
            int len = this.ReadInt32();
            if (len == -1)
                return null;
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

        public float ReadFloat()
        {
            return _reader.ReadSingle();
        }

        public float[] ReadFloatArray()
        {
            int len = this.ReadInt32();
            if (len == -1)
                return null;
            float[] arr = new float[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadFloat();
            }
            return arr;
        }

        public char ReadChar()
        {
            return _reader.ReadChar();
        }

        public char[] ReadCharArray()
        {
            int len = this.ReadInt32();
            if (len == -1)
                return null;
            char[] arr = new char[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadChar();
            }
            return arr;
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
