using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.IO;
using ProtoBuf;
using System.Reflection;

namespace LJC.FrameWork.WebApi
{
    public class APIHandler:IHttpHandler
    {
        internal Type _requestType = typeof(object);
        internal Type _responseType = typeof(object);
        internal Type _resultType=default(Type);
        internal Func<object,object> _func;
        internal string _ipLimit;

        internal APIMethod ApiMethodProp
        {
            get;
            set;
        }

        protected Exception GetInnerException(Exception ex)
        {
            if (ex == null || ex.InnerException == null)
                return ex;

            var innerex = ex.InnerException;
            while(innerex.InnerException!=null)
            {
                innerex = ex.InnerException;
            }

            return innerex;
        }

        public APIHandler(Type requestType, Type responseType, APIMethod _apimethod, Func<object, object> func)
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
            get { throw new NotImplementedException(); }
        }

        public virtual void ProcessRequest(HttpContext context)
        {
            Exception lastexp = null;

            int retCode = 1;
            string retMsg = string.Empty;
            object retResult = null;
            try
            {
                object request = GetRequest(context);

                if(request==null)
                {
                    throw new ArgumentException("未传入请求参数");
                }
                retResult = _func(request);
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
                context.Response.BinaryWrite(this.SerializeObject(context, result));
            }
            else if (ApiMethodProp.OutPutContentType == OutPutContentType.text)
            {
                context.Response.Write(retResult.ToString());
            }
            else
            {
                result = Activator.CreateInstance(_resultType, retResult, retCode, retMsg);
                context.Response.BinaryWrite(this.SerializeObject(context, result));
            }
            
            context.Response.Flush();

            context.Response.End();
            if (lastexp != null)
            {
                throw lastexp;
            }

        }

        protected object GetRequest(HttpContext context)
        {
            object request = null;

            var data = context.Request.Form["data"];
            if (data != null)
            {
                //构造参数
                request = DeSerializeObject(context, data);
            }
            else if (context.Request.InputStream.Length > 0)
            {
                byte[] buff = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    context.Request.InputStream.CopyTo(ms);
                    buff = ms.ToArray();
                }
                request = DeSerializeObject(context, buff);
            }
            else
            {
                if (_requestType.IsClass)
                {
                    StringBuilder sb = new StringBuilder();
                    StringWriter sw = new StringWriter(sb);
                    Newtonsoft.Json.JsonTextWriter writer = new JsonTextWriter(sw);
                    writer.WriteStartObject();

                    foreach (var key in context.Request.QueryString.AllKeys)
                    {
                        writer.WritePropertyName(key);
                        writer.WriteValue(context.Request.QueryString[key]);
                    }

                    writer.WriteEndObject();

                    request = JsonConvert.DeserializeObject(sb.ToString(), _requestType);
                }
                else
                {
                    if (context.Request.QueryString.AllKeys.Length != 1)
                    {
                        throw new ArgumentException("参数数目不匹配");
                    }

                    request = context.Request.QueryString[0];
                }
            }
            return request;
        }

        protected object DeSerializeObject(HttpContext context,object data)
        {
            SerType sertype = SerType.json;
            Enum.TryParse<SerType>(context.Request.QueryString["__sertype"], out sertype);
            switch (sertype)
            {
                case SerType.protobuf:
                    var metype=typeof(ProtoBuf.Serializer);
                    var me = metype.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);
                    using (System.IO.MemoryStream ms = new MemoryStream((byte[])data))
                    {
                        return me.MakeGenericMethod(_requestType).Invoke(metype, new[] { ms });
                    }
                default:
                    {
                        string str =(data is byte[])? Encoding.UTF8.GetString((byte[])data):(string)data;
                        return JsonConvert.DeserializeObject(str, _requestType);
                    }
            }
        }

        protected byte[] SerializeObject(HttpContext context,object result)
        {
            SerType sertype=SerType.json;
            Enum.TryParse<SerType>(context.Request.QueryString["__sertype"], out sertype);
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
                        context.Response.Charset = "utf-8";
                        context.Response.ContentType = "application/json";
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
                                return Encoding.UTF8.GetBytes(str);
                            }
                        }
                    }
            }
        }
    }
}
