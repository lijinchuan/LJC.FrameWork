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

        private const string Log4netDefaultConfig = "%3c%3fxml+version%3d%221.0%22+encoding%3d%22utf-8%22+%3f%3e%0d%0a%3cconfiguration%3e%0d%0a++%3cconfigSections%3e%0d%0a++++%3csection+name%3d%22log4net%22+type%3d%22log4net.Config.Log4NetConfigurationSectionHandler%2c+log4net%22%2f%3e%0d%0a++%3c%2fconfigSections%3e%0d%0a++%3clog4net%3e%0d%0a++++%3croot%3e%0d%0a++++++%3clevel+value%3d%22All%22+%2f%3e%0d%0a++++%3c%2froot%3e%0d%0a++++%3clogger+name%3d%22Logger%22%3e%0d%0a++++++%3clevel+value%3d%22ALL%22+%2f%3e%0d%0a++++++%3cappender-ref+ref%3d%22DebugAppender%22+%2f%3e%0d%0a++++++%3cappender-ref+ref%3d%22InfoAppender%22+%2f%3e%0d%0a++++++%3cappender-ref+ref%3d%22ErrorAppender%22+%2f%3e%0d%0a++++%3c%2flogger%3e%0d%0a++++%3cappender+name%3d%22DebugAppender%22+type%3d%22log4net.Appender.RollingFileAppender%22%3e%0d%0a++++++%3cparam+name%3d%22File%22+value%3d%22Log%5cDebug%5c%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22AppendToFile%22+value%3d%22true%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22RollingStyle%22+value%3d%22Date%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22DatePattern%22+value%3d%22yyyy-MM-dd%26quot%3b.txt%26quot%3b%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22StaticLogFileName%22+value%3d%22false%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22ImmediateFlush%22+value%3d%22true%22+%2f%3e%0d%0a++++++%3clayout+type%3d%22log4net.Layout.PatternLayout%2clog4net%22%3e%0d%0a++++++++%3cparam+name%3d%22ConversionPattern%22+value%3d%22%5b%25d%7bHH%3amm%3ass.ffff%7d%5d+%25m%25n%22+%2f%3e%0d%0a++++++%3c%2flayout%3e%0d%0a++++++%3cfilter+type%3d%22log4net.Filter.LevelRangeFilter%22%3e%0d%0a++++++++%3cparam+name%3d%22LevelMin%22+value%3d%22DEBUG%22+%2f%3e%0d%0a++++++++%3cparam+name%3d%22LevelMax%22+value%3d%22DEBUG%22+%2f%3e%0d%0a++++++%3c%2ffilter%3e%0d%0a++++%3c%2fappender%3e%0d%0a++++%3cappender+name%3d%22InfoAppender%22+type%3d%22log4net.Appender.RollingFileAppender%22%3e%0d%0a++++++%3cparam+name%3d%22File%22+value%3d%22Log%5cInfo%5c%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22AppendToFile%22+value%3d%22true%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22RollingStyle%22+value%3d%22Date%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22DatePattern%22+value%3d%22yyyy-MM-dd%26quot%3b.txt%26quot%3b%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22StaticLogFileName%22+value%3d%22false%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22ImmediateFlush%22+value%3d%22true%22+%2f%3e%0d%0a++++++%3clayout+type%3d%22log4net.Layout.PatternLayout%2clog4net%22%3e%0d%0a++++++++%3cparam+name%3d%22ConversionPattern%22+value%3d%22%5b%25d%7bHH%3amm%3ass.ffff%7d%5d+%25m%25n%22+%2f%3e%0d%0a++++++%3c%2flayout%3e%0d%0a++++++%3cfilter+type%3d%22log4net.Filter.LevelRangeFilter%22%3e%0d%0a++++++++%3cparam+name%3d%22LevelMin%22+value%3d%22INFO%22+%2f%3e%0d%0a++++++++%3cparam+name%3d%22LevelMax%22+value%3d%22INFO%22+%2f%3e%0d%0a++++++%3c%2ffilter%3e%0d%0a++++%3c%2fappender%3e%0d%0a++++%3cappender+name%3d%22ErrorAppender%22+type%3d%22log4net.Appender.RollingFileAppender%22%3e%0d%0a++++++%3cparam+name%3d%22File%22+value%3d%22Log%5cError%5c%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22AppendToFile%22+value%3d%22true%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22RollingStyle%22+value%3d%22Date%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22DatePattern%22+value%3d%22yyyy-MM-dd%26quot%3b.txt%26quot%3b%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22StaticLogFileName%22+value%3d%22false%22+%2f%3e%0d%0a++++++%3cparam+name%3d%22ImmediateFlush%22+value%3d%22true%22+%2f%3e%0d%0a++++++%3clayout+type%3d%22log4net.Layout.PatternLayout%2clog4net%22%3e%0d%0a++++++++%3cparam+name%3d%22ConversionPattern%22+value%3d%22%5b%25d%7bHH%3amm%3ass.ffff%7d%5d+%25m%25n%22+%2f%3e%0d%0a++++++%3c%2flayout%3e%0d%0a++++++%3cfilter+type%3d%22log4net.Filter.LevelRangeFilter%22%3e%0d%0a++++++++%3cparam+name%3d%22LevelMin%22+value%3d%22ERROR%22+%2f%3e%0d%0a++++++++%3cparam+name%3d%22LevelMax%22+value%3d%22ERROR%22+%2f%3e%0d%0a++++++%3c%2ffilter%3e%0d%0a++++%3c%2fappender%3e%0d%0a++%3c%2flog4net%3e%0d%0a%3c%2fconfiguration%3e";

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
