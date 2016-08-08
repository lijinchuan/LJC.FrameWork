using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class ObjTextWriter : ObjTextReaderWriterBase, IDisposable
    {
        private StreamWriter _sw;

        private ObjTextWriter(string textfile, ObjTextReaderWriterEncodeType encodetype)
        {
            this.readwritePath = textfile;
            var fs = File.Open(textfile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            _sw = new StreamWriter(fs, Encoding.UTF8);
            int firstchar = fs.ReadByte();
            if (firstchar == -1)
            {
                this._encodeType = encodetype;
                fs.WriteByte((byte)encodetype);
            }
            else
            {
                this._encodeType = (ObjTextReaderWriterEncodeType)firstchar;

                if (CheckHasEndSpan(_sw.BaseStream))
                {
                    _sw.BaseStream.Position = _sw.BaseStream.Length - 3;
                }
                else
                {
                    _sw.BaseStream.Position = _sw.BaseStream.Length;
                }
            }
            _sw.Flush();
            _canReadFromBack = CanReadFormBack;
        }


        public static ObjTextWriter CreateWriter(string textfile, ObjTextReaderWriterEncodeType encodetype = ObjTextReaderWriterEncodeType.json)
        {
            return new ObjTextWriter(textfile, encodetype);
        }

        public void Flush()
        {
            _sw.Flush();
        }

        public void AppendObject<T>(T obj) where T : class
        {
            if (ObjTextReaderWriterEncodeType.protobuf == this._encodeType
                || ObjTextReaderWriterEncodeType.protobufex == this._encodeType)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize<T>(ms, obj);
                    Append(ms.ToArray());
                    _sw.BaseStream.Write(ObjTextReaderWriterBase.splitBytes, 0, 2);
                }
            }
            else
            {
                string str = JsonUtil<T>.Serialize(obj);
                if (ObjTextReaderWriterEncodeType.jsongzip == this._encodeType)
                {
                    var jsonByte = Encoding.UTF8.GetBytes(str);
                    var compressbytes = GZip.Compress(jsonByte);
                    Append(compressbytes);
                }
                else
                {
                    Append(str);
                }
            }
        }

        private void Append(string objtr)
        {
            if (string.IsNullOrEmpty(objtr))
                return;

            lock (this)
            {
                _sw.WriteLine();
                _sw.Write(objtr);
                _sw.Write(splitChar);
            }
        }

        private void Append(byte[] objstream)
        {
            if (objstream == null)
                return;

            var lenbyte = BitConverter.GetBytes(objstream.Length);

            lock (this)
            {
                _sw.BaseStream.Write(lenbyte, 0, lenbyte.Length);
                _sw.BaseStream.Write(objstream, 0, objstream.Length);
                if (_canReadFromBack)
                {
                    _sw.BaseStream.Write(lenbyte, 0, lenbyte.Length);
                }
            }
        }

        public void Dispose()
        {
            if (_sw != null)
            {
                _sw.Close();
                _sw = null;
            }
        }
    }
}
