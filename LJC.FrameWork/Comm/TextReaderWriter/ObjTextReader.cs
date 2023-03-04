using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LJC.FrameWork.Comm
{
    public class ObjTextReader : ObjTextReaderWriterBase, IDisposable
    {
        private StreamReader _sr;

        private ObjTextReader(string textfile)
        {
            this.readwritePath = textfile;
            //var fs = File.Open(textfile, FileMode.Open, FileAccess.Read);
            var fs = new FileStream(textfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _sr = new StreamReader(fs, Encoding.UTF8);
            int firstchar = _sr.BaseStream.ReadByte();
            if (firstchar == -1)
            {
                throw new Exception("未知编码方式。");
            }
            else
            {
                this._encodeType = (ObjTextReaderWriterEncodeType)firstchar;
            }

            _canReadFromBack = CanReadFormBack;
        }

        public bool SetPostion(long pos)
        {
            if(_sr.BaseStream.Length<pos)
            {
                return false;
            }
            _sr.BaseStream.Position = pos;
            return true;
        }


        public static ObjTextReader CreateReader(string textfile)
        {
            return new ObjTextReader(textfile);
        }

        public T ReadObjectFromBack<T>(bool autoReset = true) where T : class
        {
            if (!_canReadFromBack)
                throw new Exception("不支持从后向前读");

            if (!PostionLast(_sr.BaseStream, autoReset))
                return default(T);

            var oldpostion = _sr.BaseStream.Position;

            byte[] bylen = new byte[4];
            _sr.BaseStream.Read(bylen, 0, 4);
            var len = BitConverter.ToInt32(bylen, 0);

            _sr.BaseStream.Position = oldpostion - len;
            oldpostion = _sr.BaseStream.Position;
            var contentbyte = new Byte[len];
            _sr.BaseStream.Read(contentbyte, 0, len);

            //if (oldpostion >= 8)
            //    _sr.BaseStream.Position = oldpostion - 8;
            //else
            //    _sr.BaseStream.Position = oldpostion - 4;

            _sr.BaseStream.Position = oldpostion - 4;

            using (MemoryStream ms = new MemoryStream(contentbyte))
            {
                switch (this._encodeType)
                {
                    case ObjTextReaderWriterEncodeType.protobufex:
                    case ObjTextReaderWriterEncodeType.protobuf:
                        {
                            throw new NotImplementedException();
                        }
                    case ObjTextReaderWriterEncodeType.jsonbuf:
                    case ObjTextReaderWriterEncodeType.jsonbufex:
                        {
                            return JsonUtil<T>.Deserialize(Encoding.UTF8.GetString(contentbyte));
                        }
                    case ObjTextReaderWriterEncodeType.entitybufex:
                    case ObjTextReaderWriterEncodeType.entitybuf:
                        {
                            return LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<T>(contentbyte);
                        }
                    case ObjTextReaderWriterEncodeType.entitybuf2:
                        {
                            return LJC.FrameWork.EntityBuf2.EntityBufCore2.DeSerialize<T>(contentbyte);
                        }
                    default:
                        {
                            throw new NotSupportedException();
                        }
                }

            }
        }

        private string ReadLine(Stream s)
        {
            using (System.IO.MemoryStream ms = new MemoryStream())
            {
                int b = 0;
                while ((b=s.ReadByte())!=-1)
                {
                    byte bt = (byte)b;
                    ms.WriteByte(bt);
                    if (bt == '\n')
                    {
                        break;
                    }
                }

                return System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private T ReadObject<T>(Stream s) where T : class
        {
            while (CheckNexIsEndSpan(s))
            {
                s.Position += 3;
            }

            var fixpos = s.Position;

            byte[] bylen = new byte[4];
            int readreallen = s.Read(bylen, 0, 4);
            if (readreallen != 4)
            {
                //throw new Exception("读取长度失败");
                return default(T);
            }
            var len = BitConverter.ToInt32(bylen, 0);
            if (len == 0 || len == 12565487)
                return default(T);

            if (len > 10240000)
            {
                throw new OverflowException(string.Format("太长了:[{0} {1} {2} {3}]", bylen[0], bylen[1], bylen[2], bylen[3]) + len + ",mem len:" + s.Length);
            }

            //检查长度
            if (s.Length - s.Position < len)
            {
                //s.Position -= 4;
                s.Position = fixpos;
                return default(T);
            }

            var contentbyte = new Byte[len];
            s.Read(contentbyte, 0, len);

            switch (_encodeType)
            {
                case ObjTextReaderWriterEncodeType.protobuf:
                case ObjTextReaderWriterEncodeType.protobufex:
                    {
                        //if (_canReadFromBack)
                        //{
                        //    s.Position += 4;
                        //}

                        ////扫过分隔符
                        //s.Position += 2;
                        if (!SkipNextSplitChar(s))
                        {
                            //throw new Exception("定位分割符失败");
                            s.Position = fixpos;
                            return default(T);
                        }

                        using (MemoryStream ms = new MemoryStream(contentbyte))
                        {
                            throw new NotImplementedException();
                        }
                    }
                case ObjTextReaderWriterEncodeType.jsonbuf:
                case ObjTextReaderWriterEncodeType.jsonbufex:
                    {

                        //if (_canReadFromBack)
                        //{
                        //    s.Position += 4;
                        //}

                        ////扫过分隔符
                        //s.Position += 2;

                        if (!SkipNextSplitChar(s))
                        {
                            //throw new Exception("定位分割符失败");
                            s.Position = fixpos;
                            return default(T);
                        }

                        using (MemoryStream ms = new MemoryStream(contentbyte))
                        {
                            //return ProtoBuf.Serializer.Deserialize<T>(ms);
                            return JsonUtil<T>.Deserialize(Encoding.UTF8.GetString(ms.ToArray()));
                        }
                    }
                case ObjTextReaderWriterEncodeType.entitybuf:
                case ObjTextReaderWriterEncodeType.entitybufex:
                    {
                        //if (_canReadFromBack)
                        //{
                        //    s.Position += 4;
                        //}

                        ////扫过分隔符
                        //s.Position += 2;

                        if (!SkipNextSplitChar(s))
                        {
                            //throw new Exception("定位分割符失败");
                            s.Position = fixpos;
                            return default(T);
                        }

                        return LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<T>(contentbyte);
                    }
                case ObjTextReaderWriterEncodeType.entitybuf2:
                    {
                        //if (_canReadFromBack)
                        //{
                        //    s.Position += 4;
                        //}

                        ////扫过分隔符
                        //s.Position += 2;

                        if (!SkipNextSplitChar(s))
                        {
                            //throw new Exception("定位分割符失败");
                            s.Position = fixpos;
                            return default(T);
                        }

                        return LJC.FrameWork.EntityBuf2.EntityBufCore2.DeSerialize<T>(contentbyte);

                    }
                default:
                    {
                        throw new NotSupportedException("不支持的类型:" + _encodeType);
                    }
            }
        }

        public T ReadObject<T>() where T : class
        {
            return ReadObject<T>(_sr.BaseStream);
        }

        [Obsolete("replace by ReadObjectsWating")]
        public IEnumerable<T> ReadObjectWating<T>() where T : class
        {
            //var oldpost = _sr.BaseStream.Position;
            var oldlen = 0L;
            T item=default(T);
            T last = default(T);
            while(true)
            {
                while((item=ReadObject<T>())!=null)
                {
                    last = item;
                    yield return item;
                }
                //oldlen=_sr.BaseStream.Length;
                oldlen = _sr.BaseStream.Position;
                while (_sr.BaseStream.Length - oldlen < 4)
                {
                    Thread.Sleep(1);
                }
            }
        }

        public IEnumerable<T> ReadObjectsWating<T>(int timeout = 0, Action<long> hook = null, byte[] bytes = null) where T : class
        {
            //var oldpost = _sr.BaseStream.Position;
            var oldlen = 0L;
            T item = default(T);
            T last = default(T);
            DateTime wtime = DateTime.Now;
            int wms = 0;
            int sleelms = 1;

            int bufferlen = bytes == null ? 1024 * 1024 * 10 : bytes.Length;
            if (bytes == null)
            {
                bytes = new byte[bufferlen];
            }

            while (true)
            {
                int readlen = _sr.BaseStream.Read(bytes, 0, bufferlen);

                if (readlen > 0)
                {
                    _sr.BaseStream.Position -= readlen;
                    oldlen = _sr.BaseStream.Position;

                    long readpos = oldlen;
                    using (MemoryStream ms = new MemoryStream(bytes, 0, readlen))
                    {
                        while ((item = ReadObject<T>(ms)) != null)
                        {
                            last = item;
                            _sr.BaseStream.Position = oldlen + ms.Position;
                            if (hook != null)
                            {
                                hook(readpos);
                            }
                            readpos = oldlen + ms.Position;
                            yield return item;
                        }

                        if (oldlen != _sr.BaseStream.Position)
                        {
                            oldlen = _sr.BaseStream.Position;
                            wtime = DateTime.Now;
                            sleelms = 1;
                        }
                        else if (timeout > 0)
                        {
                            break;
                        }
                    }
                }
                else if (timeout > 0)
                {
                    break;
                }

                bool exit = false;
                while (_sr.BaseStream.Length - oldlen < 4)
                {
                    Thread.Sleep(sleelms);
                    wms += sleelms;

                    if (timeout > 0 && wms > timeout)
                    {
                        exit = true;
                        break;
                    }
                    else
                    {
                        if (sleelms++ > 100)
                        {
                            sleelms = 1;
                        }
                    }
                }

                if (exit)
                {
                    break;
                }
            }
        }

        public long ReadedPostion()
        {
            return _sr.BaseStream.Position;
        }

        internal bool PostionNextSplitChar()
        {
            byte[] bytes=new byte[2];
            var len=_sr.BaseStream.Length;
            if (len >= 2)
            {
                while (_sr.BaseStream.Position <= len - 2)
                {
                    _sr.BaseStream.Read(bytes, 0, 2);
                    if (bytes[0] == splitBytes[0] && bytes[1] == splitBytes[1])
                    {
                        return true;
                    }

                    if (bytes[1] == splitBytes[0])
                    {
                        _sr.BaseStream.Position -= 1;
                    }
                }
            }
            _sr.BaseStream.Position = len;
            return false;
        }

        public long Length()
        {
            return this._sr.BaseStream.Length;
        }

        public void Dispose()
        {
            if (_sr != null)
            {
                _sr.Close();
                _sr.Dispose();
                _sr = null;
            }
        }
    }
}
