using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class ObjTextReaderWriterBase
    {
        protected const char splitChar = '☺';
        protected static byte[] splitBytes = BitConverter.GetBytes(splitChar);
        protected string readwritePath = string.Empty;
        protected ObjTextReaderWriterEncodeType _encodeType = ObjTextReaderWriterEncodeType.json;
        //是否可以从后往前读
        protected bool _canReadFromBack = false;
        private static byte[] endSpanChar = new byte[] { (byte)239, (byte)187, (byte)191 };

        protected bool CanReadFormBack
        {
            get
            {
                return ObjTextReaderWriterEncodeType.protobufex == this._encodeType;
            }
        }

        protected bool CheckNexIsEndSpan(Stream s)
        {
            if(s.Length-s.Position<4)
            {
                return false;
            }

            var oldpos = s.Position;
            var byts3 = new byte[3];
            s.Read(byts3, 0, 3);

            s.Position = oldpos;
            return byts3[0] == endSpanChar[0] && byts3[1] == endSpanChar[1] && byts3[2] == endSpanChar[2];
        }

        protected bool CheckHasEndSpan(Stream s)
        {
            lock (this)
            {
                var oldpos = s.Position;
                if (s.Length >= 4)
                {
                    var byts3 = new byte[3];
                    s.Position = s.Length - 3;
                    s.Read(byts3, 0, 3);

                    s.Position = oldpos;
                    return byts3[0] == endSpanChar[0] && byts3[1] == endSpanChar[1] && byts3[2] == endSpanChar[2];
                }
                return false;
            }
        }
    }
}
