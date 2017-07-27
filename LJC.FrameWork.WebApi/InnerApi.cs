using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.WebApi
{
    internal class PraviteApi
    {
        [APIMethod(OutPutContentType = OutPutContentType.text,IsVisible=false)]
        public string ApiList()
        {
            string baseUrl = string.Empty;
            if(System.Web.HttpContext.Current.Request.Url!=null)
            {
                baseUrl = System.Web.HttpContext.Current.Request.ApplicationPath;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head>接口列表</head><body><ul>");
            foreach (var api in APIFactory.apiFunMapper)
            {
                if (!api.Value.ApiMethodProp.IsVisible)
                {
                    continue;
                }

                sb.AppendFormat("<li><a href='{0}/api/{1}/json' target='_blank'>{1}</a>&nbsp;&nbsp;<span style=\"color:#a0a0a0;font-size:12px;font-style:italic;\">{2}</span></li>", baseUrl, api.Key, api.Value.ApiMethodProp.Function);
            }
            sb.AppendFormat("</body></ul>");

            return sb.ToString();
        }

        [APIMethod(OutPutContentType=OutPutContentType.text,IsVisible=false)]
        public string HelloApi()
        {
            return "hello";
        }
    }
}
