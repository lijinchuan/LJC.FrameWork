using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace LJC.FrameWork.Comm
{
    /// <summary>
    /// httpClient工厂
    /// </summary>
    public static class HttpClientFactory
    {
        private static ConcurrentDictionary<string, HttpClient> httpClients = new ConcurrentDictionary<string, HttpClient>();

        /// <summary>
        /// 获取一个httpClient实例，不要释放它
        /// </summary>
        /// <param name="address">地址，大小写没关系</param>
        /// <returns></returns>
        public static HttpClient GetHttpClient(string address, bool allowAutoRedirect = true)
        {
            var uri = new Uri(address);
            
            var host = uri.Host.ToLower() + "," + allowAutoRedirect;
            if (httpClients.TryGetValue(host, out HttpClient httpClient))
            {
                return httpClient;
            }
            var newClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = allowAutoRedirect,
                //UseProxy=false
            });
            if (httpClients.TryAdd(host, newClient))
            {
                return newClient;
            }
            else
            {
                newClient.Dispose();
                if (httpClients.TryGetValue(host, out HttpClient httpClient1))
                {
                    return httpClient1;
                }

                throw new Exception("获取httpclient失败");
            }
        }
    }
}
