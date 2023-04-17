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
            server.RequestSession(request).Touch();

            var url = request.Url;
            if (url.StartsWith("http",StringComparison.OrdinalIgnoreCase))
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

            if (request.Url.EndsWith("/"))
            {
                url += "/";
            }

            var ipHeader = "X-Forwarded-For";
            if (request.Header.ContainsKey(ipHeader))
            {
                request.Header[ipHeader] = request.From.ToString() + "," + request.Header[ipHeader];
            }
            else
            {
                request.Header.Add(ipHeader, request.From.ToString());
            }
            var simulateResponse = SimulateServerManager.TransferRequest(new Contract.WebRequest
            {
                Host = request.Host,
                VirUrl = url,
                Cookies = request.Cookies,
                Headers = request.Header,
                Method = request.Method,
                InputData = request.RawData
            });
            if (simulateResponse != null)
            {
                response.Header = simulateResponse.Headers??new Dictionary<string, string>();
                response.RawContent = simulateResponse.ResponseData;
                if (!string.IsNullOrWhiteSpace(simulateResponse.ContentType))
                {
                    response.ContentType = simulateResponse.ContentType;
                }
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
