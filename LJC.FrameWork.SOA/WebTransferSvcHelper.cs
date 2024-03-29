﻿using LJC.FrameWork.SOA.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.SOA
{
    public static class WebTransferSvcHelper
    {
        public static WebMapper Find(WebRequest webRequest,List<WebMapper> webMappers)
        {
            WebMapper webMapper = null;
            foreach (var web in webMappers)
            {
                if (IsMatch(web, webRequest.Host, webRequest.VirUrl))
                {
                    webMapper = web;
                    break;
                }
            }

            return webMapper;

            bool IsMatch(WebMapper web, string host,string url)
            {
                if (web.MappingPort > 0)
                {
                    var hostAndPort = host.Split(':');
                    var port = 80;
                    if (hostAndPort.Length == 2)
                    {
                        port = int.Parse(hostAndPort[1]);
                    }
                    if (web.MappingPort != port)
                    {
                        return false;
                    }
                    if (web.MappingPort == port && string.IsNullOrWhiteSpace(web.MappingRoot))
                    {
                        return true;
                    }
                }
                if (!string.IsNullOrWhiteSpace(web.MappingRoot))
                {
                    var mappingRoot = web.MappingRoot;
                    if (!mappingRoot.EndsWith("/"))
                    {
                        mappingRoot += "/";
                    }
                    return (url.Split('?').First()+"/").StartsWith(mappingRoot, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }
        }

        public static string RelaceLocation(string location,string requestHost,string realUrl)
        {
            var targetHost = new Uri(realUrl).Host;
            var port = new Uri(realUrl).Port;
            var targetHostAndPort = targetHost + ":" + port;

            var tranferHost = requestHost;
            if (location.Contains(targetHostAndPort))
            {
                return location.Replace(targetHostAndPort, tranferHost);
            }
            else if (location.Contains(targetHost))
            {
                return location.Replace(targetHost, tranferHost);
            }
            return location;
        }
    }
}
