using LJC.FrameWork.Net.HTTP.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    public class APIHandler : IHttpHandler
    {

        internal Type _requestType = typeof(object);
        internal Type _responseType = typeof(object);
        internal Type _resultType = default(Type);
        internal Func<object, object> _func;
        internal string _ipLimit;

        internal APIMethodAttribute ApiMethodProp
        {
            get;
            set;
        }

        protected Exception GetInnerException(Exception ex)
        {
            if (ex == null || ex.InnerException == null)
                return ex;

            var innerex = ex.InnerException;
            while (innerex.InnerException != null)
            {
                innerex = innerex.InnerException;
            }

            return innerex;
        }

        public APIHandler(Type requestType, Type responseType, APIMethodAttribute _apimethod, Func<object, object> func)
        {
            _func = func;
            _requestType = requestType;
            _responseType = responseType;
            ApiMethodProp = _apimethod;

            _resultType = typeof(APIResult<>).MakeGenericType(_responseType);
        }

        internal APIHandler()
        {

        }

        public bool IsReusable
        {
            get { return false; }
        }

        protected object GetRequest(HttpRequest request, string context)
        {
            object requestobj = null;

            var data = request.Query.ContainsKey("data") ? request.Query["data"] : null;
            if (data != null)
            {
                //构造参数
                requestobj = DeSerializeObject(request, data);
            }
            else
            {
                requestobj = DeSerializeObject(request, context);
            }


            return requestobj;
        }

        protected object DeSerializeObject(HttpRequest request, object data)
        {
            SerType sertype = SerType.json;
            Enum.TryParse<SerType>(request.Query.ContainsKey("__sertype") ? request.Query["__sertype"] : string.Empty, out sertype);
            switch (sertype)
            {
                case SerType.protobuf:
                    var metype = typeof(ProtoBuf.Serializer);
                    var me = metype.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);
                    using (System.IO.MemoryStream ms = new MemoryStream((byte[])data))
                    {
                        return me.MakeGenericMethod(_requestType).Invoke(metype, new[] { ms });
                    }
                default:
                    {
                        string str = (data is byte[]) ? Encoding.Default.GetString((byte[])data) : (string)data;
                        return JsonConvert.DeserializeObject(str, _requestType);
                    }
            }
        }

        protected byte[] SerializeObject(HttpRequest request, HttpResponse response, object result)
        {
            SerType sertype = SerType.json;
            Enum.TryParse<SerType>(request.Query.ContainsKey("__sertype") ? request.Query["__sertype"] : string.Empty, out sertype);
            switch (sertype)
            {
                case SerType.protobuf:
                    {
                        using (System.IO.MemoryStream ms = new MemoryStream())
                        {
                            ProtoBuf.Serializer.Serialize(ms, result);
                            return ms.ToArray();
                        }
                    }
                default:
                    {
                        response.ContentType = "application/json";
                        using (StringWriter sw = new StringWriter())
                        {
                            using (JsonTextWriter writer = new JsonTextWriter(sw)
                            {
                                Formatting = Formatting.Indented,
                                Indentation = 4,
                                IndentChar = ' '
                            })
                            {
                                new Newtonsoft.Json.JsonSerializer().Serialize(writer, result);
                                //var str = JsonConvert.SerializeObject(result);
                                var str = sw.ToString();
                                return Encoding.Default.GetBytes(str);
                            }
                        }
                    }
            }
        }

        public virtual bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {

            Exception lastexp = null;

            int retCode = 1;
            string retMsg = string.Empty;
            object retResult = null;
            try
            {
                var req = GetRequest(request, request.Content);
                if (req == null)
                {
                    throw new ArgumentException("未传入请求参数");
                }
                retResult = _func(req);
            }
            catch (Exception ex)
            {
                retCode = 0;
                retMsg = GetInnerException(ex).Message;
                lastexp = ex;
            }

            object result = null;

            if (ApiMethodProp.OutPutContentType == OutPutContentType.apiObject)
            {
                result = retResult;
                response.RawContent = this.SerializeObject(request, response, result);
            }
            else if (ApiMethodProp.OutPutContentType == OutPutContentType.text)
            {
                response.Content = retResult.ToString();
            }
            else
            {
                result = Activator.CreateInstance(_resultType, retResult, retCode, retMsg);
                response.RawContent = this.SerializeObject(request, response, result);
            }


            //context.Response.Flush();

            //context.Response.End();

            if (lastexp != null)
            {
                response.ReturnCode = 500;
                throw lastexp;
            }
            response.ReturnCode = 200;
            return true;
        }
    }
}
