using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

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

        public T ReadObjectFromBack<T>(bool autoReset=true) where T : class
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

            _sr.BaseStream.Position = oldpostion-4;

            using (MemoryStream ms = new MemoryStream(contentbyte))
            {
                return ProtoBuf.Serializer.Deserialize<T>(ms);
            }
        }

        public T ReadObject<T>() where T : class
        {
            if(CheckNexIsEndSpan(_sr.BaseStream))
            {
                _sr.BaseStream.Position += 3;
            }

            if (_encodeType == ObjTextReaderWriterEncodeType.protobuf
                || _encodeType == ObjTextReaderWriterEncodeType.protobufex)
            {
                byte[] bylen = new byte[4];
                _sr.BaseStream.Read(bylen, 0, 4);
                var len = BitConverter.ToInt32(bylen, 0);
                //239 187 191
                if (len <= 0 || len == 12565487)
                    return default(T);

                //检查长度
                if (_sr.BaseStream.Length - _sr.BaseStream.Position < len)
                {
                    _sr.BaseStream.Position -= 4;
                    return default(T);
                }

                var contentbyte = new Byte[len];
                _sr.BaseStream.Read(contentbyte, 0, len);

                if (_canReadFromBack)
                {
                    _sr.BaseStream.Position += 4;
                }

                //扫过分隔符
                _sr.BaseStream.Position += 2;

                using (MemoryStream ms = new MemoryStream(contentbyte))
                {
                    return ProtoBuf.Serializer.Deserialize<T>(ms);
                }
            }
            else if (_encodeType == ObjTextReaderWriterEncodeType.jsonbuf
               || _encodeType == ObjTextReaderWriterEncodeType.jsonbufex)
            {
                byte[] bylen = new byte[4];
                _sr.BaseStream.Read(bylen, 0, 4);
                var len = BitConverter.ToInt32(bylen, 0);
                //239 187 191
                if (len <= 0 || len == 12565487)
                    return default(T);

                if (len > 10240000)
                {
                    throw new Newtonsoft.Json.JsonReaderException("太长了:" + len);
                }

                //检查长度
                if (_sr.BaseStream.Length - _sr.BaseStream.Position < len)
                {
                    _sr.BaseStream.Position -= 4;
                    return default(T);
                }

                var contentbyte = new Byte[len];
                _sr.BaseStream.Read(contentbyte, 0, len);

                if (_canReadFromBack)
                {
                    _sr.BaseStream.Position += 4;
                }

                //扫过分隔符
                _sr.BaseStream.Position += 2;

                using (MemoryStream ms = new MemoryStream(contentbyte))
                {
                    //return ProtoBuf.Serializer.Deserialize<T>(ms);
                    return JsonUtil<T>.Deserialize(Encoding.UTF8.GetString(ms.ToArray()));
                }

            }
            else if (_encodeType == ObjTextReaderWriterEncodeType.jsongzip)
            {
                byte[] bylen = new byte[4];
                _sr.BaseStream.Read(bylen, 0, 4);
                var len = BitConverter.ToInt32(bylen, 0);
                if (len == 0 || len == 12565487)
                    return default(T);

                //检查长度
                if (_sr.BaseStream.Length - _sr.BaseStream.Position < len)
                {
                    _sr.BaseStream.Position -= 4;
                    return default(T);
                }

                var contentbyte = new Byte[len];
                _sr.BaseStream.Read(contentbyte, 0, len);

                var decomparssbytes = GZip.Decompress(contentbyte);
                var jsonstr = Encoding.UTF8.GetString(decomparssbytes);
                return JsonUtil<T>.Deserialize(jsonstr);
            }
            else
            {
                //string s = _sr.ReadLine().Trim((char)65279, (char)1); //过滤掉第一行
                string s = _sr.ReadLine();

                if (s == null)
                    return default(T);

                s = s.Trim((char)65279, (char)1);

                while ((string.IsNullOrEmpty(s) || !s.Last().Equals(splitChar))
                    && !_sr.EndOfStream)
                {
                    s += _sr.ReadLine().Trim((char)65279, (char)1);
                }

                if (!string.IsNullOrEmpty(s) && s.Last().Equals(splitChar))
                {
                    s = s.Remove(s.Length - 1, 1);
                    return JsonUtil<T>.Deserialize(s);
                }

                return default(T);
            }
        }

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
                oldlen=_sr.BaseStream.Length;
                while (_sr.BaseStream.Length - oldlen < 4)
                {
                    Thread.Sleep(1);
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

        public void Dispose()
        {
            if (_sr != null)
            {
                _sr.Close();
                _sr = null;
            }
        }
    }
}
