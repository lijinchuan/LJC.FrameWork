using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.LogManager
{
    internal class NoneLogWriter:ILogWriter
    {
        public LogLevel level;

        public NoneLogWriter(LogLevel lev)
        {
            level = lev;
        }
        /// <summary>
        /// 什么事也不干
        /// </summary>
        /// <param name="logTit"></param>
        /// <param name="logBody"></param>
        /// <param name="category"></param>
        public void WriteLog(string logTit, string logBody, LogCategory category)
        {
            //throw new NotImplementedException();
        }
    }
}
