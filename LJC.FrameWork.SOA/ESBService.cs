using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;
using System.Threading;
using LJC.FrameWork.LogManager;
using LJC.FrameWork.SocketApplication.SocketSTD;
using LJC.FrameWork.SOA.Contract;
using LJC.FrameWork.EntityBuf;
using System.Text.RegularExpressions;
using System.IO;
using LJC.FrameWork.Comm;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LJC.FrameWork.SOA
{
    public class ESBService:SessionClient,IService
    {
        static ESBService()
        {
            ThreadPoolHelper.CheckSetMinThreads(100, 100);

            //System.Net.ServicePointManager.Expect100Continue = false;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | (System.Net.SecurityProtocolType)768 | (System.Net.SecurityProtocolType)3072 | System.Net.SecurityProtocolType.Ssl3;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);

        }

        private static bool CheckValidationResult(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        {
            return true;
        }

        private const int Func_WebRequest = -100;

        private bool SupportTcpServiceRidrect
        {
            get;
            set;
        }

        private bool SupportUDPServiceRedirect
        {
            get;
            set;
        }

        private string ServiceName
        {
            get;
            set;
        }

        private string EndPointName
        {
            get;
            set;
        }
       

        public ESBService(string serverIP, int serverPort,int sNo,bool supportTcpServiceRedirect=false,bool supportUdpServiceRedirect=false,
            string serviceName=null,string endPointName=null)
            : base(serverIP, serverPort,false)
        {
            this.ServiceNo = sNo;
            this.BeferLogout += this.UnRegisterService;
            this.OnClientReset += ESBService_OnClientReset;

            this.SupportTcpServiceRidrect = supportTcpServiceRedirect;
            this.SupportUDPServiceRedirect = supportUdpServiceRedirect;

            this.ServiceName = serviceName;
            this.EndPointName = endPointName;
        }

        public ESBService(int sNo, bool supportTcpServiceRidrect = false, bool supportUdpServiceRedirect = false,
            string serviceName = null, string endPointName = null)
           : base(ESBConfig.ReadConfig().ESBServer, ESBConfig.ReadConfig().ESBPort,false)
        {
            this.ServiceNo = sNo;
            this.BeferLogout += this.UnRegisterService;
            this.OnClientReset += ESBService_OnClientReset;

            this.SupportTcpServiceRidrect = supportTcpServiceRidrect;
            this.SupportUDPServiceRedirect = supportUdpServiceRedirect;

            this.ServiceName = serviceName;
            this.EndPointName = endPointName;
        }

        void ESBService_OnClientReset()
        {
            int trytime = 0, maxtrytimes = 10;
            while (true)
            {
                try
                {
                    if (RegisterService())
                    {
                        LogHelper.Instance.Info("连接重置后注册服务成功");
                        break;
                    }
                    else
                    {
                        LogHelper.Instance.Info("连接重置后注册服务失败");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("连接重置后注册服务失败", ex);
                }
                if (trytime++ >= maxtrytimes)
                {
                    break;
                }
                Thread.Sleep(100 * trytime);
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void StartService()
        {
            while (!StartClient())
            {
                Thread.Sleep(1000);
            }
            Login(null, null);
        }

        public int ServiceNo
        {
            get;
            private set;
        }

        protected sealed override void ReciveMessage(Message message)
        {
            //此服务特殊，不可通过转发调用
            if (message.IsMessage((int)SOAMessageType.QueryServiceNo))
            {
                var responseMsg = new Message((int)SOAMessageType.QueryServiceNo);
                responseMsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                QueryServiceNoResponse responseBody = new QueryServiceNoResponse();
                responseBody.ServiceNo = ServiceNo;

                responseMsg.SetMessageBody(responseBody);
                SendMessage(responseMsg);

                return;
            }
            else if (message.IsMessage((int)SOAMessageType.DoSOATransferRequest))
            {
                SOATransferRequest request = null;
                try
                {
                    var responseMsg = new Message((int)SOAMessageType.DoSOATransferResponse);
                    responseMsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                    SOATransferResponse responseBody = new SOATransferResponse();
                    request = message.GetMessageBody<SOATransferRequest>();
                    responseBody.ClientTransactionID = request.ClientTransactionID;
                    responseBody.ClientId = request.ClientId;

                    if (SocketApplicationEnvironment.TraceMessage)
                    {
                        LogHelper.Instance.Debug(string.Format("接收服务请求,请求号:{0}", request.ClientTransactionID));
                    }

                    try
                    {
                        var result = DoResponse(request.FundId, request.Param, request.ClientId);
                        responseBody.Result = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(result);
                        responseBody.IsSuccess = true;

                        if (SocketApplicationEnvironment.TraceMessage)
                        {
                            LogHelper.Instance.Debug(string.Format("处理请求：请求号:{0},客户端请求号:{1},服务号:{2},功能号:{3},结果:{4},序列化结果:{5}",
                                responseMsg.MessageHeader.TransactionID, request.ClientTransactionID, ServiceNo, request.FundId, Comm.JsonUtil<object>.Serialize(result), Convert.ToBase64String(responseBody.Result)));

                        }
                    }
                    catch (Exception ex)
                    {
                        responseBody.IsSuccess = false;
                        responseBody.ErrMsg = ex.Message;

                        LogHelper.Instance.Error(string.Format("服务转发出错,请求号:{0},服务号:{1},功能号:{2}",
                            request.ClientTransactionID, ServiceNo, request.FundId), ex);

                    }

                    responseMsg.SetMessageBody(responseBody);

                    SendMessage(responseMsg);

                    return;
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error(string.Format("服务转发出错,请求号:{0},服务号:{1},功能号:{2}",
                        request == null ? "0" : request.ClientTransactionID,
                        ServiceNo, request == null ? 0 : request.FundId), ex);

                    return;
                }
            }
            else if (message.IsMessage((int)SOAMessageType.SOATransferWebRequest))
            {
                SOATransferWebRequest request = null;
                try
                {
                    var responseMsg = new Message((int)SOAMessageType.SOATransferWebResponse);
                    responseMsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                    SOATransferWebResponse responseBody = new SOATransferWebResponse();
                    request = message.GetMessageBody<SOATransferWebRequest>();
                    responseBody.ClientTransactionID = request.ClientTransactionID;
                    responseBody.ClientId = request.ClientId;

                    if (SocketApplicationEnvironment.TraceMessage)
                    {
                        LogHelper.Instance.Debug(string.Format("接收服务请求,请求号:{0}", request.ClientTransactionID));
                    }

                    try
                    {
                        var result = DoResponse(Func_WebRequest, request.Param, request.ClientId);
                        responseBody.Result = EntityBuf.EntityBufCore.Serialize(result);
                        responseBody.IsSuccess = true;

                        if (SocketApplicationEnvironment.TraceMessage)
                        {
                            LogHelper.Instance.Debug(string.Format("处理请求：请求号:{0},客户端请求号:{1},服务号:{2},功能号:{3},结果:{4},序列化结果:{5}",
                                responseMsg.MessageHeader.TransactionID, request.ClientTransactionID, ServiceNo, request.FundId, Comm.JsonUtil<object>.Serialize(result), Convert.ToBase64String(responseBody.Result)));

                        }
                    }
                    catch (Exception ex)
                    {
                        responseBody.IsSuccess = false;
                        responseBody.ErrMsg = ex.Message;

                        LogHelper.Instance.Error(string.Format("服务转发出错,请求号:{0},服务号:{1},功能号:{2}",
                            request.ClientTransactionID, ServiceNo, 0), ex);

                    }

                    responseMsg.SetMessageBody(responseBody);

                    SendMessage(responseMsg);

                    return;
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error(string.Format("服务转发出错,请求号:{0},服务号:{1},功能号:{2}",
                        request == null ? "0" : request.ClientTransactionID,
                        ServiceNo, request == null ? 0 : request.FundId), ex);

                    return;
                }
            }

            base.ReciveMessage(message);
        }

        protected override byte[] DoMessage(LJC.FrameWork.SocketApplication.Message message)
        {
            if (message.IsMessage((int)SOAMessageType.RegisterService))
            {
                return message.MessageBuffer;
            }
            else if (message.IsMessage((int)SOAMessageType.UnRegisterService))
            {
                return message.MessageBuffer;
            }
            return base.DoMessage(message);
        }

        private WebResponse DoWebResponseWithHttpClient(WebRequest request, string realUrl)
        {
            WebResponse response = new WebResponse();
            var client = HttpClientFactory.GetHttpClient(realUrl,false);
            
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod(request.Method), new Uri(realUrl)))
            {
                if (request.InputData?.Length > 0)
                {
                    httpRequestMessage.Content = new ByteArrayContent(request.InputData);
                    httpRequestMessage.Content.Headers.ContentLength = request.InputData.Length;
                }
                else if (httpRequestMessage.Method == HttpMethod.Post)
                {
                    httpRequestMessage.Content = new ByteArrayContent(new byte[0]);
                    httpRequestMessage.Content.Headers.ContentLength = 0;
                }

                foreach (var kv in request.Headers)
                {
                    if (kv.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                        || kv.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase)
                        || kv.Key.Equals("host", StringComparison.OrdinalIgnoreCase)
                        || kv.Key.Equals("Expect", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (kv.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) && httpRequestMessage.Content != null)
                    {
                        //application/x-www-form-urlencoded; charset=UTF-8
                        httpRequestMessage.Content.Headers.Add(kv.Key, kv.Value);
                        continue;
                    }
                    httpRequestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }

                if (request.Cookies?.Any() == true)
                {
                    System.Net.CookieContainer cookieContainer = new System.Net.CookieContainer();
                    var domin = httpRequestMessage.RequestUri.Host.Split(':').First();
                    foreach (var cookie in request.Cookies)
                    {
                        //cookieContainer.Add(new System.Net.Cookie(cookie.Key, WebUtility.UrlEncode(cookie.Value)));
                        cookieContainer.Add(new System.Net.Cookie
                        {
                            Name = cookie.Key,
                            Value = WebUtility.UrlEncode(cookie.Value),
                            Domain = domin,
                            //Domain=new Uri(matchedMapper.TragetWebHost).Host,
                            Path = "/"
                        });
                    }
                    httpRequestMessage.Headers.Add("Cookie", cookieContainer.GetCookieHeader(httpRequestMessage.RequestUri));
                }
                
                using (var httpResponseMessage = client.SendAsync(httpRequestMessage).Result)
                {
                    response.Headers = new Dictionary<string, string>();
                    var headers = httpResponseMessage.Headers.ToList();
                    AddHeader(headers);

                    byte[] contentBuffer = httpResponseMessage.Content.ReadAsByteArrayAsync().Result;
                    response.ResponseData = contentBuffer;
                    response.ContentType = httpResponseMessage.Content.Headers.ContentType?.ToString();
                    AddHeader(httpResponseMessage.Content.Headers.ToList());

                    response.ResponseCode = (int)httpResponseMessage.StatusCode;
                }
            }

            return response;

            void AddHeader(List<KeyValuePair<string,IEnumerable<string>>> headers)
            {
                for (var i = 0; i < headers.Count; i++)
                {
                    var name = headers[i].Key;
                    var values = headers[i].Value;

                    if (name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                               || name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
                               || name.Equals("Server", StringComparison.OrdinalIgnoreCase)
                               || name.Equals("Date", StringComparison.OrdinalIgnoreCase)
                               || name.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var value = string.Join(",", values);
                    if (name.Equals("Location", StringComparison.OrdinalIgnoreCase))
                    {
                        value = WebTransferSvcHelper.RelaceLocation(value, request.Host, realUrl);
                    }

                    response.Headers.Add(name, value);

                }
            }
        }

        private WebResponse DoWebResponseWithHttpWebRequest(WebRequest request, string realUrl,WebProxy proxy)
        {
            WebResponse response = new WebResponse();
            System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(realUrl);
            webRequest.Method = request.Method;

            //System.Net.NetworkCredential credential = HttpRequestEx.GetCredential();
            //if (credential != null)
            //    webRequest.Proxy.Credentials = credential;
            if (proxy != null)
            {
                webRequest.SetCredential(proxy.Address, proxy.UserName, proxy.UserPassWord);
            }

            foreach (var kv in request.Headers)
            {
                if (kv.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                    || kv.Key.Equals("host", StringComparison.OrdinalIgnoreCase)
                    || kv.Key.Equals("Expect", StringComparison.OrdinalIgnoreCase))
                {
                    continue; 
                }

                if (kv.Key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                {
                    webRequest.Referer = kv.Value;
                }
                else if (kv.Key.Equals("Expect", StringComparison.OrdinalIgnoreCase))
                {
                    webRequest.Expect = kv.Value;
                }
                else if (kv.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                {
                    //webRequest.KeepAlive = "keep-alive".Equals(kv.Value, StringComparison.OrdinalIgnoreCase);
                }
                else if (kv.Key.Equals("Proxy-Connection", StringComparison.OrdinalIgnoreCase))
                {
                    
                }
                else if (kv.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                {
                    webRequest.UserAgent = kv.Value;
                }
                else if (kv.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                {
                    webRequest.Accept = kv.Value;
                }
                else if (kv.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    webRequest.ContentType = kv.Value;
                }
                else if (kv.Key.Equals("If-Modified-Since", StringComparison.OrdinalIgnoreCase))
                {
                    webRequest.IfModifiedSince = DateTime.Parse(kv.Value);
                }
                else
                {
                    webRequest.Headers.Add(kv.Key, kv.Value);
                }
            }
            webRequest.AllowAutoRedirect = false;
            webRequest.KeepAlive = false;
            webRequest.AllowWriteStreamBuffering = true;
            webRequest.CookieContainer = new System.Net.CookieContainer();
            var cookDomain = webRequest.Host.Split(':').First();
            foreach (var kv in request.Cookies)
            {
                webRequest.CookieContainer.Add(new System.Net.Cookie
                {
                    Name = kv.Key,
                    Value = WebUtility.UrlEncode(kv.Value),
                    Domain = cookDomain,
                    //Domain=new Uri(matchedMapper.TragetWebHost).Host,
                    Path = "/"
                });
            }

            if (request.TimeOut > 0)
            {
                webRequest.Timeout = request.TimeOut;
            }


            var buff = request.InputData;
            if (buff != null && buff.Length > 0)
            {
                //byte[] buff = this.WebEncoding.GetBytes(data);
                //webRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8;";
                //webRequest.ContentType = contentType;
                webRequest.ContentLength = buff.Length;

                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(buff, 0, buff.Length);
                }
            }
            else
            {
                webRequest.ContentLength = 0;
                //using (Stream requestStream = webRequest.GetRequestStream())
                //{
                //    requestStream.Write(new byte[0], 0, 0);
                //}
            }
            Console.WriteLine(webRequest.RequestUri.ToString());

            System.Net.HttpWebResponse webResponse = null;
            try
            {
                webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse();

            }
            catch (System.Net.WebException ex)
            {
                webResponse = (System.Net.HttpWebResponse)ex.Response;
                if (webResponse == null)
                {
                    throw;
                }
            }
            finally
            {
                try
                {
                    if (webResponse != null)
                    {
                        response.Headers = new Dictionary<string, string>();

                        for (var i = 0; i < webResponse.Headers.Count; i++)
                        {
                            var name = webResponse.Headers.GetKey(i);

                            if (name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                                || name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
                                || name.Equals("Server", StringComparison.OrdinalIgnoreCase)
                                || name.Equals("Date", StringComparison.OrdinalIgnoreCase)
                                || name.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            var value = webResponse.Headers.Get(i);
                            if (name.Equals("Location", StringComparison.OrdinalIgnoreCase))
                            {
                                value = WebTransferSvcHelper.RelaceLocation(value, request.Host, realUrl);
                            }
                            response.Headers.Add(name, value);
                        }

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

                            contentBuffer = ms.ToArray();

                            response.ResponseData = contentBuffer;
                        }

                        response.ResponseCode = (int)webResponse.StatusCode;
                        response.ContentType = webResponse.ContentType;
                    }
                }
                finally
                {
                    if (webResponse != null)
                    {
                        webResponse.Close();
                    }
                }
            }
            return response;
        }

        private object DoWebResponse(WebRequest request)
        {
            WebResponse response = new WebResponse();
            
            var webMappers = ServiceConfig.ReadConfig()?.WebMappers;

            WebMapper matchedMapper = WebTransferSvcHelper.Find(request,webMappers);
            if (matchedMapper != null)
            {
                var virUrl = request.VirUrl;
                if (!string.IsNullOrWhiteSpace(matchedMapper.MappingRoot) && virUrl.StartsWith(matchedMapper.MappingRoot, StringComparison.OrdinalIgnoreCase))
                {
                    virUrl = virUrl.Substring(matchedMapper.MappingRoot.Length);
                }

                var realUrl = matchedMapper.TragetWebHost;
                if (!string.IsNullOrWhiteSpace(virUrl))
                {
                    realUrl = realUrl.TrimEnd('/') + '/' + virUrl.TrimStart('/');
                }

                //代理
                WebProxy proxy = null;
                if (!string.IsNullOrEmpty(matchedMapper.UseProxyName))
                {
                    proxy = ServiceConfig.ReadConfig().WebProxies?.FirstOrDefault(p => p.Name.Equals(matchedMapper.UseProxyName, StringComparison.OrdinalIgnoreCase));
                }

                if (realUrl.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                    || proxy != null
                    || request.Cookies?.Any() == true
                    || request.Headers?.Keys.Any(p=>p.Equals("Cookie",StringComparison.OrdinalIgnoreCase)) == true
                    )
                {
                    response = DoWebResponseWithHttpWebRequest(request, realUrl, proxy);
                }
                else
                {
                    response = DoWebResponseWithHttpClient(request, realUrl);
                }
            }

            return response;
        }

        public virtual object DoResponse(int funcId, byte[] Param,string clientid)
        {
            if (funcId == Func_WebRequest)
            {
                try
                {
                    return DoWebResponse(EntityBufCore.DeSerialize<WebRequest>(Param));
                }
                catch(Exception ex)
                {
                    LogHelper.Instance.Error("DoResponse", ex);

                    return new WebResponse
                    {
                        ResponseCode=500,
                        ResponseData=Encoding.UTF8.GetBytes(ex.Message),
                        ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8")
                    };
                }
            }

            return null;
        }

        public bool RegisterService()
        {
            if (this.ServiceNo < 0)
                throw new Exception("注册服务失败：服务号不能为负数");

            StartRedirectService();

            Message msg = new Message((int)SOAMessageType.RegisterService);
            msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
            RegisterServiceRequest req = new RegisterServiceRequest();
            req.ServiceNo = this.ServiceNo;
            if (SupportTcpServiceRidrect)
            {
                req.RedirectTcpIps = RedirectTcpServiceServer.GetBindIps();
                req.RedirectTcpPort = RedirectTcpServiceServer.GetBindTcpPort();
            }
            if (SupportUDPServiceRedirect)
            {
                req.RedirectUdpIps = RedirectUpdServiceServer.GetBindIps();
                req.RedirectUdpPort = RedirectUpdServiceServer.GetBindUdpPort();
            }

            if (!string.IsNullOrWhiteSpace(this.ServiceName) && !string.IsNullOrWhiteSpace(this.EndPointName))
            {
                msg.AddCustomData(nameof(this.ServiceName), this.ServiceName);
                msg.AddCustomData(nameof(this.EndPointName), this.EndPointName);
            }

            var serviceConfig = ServiceConfig.ReadConfig();
            if (serviceConfig?.WebMappers?.Any() == true)
            {
                msg.AddCustomData(nameof(ServiceConfig.WebMappers), Comm.SerializerHelper.SerializerToXML(serviceConfig.WebMappers));
            }

            msg.SetMessageBody(req);

           bool boo= SendMessageAnsy<RegisterServiceResponse>(msg).IsSuccess;

           return boo;
        }

        /// <summary>
        /// 发送soa通知
        /// </summary>
        /// <param name="recivers"></param>
        /// <param name="type"></param>
        /// <param name="body"></param>
        /// <param name="needresult"></param>
        /// <returns></returns>
        public SOANoticeResponse SendNotice(string[] recivers, int type, byte[] body,bool needresult)
        {
            if (recivers == null || recivers.Length == 0)
            {
                return new SOANoticeResponse();
            }

            Message m = new Message((int)SOAMessageType.SOANoticeRequest);
            
            m.SetMessageBody(new Contract.SOANoticeRequest
            {
                NeedResult=needresult,
                NoticeBody=body,
                NoticeType=type,
                ReciveClients=recivers
            });

            if (needresult)
            {
                m.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
                return SendMessageAnsy<Contract.SOANoticeResponse>(m);
            }
            else
            {
                return new SOANoticeResponse
                {
                    IsDone = SendMessage(m)
                };
            }
        }

        public void UnRegisterService()
        {
            Message msg = new Message((int)SOAMessageType.UnRegisterService);
            msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
            UnRegisterServiceRequest req = new UnRegisterServiceRequest();
            req.ServiceNo = this.ServiceNo;
            msg.SetMessageBody(req);

            bool boo = SendMessageAnsy<UnRegisterServiceResponse>(msg).IsSuccess;

            if (boo)
            {
                Console.WriteLine("取消注册成功");
            }

        }

        protected override void OnLoginSuccess()
        {
            base.OnLoginSuccess();
            while (true)
            {
                try
                {
                    if (RegisterService())
                    {
                        LogHelper.Instance.Info("注册服务成功");
                        break;
                    }
                    else
                    {
                        LogHelper.Instance.Info("注册服务失败");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("注册服务失败", ex);
                }
                Thread.Sleep(3000);
            }
        }

        protected override void OnError(Exception e)
        {
            base.OnError(e);
        }

        protected override void OnSessionResume()
        {
            base.OnSessionResume();

            while (true)
            {
                try
                {
                    if (RegisterService())
                    {
                        LogHelper.Instance.Info("连接恢复后注册服务成功");
                        break;
                    }
                    else
                    {
                        LogHelper.Instance.Info("连接恢复后注册服务失败");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("连接恢复后注册服务失败", ex);
                }

                Thread.Sleep(3000);
            }
        }

        #region 开通直连服务
        private ESBRedirectService RedirectTcpServiceServer = null;
        private ESBUDPService RedirectUpdServiceServer = null;

        public void StartRedirectService()
        {
            var addrs = NetworkHelper.GetActiveIpV4s(true);
            var bindips = NetworkHelper.GetActiveIpV4s();

            if (addrs.Length > 0)
            {
                int iport = 0;
                if (SupportTcpServiceRidrect&& RedirectTcpServiceServer == null)
                {
                    int trytimes = 0;
                    while (true)
                    {
                        try
                        {
                            iport = SocketApplicationComm.GetIdelTcpPort();

                            RedirectTcpServiceServer = new ESBRedirectService(ServiceNo, bindips.Select(p => p.ToString()).ToArray(), iport);
                            RedirectTcpServiceServer.DoResponseAction = DoResponse;
                            RedirectTcpServiceServer.StartServer();
                            break;
                        }
                        catch (Exception ex)
                        {
                            trytimes++;
                            if (trytimes >= 10)
                            {
                                OnError(new Exception("启动tcp直连服务端口失败,已尝试" + trytimes + "次，端口:" + iport, ex));
                                break;
                            }
                        }
                    }
                }

                if (SupportUDPServiceRedirect && this.RedirectUpdServiceServer == null)
                {
                    int trytimes = 0;
                    while (true)
                    {
                        try
                        {
                            iport = SocketApplicationComm.GetIdelUdpPort(iport);

                            RedirectUpdServiceServer = new ESBUDPService(ServiceNo, bindips.Select(p => p.ToString()).ToArray(), iport);
                            RedirectUpdServiceServer.DoResponseAction = DoResponse;
                            RedirectUpdServiceServer.StartServer();
                            break;
                        }
                        catch (Exception ex)
                        {
                            trytimes++;
                            if (trytimes >= 10)
                            {
                                OnError(new Exception("启动udp直连服务端口失败,已尝试" + trytimes + "次，端口:" + iport, ex));
                                break;
                            }
                        }
                    }
                }
            }
        }

        #endregion

    }
}
