using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace LJC.FrameWork.LogManager
{
    public class LogHelper
    {
        private static LogHelper _LogHelper = null;
        private log4net.ILog logger = null;
        private static string configfile = System.AppDomain.CurrentDomain.BaseDirectory + "\\log4net.config";

        private const string Log4netDefaultConfig = "%3C%3Fxml%20version%3D%221.0%22%20encoding%3D%22utf-8%22%20%3F%3E%0A%3Cconfiguration%3E%0A%20%20%3CconfigSections%3E%0A%20%20%20%20%3Csection%20name%3D%22log4net%22%20type%3D%22log4net.Config.Log4NetConfigurationSectionHandler%2C%20log4net%22%2F%3E%0A%20%20%3C%2FconfigSections%3E%0A%20%20%3Clog4net%3E%0A%20%20%20%20%3Croot%3E%0A%20%20%20%20%20%20%3Clevel%20value%3D%22All%22%20%2F%3E%0A%20%20%20%20%3C%2Froot%3E%0A%20%20%20%20%3Clogger%20name%3D%22Logger%22%3E%0A%20%20%20%20%20%20%3Clevel%20value%3D%22ALL%22%20%2F%3E%0A%20%20%20%20%20%20%3Cappender-ref%20ref%3D%22DebugAppender%22%20%2F%3E%0A%20%20%20%20%20%20%3Cappender-ref%20ref%3D%22InfoAppender%22%20%2F%3E%0A%20%20%20%20%20%20%3Cappender-ref%20ref%3D%22ErrorAppender%22%20%2F%3E%0A%20%20%20%20%3C%2Flogger%3E%0A%20%20%20%20%3Cappender%20name%3D%22DebugAppender%22%20type%3D%22log4net.Appender.RollingFileAppender%22%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22File%22%20value%3D%22Log%5CDebug%5C%22%20%2F%3E%0A%09%20%20%3Cparam%20name%3D%22Encoding%22%20value%3D%22utf-8%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22AppendToFile%22%20value%3D%22true%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22RollingStyle%22%20value%3D%22Date%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22DatePattern%22%20value%3D%22yyyy-MM-dd%26quot%3B.txt%26quot%3B%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22StaticLogFileName%22%20value%3D%22false%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22ImmediateFlush%22%20value%3D%22true%22%20%2F%3E%0A%20%20%20%20%20%20%3Clayout%20type%3D%22log4net.Layout.PatternLayout%2Clog4net%22%3E%0A%20%20%20%20%20%20%20%20%3Cparam%20name%3D%22ConversionPattern%22%20value%3D%22%5B%25d%7BHH%3Amm%3Ass.ffff%7D%5D%20%25m%25n%22%20%2F%3E%0A%20%20%20%20%20%20%3C%2Flayout%3E%0A%20%20%20%20%20%20%3Cfilter%20type%3D%22log4net.Filter.LevelRangeFilter%22%3E%0A%20%20%20%20%20%20%20%20%3Cparam%20name%3D%22LevelMin%22%20value%3D%22DEBUG%22%20%2F%3E%0A%20%20%20%20%20%20%20%20%3Cparam%20name%3D%22LevelMax%22%20value%3D%22DEBUG%22%20%2F%3E%0A%20%20%20%20%20%20%3C%2Ffilter%3E%0A%20%20%20%20%3C%2Fappender%3E%0A%20%20%20%20%3Cappender%20name%3D%22InfoAppender%22%20type%3D%22log4net.Appender.RollingFileAppender%22%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22File%22%20value%3D%22Log%5CInfo%5C%22%20%2F%3E%0A%09%20%20%3Cparam%20name%3D%22Encoding%22%20value%3D%22utf-8%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22AppendToFile%22%20value%3D%22true%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22RollingStyle%22%20value%3D%22Date%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22DatePattern%22%20value%3D%22yyyy-MM-dd%26quot%3B.txt%26quot%3B%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22StaticLogFileName%22%20value%3D%22false%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22ImmediateFlush%22%20value%3D%22true%22%20%2F%3E%0A%20%20%20%20%20%20%3Clayout%20type%3D%22log4net.Layout.PatternLayout%2Clog4net%22%3E%0A%20%20%20%20%20%20%20%20%3Cparam%20name%3D%22ConversionPattern%22%20value%3D%22%5B%25d%7BHH%3Amm%3Ass.ffff%7D%5D%20%25m%25n%22%20%2F%3E%0A%20%20%20%20%20%20%3C%2Flayout%3E%0A%20%20%20%20%20%20%3Cfilter%20type%3D%22log4net.Filter.LevelRangeFilter%22%3E%0A%20%20%20%20%20%20%20%20%3Cparam%20name%3D%22LevelMin%22%20value%3D%22INFO%22%20%2F%3E%0A%20%20%20%20%20%20%20%20%3Cparam%20name%3D%22LevelMax%22%20value%3D%22INFO%22%20%2F%3E%0A%20%20%20%20%20%20%3C%2Ffilter%3E%0A%20%20%20%20%3C%2Fappender%3E%0A%20%20%20%20%3Cappender%20name%3D%22ErrorAppender%22%20type%3D%22log4net.Appender.RollingFileAppender%22%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22File%22%20value%3D%22Log%5CError%5C%22%20%2F%3E%0A%09%20%20%3Cparam%20name%3D%22Encoding%22%20value%3D%22utf-8%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22AppendToFile%22%20value%3D%22true%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22RollingStyle%22%20value%3D%22Date%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22DatePattern%22%20value%3D%22yyyy-MM-dd%26quot%3B.txt%26quot%3B%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22StaticLogFileName%22%20value%3D%22false%22%20%2F%3E%0A%20%20%20%20%20%20%3Cparam%20name%3D%22ImmediateFlush%22%20value%3D%22true%22%20%2F%3E%0A%20%20%20%20%20%20%3Clayout%20type%3D%22log4net.Layout.PatternLayout%2Clog4net%22%3E%0A%20%20%20%20%20%20%20%20%3Cparam%20name%3D%22ConversionPattern%22%20value%3D%22%5B%25d%7BHH%3Amm%3Ass.ffff%7D%5D%20%25m%25n%22%20%2F%3E%0A%20%20%20%20%20%20%3C%2Flayout%3E%0A%20%20%20%20%20%20%3Cfilter%20type%3D%22log4net.Filter.LevelRangeFilter%22%3E%0A%20%20%20%20%20%20%20%20%3Cparam%20name%3D%22LevelMin%22%20value%3D%22ERROR%22%20%2F%3E%0A%20%20%20%20%20%20%20%20%3Cparam%20name%3D%22LevelMax%22%20value%3D%22ERROR%22%20%2F%3E%0A%20%20%20%20%20%20%3C%2Ffilter%3E%0A%20%20%20%20%3C%2Fappender%3E%0A%20%20%3C%2Flog4net%3E%0A%3C%2Fconfiguration%3E";

        static LogHelper()
        {
            if (!File.Exists(configfile))
            {
                using (StreamWriter sw = new StreamWriter(configfile, false, Encoding.UTF8))
                {
                    sw.Write(System.Web.HttpUtility.UrlDecode(Log4netDefaultConfig));
                }
            }
        }

        public LogHelper()
        {
            logger = log4net.LogManager.GetLogger("Logger");
        }

        public static LogHelper Instance
        {
            get
            {
                if (null == _LogHelper)
                    _LogHelper = new LogHelper();

                return _LogHelper;
            }
        }

        public bool IsDebugEnabled
        {
            get
            {
                return logger.IsDebugEnabled;
            }
        }
        public bool IsInfoEnabled
        {
            get
            {
                return logger.IsInfoEnabled;
            }
        }
        public bool IsWarnEnabled
        {
            get
            {
                return logger.IsWarnEnabled;
            }
        }
        public bool IsErrorEnabled
        {
            get
            {
                return logger.IsErrorEnabled;
            }
        }
        public bool IsFatalEnabled
        {
            get
            {
                return logger.IsFatalEnabled;
            }
        }

        public void Debug(object message, Exception exception = null)
        {
            logger.Debug(message, LoggerException.GetException(exception));
        }

        public void Info(object message, Exception exception = null)
        {
            logger.Info(message, LoggerException.GetException(exception));
        }

        public void Warn(object message, Exception exception = null)
        {
            logger.Warn(message, LoggerException.GetException(exception));
        }

        public void Error(object message, Exception exception = null)
        {
            logger.Error(message, LoggerException.GetException(exception));
        }

        public void Fatal(object message, Exception exception = null)
        {
            logger.Fatal(message, LoggerException.GetException(exception));
        }
    }
}
