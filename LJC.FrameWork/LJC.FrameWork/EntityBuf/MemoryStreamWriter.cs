using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.EntityBuf
{
    public class MemoryStreamWriter:IDisposable
    {
        private MemoryStream _ms;

        public MemoryStreamWriter(MemoryStream ms)
        {
            _ms = ms;
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[_ms.Length];
            _ms.Seek(0, SeekOrigin.Begin);
            _ms.Read(bytes, 0, (int)_ms.Length);
            return bytes;
        }

        public void WriteBool(bool boo)
        {
            _ms.WriteByte((byte)EntityType.BOOL);
            _ms.WriteByte((byte)EntityBufTypeFlag.Empty);
            byte[] bts = BitConverter.GetBytes(boo);
            _ms.Write(bts, 0, bts.Length);
        }

        public void WriteBoolArray(bool[] booArray)
        {
            _ms.WriteByte((byte)EntityType.BOOL);
            _ms.WriteByte((byte)EntityBufTypeFlag.ArrayFlag);
            int len = booArray.Length;
            _ms.Write(BitConverter.GetBytes(len), 0, 4);
            BitArray ba = new BitArray(booArray);

            foreach (byte b in BitHelper.ConvertToByteArray(ba))
            {
                _ms.WriteByte(b);
            }
        }

        public void WriteInt32(Int32 num)
        {
            _ms.WriteByte((byte)EntityType.INT32);
            _ms.WriteByte((byte)EntityBufTypeFlag.Empty);
            _ms.Write(BitConverter.GetBytes(num), 0, 4);
        }

        public void WriteInt32Array(Int32[] intArray)
        {
            _ms.WriteByte((byte)EntityType.INT32);
            _ms.WriteByte((byte)EntityBufTypeFlag.ArrayFlag);
            int len = 0;
            if (intArray != null)
            {
                len = intArray.Length;
            }
            _ms.Write(BitConverter.GetBytes(len), 0, 4);
            foreach (int num in intArray)
            {
                _ms.Write(BitConverter.GetBytes(num), 0, 4);
            }
        }

        public void WriteInt64(Int64 num)
        {
            _ms.WriteByte((byte)EntityType.INT64);
            _ms.WriteByte((byte)EntityBufTypeFlag.Empty);
            _ms.Write(BitConverter.GetBytes(num), 0, 8);
        }

        public void WriteInt64Array(Int64[] intArray)
        {
            _ms.WriteByte((byte)EntityType.INT64);
            _ms.WriteByte((byte)EntityBufTypeFlag.ArrayFlag);
            int len = 0;
            if (intArray != null)
            {
                len = intArray.Length;
            }
            _ms.Write(BitConverter.GetBytes(len), 0, 4);
            foreach (int num in intArray)
            {
                _ms.Write(BitConverter.GetBytes(num), 0, 8);
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
                _ms.Write(BitConverter.GetBytes(-1), 0, 4);
            }
        }


        public void WriteString(string str)
        {
            _ms.WriteByte((byte)EntityType.STRING);
            _ms.WriteByte((byte)EntityBufTypeFlag.Empty);
            if (str != null)
            {
                byte[] byts = Encoding.UTF8.GetBytes(str);
                _ms.Write(BitConverter.GetBytes(byts.Length), 0, 4);
                _ms.Write(byts, 0, byts.Length);
            }
            else
            {
                _ms.Write(BitConverter.GetBytes(-1), 0, 4);
            }
        }

        public void WriteStringArray(string[] strArray)
        {
            _ms.WriteByte((byte)EntityType.STRING);
            _ms.WriteByte((byte)EntityBufTypeFlag.ArrayFlag);
            int len = strArray == null ? -1 : strArray.Length;
            _ms.Write(BitConverter.GetBytes(len), 0, 4);
            if (strArray != null)
            {
                foreach (string s in strArray)
                {
                    if (s != null)
                    {
                        byte[] byts = Encoding.UTF8.GetBytes(s);
                        _ms.Write(BitConverter.GetBytes(byts.Length), 0, 4);
                        _ms.Write(byts, 0, byts.Length);
                    }
                    else
                    {
                        _ms.Write(BitConverter.GetBytes(-1), 0, 4);
                    }
                }
            }
        }

        public void WriteDateTime(DateTime dateTime)
        {
            _ms.WriteByte((byte)EntityType.DATETIME);
            _ms.WriteByte((byte)EntityBufTypeFlag.Empty);
            byte[] byts = BitConverter.GetBytes(dateTime.ToOADate());
            _ms.Write(byts, 0, byts.Length);
        }

        public void WriteDateTimeArray(DateTime[] dateTimes)
        {
            _ms.WriteByte((byte)EntityType.DATETIME);
            _ms.WriteByte((byte)EntityBufTypeFlag.ArrayFlag);
            byte[] byts = BitConverter.GetBytes(dateTimes.Length);
            _ms.Write(byts, 0, byts.Length);
            foreach (DateTime dt in dateTimes)
            {
                byts = BitConverter.GetBytes(dt.ToOADate());
                _ms.Write(byts, 0, byts.Length);
            }
        }

        public void WriteByteArray(byte[] data)
        {
            this.WriteInt32(data.Length);
            this.WriteBytes(data);
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
            WriteDouble((double)data);
        }

        public void WriteDeciamlArray(decimal[] data)
        {
            _ms.Write(BitConverter.GetBytes(data.Length), 0, 4);
            foreach (decimal d in data)
            {
                WriteDouble((double)d);
            }
        }

        public void WriteDoubleArray(double[] data)
        {
            _ms.Write(BitConverter.GetBytes(data.Length), 0, 4);
            foreach (double d in data)
            {
                WriteDouble(d);
            }
        }

        public void WriteDouble(double data)
        {
            byte[] buffer = BitConverter.GetBytes(data);
            _ms.Write(buffer, 0, buffer.Length);
        }

        void IDisposable.Dispose()
        {
            _ms.Close();
        }
    }
}
