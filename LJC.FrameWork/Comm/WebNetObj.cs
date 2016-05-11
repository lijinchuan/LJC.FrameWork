using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace LJC.FrameWork.Comm
{
    [Obsolete("请用HttpRequestEx代替")]
    public class WebNetObj
    {

        //private WebRequestMethodEnum _webReqMethod;
        ///// <summary>
        ///// 表单提交类型
        ///// </summary>
        //public string Method
        //{
        //    get
        //    {
        //        return _webReqMethod.ToString();
        //    }
        //}

        public string Accept = "text/html, application/xhtml+xml, */*";
        public string UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0; BOIE9;ZHCN)";
        public string Referer;
        /// <summary>
        /// 本地保存的cookie，提交到服务器的cookie
        /// </summary>
        private CookieContainer clientCookie;

        private Encoding _encoding;
        /// <summary>
        /// 页面编码
        /// </summary>
        public Encoding WebEncoding
        {
            get
            {
                return _encoding;
            }
        }

        /// <summary>
        /// 查询服务器返回的cookie值 
        /// </summary>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public string GetCookieValue(string cookieName)
        {
            Cookie c= Cookies.Find(k => k.Name == cookieName);

            if (c != null)
                return c.Value;

            return "";
        }


        private List<Cookie> _cookies=new List<Cookie>();
        public List<Cookie> Cookies
        {
            get
            {
                return _cookies;
            }
        }

        /// <summary>
        /// 取上网代理
        /// </summary>
        /// <returns></returns>
        private NetworkCredential GetCredential()
        {
            string user = ConfigHelper.AppConfig("NetWorkUsername");
            string pwd = ConfigHelper.AppConfig("NetWorkPwd");
            string domain = ConfigHelper.AppConfig("NetWorkDomain");

            if (user != string.Empty)
            {
                return new NetworkCredential(user, pwd, domain);
            }

            return null;
        }

        public bool GetWebContent(string url,out string html)
        {
            html=string.Empty;
            try
            {
                html = GetWebContent(url,null);
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(html))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 取网页源码
        /// </summary>
        /// <returns></returns>
        public string GetWebContent(string url,string data,WebRequestMethodEnum method=WebRequestMethodEnum.GET)
        {

            string result=string.Empty;
            //string requestUrl=CombURL(_baseUrl, url);
            
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            if (!string.IsNullOrWhiteSpace(data)&&method==WebRequestMethodEnum.GET)
            {
                webRequest.Method = WebRequestMethodEnum.POST.ToString();
            }
            else
            {
                webRequest.Method = method.ToString();
            }

            webRequest.Accept = Accept;
            webRequest.AllowAutoRedirect = true;
            webRequest.KeepAlive = true;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = UserAgent;
            if (!string.IsNullOrEmpty(Referer))
                webRequest.Referer = Referer;


            NetworkCredential credential = GetCredential();
            if (credential != null)
                webRequest.Proxy.Credentials = credential;

            if (clientCookie != null)
                webRequest.CookieContainer = clientCookie;
            else
                webRequest.CookieContainer = new CookieContainer();

            if (!string.IsNullOrWhiteSpace(data))
            {
                byte[] buff = this.WebEncoding.GetBytes(data);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = buff.Length;

                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(buff, 0, buff.Length);
                }
            }

            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream s = webResponse.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(s, WebEncoding))
                        {
                            result = sr.ReadToEnd();
                        }
                    }

                    if (webResponse.Cookies.Count > 0)
                    {
                        if (clientCookie == null)
                            clientCookie = new CookieContainer();
                        for (int i = 0; i < webResponse.Cookies.Count; i++)
                        {
                            //serverCookie.Add(webResponse.Cookies[i]);
                            if (!_cookies.Exists(p => p.Name.Equals(webResponse.Cookies[i].Name)))
                            {
                                _cookies.Add(webResponse.Cookies[i]);
                                clientCookie.Add(webResponse.Cookies[i]);
                            }
                            else
                            {
                                _cookies.RemoveAll(p => p.Name.Equals(webResponse.Cookies[i].Name));
                                clientCookie.Add(webResponse.Cookies[i]);
                            }
                        }
                    }
                }
            }


            return result;
        }

        /// <summary>
        /// 返回请求的数据流，主要用于读取网络图片等
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Stream GetStream(string url)
        {
            MemoryStream m=new MemoryStream();
             HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));

             HttpWebResponse response=(HttpWebResponse) webRequest.GetResponse();

            if(response.StatusCode==HttpStatusCode.OK)
            {
               return response.GetResponseStream();
            }

            return null;
        }


        public WebNetObj(CookieContainer cliCookie=null,WebRequestMethodEnum method=WebRequestMethodEnum.POST,Encoding encoding=null)
        {
            if (encoding == null)
                encoding = Encoding.GetEncoding("GB2312");

            _encoding = encoding;

            clientCookie = cliCookie;

            _cookies = new List<Cookie>();
        }
    }
}
