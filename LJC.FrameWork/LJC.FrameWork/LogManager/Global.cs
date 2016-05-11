using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using LJC.FrameWork.Data.QuickDataBase;
using System.Configuration;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.LogManager
{
    internal static class Global
    {
        internal static ConcurrentQueue<Log> LogPool;
        internal static ConcurrentQueue<Log> TextLogPool;
        /// <summary>
        /// 是否记录调试日志
        /// </summary>
        internal static bool DebugLog
        {
            get
            {
                
                //return (new DataContextMoudle<RunConfig>(new RunConfig()).ExecuteList().FirstOrDefault() ?? new RunConfig()).LogDebug;
                return new Regex("^(t|T|1|true|True)$").IsMatch(Comm.ConfigHelper.AppConfig("DebugLog"));
            }
        }

        static Global()
        {
            LogPool = new ConcurrentQueue<Log>();
            TextLogPool = new ConcurrentQueue<Log>();
        }
    }
}
