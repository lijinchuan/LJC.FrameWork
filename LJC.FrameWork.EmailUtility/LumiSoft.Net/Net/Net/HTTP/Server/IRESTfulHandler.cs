using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Net.HTTP.Server
{
    public interface IRESTfulHandler
    {
        bool Process(HttpServer server, HttpRequest request, HttpResponse response, Dictionary<string, string> param);
    }
}
