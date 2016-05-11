using LJC.FrameWork.Comm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.LogManager
{
    [Obsolete("使用LogHelper代替")]
    /// <summary>
    /// 记录日志
    /// 配置项：
    /// DebugTextLog-是否记录调试日志到文本文件中,值为1（记录），0-不记录
    /// </summary>
    public static class Logger
    {
        static Logger()
        {
            Init();
        }

        private static bool debugTextLog = ConfigHelper.AppConfig("DebugTextLog").Equals("1");

        public static void Init()
        {
            string logForder = AppDomain.CurrentDomain.BaseDirectory + "Log";
            if (!Directory.Exists(logForder))
            {
                Directory.CreateDirectory(logForder);
            }
        }
        /// <summary>
        /// 记录DEBUG日志
        /// </summary>
        /// <param name="logTit"></param>
        /// <param name="logBody"></param>
        /// <param name="category"></param>
        public static void Debug(string logTit, string logBody, LogCategory category)
        {
            LogFactory.CreateLogWriter(LogLevel.Debug).WriteLog(logTit, logBody, category);
        }

        /// <summary>
        /// 记录一般严重性日志日志
        /// </summary>
        /// <param name="logTit"></param>
        /// <param name="logBody"></param>
        /// <param name="category"></param>
        public static void Normal(string logTit, string logBody, LogCategory category)
        {
            LogFactory.CreateLogWriter(LogLevel.Normal).WriteLog(logTit, logBody, category);
        }

        /// <summary>
        /// 记录DEBUG日志
        /// </summary>
        /// <param name="logTit"></param>
        /// <param name="logBody"></param>
        /// <param name="category"></param>
        public static void Error(string logTit, string logBody, LogCategory category)
        {
            LogFactory.CreateLogWriter(LogLevel.Error).WriteLog(logTit, logBody, category);
        }

        public static void Real(string logTit, string logBody, LogCategory category)
        {
            LogFactory.CreateLogWriter(LogLevel.Real).WriteLog(logTit, logBody, category);
        }

        /// <summary>
        /// 记录严重日志
        /// </summary>
        /// <param name="logTit"></param>
        /// <param name="logBody"></param>
        /// <param name="category"></param>
        public static void Serious(string logTit, string logBody, LogCategory category)
        {
            LogFactory.CreateLogWriter(LogLevel.Serious).WriteLog(logTit, logBody, category);
        }

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="logTit"></param>
        /// <param name="logBody"></param>
        /// <param name="category"></param>
        public static void Fatal(string logTit, string logBody, LogCategory category)
        {
            LogFactory.CreateLogWriter(LogLevel.Fatal).WriteLog(logTit, logBody, category);
        }

        public static void TextLog(string logTit, string logBody, LogCategory category)
        {
            LogFactory.CreateLogWriter(LogLevel.Text).WriteLog(logTit, logBody, category);
        }

        public static void DebugTextLog(string logTit, string logBody, LogCategory category)
        {
            if (!debugTextLog)
                return;

            TextLog(logTit, logBody, category);
        }

        public static void DebugTextLog(string logTit, Exception ex, LogCategory category)
        {
            if (!debugTextLog)
                return;

            TextLog(logTit, ex, category);
        }

        private static string GetExceptionMsg(Exception e)
        {
            if (e == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Message);
            sb.AppendLine(e.StackTrace);
            Exception ex = e.InnerException;
            while (ex != null)
            {
                sb.AppendLine("++++++++++++++++++++++");
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }

        public static void TextLog(string logTit, Exception e, LogCategory category)
        {
            LogFactory.CreateLogWriter(LogLevel.Text).WriteLog(logTit, GetExceptionMsg(e), category);
        }
    }
}
