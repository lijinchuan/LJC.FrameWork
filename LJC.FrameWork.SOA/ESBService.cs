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
using System.Security.Policy;
using System.Security;
using System.Runtime.InteropServices;

namespace LJC.FrameWork.SOA
{
    public class ESBService:SessionClient,IService
    {
        private sealed class ChunkedWebRequestContext
        {
            public WebRequest Request { get; set; }
            public System.Net.HttpWebRequest HttpRequest { get; set; }
            public Stream RequestStream { get; set; }
            public string RealUrl { get; set; }
            public WebProxy Proxy { get; set; }
        }

        private readonly object chunkRequestLocker = new object();
        private readonly Dictionary<string, ChunkedWebRequestContext> chunkRequestContexts = new Dictionary<string, ChunkedWebRequestContext>();

        static ESBService()
        {
            ThreadPoolHelper.CheckSetMinThreads(100, 100);

            //System.Net.ServicePointManager.Expect100Continue = false;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | (System.Net.SecurityProtocolType)768 | (System.Net.SecurityProtocolType)3072 | System.Net.SecurityProtocolType.Ssl3;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);

        }

        protected T GetParam<T>(Dictionary<string, string> messageHeader, byte[] data)
        {
            var isJson = messageHeader?[Consts.HeaderKey_ContentType] == Consts.HeaderValue_ContentType_JSONValue;
            if (isJson)
            {
                return JsonHelper.JsonToEntity<T>(Encoding.UTF8.GetString(data));
            }

            return EntityBufCore.DeSerialize<T>(data);
        }

        protected byte[] BuildResult(Dictionary<string, string> messageHeader, object result)
        {
            var isJson = messageHeader?[Consts.HeaderKey_ContentType] == Consts.HeaderValue_ContentType_JSONValue;
            if (isJson)
            {
                return Encoding.UTF8.GetBytes(JsonHelper.ToJson(result));
            }

            return EntityBufCore.Serialize(result);
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
                        var result = DoResponse(request.FundId, request.Param, request.ClientId, message.MessageHeader.CustomData);
                        responseBody.Result = BuildResult(message.MessageHeader.CustomData, result);
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

                    if (request.IsChunked)
                    {
                        HandleChunkedWebRequest(message, request);
                        return;
                    }

                    try
                    {
                        var results = (IEnumerable<WebResponse>)DoResponse(Func_WebRequest, request.Param, request.ClientId, message.MessageHeader.CustomData);
                        foreach (var result in results)
                        {
                            if (result.No > 1)
                            {
                                var kill = false;
                                var killMsg = "";
                                var k = 0;
                                var checkMax = 100;
                                var sleepMs = 100;
                                var timeout = 5000;

                                //限速
                                for (; k < checkMax; k++)
                                {
                                    var breakWhile = false;
                                    for (var i = 0; i < 3; i++)
                                    {
                                        try
                                        {
                                            var qmsg = new Message((int)SOAMessageType.QueryClientSessionRequest); // QueryClientSessionRequest
                                            qmsg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
                                            qmsg.SetMessageBody(new Contract.QueryClientSessionRequest
                                            {
                                                ClientTransactionID = request.ClientTransactionID
                                            });
                                            
                                            QueryClientSessionResponse qresp = SendMessageAnsy<Contract.QueryClientSessionResponse>(qmsg, timeout);

                                            if (!qresp.Exists)
                                            {
                                                killMsg = $"停止发送分片：客户端任务已不存在, tx={request.ClientTransactionID}";
                                                kill = true;
                                                break;
                                            }
                                            else if (qresp.LastNo >= result.No - 2)
                                            {
                                                breakWhile = true;
                                                break;
                                            }

                                            Thread.Sleep(sleepMs);
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            if (i == 2)
                                            {
                                                kill = true;
                                                killMsg = $"停止发送分片：查询客户端任务失败超过3次, tx={request.ClientTransactionID}, error={ex.Message}";
                                                break;
                                            }
                                            Thread.Sleep(sleepMs);
                                        }
                                    }

                                    if (kill)
                                    {
                                        break;
                                    }

                                    if (breakWhile)
                                    {
                                        break;
                                    }
                                }

                                if (k >= checkMax)
                                {
                                    kill = true;
                                    killMsg = $"等待网关超出最大检查次数，退出";
                                }

                                if(kill)
                                {
                                    LogHelper.Instance.Debug(killMsg);
                                    break;
                                }
                            }

                            responseBody.Result = BuildResult(message.MessageHeader.CustomData, result);
                            responseBody.IsSuccess = true;

                            responseMsg.SetMessageBody(responseBody);

                            SendMessage(responseMsg);

                            if (SocketApplicationEnvironment.TraceMessage)
                            {
                                LogHelper.Instance.Debug(string.Format("处理请求：请求号:{0},客户端请求号:{1},服务号:{2},功能号:{3},结果:{4},序列化结果:{5}",
                                    responseMsg.MessageHeader.TransactionID, request.ClientTransactionID, ServiceNo, request.FundId, Comm.JsonUtil<object>.Serialize(result), Convert.ToBase64String(responseBody.Result)));

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        responseBody.IsSuccess = false;
                        responseBody.ErrMsg = ex.Message;

                        responseMsg.SetMessageBody(responseBody);

                        SendMessage(responseMsg);

                        LogHelper.Instance.Error(string.Format("服务转发出错,请求号:{0},服务号:{1},功能号:{2}",
                            request.ClientTransactionID, ServiceNo, 0), ex);

                    }

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
            else if (message.IsMessage((int)SOAMessageType.SOACheckHealth))
            {
                var responseMsg = new Message((int)SOAMessageType.SOACheckHealth);
                responseMsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                SOACheckHealthResponse responseBody = new SOACheckHealthResponse();
                responseBody.Ok = true;
                responseBody.Message = "在线";

                responseMsg.SetMessageBody(responseBody);
                SendMessage(responseMsg);

                return;
            }

            base.ReciveMessage(message);
        }

        private void HandleChunkedWebRequest(Message message, SOATransferWebRequest request)
        {
            try
            {
                if (request.IsMeta)
                {
                    var meta = GetParam<WebRequest>(message.MessageHeader.CustomData, request.Param);
                    var context = CreateChunkedWebRequestContext(meta);

                    lock (chunkRequestLocker)
                    {
                        chunkRequestContexts[request.ClientTransactionID] = context;
                    }

                    AckMessage();
                    return;
                }


                ChunkedWebRequestContext chunkContext = null;
                lock (chunkRequestLocker)
                {
                    chunkRequestContexts.TryGetValue(request.ClientTransactionID, out chunkContext);
                }

                if (chunkContext == null)
                {
                    throw new Exception($"分片请求上下文不存在:{request.ClientTransactionID}");
                }

                if (request.Param != null && request.Param.Length > 0)
                {
                    lock (chunkContext)
                    {
                        if (chunkContext.RequestStream == null)
                        {
                            throw new Exception($"分片请求流未初始化:{request.ClientTransactionID}");
                        }

                        chunkContext.RequestStream.Write(request.Param, 0, request.Param.Length);
                        chunkContext.RequestStream.Flush();
                    }
                }

                AckMessage();

                if (!request.IsLastChunk)
                {
                    return;
                }

                lock (chunkRequestLocker)
                {
                    chunkRequestContexts.Remove(request.ClientTransactionID);
                }

                lock (chunkContext)
                {
                    try
                    {
                        chunkContext.RequestStream?.Flush();
                    }
                    catch { }

                    try
                    {
                        chunkContext.RequestStream?.Dispose();
                    }
                    catch { }

                    chunkContext.RequestStream = null;
                }

                var responseMsg = new Message((int)SOAMessageType.SOATransferWebResponse);
                responseMsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                SOATransferWebResponse responseBody = new SOATransferWebResponse();
                responseBody.ClientTransactionID = request.ClientTransactionID;
                responseBody.ClientId = request.ClientId;

                var results = SendChunkedRequestAndReadResponse(chunkContext);
                foreach (var result in results)
                {
                    responseBody.Result = BuildResult(message.MessageHeader.CustomData, result);
                    responseBody.IsSuccess = true;
                    responseBody.ErrMsg = null;

                    responseMsg.SetMessageBody(responseBody);
                    SendMessage(responseMsg);

                    responseBody = new SOATransferWebResponse
                    {
                        ClientTransactionID = request.ClientTransactionID,
                        ClientId = request.ClientId
                    };
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error("HandleChunkedWebRequest出错", ex);
                lock (chunkRequestLocker)
                {
                    chunkRequestContexts.Remove(request.ClientTransactionID);
                }
                try
                {
                    var responseMsg = new Message((int)SOAMessageType.SOATransferWebResponse);
                    responseMsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                    responseMsg.SetMessageBody(new SOATransferWebResponse
                    {
                        ClientTransactionID = request.ClientTransactionID,
                        ClientId = request.ClientId,
                        IsSuccess = false,
                        ErrMsg = ex.Message,
                        Result = Encoding.UTF8.GetBytes(ex.Message)
                    });
                    SendMessage(responseMsg);
                }
                catch
                {
                }
            }

            void AckMessage()
            {
                //确认消息
                var ackMsg = new Message((int)SOAMessageType.AckTrunkRequest);
                ackMsg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
                ackMsg.SetMessageBody(new AckChunkRequest
                {
                    ClientId = request.ClientTransactionID,
                    ChunkNo = request.ChunkNo
                });
                var ackResp = SendMessageAnsy<AckChunkResponse>(ackMsg);
                if (!ackResp.Success)
                {
                    throw new Exception($"数据接收失败，确认失败:{ackResp.Message}");
                }
            }
        }

        private ChunkedWebRequestContext CreateChunkedWebRequestContext(WebRequest webRequest)
        {
            var list = ServiceConfig.ReadConfig()?.WebMappers;
            WebMapper matchedMapper = WebTransferSvcHelper.Find(webRequest, list);
            if (matchedMapper == null)
            {
                throw new Exception("Not Found");
            }

            var virUrl = webRequest.VirUrl;
            if (!string.IsNullOrWhiteSpace(matchedMapper.MappingRoot) && virUrl.StartsWith(matchedMapper.MappingRoot, StringComparison.OrdinalIgnoreCase))
            {
                virUrl = virUrl.Substring(matchedMapper.MappingRoot.Length);
            }

            var realUrl = matchedMapper.TragetWebHost;
            if (!string.IsNullOrWhiteSpace(virUrl))
            {
                realUrl = realUrl.TrimEnd('/') + '/' + virUrl.TrimStart('/');
            }

            WebProxy proxy = null;
            if (!string.IsNullOrEmpty(matchedMapper.UseProxyName))
            {
                proxy = ServiceConfig.ReadConfig().WebProxies?.FirstOrDefault(p => p.Name.Equals(matchedMapper.UseProxyName, StringComparison.OrdinalIgnoreCase));
            }

            var httpRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(realUrl);
            httpRequest.Method = webRequest.Method;
            httpRequest.AllowAutoRedirect = false;
            httpRequest.KeepAlive = false;
            httpRequest.AllowWriteStreamBuffering = false;
            httpRequest.SendChunked = true;

            if (proxy != null)
            {
                httpRequest.SetCredential(proxy.Address, proxy.UserName, proxy.UserPassWord);
            }

            foreach (var kv in webRequest.Headers)
            {
                if (kv.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                    || kv.Key.Equals("host", StringComparison.OrdinalIgnoreCase)
                    || kv.Key.Equals("Expect", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (kv.Key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                {
                    httpRequest.Referer = kv.Value;
                }
                else if (kv.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (kv.Key.Equals("Proxy-Connection", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (kv.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                {
                    httpRequest.UserAgent = kv.Value;
                }
                else if (kv.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                {
                    httpRequest.Accept = kv.Value;
                }
                else if (kv.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    httpRequest.ContentType = kv.Value;
                }
                else if (kv.Key.Equals("If-Modified-Since", StringComparison.OrdinalIgnoreCase))
                {
                    httpRequest.IfModifiedSince = DateTime.Parse(kv.Value);
                }
                else
                {
                    httpRequest.Headers.Add(kv.Key, kv.Value);
                }
            }

            if (!httpRequest.Headers.AllKeys.Any(p => "Cookie".Equals(p, StringComparison.OrdinalIgnoreCase)))
            {
                httpRequest.CookieContainer = new System.Net.CookieContainer();
                var cookDomain = httpRequest.Host.Split(':').First();
                foreach (var kv in webRequest.Cookies)
                {
                    httpRequest.CookieContainer.Add(new System.Net.Cookie
                    {
                        Name = kv.Key,
                        Value = WebUtility.UrlEncode(kv.Value),
                        Domain = cookDomain,
                        Path = "/"
                    });
                }
            }

            if (webRequest.TimeOut > 0)
            {
                httpRequest.Timeout = webRequest.TimeOut;
            }

            var requestStream = httpRequest.GetRequestStream();

            return new ChunkedWebRequestContext
            {
                Request = webRequest,
                HttpRequest = httpRequest,
                RequestStream = requestStream,
                RealUrl = realUrl,
                Proxy = proxy
            };
        }

        private IEnumerable<WebResponse> SendChunkedRequestAndReadResponse(ChunkedWebRequestContext chunkContext)
        {
            var responses = new List<WebResponse>();
            try
            {
                using (var httpResponse = (System.Net.HttpWebResponse)chunkContext.HttpRequest.GetResponse())
                {
                    FillWebResponseChunks(chunkContext.Request, chunkContext.RealUrl, httpResponse, responses);
                }
            }
            catch (System.Net.WebException webEx)
            {
                var httpResponse = webEx.Response as System.Net.HttpWebResponse;
                if (httpResponse != null)
                {
                    using (httpResponse)
                    {
                        FillWebResponseChunks(chunkContext.Request, chunkContext.RealUrl, httpResponse, responses);
                    }
                }
                else
                {
                    responses.Add(new WebResponse
                    {
                        ResponseCode = 500,
                        ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8"),
                        ResponseData = Encoding.UTF8.GetBytes(webEx.Message),
                        IsLast = true
                    });
                }
            }
            catch (Exception ex)
            {
                responses.Add(new WebResponse
                {
                    ResponseCode = 500,
                    ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8"),
                    ResponseData = Encoding.UTF8.GetBytes(ex.Message),
                    IsLast = true
                });
            }

            foreach (var item in responses)
            {
                yield return item;
            }
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

        private IEnumerable<WebResponse> DoWebResponseWithHttpClient(WebRequest request, string realUrl)
        {
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

                if (request.Cookies?.Any() == true && !request.Headers.Any(p => "Cookie".Equals(p.Key, StringComparison.OrdinalIgnoreCase)))
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
                    WebResponse response = new WebResponse();
                    response.Headers = new Dictionary<string, string>();
                    var headers = httpResponseMessage.Headers.ToList();
                    AddHeader(response,headers);

                    response.ContentType = httpResponseMessage.Content.Headers.ContentType?.ToString();
                    AddHeader(response, httpResponseMessage.Content.Headers.ToList());

                    response.ResponseCode = (int)httpResponseMessage.StatusCode;

                    var s = httpResponseMessage.Content.ReadAsStreamAsync().Result;

                    var buffer = new byte[1024 * 1000];
                    var readCount = s.Read(buffer, 0, buffer.Length);
                    var no = 0;

                    while (true)
                    {
                        byte[] next = null;
                        var readCount2 = 0;

                        response.No = ++no;

                        if (readCount == buffer.Length)
                        {
                            next = new byte[1024 * 1000];
                            readCount2 = s.Read(next, 0, next.Length);
                        }

                        if (next == null)
                        {
                            response.IsLast = true;
                            byte[] newArray = buffer;
                            if (readCount < buffer.Length)
                            {
                                newArray = new byte[readCount];
                                Array.Copy(buffer, newArray, readCount);
                            }

                            response.ResponseData = newArray;
                            yield return response;
                            yield break;
                        }
                        else
                        {
                            response.ResponseData = buffer;

                            yield return response;
                        }

                        response = new WebResponse();

                        buffer = next;
                        readCount = readCount2;
                    }
                }
            }

            void AddHeader(WebResponse response,List<KeyValuePair<string,IEnumerable<string>>> headers)
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

        private IEnumerable<WebResponse> DoWebResponseWithHttpWebRequestChunked(WebRequest request, string realUrl, WebProxy proxy, IList<byte[]> requestChunks)
        {
            System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(realUrl);
            webRequest.Method = request.Method;

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

            if (!webRequest.Headers.AllKeys.Any(p => "Cookie".Equals(p, StringComparison.OrdinalIgnoreCase)))
            {
                webRequest.CookieContainer = new System.Net.CookieContainer();
                var cookDomain = webRequest.Host.Split(':').First();
                foreach (var kv in request.Cookies)
                {
                    webRequest.CookieContainer.Add(new System.Net.Cookie
                    {
                        Name = kv.Key,
                        Value = WebUtility.UrlEncode(kv.Value),
                        Domain = cookDomain,
                        Path = "/"
                    });
                }
            }

            if (request.TimeOut > 0)
            {
                webRequest.Timeout = request.TimeOut;
            }

            webRequest.ContentLength = request.InputDataLength > 0 ? request.InputDataLength : requestChunks.Sum(p => p == null ? 0 : p.Length);

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                foreach (var chunk in requestChunks)
                {
                    if (chunk == null || chunk.Length == 0)
                    {
                        continue;
                    }

                    requestStream.Write(chunk, 0, chunk.Length);
                }
            }

            var responses = new List<WebResponse>();
            try
            {
                using (System.Net.HttpWebResponse webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse())
                {
                    FillWebResponseChunks(request, realUrl, webResponse, responses);
                }
            }
            catch (System.Net.WebException webEx)
            {
                var httpResponse = webEx.Response as System.Net.HttpWebResponse;
                if (httpResponse != null)
                {
                    using (httpResponse)
                    {
                        FillWebResponseChunks(request, realUrl, httpResponse, responses);
                    }
                }
                else
                {
                    responses.Add(new WebResponse
                    {
                        ResponseCode = 500,
                        ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8"),
                        ResponseData = Encoding.UTF8.GetBytes(webEx.Message),
                        IsLast = true
                    });
                }
            }
            catch (Exception ex)
            {
                responses.Add(new WebResponse
                {
                    ResponseCode = 500,
                    ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8"),
                    ResponseData = Encoding.UTF8.GetBytes(ex.Message),
                    IsLast = true
                });
            }

            foreach (var item in responses)
            {
                yield return item;
            }
        }

        private void FillWebResponseChunks(WebRequest request, string realUrl, System.Net.HttpWebResponse webResponse, List<WebResponse> responses)
        {
            var response = new WebResponse();
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

            response.ResponseCode = (int)webResponse.StatusCode;
            response.ContentType = webResponse.ContentType;
            Stream s = webResponse.GetResponseStream();

            if (s == null)
            {
                response.IsLast = true;
                response.ResponseData = new byte[0];
                responses.Add(response);
                return;
            }

            var buffer = new byte[1024 * 4];
            var readCount = 0;
            using (var ms = new MemoryStream())
            {
                while ((readCount = s.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, readCount);
                }

                response.IsLast = true;
                response.ResponseData = ms.ToArray();
                responses.Add(response);
            }
        }

        private IEnumerable<WebResponse> DoWebResponseWithHttpWebRequest(WebRequest request, string realUrl,WebProxy proxy)
        {
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

            if (!webRequest.Headers.AllKeys.Any(p => "Cookie".Equals(p, StringComparison.OrdinalIgnoreCase)))
            {
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

            var responses = new List<WebResponse>();
            try
            {
                using (System.Net.HttpWebResponse webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse())
                {
                    FillWebResponseChunks(request, realUrl, webResponse, responses);
                }
            }
            catch (System.Net.WebException webEx)
            {
                var httpResponse = webEx.Response as System.Net.HttpWebResponse;
                if (httpResponse != null)
                {
                    using (httpResponse)
                    {
                        FillWebResponseChunks(request, realUrl, httpResponse, responses);
                    }
                }
                else
                {
                    responses.Add(new WebResponse
                    {
                        ResponseCode = 500,
                        ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8"),
                        ResponseData = Encoding.UTF8.GetBytes(webEx.Message),
                        IsLast = true
                    });
                }
            }
            catch (Exception ex)
            {
                responses.Add(new WebResponse
                {
                    ResponseCode = 500,
                    ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8"),
                    ResponseData = Encoding.UTF8.GetBytes(ex.Message),
                    IsLast = true
                });
            }

            foreach (var item in responses)
            {
                yield return item;
            }
        }

        private IEnumerable<WebResponse> DoWebResponse(WebRequest request)
        {
            IEnumerable<WebResponse> responses = null;
            
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
                    //|| request.Cookies?.Any() == true
                    //|| request.Headers?.Keys.Any(p=>p.Equals("Cookie",StringComparison.OrdinalIgnoreCase)) == true
                    )
                {
                    responses = DoWebResponseWithHttpWebRequest(request, realUrl, proxy);
                }
                else
                {
                    responses = DoWebResponseWithHttpClient(request, realUrl);
                }
            }

            foreach (var response in responses)
            {

                if (!string.IsNullOrEmpty(matchedMapper.MappingRoot))
                {
                    if (response.Headers?.Any() == true)
                    {
                        foreach (var head in response.Headers)
                        {
                            if (head.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!matchedMapper.NoRewirteCookie)
                                {
                                    response.Headers[head.Key] = Regex.Replace(response.Headers[head.Key], @"Path=[^;]+", "Path=/" + matchedMapper.MappingRoot.TrimStart('/'), RegexOptions.IgnoreCase);
                                }
                                break;
                            }
                        }
                    }
                }

                yield return response;
            }
        }

        public virtual object DoResponse(int funcId, byte[] Param,string clientid,Dictionary<string,string> header)
        {
            if (funcId == Func_WebRequest)
            {
                try
                {
                    return DoWebResponse(GetParam<WebRequest>(header, Param));
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
                req.RedirectTcpIps = RedirectTcpServiceServer.BindIps;
                req.RedirectTcpPort = RedirectTcpServiceServer.GetBindTcpPort();
            }
            if (SupportUDPServiceRedirect)
            {
                req.RedirectUdpIps = RedirectUpdServiceServer.BindIps;
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
            int trytime = 0, maxtrytimes = 10;
            while (true)
            {
                try
                {
                    if (RegisterService())
                    {
                        LogHelper.Instance.Info("登录后注册服务成功");
                        break;
                    }
                    else
                    {
                        LogHelper.Instance.Info("登录后注册服务失败");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("登录后注册服务失败", ex);
                }
                if (trytime++ >= maxtrytimes)
                {
                    break;
                }
                Thread.Sleep(100 * trytime);
            }
        }

        protected override void OnError(Exception e)
        {
            base.OnError(e);
        }

        protected override void OnSessionResume()
        {
            base.OnSessionResume();

            int trytime = 0, maxtrytimes = 10;
            while (true)
            {
                try
                {
                    if (RegisterService())
                    {
                        LogHelper.Instance.Info("会话恢复后注册服务成功");
                        break;
                    }
                    else
                    {
                        LogHelper.Instance.Info("会话恢复后注册服务失败");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("会话恢复后注册服务失败", ex);
                }
                if (trytime++ >= maxtrytimes)
                {
                    break;
                }
                Thread.Sleep(100 * trytime);
            }
        }

        #region 开通直连服务
        private ESBRedirectService RedirectTcpServiceServer = null;
        private ESBUDPService RedirectUpdServiceServer = null;

        private bool CheckIpIsChange(System.Net.IPAddress[] newIps)
        {
            if (RedirectTcpServiceServer == null)
            {
                return false;
            }
            var binds = RedirectTcpServiceServer.BindIps;
            if (binds.Length != newIps.Length)
            {
                return true;
            }

            foreach (var ip in newIps)
            {
                if (!binds.Any(q => q == ip.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        public void StartRedirectService()
        {
            var addrs = NetworkHelper.GetActiveIpV4s(true);
            var bindips = NetworkHelper.GetActiveIpV4s();

            if (addrs.Length > 0)
            {
                int iport = 0;
                var ipIsChanged = CheckIpIsChange(bindips);

                if (SupportTcpServiceRidrect && RedirectTcpServiceServer == null)
                {
                    int trytimes = 0;
                    while (true)
                    {
                        try
                        {
                            if (RedirectTcpServiceServer != null)
                            {
                                RedirectTcpServiceServer.Dispose();
                            }

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
                else if (SupportTcpServiceRidrect && RedirectTcpServiceServer != null && ipIsChanged)
                {
                    RedirectTcpServiceServer.BindIps = bindips.Select(p => p.ToString()).ToArray();
                    LogHelper.Instance.Info("IP改变：" + ipIsChanged+","+string.Join("、",RedirectTcpServiceServer.BindIps));
                }

                if (SupportUDPServiceRedirect && RedirectUpdServiceServer == null)
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
                else if (SupportUDPServiceRedirect && RedirectUpdServiceServer != null && ipIsChanged)
                {
                    RedirectUpdServiceServer.BindIps = bindips.Select(p => p.ToString()).ToArray();
                }
            }
        }

        #endregion

    }
}
