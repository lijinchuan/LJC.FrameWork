using LJC.FrameWork.Comm;
using LJC.FrameWork.HttpApi.EntityBuf;
using LJC.FrameWork.Net.HTTP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    public class APIJsonHandler : APIHandler
    {
        private APIHandler _hander;
        private string _apimethodname;

        public APIJsonHandler(string apimethodname, APIHandler hander)
        {
            this._apimethodname = apimethodname;
            this._hander = hander;
        }

        public override bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            string json = LocalCacheManager<string>.Find(_apimethodname + "_json", () =>
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<html>");
                sb.Append(@"<head><title>接口文档</title><style>.heading1{background:#003366;margin-top:0px; color:#fff; padding-top:5px; line-height:40px; width:100%;display:block;} table,pre{ background:#e5e5cc;} table{border-left:solid 1px #ccc;font-size:11px;width:98%;margin-left:5px;border-bottom:none;border-top:solid 1px #ccc;border-right:none;} td,th{border-top:none;border-right:solid 1px #ccc;border-bottom:solid 1px #ccc;} iframe{width:100%; background:#e5e5cc; margin-bottom:15px;}
                      </style></head>");
                sb.Append("<body style='width:100%;margin:0px;padding:0px;'>");
                sb.Append("<h1 class=\"heading1\">" + (this._hander.ApiMethodProp.Aliname ?? this._apimethodname) + "接口文档</h1>");
                sb.AppendFormat("<span class='title'>接口地址:</span><span style=\"color:blue;\"><a href=\"###\">{0}</a></span><br/><p/>", new Regex("/json$", RegexOptions.IgnoreCase).Replace(request.Url, ""));
                sb.AppendFormat("<span class='title'>接口功能:</span><span style=\"color:blue;\">{0}</span><br/><p/>", _hander.ApiMethodProp.Function ?? string.Empty);
                sb.Append("<span class='title'><font>接口数据序列化格式：json</font></span><br/><p/>");
                sb.AppendFormat("<span class='title'>接口请求json示例:</span><span style=\"color:blue;display:none;\"><a href=\"{0}\" target=\"_blank\">{0}</a></span><br/><p/>", new Regex("/json$", RegexOptions.IgnoreCase).Replace(request.Url, "") + "/req");

                sb.AppendFormat("<iframe id=\"req\" frameborder=\"no\" border=\"0\" marginwidth=\"0\" marginheight=\"0\" src=\"{0}/req\"></iframe>", new Regex("/json$", RegexOptions.IgnoreCase).Replace(request.Url, ""));

                sb.AppendFormat("<span class='title'>接口响应json示例:</span><span style=\"color:blue;display:none;\"><a href=\"{0}\" target=\"_blank\">{0}</a></span><br/><p/>", new Regex("/json$", RegexOptions.IgnoreCase).Replace(request.Url, "") + "/resp");

                sb.AppendFormat("<iframe id=\"resp\" frameborder=\"no\" border=\"0\" marginwidth=\"0\" marginheight=\"0\" src=\"{0}/resp\"></iframe>", new Regex("/json$", RegexOptions.IgnoreCase).Replace(request.Url, ""));

                sb.Append("<span class='title'>请求参数说明:</span><p/>");

                sb.Append("<table cellpadding=0 cellspacing=0 border=\"0\">");
                sb.AppendFormat("<tr><th>参数名</th><th>参数类型</th><th>备注</th></tr>");
                EntityBufCore.GetPropToTable(_hander._requestType, sb);
                sb.Append("</table>");
                //context.Response.Write(string.Format("{0}", sb.ToString()));

                sb.Append("<br/>");
                sb.Append("响应类型:<p/>");

                sb.Append("<table cellpadding=0 cellspacing=0 border=\"0\">");
                sb.AppendFormat("<tr><th>参数名</th><th>参数类型</th><th>备注</th></tr>");

                var apiType = typeof(APIResult<>);
                var apiResultType = (_hander.ApiMethodProp.OutPutContentType == OutPutContentType.apiObject || !_hander.ApiMethodProp.StandApiOutPut) ?
                    _hander._responseType : apiType.MakeGenericType(new[] { _hander._responseType });

                EntityBufCore.GetPropToTable(apiResultType, sb);
                sb.Append("</table>");

                sb.Append("<br/>");
                sb.AppendFormat("<a href=\"{0}\">接口调用测试</a>", new Regex("/json$", RegexOptions.IgnoreCase).Replace(request.Url, "") + "/invoke");

                if (!string.IsNullOrWhiteSpace(_ipLimit))
                {
                    sb.Append("<br/>");
                    sb.AppendFormat("<div style='font-weight:bold;color:red;'>ip限制:{0}</div>", _ipLimit);
                }

                sb.Append("<p/></p>");
                sb.Append("</body>");
                sb.Append("</html>");

                return sb.ToString();
            }, 1440);

            response.Content = json;
            response.ReturnCode = 200;

            return true;
        }
    }
}
