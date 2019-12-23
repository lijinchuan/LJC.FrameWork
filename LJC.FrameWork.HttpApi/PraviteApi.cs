using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    class PraviteApi
    {
        [APIMethod(OutPutContentType = OutPutContentType.text, IsVisible = false)]
        public string ApiList()
        {
            string baseUrl = string.Empty;
            //if (System.Web.HttpContext.Current.Request.Url != null)
            //{
            //    baseUrl = System.Web.HttpContext.Current.Request.ApplicationPath;
            //}
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html>");
            sb.Append("<head><title>接口列表</title><style>.heading1{background:#003366;margin-top:0px; color:#fff; padding-top:5px; line-height:40px; width:100%;display:block;} table,pre{ background:#e5e5cc;} table{border-left:solid 1px #ccc;border-bottom:none;border-top:solid 1px #ccc;border-right:none;} td,th{border-top:none;border-right:solid 1px #ccc;border-bottom:solid 1px #ccc;}</style></head>");
            //sb.Append("<HEAD><link rel=\"alternate\" type=\"text/xml\" href=\"##\"/><STYLE type=\"text/css\">#content{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}BODY{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}P{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}PRE{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}.heading1{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #003366}.intro{MARGIN-LEFT: -15px}</STYLE><TITLE>接口文档</TITLE></HEAD>");
            sb.Append("<body style='width:100%;margin:0px;'>");
            sb.Append("<h1 class=\"heading1\">接口列表</h1>");
            sb.Append("<ul>");
            foreach (var gp in APIFactory.apigplist)
            {
                if (gp.Key.Equals("PraviteApi"))
                {
                    continue;
                }
                sb.Append("<p><h2>" + gp.Key + "</h2></p>");
                foreach (var api in gp.Value)
                {
                    if (!api.ApiMethodProp.IsVisible)
                    {
                        continue;
                    }

                    sb.AppendFormat("<li><a href='{0}/json' target='_blank'>{0}</a>&nbsp;&nbsp;<span style=\"color:#a0a0a0;font-size:12px;font-style:italic;\">{1}</span></li>", api.ApiMethodProp.Aliname ?? api.ApiMethodProp.MethodName, api.ApiMethodProp.Function);
                }
            }
            sb.AppendFormat("</body></ul>");

            return sb.ToString();
        }

        [APIMethod(OutPutContentType = OutPutContentType.text, IsVisible = false)]
        public string HelloApi()
        {
            return "hello";
        }
    }
}
