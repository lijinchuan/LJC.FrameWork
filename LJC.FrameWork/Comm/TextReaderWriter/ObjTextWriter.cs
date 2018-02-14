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

        /// <summary>
        /// 预追加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="append">第一个stream是内存里面的stream,第二个是文件的stream</param>
        /// <returns></returns>
        public Tuple<long, long> PreAppendObject<T>(T obj, Func<byte[], Stream, Tuple<long,long>> append) where T : class
        {
            Tuple<long, long> offset;

            using (MemoryStream ms0 = new MemoryStream())
            {
                using (System.IO.StreamWriter tempms = new StreamWriter(ms0))
                {
                    if (ObjTextReaderWriterEncodeType.protobuf == this._encodeType
                        || ObjTextReaderWriterEncodeType.protobufex == this._encodeType)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            ProtoBuf.Serializer.Serialize<T>(ms, obj);
                            offset = Append(tempms.BaseStream, ms.ToArray(), true);
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
                        offset = Append(tempms.BaseStream, Encoding.UTF8.GetBytes(json), true);
                    }
                    else if (ObjTextReaderWriterEncodeType.entitybuf == this._encodeType
                        || ObjTextReaderWriterEncodeType.entitybufex == this._encodeType)
                    {
                        var buf = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(obj);
                        offset = Append(tempms.BaseStream, buf, true);
                    }
                    else
                    {
                        string str = JsonUtil<T>.Serialize(obj);
                        if (ObjTextReaderWriterEncodeType.jsongzip == this._encodeType)
                        {
                            var jsonByte = Encoding.UTF8.GetBytes(str);
                            var compressbytes = GZip.Compress(jsonByte);
                            offset = Append(tempms.BaseStream, compressbytes, false);
                        }
                        else
                        {
                            offset = Append(tempms, str);
                        }
                    }

                    var bytes = ms0.ToArray();
                    lock (this)
                    {
                        offset = append(bytes, _sw.BaseStream);

                    }

                    if (offset == null)
                    {
                        lock (this)
                        {
                            _sw.BaseStream.Write(bytes, 0, bytes.Length);
                        }

                    }
                }
            }

            return offset;
        }

        public Tuple<long,long> AppendObject<T>(T obj) where T : class
        {
            Tuple<long,long> offset;
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

        private Tuple<long, long> Append(StreamWriter s, string objtr)
        {
            long offset = s.BaseStream.Position;
            s.WriteLine();
            s.Write(objtr);
            s.Write(splitChar);

            return new Tuple<long, long>(offset, s.BaseStream.Position);
        }

        private Tuple<long, long> Append(string objtr)
        {
            if (string.IsNullOrEmpty(objtr))
                return new Tuple<long, long>(0, 0);

            lock (this)
            {
                return Append(_sw, objtr);
            }
        }

        public void SetPosition(long pos)
        {
            _sw.BaseStream.Position = pos;
            
        }

        private Tuple<long, long> Append(Stream s, byte[] objstream, bool writesplit)
        {
            var lenbyte = BitConverter.GetBytes(objstream.Length);

            long offset = s.Position;
            s.Write(lenbyte, 0, lenbyte.Length);
            s.Write(objstream, 0, objstream.Length);
            if (_canReadFromBack)
            {
                s.Write(lenbyte, 0, lenbyte.Length);
            }

            if (writesplit)
            {
                s.Write(ObjTextReaderWriterBase.splitBytes, 0, 2);
            }

            return new Tuple<long, long>(offset, s.Position);
        }

        private Tuple<long,long> Append(byte[] objstream,bool writesplit)
        {
            if (objstream == null)
                return new Tuple<long, long>(0, 0);

            lock (this)
            {
                return Append(_sw.BaseStream, objstream, writesplit);
            }
        }

        public Tuple<long,long> Override(long start, byte[] bytes)
        {
            lock (this)
            {
                _sw.BaseStream.Position = start;
                _sw.BaseStream.Write(bytes, 0, bytes.Length);

                return new Tuple<long, long>(start, _sw.BaseStream.Position);
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
