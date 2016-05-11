using LJC.FrameWork.Comm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.WebApi
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

        public override void ProcessRequest(System.Web.HttpContext context)
        {
            string json = LocalCacheManager<string>.Find(_apimethodname + "_json", () =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("接口地址:<span style=\"color:blue;\"><a href=\"###\">{0}</a></span><br/><p/>", new Regex("/json$", RegexOptions.IgnoreCase).Replace(context.Request.Url.AbsolutePath, ""));
                sb.AppendFormat("接口功能:<span style=\"color:blue;\">{0}</span><br/><p/>", _hander.ApiMethodProp.Function ?? string.Empty);
                sb.Append("<font>接口数据序列化格式：json</font><br/><p/>");
                sb.Append("请求参数:<br/>");

                sb.Append("<table style=\"border:solid 1px yellow;\" border=\"1\">");
                sb.AppendFormat("<tr><th>参数名</th><th>参数类型</th><th>备注</th></tr>");
                EntityBufCore.GetPropToTable(_hander._requestType, sb);
                sb.Append("</table>");
                //context.Response.Write(string.Format("{0}", sb.ToString()));

                sb.Append("<br/>");
                sb.Append("响应类型:<br/>");

                sb.Append("<table style=\"border:solid 1px yellow;\" border=\"1\">");
                sb.AppendFormat("<tr><th>参数名</th><th>参数类型</th><th>备注</th></tr>");

                var apiType = typeof(APIResult<>);
                var apiResultType = (_hander.ApiMethodProp.OutPutContentType == OutPutContentType.apiObject || !_hander.ApiMethodProp.StandApiOutPut) ?
                    _hander._responseType : apiType.MakeGenericType(new[] { _hander._responseType });

                EntityBufCore.GetPropToTable(apiResultType, sb);
                sb.Append("</table>");

                if (!string.IsNullOrWhiteSpace(_ipLimit))
                {
                    sb.Append("<br/>");
                    sb.AppendFormat("<div style='font-weight:bold;color:red;'>ip限制:{0}</div>", _ipLimit);
                }

                return sb.ToString();
            }, 1440);

            context.Response.Write(json);
        }
    }
}
