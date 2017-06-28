using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LJC.FrameWork.Web
{
    public static class HttpUtil
    {
        const string HTTP_X_FORWARDED_FOR = "HTTP_X_FORWARDED_FOR";
        const string REMOTE_ADDR = "Remote_Addr";

        public static string GetRemoteIp()
        {
            var httpcontext = System.Web.HttpContext.Current;
            if (httpcontext == null)
            {
                return string.Empty;
            }

            string ip = string.Empty;
            var httpforwared = httpcontext.Request.ServerVariables.Get(HTTP_X_FORWARDED_FOR);
            if (httpforwared != null)
            {
                ip = httpforwared.ToString().Trim();
            }
            else
            {
                ip = httpcontext.Request.ServerVariables.Get(REMOTE_ADDR).ToString().Trim();
            }

            return ip;
        }

        public static string GetRemoteIp(HttpContext httpcontext)
        {
            if (httpcontext == null)
            {
                return string.Empty;
            }

            string ip = string.Empty;
            var httpforwared = httpcontext.Request.ServerVariables.Get(HTTP_X_FORWARDED_FOR);
            if (httpforwared != null)
            {
                ip = httpforwared.ToString().Trim();
            }
            else
            {
                ip = httpcontext.Request.ServerVariables.Get(REMOTE_ADDR).ToString().Trim();
            }

            return ip;
        }
    }
}
