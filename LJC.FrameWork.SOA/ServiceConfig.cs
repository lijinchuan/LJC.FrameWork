using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    [Serializable]
    /// <summary>
    /// 代理站的配置
    /// </summary>
    public class ServiceConfig
    {
        private const string fileName = "ServiceConfig.xml";
        private static string configfile = Comm.CommFun.GetRuningPath().TrimEnd(new[] { '\\' }) + "\\" + fileName;


        public List<WebMapper> WebMappers
        {
            get;
            set;
        }

        public List<WebProxy> WebProxies
        {
            get;
            set;
        }

        private static ServiceConfig _config = null;
        public static ServiceConfig ReadConfig()
        {
            if (_config != null)
                return _config;
            if (!File.Exists(configfile))
            {
                string configfile2 = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new[] { '\\' }) + "\\" + fileName;
                if (!File.Exists(configfile2))
                {
                    LogManager.LogHelper.Instance.Warn(string.Format("未找到ServiceConfig配置文件，路径：{0} 和路径 {1}", configfile, configfile2));
                    return null;
                }
                else
                {
                    configfile = configfile2;
                }
            }


            _config = Comm.SerializerHelper.DeSerializerFile<ServiceConfig>(configfile, true);

            if (_config?.WebMappers?.Count > 1)
            {
                _config.WebMappers = _config.WebMappers.OrderBy(p => p.MappingPort).ThenByDescending(p => p.MappingRoot).ToList();
            }

            return _config;
        }

        public static void WriteConfig(ServiceConfig config,List<WebMapper> webMappers,List<WebProxy> webProxies)
        {
            config.WebMappers = webMappers;
            config.WebProxies = webProxies;
            LJC.FrameWork.Comm.SerializerHelper.SerializerToXML(config, configfile);
        }
    }
}
