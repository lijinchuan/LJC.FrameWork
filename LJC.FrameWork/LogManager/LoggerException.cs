using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.LogManager
{
    internal class LoggerException : ApplicationException
    {
        private Exception _fromException = null;
        private LoggerException(Exception ex)
        {
            _fromException = ex;
        }

        public static Exception GetException(Exception ex)
        {
            if (ex == null)
                return null;

            if (ex is LoggerException)
                return ex;

            return new LoggerException(ex);
        }

        public override string Message
        {
            get
            {
                return _fromException.Message;
            }
        }

        public override string Source
        {
            get
            {
                return _fromException.Source;
            }
            set
            {
                base.Source = value;
            }
        }


        public override System.Collections.IDictionary Data
        {
            get
            {
                return _fromException.Data;
            }
        }

        public override Exception GetBaseException()
        {
            return _fromException.GetBaseException();
        }

        public override string StackTrace
        {
            get
            {
                return _fromException.StackTrace;
            }
        }

        public override string HelpLink
        {
            get
            {
                return _fromException.HelpLink;
            }
            set
            {
                base.HelpLink = value;
            }
        }

        public override string ToString()
        {
            var inexp = _fromException;
            StringBuilder sb = new StringBuilder();

            string level = "";
            while (inexp != null)
            {
                sb.AppendLine(string.Format("{0}错误信息:{1}", level, inexp.Message));
                sb.AppendLine(string.Format("{0}堆栈信息:{1}", level, inexp.StackTrace));
                sb.AppendLine(string.Format("{0}---------数据信息---------", level));
                foreach (DictionaryEntry kv in inexp.Data)
                {
                    sb.AppendLine(string.Format("{0} {1}: {2}", level, kv.Key, kv.Value));
                }
                sb.AppendLine(string.Format("{0}------------数据信息END---------------", level));

                inexp = inexp.InnerException;
                level += "+";
            }
            return sb.ToString();
        }
    }
}
