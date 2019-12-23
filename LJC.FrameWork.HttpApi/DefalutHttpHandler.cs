using LJC.FrameWork.Net.HTTP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    internal class DefalutHttpHandler : IHttpHandler
    {
        private Func<HttpRequest, IHttpHandler> handler;

        public DefalutHttpHandler(Func<HttpRequest, IHttpHandler> _handler)
        {
            this.handler = _handler;
        }

        public bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            var h = handler(request);
            bool boo = false;
            try
            {
                boo = h != null && h.Process(server, request, response);

            }
            catch (Exception ex)
            {

                new ErrorHandler(ex).Process(server, request, response);
                boo = true;
            }
            if (boo)
            {
                response.Header["charset"] = "utf-8";
                response.ContentType += ";charset=utf-8";
            }
            return boo;
        }
    }
}
