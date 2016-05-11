using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.LogManager
{
    public class TextLog:ILogWriter
    {
        static TextLog()
        {
            CommFun.SetInterval(1, LogToFile);
        }
        private static readonly string LogForderName = AppDomain.CurrentDomain.BaseDirectory + "Log";
        private static string LogFileName
        {
            get
            {
                string filename = string.Concat(LogForderName, "\\", DateTime.Now.ToString("yyyyMMdd"), ".log");
                return filename;
            }
        }
        public void WriteLog(string logTit, string logBody, LogCategory category)
        {
            Log log = new Log
            {
                Category=category,
                LogBody=logBody,
                LogTit=logTit,
                LogTime=DateTime.Now
            };
            Global.TextLogPool.Enqueue(log);
        }
        private static bool LogToFile()
        {
            Log log;
            try
            {
                using (StreamWriter sw = new StreamWriter(LogFileName, true, Encoding.UTF8))
                {
                    while (Global.TextLogPool.TryDequeue(out log))
                    {
                        sw.WriteLine(log.LogTime.ToString("yyyyMMdd HH:mm:ss")+" "+log.LogTit);
                        sw.WriteLine(log.Category.ToString());
                        sw.WriteLine(log.LogBody);
                        sw.WriteLine("----------------------------------------------");

                    }
                }
            }
            catch
            {

            }
            return false;
        }
    }
}
