using LJC.FrameWork.SOA.Contract;
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
                if (IsMatch(web, webRequest.Url))
                {
                    webMapper = web;
                    break;
                }
            }

            if (webMapper == null)
            {
                var refer = webRequest.Headers?.FirstOrDefault(p => p.Key.Equals("Referer", StringComparison.OrdinalIgnoreCase));
                if (refer != null && refer.Value.Key != null)
                {
                    var referUrl = refer.Value.Value;
                    //if (referUrl.Equals("http://localhost:8081/"))
                    //{
                    //    referUrl += "quancheng";
                    //}
                    var reliUrl = string.Join("/", referUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Skip(2));

                    foreach (var web in webMappers)
                    {
                        if (IsMatch(web, reliUrl))
                        {
                            webMapper = web;
                            break;
                        }
                    }
                }
            }

            return webMapper;

            bool IsMatch(WebMapper web,string url)
            {
                if (!string.IsNullOrWhiteSpace(web.RegexRoute) && Regex.IsMatch(url, web.RegexRoute, RegexOptions.IgnoreCase))
                {
                    return true;
                }
                else if (url.StartsWith(web.VirRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
