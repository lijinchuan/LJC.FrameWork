using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.EntityBuf
{
    public class MemoryStreamWriter : IDisposable
    {
        private MemoryStream _ms;
        private static byte[] bytesNull = BitConverter.GetBytes(-1);
        private static byte[] bytesZero = BitConverter.GetBytes(0);
        private static float maxFloat = 9999999f;

        public MemoryStreamWriter(MemoryStream ms)
        {
            _ms = ms;
        }

        public byte[] GetBytes()
        {
            //byte[] bytes = new byte[_ms.Length];
            //_ms.Seek(0, SeekOrigin.Begin);
            //_ms.Read(bytes, 0, (int)_ms.Length);
            //return bytes;
            return _ms.ToArray();
        }

        public void WriteBool(bool boo)
        {
            byte[] bts = BitConverter.GetBytes(boo);
            _ms.Write(bts, 0, bts.Length);
        }

        public void WriteBoolArray(bool[] booArray)
        {
            if (booArray == null)
            {
                _ms.WriteByte((byte)ArrayTypeFlag.NULL);
                return;
            }
            else if (booArray.Length == 0)
            {
                _ms.WriteByte((byte)ArrayTypeFlag.Empty);
                return;
            }

            ArrayTypeFlag flag = ArrayTypeFlag.DEFAULT;
            int len = booArray.Length;
            var bytelen = CompressInt32(len);
            if (bytelen.Length == 1)
            {
                flag = ArrayTypeFlag.ByteLen;
            }
            else if (bytelen.Length == 2)
            {
                flag = ArrayTypeFlag.ShortLen;
            }

            _ms.WriteByte((byte)flag);
            _ms.Write(bytelen, 0, bytelen.Length);
            BitArray ba = new BitArray(booArray);

            foreach (byte b in BitHelper.ConvertToByteArray(ba))
            {
                _ms.WriteByte(b);
            }
        }

        public void WriteInt16(Int16 num)
        {
            if (num == 0)
            {
                _ms.WriteByte((byte)ShortTypeEnum.Zero);
                return;
            }

            ShortTypeEnum flag = ShortTypeEnum.DEFAULT;

            if (num < 0)
            {
                flag |= ShortTypeEnum.Minus;
                num = Math.Abs(num);
            }

            byte[] bytes;
            if (num <= byte.MaxValue)
            {
                flag |= ShortTypeEnum.ByteVal;
                bytes = new byte[] { (byte)num };
            }
            else
            {
                bytes = BitConverter.GetBytes(num);
            }

            _ms.WriteByte((byte)flag);
            _ms.Write(bytes, 0, bytes.Length);
        }

        public void WriteInt16Array(Int16[] numArray)
        {
            if (numArray == null)
            {
                _ms.Write(bytesNull, 0, 4);
                return;
            }
            int len = numArray.Length;
            WriteInt32(len);
            foreach (Int16 i in numArray)
            {
                WriteInt16(i);
            }
        }

        public void WriteInt32(Int32 num)
        {
            if (num == 0)
            {
                _ms.WriteByte((byte)IntTypeFlag.Zero);
                return;
            }
            IntTypeFlag flag = IntTypeFlag.DEFAULT;
            if (num < 0)
            {
                flag = IntTypeFlag.Minus;
                num = Math.Abs(num);
            }
            byte[] byts = null;

            if (num <= byte.MaxValue)
            {
                byts = new byte[] { (byte)num };
                flag |= IntTypeFlag.BYTE;
            }
            else if (num <= ushort.MaxValue)
            {
                byts = BitConverter.GetBytes((ushort)num);
                flag |= IntTypeFlag.SHORT;
            }

            if (byts == null)
            {
                byts = BitConverter.GetBytes(num);
                flag |= IntTypeFlag.INT;
            }

            _ms.WriteByte((byte)flag);
            _ms.Write(byts, 0, byts.Length);
        }

        public void WriteInt32Array(Int32[] numArray)
        {
            if (numArray == null)
            {
                _ms.WriteByte((byte)ArrayTypeFlag.NULL);
                return;
            }
            bool isCompress = numArray.Where(p => p <= ushort.MaxValue).Count() > (numArray.Length * 2 / 3);
            ArrayTypeFlag flag = isCompress ? ArrayTypeFlag.Compress : ArrayTypeFlag.DEFAULT;
            byte[] bytelen= CompressInt32(numArray.Length);
            if (bytelen.Length == 1)
            {
                _ms.WriteByte((byte)(ArrayTypeFlag.ByteLen|flag));
            }
            else if (bytelen.Length == 2)
            {
                _ms.WriteByte((byte)(ArrayTypeFlag.ShortLen|flag));
            }
            else
            {
                _ms.WriteByte((byte)(ArrayTypeFlag.IntLen|flag));
            }
            _ms.Write(bytelen,0,bytelen.Length);
            foreach (int num in numArray)
            {
                if (!isCompress)
                {
                    _ms.Write(BitConverter.GetBytes(num), 0, 4);
                }
                else
                {
                    WriteInt32(num);
                }
            }
        }

        public void WriteInt64(Int64 num)
        {
            if (num == default(Int64))
            {
                _ms.WriteByte((byte)LongTypeEnum.Zero);
                return;
            }
            LongTypeEnum flag = LongTypeEnum.DEFAULT;
            if (num < 0)
            {
                flag = LongTypeEnum.Minus;
                num = Math.Abs(num);
            }

            byte[] byts = null;
            if (num <= byte.MaxValue)
            {
                flag |= LongTypeEnum.ByteVal;
                byts = new byte[] { (byte)num };
            }
            else if (num < ushort.MaxValue)
            {
                flag |= LongTypeEnum.ShortVal;
                byts = BitConverter.GetBytes((ushort)num);
            }
            else if (num < UInt32.MaxValue)
            {
                flag |= LongTypeEnum.IntVal;
                byts = BitConverter.GetBytes((UInt32)num);
            }
            else
            {
                byts = BitConverter.GetBytes(num);
            }
            _ms.WriteByte((byte)flag);
            _ms.Write(byts, 0, byts.Length);
        }

        public void WriteInt64Array(Int64[] intArray)
        {
            if (intArray == null)
            {
                _ms.WriteByte((byte)ArrayTypeFlag.NULL);
                return;
            }
            else if (intArray.Length == 0)
            {
                _ms.WriteByte((byte)ArrayTypeFlag.Empty);
                return;
            }
            var flag=ArrayTypeFlag.DEFAULT;
            int len = intArray.Length;
            byte[] byts= CompressInt32(len);
            if(byts.Length==1)
            {
                flag|=ArrayTypeFlag.ByteLen;
            }
            else if (byts.Length == 2)
            {
                flag |= ArrayTypeFlag.ShortLen;
            }

            _ms.WriteByte((byte)flag);
            _ms.Write(byts, 0, byts.Length);
            foreach (Int64 num in intArray)
            {
                WriteInt64(num);
            }
        }

        public void WriteAsii(string str)
        {
            if (str != null)
            {
                byte[] byts = Encoding.ASCII.GetBytes(str);
                _ms.Write(BitConverter.GetBytes(byts.Length), 0, 4);
                _ms.Write(byts, 0, byts.Length);
            }
            else
            {
                _ms.Write(bytesNull, 0, 4);
            }
        }


        public void WriteString(string str)
        {
            if (str == null)
            {
                _ms.WriteByte((byte)StringTypeFlag.NULL);
                return;
            }
            else if (string.IsNullOrWhiteSpace(str))
            {
                _ms.WriteByte((byte)StringTypeFlag.Empty);
                return;
            }

            byte[] byts;
            StringTypeFlag flag = StringTypeFlag.DEFAULT;
            //bool isAsscii = StringHelper.IsAscii(str);
            bool isAsscii = false;
            if (isAsscii)
            {
                flag = StringTypeFlag.AssciiEncoding;
                byts = Encoding.ASCII.GetBytes(str);
            }
            else
            {
                flag = StringTypeFlag.UTF8Encoding;
                byts = Encoding.UTF8.GetBytes(str);
            }

            byte[] lenbytes=null;
            if (byts.Length <= byte.MaxValue)
            {
                flag |= StringTypeFlag.ByteLen;
                lenbytes = new byte[] { (byte)byts.Length };
            }
            else if (byts.Length <= Int16.MaxValue)
            {
                flag |= StringTypeFlag.ShortLen;
                lenbytes = BitConverter.GetBytes((Int16)byts.Length);
            }
            else if (byts.LongLength<=Int32.MaxValue)
            {
                flag |= StringTypeFlag.IntLen;
                lenbytes = BitConverter.GetBytes(byts.Length);
            }
            else if (byts.LongLength < Int64.MaxValue)
            {
                flag |= StringTypeFlag.LongLen;
                lenbytes = BitConverter.GetBytes(byts.LongLength);
            }
            else
            {
                throw new OverflowException("字符串太长。");
            }

            _ms.WriteByte((byte)flag);
            _ms.Write(lenbytes, 0, lenbytes.Length);
            
            _ms.Write(byts, 0, byts.Length);

            
        }

        private byte[] CompressInt32(Int32 num)
        {
            byte[] byts = null;
            if (num >= 0)
            {
                if (num <= byte.MaxValue)
                {
                    byts = new byte[] { (byte)num };
                }
                else if (num <= UInt16.MaxValue)
                {
                    byts = BitConverter.GetBytes((UInt16)num);
                }
            }
            if (byts == null)
                byts = BitConverter.GetBytes(num);
            return byts;
        }

        public void WriteStringArray(string[] strArray)
        {
            if (strArray == null)
            {
                _ms.WriteByte((byte)StringTypeFlag.NULL);
                return;
            }

            byte[] lenbytes = CompressInt32(strArray.Length);
            if (lenbytes.Length == 1)
            {
                _ms.WriteByte((byte)StringTypeFlag.ByteLen);
            }
            else if (lenbytes.Length == 2)
            {
                _ms.WriteByte((byte)StringTypeFlag.ShortLen);
            }
            else
            {
                _ms.WriteByte((byte)StringTypeFlag.IntLen);
            }

            _ms.Write(lenbytes, 0, lenbytes.Length);
            foreach (string s in strArray)
            {
                WriteString(s);
            }
        }

        public void WriteDateTime(DateTime dateTime)
        {
            byte[] byts = BitConverter.GetBytes(dateTime.ToOADate());
            _ms.Write(byts, 0, byts.Length);
        }

        public void WriteDateTimeArray(DateTime[] dateTimes)
        {
            if (dateTimes == null)
            {
                _ms.WriteByte((byte)ArrayTypeFlag.NULL);
                return;
            }
            else if (dateTimes.Length == 0)
            {
                _ms.WriteByte((byte)ArrayTypeFlag.Empty);
                return;
            }

            var flag = ArrayTypeFlag.DEFAULT;
            int len = dateTimes.Length;
            var bytslen = CompressInt32(len);
            if (bytslen.Length == 1)
            {
                flag = ArrayTypeFlag.ByteLen;
            }
            else if (bytslen.Length == 2)
            {
                flag = ArrayTypeFlag.ShortLen;
            }

            _ms.WriteByte((byte)flag);
            _ms.Write(bytslen, 0, bytslen.Length);
            foreach (DateTime dt in dateTimes)
            {
                var byts = BitConverter.GetBytes(dt.ToOADate());
                _ms.Write(byts, 0, byts.Length);
            }
        }

        public void WriteByteArray(byte[] data)
        {
            if (data == null)
            {
                this.WriteInt32(-1);
            }
            else
            {
                this.WriteInt32(data.Length);
                this.WriteBytes(data);
            }
        }

        public void WriteByte(byte data)
        {
            _ms.WriteByte(data);
        }

        public void WriteBytes(byte[] data)
        {
            _ms.Write(data, 0, data.Length);
        }

        public void WriteDecimal(decimal data)
        {
            if (data == 0m)
            {
                _ms.WriteByte((byte)DecimalTypeFlag.Zero);
                return;
            }
            DecimalTypeFlag flag = DecimalTypeFlag.DEFAULT;
            if (data < 0)
            {
                flag = DecimalTypeFlag.Minus;
                data = Math.Abs(data);
            }
            byte[] byts = null;
            decimal mod = data % 1;
            if (0==mod)
            {
                if (data <= byte.MaxValue)
                {
                    flag |= DecimalTypeFlag.ByteVal;
                    byts = new byte[] { (byte)data };
                }
                else if (data <= ushort.MaxValue)
                {
                    flag |= DecimalTypeFlag.ShortVal;
                    byts = BitConverter.GetBytes((ushort)data);
                }
                else if (data <= UInt32.MaxValue)
                {
                    flag |= DecimalTypeFlag.IntVal;
                    byts = BitConverter.GetBytes((UInt32)data);
                }
                else if (data <= UInt64.MaxValue)
                {
                    flag |= DecimalTypeFlag.Int64Val;
                    byts = BitConverter.GetBytes((UInt64)data);
                }
            }
            else if ((mod * 10 % 1 == 0&&data<=9999999)
                || (mod * 100 % 1 == 0&&data<=999999)
                || (mod * 1000 % 1 == 0&&data<=99999)
                || (mod * 10000 % 1 == 0 && data <= 9999)
                || (mod * 100000 % 1 == 0 && data <= 999)
                || (mod * 1000000 % 1 == 0 && data <= 99)
                || (mod * 10000000 % 1 == 0 && data <= 9))
            {
                flag |= DecimalTypeFlag.FloatVal;
                byts = BitConverter.GetBytes((float)data);
            }
            else
            {
                flag |= DecimalTypeFlag.DoubleVal;
                byts = BitConverter.GetBytes((double)data);
            }

            _ms.WriteByte((byte)flag);
            _ms.Write(byts, 0, byts.Length);
        }

        public void WriteDeciamlArray(decimal[] data)
        {
            if (data == null)
            {
                _ms.Write(bytesNull, 0, 4);
                return;
            }
            _ms.Write(BitConverter.GetBytes(data.Length), 0, 4);
            foreach (decimal d in data)
            {
                WriteDouble((double)d);
            }
        }

        public void WriteDoubleArray(double[] data)
        {
            if (data == null||data.Length==0)
            {
                WriteInt32(-1);
                //WriteInt32(bytesNull, 0, 4);
                return;
            }
            WriteInt32(data.Length);
            foreach (double d in data)
            {
                WriteDouble(d);
            }
        }

        public void WriteDouble(double data)
        {
            var bytes= BitConverter.GetBytes(data);
            _ms.Write(bytes, 0, bytes.Length);

            //DoubleTypeFlag flag = DoubleTypeFlag.DEFAULT;
            //if (data < 0.0)
            //{
            //    flag = DoubleTypeFlag.Minus;
            //    data = Math.Abs(data);
            //}
            //byte[] buffer = null;
            //int mod = (int)(data % 1);
            //if (mod == 0)
            //{
            //    if (data <= byte.MaxValue)
            //    {
            //        flag |= DoubleTypeFlag.ByteVal;
            //        buffer = new byte[] { (byte)flag };
            //    }
            //    else if (data <= ushort.MaxValue)
            //    {
            //        flag |= DoubleTypeFlag.ShortVal;
            //        buffer = BitConverter.GetBytes((ushort)data);
            //    }
            //    else if (data <= UInt32.MaxValue)
            //    {
            //        flag |= DoubleTypeFlag.IntVal;
            //        buffer = BitConverter.GetBytes((UInt32)data);
            //    }
            //}
            //else if (((data < 10000000) && ((mod * 10) % 1 == 0))
            //    ||((data < 1000000) && ((mod * 100) % 1 == 0))
            //    ||((data < 100000) && ((mod * 1000) % 1 == 0))
            //    ||((data < 10000) && ((mod * 10000) % 1 == 0))
            //    ||((data < 1000) && ((mod * 100000) % 1 == 0))
            //    ||((data < 100) && ((mod * 1000000) % 1 == 0))
            //    ||((data < 10) && ((mod * 10000000) % 1 == 0)))
            //{
            //    flag |= DoubleTypeFlag.FloatVal;
            //    buffer = BitConverter.GetBytes((float)data);
            //}

            //if (buffer == null)
            //{
            //    buffer = BitConverter.GetBytes(data);
            //    flag = DoubleTypeFlag.DEFAULT;
            //}
            //_ms.WriteByte((byte)flag);
            //_ms.Write(buffer, 0, buffer.Length);
        }

        void IDisposable.Dispose()
        {
            _ms.Close();
        }
    }
}
