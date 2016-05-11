using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.LogManager
{
    internal class NomalLogwriter:ILogWriter
    {
        public LogLevel level;

        public NomalLogwriter(LogLevel lev)
        {
            level = lev;
        }

        public void WriteLog(string logTit,string logBody,LogCategory category)
        {
            Log log = new Log
            {
                Category=category,
                LogTime=DateTime.Now,
                LogBody=logBody,
                LogTit=logTit,
                Level=level
            };

            Global.LogPool.Enqueue(log);
        }

    }
}
