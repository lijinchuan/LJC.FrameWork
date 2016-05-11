using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Concurrent;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.LogManager
{
    internal static class LogFactory
    {
        static LogFactory()
        {
            CommFun.SetInterval(60000, WriteLog);
        }

        static bool WriteLog()
        {
            try
            {
                Log log;
                while (Global.LogPool.TryDequeue(out log))
                {
                    try
                    {
                        LogWriter.LogToDB(log);
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

            }

            return false;
        }

        public static ILogWriter CreateLogWriter(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Real:
                case LogLevel.Fatal:
                case LogLevel.Serious:
                    return new UrgentLogwriter(level);
                case LogLevel.Debug:
                    if (Global.DebugLog)
                        return new NomalLogwriter(level);
                    else
                        return new NoneLogWriter(level);
                case LogLevel.Text:
                    return new TextLog();
                default:
                    return new NomalLogwriter(level);
            }
        }

        public static ILogReader CreateLogReader(LogLevel level)
        {
            return new LevelLogReader(level);
        }


        public static ILogReader CreateLogReader(LogCategory category)
        {
            return new CategoryLogReader(category);
        }
    }
}
