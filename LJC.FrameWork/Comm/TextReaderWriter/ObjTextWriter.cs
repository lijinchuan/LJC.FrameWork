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
            Flush();
            _canReadFromBack = CanReadFormBack;
            if(_canReadFromBack)
            {
                if(PostionLast(_sw.BaseStream))
                {
                    _sw.BaseStream.Position += 6;
                }
                else
                {
                    _sw.BaseStream.Position = 1;
                }
            }
        }


        public static ObjTextWriter CreateWriter(string textfile, ObjTextReaderWriterEncodeType encodetype = ObjTextReaderWriterEncodeType.json)
        {
            return new ObjTextWriter(textfile, encodetype);
        }

        public void Flush()
        {
            if (!_isdispose)
            {
                lock (this)
                {
                    _sw.Flush();
                }
            }
        }

        public long AppendObject<T>(T obj) where T : class
        {
            var offset = 0L;
            if (ObjTextReaderWriterEncodeType.protobuf == this._encodeType
                || ObjTextReaderWriterEncodeType.protobufex == this._encodeType)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize<T>(ms, obj);
                    offset = Append(ms.ToArray(), true);
                }
            }
            else if (ObjTextReaderWriterEncodeType.jsonbuf == this._encodeType
               || ObjTextReaderWriterEncodeType.jsonbufex == this._encodeType)
            {
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    ProtoBuf.Serializer.Serialize<T>(ms, obj);
                //    Append(ms.ToArray(), true);
                //}

                var json = JsonUtil<T>.Serialize(obj);
                offset = Append(Encoding.UTF8.GetBytes(json), true);
            }
            else if (ObjTextReaderWriterEncodeType.entitybuf == this._encodeType
                || ObjTextReaderWriterEncodeType.entitybufex == this._encodeType)
            {
                var buf = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(obj);
                offset = Append(buf, true);
            }
            else
            {
                string str = JsonUtil<T>.Serialize(obj);
                if (ObjTextReaderWriterEncodeType.jsongzip == this._encodeType)
                {
                    var jsonByte = Encoding.UTF8.GetBytes(str);
                    var compressbytes = GZip.Compress(jsonByte);
                    offset = Append(compressbytes, false);
                }
                else
                {
                    offset = Append(str);
                }
            }

            return offset;
        }

        private long Append(string objtr)
        {
            if (string.IsNullOrEmpty(objtr))
                return 0L;

            lock (this)
            {
                long offset = _sw.BaseStream.Position;
                _sw.WriteLine();
                _sw.Write(objtr);
                _sw.Write(splitChar);

                return offset;
            }
        }

        public void SetPosition(long pos)
        {
            _sw.BaseStream.Position = pos;
            
        }

        private long Append(byte[] objstream,bool writesplit)
        {
            if (objstream == null)
                return 0;
            var lenbyte = BitConverter.GetBytes(objstream.Length);

            lock (this)
            {
                long offset = _sw.BaseStream.Position;
                _sw.BaseStream.Write(lenbyte, 0, lenbyte.Length);
                _sw.BaseStream.Write(objstream, 0, objstream.Length);
                if (_canReadFromBack)
                {
                    _sw.BaseStream.Write(lenbyte, 0, lenbyte.Length);
                }

                if(writesplit)
                {
                    _sw.BaseStream.Write(ObjTextReaderWriterBase.splitBytes, 0, 2);
                }

                return offset;
            }
        }

        public void Override(long start, byte[] bytes)
        {
            lock (this)
            {
                _sw.BaseStream.Position = start;
                long offset = _sw.BaseStream.Position;
                _sw.BaseStream.Write(bytes, 0, bytes.Length);
            }
        }

        private bool _isdispose = false;
        public void Dispose(bool disposing)
        {
            if (!_isdispose)
            {
                if (_sw != null)
                {
                    lock (this)
                    {
                        _sw.Close();
                    }
                }
                GC.SuppressFinalize(this);
            }

            _isdispose = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ObjTextWriter()
        {
            Dispose(false);
        }
    }
}
