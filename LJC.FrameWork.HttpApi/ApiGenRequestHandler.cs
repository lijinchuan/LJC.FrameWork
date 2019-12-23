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
    public class ApiGenRequestHandler : APIHandler
    {
        private APIHandler _hander;
        private string _apimethodname;

        public ApiGenRequestHandler(string apimethodname, APIHandler hander)
        {
            this._apimethodname = apimethodname;
            this._hander = hander;
        }

        public override bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            string json = string.Empty;

            var apiType = typeof(APIResult<>);

            var apiResultType = _hander._requestType;

            json = JsonUtil<object>.Serialize(EntityBufCore.DeSerialize(apiResultType, EntityBufCoreEx.GenSerialize(apiResultType), false), true);
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<body>");
            sb.Append("<pre>" + json + "</pre>");


            sb.Append("</body>");
            sb.Append(@"<script>
                           var ifr=parent&&parent.document.getElementById('req');
                           if(ifr)
                           {
                              ifr.height=document.body.scrollHeight;
                           }
                     </script>");

            response.Content = sb.ToString();
            response.ReturnCode = 200;

            return true;
        }
    }
}
