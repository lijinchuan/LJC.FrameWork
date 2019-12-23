using LJC.FrameWork.Comm;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.Net.HTTP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    public class ApiGenRespHandler : APIHandler
    {
        private APIHandler _hander;
        private string _apimethodname;

        public ApiGenRespHandler(string apimethodname, APIHandler hander)
        {
            this._apimethodname = apimethodname;
            this._hander = hander;
        }

        public override bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            string json = string.Empty;
            StringBuilder sb = new StringBuilder();

            var apiType = typeof(APIResult<>);
            var apiResultType = (_hander.ApiMethodProp.OutPutContentType == OutPutContentType.apiObject || !_hander.ApiMethodProp.StandApiOutPut) ?
                _hander._responseType : apiType.MakeGenericType(new[] { _hander._responseType });

            json = JsonUtil<object>.Serialize(EntityBufCore.DeSerialize(apiResultType, EntityBufCoreEx.GenSerialize(apiResultType), false), true);
            sb.Append("<html>");
            sb.Append("<body>");
            sb.Append("<pre>" + json + "</pre>");

            sb.Append("</body>");
            sb.Append(@"<script>
                        var ifr=parent&&parent.document.getElementById('resp');
                        if(ifr)
                        {
                           ifr.height=document.body.scrollHeight;
                        }
                       </script>");
            sb.Append("</html>");

            response.Content = sb.ToString();
            response.ReturnCode = 200;

            return true;
        }

    }
}
