using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.Comm
{
    /// <summary>
    /// 网页请求页代理类
    /// 如果是代理上网，请在config里面配置上网用户名和密码
    /// NetWorkUsername --上网用户名
    /// NetWorkPwd  --上网密码
    /// NetWorkDomain
    /// </summary>
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

        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        public Dictionary<string, string> Headers
        {
            get
            {
                return _headers;
            }
        }
        #endregion

        #region 编码方式
        private Encoding _encoding = Encoding.UTF8;
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

        private Encoding _defaultEncoding = Encoding.UTF8;
        /// <summary>
        /// 默认编码，当获取编码失败时，会默认使用此编码
        /// </summary>
        public Encoding DefaultEncoding
        {
            get
            {
                return _defaultEncoding;
            }
            set
            {
                _defaultEncoding = value;
            }
        }


        private bool _supportCompression = true;
        /// <summary>
        /// 是否支持压缩
        /// </summary>
        public bool SupportCompression
        {
            get
            {
                return _supportCompression;
            }
            set
            {
                _supportCompression = value;
            }
        }

        #endregion

        #region cookie相关

        /// <summary>
        /// 服务器给本地的cookie
        /// </summary>
        private CookieContainer serverCookie;

        public void AppendCookie(string cookieName, string cookieValue, string domain, string path)
        {
            if (serverCookie == null)
            {
                serverCookie = new CookieContainer();
                serverCookie.PerDomainCapacity = 255;
            }
            serverCookie.Add(new Cookie
            {
                Name = cookieName,
                Value = cookieValue,
                Domain = domain,
                Path = path
            });
        }

        private List<Cookie> _cookies = new List<Cookie>();
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

        private int _timeOut = 120 * 1000;
        /// <summary>
        ///超时时间，以秒单位，默认120秒，如果为0，则不限制
        /// </summary>
        public int TimeOut
        {
            get
            {
                return _timeOut;
            }
            set
            {
                if (value <= 0)
                    _timeOut = 0;

                _timeOut = value * 1000;
            }
        }

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
            int num = mycerts.Init();
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
                if (host.IndexOf(s) > -1)
                    return true;
            }

            return false;
        }

        public HttpResponseEx DoFormRequest(string url, Dictionary<string, string> form, bool ignoreConflict = true)
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

        private static Regex regHTMLMetaEncoding = new Regex("<meta\\s[^>]{0,}charset=\"?([\\d\\w-]+)[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Encoding GetHTMLMetaEncoding(string html)
        {
            int h1 = html.IndexOf("<head", 0, StringComparison.OrdinalIgnoreCase);
            if (h1 > -1)
            {
                int h2 = html.IndexOf("</head", h1, StringComparison.OrdinalIgnoreCase);
                if (h2 > h1)
                {
                    string headHtml = html.Substring(h1, h2 - h1);

                    var m = regHTMLMetaEncoding.Match(headHtml);
                    if (m.Success)
                    {
                        try
                        {
                            Encoding htmlEncode = Encoding.GetEncoding(m.Groups[1].Value);
                            return htmlEncode;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// post方法发送数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="saveCookie">是否保存cookie</param>
        /// <param name="getContent">是否读取内容</param>
        /// <returns></returns>
        public HttpResponseEx DoRequest(string url, string data, WebRequestMethodEnum method = WebRequestMethodEnum.GET, bool saveCookie = true, bool getContent = true,
            string contentType = "application/x-www-form-urlencoded;charset=UTF-8;")
        {
            byte[] buff = null;

            if (!string.IsNullOrEmpty(data))
            {
                buff = this.WebEncoding.GetBytes(data);

            }

            var response = DoRequest(url, buff, method, saveCookie, getContent, contentType);

            if (!response.Successed)
            {
                return response;
            }

            try
            {

                WebEncoding = Encoding.GetEncoding(response.CharacterSet);
            }
            catch
            {
                WebEncoding = DefaultEncoding;
            }

            if (response.ResponseBytes != null && response.ResponseBytes.Length > 0)
            {
                response.ResponseContent = WebEncoding.GetString(response.ResponseBytes);
                if (SupportCompression)
                {
                    var htmlEncoding = GetHTMLMetaEncoding(response.ResponseContent);
                    if (htmlEncoding != null && htmlEncoding != WebEncoding)
                    {
                        response.ResponseContent = htmlEncoding.GetString(response.ResponseBytes);
                        WebEncoding = htmlEncoding;
                    }
                }

                response.ResponseBytes = null;
            }
            return response;
        }

        ///// <summary>
        ///// post方法发送数据
        ///// </summary>
        ///// <param name="url"></param>
        ///// <param name="data"></param>
        ///// <param name="saveCookie">是否保存cookie</param>
        ///// <param name="getContent">是否读取内容</param>
        ///// <returns></returns>
        //public HttpResponseEx DoRequest(string url, string data, WebRequestMethodEnum method = WebRequestMethodEnum.GET, bool saveCookie = true, bool getContent = true,
        //    string contentType = "application/x-www-form-urlencoded;charset=UTF-8;")
        //{
        //    var ret = new HttpResponseEx();
        //    try
        //    {
        //        if (!CheckIsInDomain(url))
        //            throw new Exception(string.Format("拒绝访问，地址{0}不在可访问的域名之列。", url));

        //        string result = string.Empty;

        //        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
        //        if (data != null && data.Length > 0)
        //            webRequest.Method = WebRequestMethodEnum.POST.ToString();

        //        webRequest.Accept = Accept;
        //        webRequest.AllowAutoRedirect = true;
        //        webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, this.acceptLanguage);
        //        webRequest.KeepAlive = true;
        //        webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
        //        webRequest.UserAgent = UserAgent;
        //        if (SupportCompression)
        //        {
        //            //webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
        //            webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
        //        }
        //        if (!string.IsNullOrEmpty(Referer))
        //            webRequest.Referer = Referer;

        //        NetworkCredential credential = GetCredential();
        //        if (credential != null)
        //            webRequest.Proxy.Credentials = credential;

        //        if (serverCookie != null)
        //            webRequest.CookieContainer = serverCookie;
        //        else
        //            webRequest.CookieContainer = new CookieContainer();

        //        if (_certificate != null)
        //        {
        //            webRequest.UserAgent = "Client Cert Sample";
        //            webRequest.ClientCertificates.Add(_certificate);
        //        }

        //        if (!string.IsNullOrWhiteSpace(data))
        //        {
        //            byte[] buff = this.WebEncoding.GetBytes(data);
        //            //webRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8;";
        //            webRequest.ContentType = contentType;
        //            webRequest.ContentLength = buff.Length;

        //            using (Stream requestStream = webRequest.GetRequestStream())
        //            {
        //                requestStream.Write(buff, 0, buff.Length);
        //            }
        //        }

        //        using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
        //        {
        //            if (webResponse.StatusCode == HttpStatusCode.OK)
        //            {
        //                ret.PraseHeader(webResponse);

        //                if (getContent)
        //                {
        //                    byte[] contentBuffer = null;
        //                    using (MemoryStream ms = new MemoryStream())
        //                    {
        //                        using (Stream s = webResponse.GetResponseStream())
        //                        {

        //                            byte[] buffer = new byte[2048];
        //                            int len = 0;
        //                            while ((len = s.Read(buffer, 0, 1024)) > 0)
        //                            {
        //                                ms.Write(buffer, 0, len);
        //                            }
        //                        }

        //                        if (webResponse.ContentEncoding.Equals("gzip", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            contentBuffer = Comm.GZip.Decompress(ms.ToArray());
        //                        }
        //                        else
        //                        {
        //                            contentBuffer = ms.ToArray();
        //                        }

        //                        try
        //                        {

        //                            WebEncoding = Encoding.GetEncoding(webResponse.CharacterSet);
        //                        }
        //                        catch
        //                        {
        //                            WebEncoding = DefaultEncoding;
        //                        }
        //                    }

        //                    result = WebEncoding.GetString(contentBuffer);


        //                    if (SupportCompression)
        //                    {
        //                        var htmlEncoding = GetHTMLMetaEncoding(result);
        //                        if (htmlEncoding != null && htmlEncoding != WebEncoding)
        //                        {
        //                            result = htmlEncoding.GetString(contentBuffer);
        //                            WebEncoding = htmlEncoding;
        //                        }
        //                    }
        //                }

        //                if (saveCookie && webResponse.Cookies.Count > 0)
        //                {
        //                    if (serverCookie == null)
        //                        serverCookie = new CookieContainer();
        //                    serverCookie.PerDomainCapacity = 255;
        //                    for (int i = 0; i < webResponse.Cookies.Count; i++)
        //                    {
        //                        if (!_cookies.Exists(p => p.Name.Equals(webResponse.Cookies[i].Name)))
        //                        {
        //                            _cookies.Add(webResponse.Cookies[i]);
        //                            serverCookie.Add(webResponse.Cookies[i]);
        //                        }
        //                        else
        //                        {
        //                            _cookies.RemoveAll(p => p.Name.Equals(webResponse.Cookies[i].Name));
        //                            _cookies.Add(webResponse.Cookies[i]);
        //                            serverCookie.Add(webResponse.Cookies[i]);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        ret.ResponseContent = result;
        //        ret.Successed = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ret.ResponseContent = string.Empty;
        //        ret.Successed = false;
        //        ret.ErrorMsg = ex;
        //    }
        //    return ret;
        //}

        /// <summary>
        /// post方法发送数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="saveCookie">是否保存cookie</param>
        /// <param name="getContent">是否读取内容</param>
        /// <returns></returns>
        public HttpResponseEx DoRequest(string url, byte[] buff, WebRequestMethodEnum method = WebRequestMethodEnum.GET, bool saveCookie = true, bool getContent = true,
            string contentType = "application/x-www-form-urlencoded;charset=UTF-8;")
        {

            var ret = new HttpResponseEx();
            ret.RequestMills = Environment.TickCount & Int32.MaxValue;

            try
            {
                if (!CheckIsInDomain(url))
                    throw new Exception(string.Format("拒绝访问，地址{0}不在可访问的域名之列。", url));

                //string result = string.Empty;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                if (buff != null && buff.Length > 0 && method == WebRequestMethodEnum.GET)
                    webRequest.Method = WebRequestMethodEnum.POST.ToString();
                else
                    webRequest.Method = method.ToString();

                webRequest.Accept = Accept;
                webRequest.AllowAutoRedirect = true;
                webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, this.acceptLanguage);
                webRequest.KeepAlive = true;
                webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                webRequest.UserAgent = UserAgent;
                if (_timeOut > 0)
                {
                    webRequest.Timeout = _timeOut;
                }
                if (SupportCompression)
                {
                    //webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
                }
                foreach (var kv in Headers)
                {
                    webRequest.Headers.Add(kv.Key, kv.Value);
                }
                if (!string.IsNullOrEmpty(Referer))
                    webRequest.Referer = Referer;

                NetworkCredential credential = GetCredential();
                if (credential != null)
                    webRequest.Proxy.Credentials = credential;

                if (serverCookie != null)
                    webRequest.CookieContainer = serverCookie;
                else
                    webRequest.CookieContainer = new CookieContainer();


                if (buff != null && buff.Length > 0)
                {
                    //byte[] buff = this.WebEncoding.GetBytes(data);
                    //webRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8;";
                    webRequest.ContentType = contentType;
                    webRequest.ContentLength = buff.Length;

                    using (Stream requestStream = webRequest.GetRequestStream())
                    {
                        requestStream.Write(buff, 0, buff.Length);
                    }
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    //int statusCode = (int)webResponse.StatusCode;
                    //if (statusCode >= 200 && statusCode < 400)
                    {
                        ret.PraseHeader(webResponse);

                        if (getContent)
                        {
                            byte[] contentBuffer = null;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                using (Stream s = webResponse.GetResponseStream())
                                {
                                    byte[] buffer = new byte[2048];
                                    int len = 0;
                                    while ((len = s.Read(buffer, 0, 1024)) > 0)
                                    {
                                        ms.Write(buffer, 0, len);
                                    }
                                }

                                if (webResponse.ContentEncoding.Equals("gzip", StringComparison.OrdinalIgnoreCase))
                                {
                                    contentBuffer = Comm.GZip.Decompress(ms.ToArray());
                                }
                                else
                                {
                                    contentBuffer = ms.ToArray();
                                }

                                ret.CharacterSet = webResponse.CharacterSet;

                                ret.ResponseBytes = contentBuffer;
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

                ret.Successed = true;
            }
            catch (WebException ex)
            {
                ret.PraseHeader((HttpWebResponse)ex.Response);
                ret.Successed = false;
                ret.ErrorMsg = ex;
                ret.ResponseContent = string.Empty;
            }
            catch (Exception ex)
            {
                ret.ResponseContent = string.Empty;
                ret.Successed = false;
                ret.ErrorMsg = ex;
            }

            ret.RequestMills = Environment.TickCount & Int32.MaxValue - ret.RequestMills;
            return ret;
        }
        #endregion

        /// <summary>
        /// 返回请求的数据流，主要用于读取网络图片等
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Stream GetStream(string url, bool saveCookie = true)
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
                var s = response.GetResponseStream();

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
