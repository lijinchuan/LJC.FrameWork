using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ProtoBuf;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.WebApi
{
    public class APIClient
    {
        public static T GetResponse<T>(string url,object request)
        {
            HttpRequestEx httprequest = new HttpRequestEx();
            string data = string.Empty;

            if (request != null)
            {
                data = JsonConvert.SerializeObject(request);
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("data", data);
            var response = httprequest.DoFormRequest(url, dic);

            if (response.Successed)
            {
                return JsonConvert.DeserializeObject<T>(response.ResponseContent);
            }

            return default(T);
        }

        public static APIResult<T> GetAPIResponse<T>(string url,object request)
        {
            try
            {
                HttpRequestEx httprequest = new HttpRequestEx();
                string data = string.Empty;

                if (request != null)
                {
                    data = JsonConvert.SerializeObject(request);
                }
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("data", data);
                var response = httprequest.DoFormRequest(url, dic);

                if (response.Successed)
                {
                    return JsonConvert.DeserializeObject<APIResult<T>>(response.ResponseContent);
                }
                else
                {
                    return new APIResult<T>
                    {
                        ResponseBody = default(T),
                        ResultCode = 0,
                        ResultMessage = response.ErrorMsg.Message,
                    };
                }
            }
            catch(Exception ex)
            {
                return new APIResult<T>
                {
                    ResponseBody = default(T),
                    ResultCode = 0,
                    ResultMessage = ex.Message,
                };
            }
        }

        public static APIResult<T> GetAPIProtobuf<T>(string url,object request)
        {
            try
            {
                url = url + "?__sertype=" + SerType.protobuf.ToString();
                HttpRequestEx httprequest = new HttpRequestEx();
                byte[] buff = null;
                if (request != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.Serialize(ms, request);
                        buff = ms.ToArray();
                    }
                }
                var response = httprequest.DoRequest(url, buff);

                if (response.Successed)
                {
                    using (MemoryStream ms = new MemoryStream(response.ResponseBytes))
                    {
                        APIResult<T> result = ProtoBuf.Serializer.Deserialize<APIResult<T>>(ms);

                        return result;
                    }
                }
                else
                {
                    return new APIResult<T>
                    {
                        ResponseBody = default(T),
                        ResultCode = 0,
                        ResultMessage = response.ErrorMsg.Message,
                    };
                }
            }
            catch (Exception ex)
            {
                return new APIResult<T>
                {
                    ResponseBody = default(T),
                    ResultCode = 0,
                    ResultMessage = ex.Message,
                };
            }
        }
    }
}
