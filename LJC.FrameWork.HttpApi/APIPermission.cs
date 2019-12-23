using LJC.FrameWork.Comm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    internal class APIPermission
    {
        string _ipsConfig = string.Empty;
        public APIPermission(string ipsConfig)
        {
            //var ips = ConfigHelper.AppConfig(ipsConfig);
            _ipsConfig = ipsConfig;
        }

        static long IP2Long(string ip)
        {
            string[] ipBytes;
            double num = 0;
            if (!string.IsNullOrEmpty(ip))
            {
                ipBytes = ip.Split('.');
                for (int i = ipBytes.Length - 1; i >= 0; i--)
                {
                    num += ((int.Parse(ipBytes[i]) % 256) * Math.Pow(256, (3 - i)));
                }
            }
            return (long)num;
        }

        public bool CheckPermission(string ip)
        {
            var ips = ConfigHelper.AppConfig(_ipsConfig);
            if (string.IsNullOrEmpty(ips))
                return true;

            foreach (var subip in ips.Split(','))
            {
                var subip2 = subip.Split('-');
                if (subip2.Length == 1)
                {
                    if (ip.Equals(subip2[0]))
                        return true;
                }
                else if (subip2.Length == 2)
                {
                    var iplong = IP2Long(ip);
                    if (iplong >= IP2Long(subip2[0]) && iplong <= IP2Long(subip2[1]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
