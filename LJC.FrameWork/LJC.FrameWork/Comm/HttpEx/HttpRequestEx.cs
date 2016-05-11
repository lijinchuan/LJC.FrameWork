using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace LJC.FrameWork.Comm
{
    [Serializable]
    public class HttpRequestEx
    {
        #region hreader相关
        private string _accept = "text/html, application/xhtml+xml, */*";
        public string Accept
        {
            get
            {
                return _accept;
            }
            set
            {
                _accept = value;
            }
        }

        private string _userAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0; BOIE9;ZHCN)";
        public string UserAgent
        {
            get
            {
                return _userAgent;
            }
            set
            {
                _userAgent = value;
            }
        }

        private string acceptLanguage = "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3";
        public string AcceptLanguage
        {
            get
            {
                return acceptLanguage;
            }
            set
            {
                acceptLanguage = value;
            }
        }

        private string _refer = string.Empty;
        public string Referer
        {
            get
            {
                return _refer;
            }
            set
            {
                _refer = value;
            }
        }
        #endregion

        #region 编码方式
        private Encoding _encoding=Encoding.UTF8;
        /// <summary>
        /// 页面编码
        /// </summary>
        public Encoding WebEncoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                _encoding = value;
            }
        }

        #endregion

        #region cookie相关

        /// <summary>
        /// 服务器给本地的cookie
        /// </summary>
        private CookieContainer serverCookie;

        public void AppendCookie(string cookieName,string cookieValue,string domain,string path)
        {
            if (serverCookie == null)
            {
                serverCookie = new CookieContainer();
                serverCookie.PerDomainCapacity = 255;
            }
            serverCookie.Add(new Cookie
            {
               Name=cookieName,
               Value=cookieValue,
               Domain=domain,
               Path=path
            });
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
        /// 查询服务器返回的cookie值 
        /// </summary>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public string GetCookieValue(string cookieName)
        {
            Cookie c = Cookies.Find(k => k.Name == cookieName);

            if (c != null)
                return c.Value;

            return "";
        }
        #endregion

        #region 上网代理设置
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

        #endregion

        #region 证书
        private X509Certificate _certificate = null;
        /// <summary>
        /// 证书路径
        /// </summary>
        public string CertificatePath
        {
            set
            {
                _certificate = new X509Certificate(value);
            }
        }

        public static IEnumerable<X509Certificate> EnumCerts()
        {
            ClientCert.ClientCerts mycerts = new ClientCert.ClientCerts();
            int num=mycerts.Init();
            for (int i = 0; i < num; i++)
            {
                yield return mycerts[i];
            }
        }
        #endregion

        #region 上网方法
        private List<string> _domain = new List<string>();
        public List<string> Domain
        {
            get
            {
                return _domain;
            }
        }

        private bool CheckIsInDomain(string url)
        {
            if (_domain.Count == 0)
                return true;

            string host = new Uri(url).Host;
            foreach (string s in _domain)
            {
                if(host.IndexOf(s)>-1)
                    return true;
            }

            return false;
        }

        public HttpResponseEx DoFormRequest(string url, Dictionary<string, string> form, bool ignoreConflict=true)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var k in form.Keys)
            {
                if (!ignoreConflict)
                {
                    sb.AppendFormat("{0}={1}&", k.Replace("/", ""), WebUtility.UrlEncode(form[k]));
                }
                else
                {
                    if (k.EndsWith("/"))
                    {
                        continue;
                    }
                    sb.AppendFormat("{0}={1}&", k, WebUtility.UrlEncode(form[k]));
                }
            }
            return DoRequest(url, sb.ToString());
        }

        /// <summary>
        /// post方法发送数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="saveCookie">是否保存cookie</param>
        /// <param name="getContent">是否读取内容</param>
        /// <returns></returns>
        public HttpResponseEx DoRequest(string url, string data, WebRequestMethodEnum method = WebRequestMethodEnum.GET, bool saveCookie = true,bool getContent=true)
        {
            var ret = new HttpResponseEx();
            try
            {
                if (!CheckIsInDomain(url))
                    throw new Exception(string.Format("拒绝访问，地址{0}不在可访问的域名之列。",url));

                string result = string.Empty;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                if (data != null && data.Length > 0)
                    webRequest.Method = WebRequestMethodEnum.POST.ToString();

                webRequest.Accept = Accept;
                webRequest.AllowAutoRedirect = true;
                webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, this.acceptLanguage);
                webRequest.KeepAlive = true;
                webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                webRequest.UserAgent = UserAgent;
                if (!string.IsNullOrEmpty(Referer))
                    webRequest.Referer = Referer;

                NetworkCredential credential = GetCredential();
                if (credential != null)
                    webRequest.Proxy.Credentials = credential;

                if (serverCookie != null)
                    webRequest.CookieContainer = serverCookie;
                else
                    webRequest.CookieContainer = new CookieContainer();

                if (_certificate != null)
                {
                    webRequest.UserAgent = "Client Cert Sample";
                    webRequest.ClientCertificates.Add(_certificate);
                }

                if (!string.IsNullOrWhiteSpace(data))
                {
                    byte[] buff = this.WebEncoding.GetBytes(data);
                    webRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8;";
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
                        ret.PraseHeader(webResponse);

                        if (getContent)
                        {
                            try
                            {
                                WebEncoding = Encoding.GetEncoding(webResponse.CharacterSet);
                            }
                            catch
                            {
                                WebEncoding = Encoding.GetEncoding("GB2312");
                            }
                            using (Stream s = webResponse.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(s, WebEncoding))
                                {
                                    result = sr.ReadToEnd();
                                }
                            }
                        }

                        if (saveCookie && webResponse.Cookies.Count > 0)
                        {
                            if (serverCookie == null)
                                serverCookie = new CookieContainer();
                            serverCookie.PerDomainCapacity = 255;
                            for (int i = 0; i < webResponse.Cookies.Count; i++)
                            {
                                if (!_cookies.Exists(p => p.Name.Equals(webResponse.Cookies[i].Name)))
                                {
                                    _cookies.Add(webResponse.Cookies[i]);
                                    serverCookie.Add(webResponse.Cookies[i]);
                                }
                                else
                                {
                                    _cookies.RemoveAll(p => p.Name.Equals(webResponse.Cookies[i].Name));
                                    _cookies.Add(webResponse.Cookies[i]);
                                    serverCookie.Add(webResponse.Cookies[i]);
                                }
                            }
                        }
                    }
                }
                
                ret.ResponseContent = result;
                ret.Successed = true;
            }
            catch(Exception ex)
            {
                ret.ResponseContent = string.Empty;
                ret.Successed = false;
                ret.ErrorMsg = ex;
            }
            return ret;
        }
        #endregion

        /// <summary>
        /// 返回请求的数据流，主要用于读取网络图片等
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Stream GetStream(string url, bool saveCookie=true)
        {
            MemoryStream m = new MemoryStream();
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            NetworkCredential credential = GetCredential();
            if (credential != null)
                webRequest.Proxy.Credentials = credential;

            if (serverCookie != null)
                webRequest.CookieContainer = serverCookie;
            else
                webRequest.CookieContainer = new CookieContainer();

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var s= response.GetResponseStream();

                if (saveCookie && response.Cookies.Count > 0)
                {
                    if (serverCookie == null)
                        serverCookie = new CookieContainer();
                    serverCookie.PerDomainCapacity = 255;
                    for (int i = 0; i < response.Cookies.Count; i++)
                    {
                        if (!_cookies.Exists(p => p.Name.Equals(response.Cookies[i].Name)))
                        {
                            _cookies.Add(response.Cookies[i]);
                            serverCookie.Add(response.Cookies[i]);
                        }
                        else
                        {
                            _cookies.RemoveAll(p => p.Name.Equals(response.Cookies[i].Name));
                            serverCookie.Add(response.Cookies[i]);
                        }
                    }
                }

                return s;
            }

            return null;
        }

    }
}
