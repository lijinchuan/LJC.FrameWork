using LJC.FrameWork.Data.EntityDataBase;
using LJC.FrameWork.Net.HTTP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    internal class ApiSimulateHandler : IHttpHandler
    {
        public bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            var url = request.Url.ToLower();
            if (url.StartsWith("http"))
            {
                var sqlArray = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (sqlArray.Length > 2)
                {
                    url = string.Join("/", sqlArray.Skip(2).ToArray());
                }
            }
            else
            {
                url = string.Join("/", url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            }
            
            var simulateResponse = SimulateServerManager.TransferRequest(new Contract.WebRequest
            {
                Host=request.Host,
                VirUrl=url,
                Cookies=request.Cookies,
                Headers=request.Header,
                Method=request.Method,
                InputData=Encoding.UTF8.GetBytes(request.Content)
            });
            if (simulateResponse != null)
            {
                response.Header = simulateResponse.Headers;
                response.RawContent = simulateResponse.ResponseData;
                
                response.ContentType = simulateResponse.ContentType;
                response.ReturnCode = simulateResponse.ResponseCode;
                response.Url = simulateResponse.Url;

                return true;
            }
            else
            {
                response.ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8");
                response.ReturnCode = 404;
                response.Content = request.Url + " 不存在！";

                return true;
            }
        }
    }
}
