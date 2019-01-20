using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace LJC.FrameWork.SOA
{
    [Serializable]
    public class ESBConfig
    {
        private static string configfile = LJC.FrameWork.Comm.CommFun.GetRuningPath() + "ESBConfig.xml";

        public string ESBServer
        {
            get;
            set;
        }

        public int ESBPort
        {
            get;
            set;
        }

        public bool IsSecurity
        {
            get;
            set;
        }

        public bool AutoStart
        {
            get;
            set;
        }

        private static ESBConfig _esbConfig = null;
        public static ESBConfig ReadConfig()
        {
            if (_esbConfig != null)
                return _esbConfig;
            if(!File.Exists(configfile))
            {
                string configfile2 = AppDomain.CurrentDomain.BaseDirectory + "ESBConfig.xml";
                if (!File.Exists(configfile2))
                {
                    throw new Exception(string.Format("未找到ESBConfig配置文件，路径：{0} 和路径 {1}", configfile, configfile2));
                }
                else
                {
                    configfile = configfile2;
                }
            }


            _esbConfig= LJC.FrameWork.Comm.SerializerHelper.DeSerializerFile<ESBConfig>(configfile,true);
            if (_esbConfig.ESBServer.IndexOf('.') == -1
                &&_esbConfig.ESBServer.IndexOf(':')==-1)
            {
                var ipaddress = System.Net.Dns.GetHostAddresses(_esbConfig.ESBServer);
                if (ipaddress == null)
                {
                    throw new Exception("配置服务地址无效。");
                }

                _esbConfig.ESBServer = ipaddress.FirstOrDefault(p => p.AddressFamily != AddressFamily.InterNetworkV6).ToString();
            }

            return _esbConfig;
        }

        public static void WriteConfig(string esbServer,int esbPort,bool autoStrat=false)
        {
            ESBConfig config=new ESBConfig();
            config.ESBServer=esbServer;
            config.ESBPort=esbPort;
            config.AutoStart = autoStrat;
            LJC.FrameWork.Comm.SerializerHelper.SerializerToXML(config,configfile);
        }
    }
}
