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

namespace LJC.FrameWork.SOA
{
    public class ESBService:SessionClient,IService
    {
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

        private object DoWebResponse(WebRequest request)
        {
            WebResponse response = new WebResponse();

            var webMappers = ServiceConfig.ReadConfig()?.WebMappers;

            WebMapper matchedMapper = null;
            if (webMappers != null && webMappers.Any())
            {

                foreach (var map in webMappers)
                {
                    if (!string.IsNullOrWhiteSpace(map.RegexRoute) && Regex.IsMatch(request.Url, map.RegexRoute, RegexOptions.IgnoreCase))
                    {
                        matchedMapper = map;
                        break;
                    }
                    else if (request.Url.StartsWith(map.VirRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedMapper = map;
                        break;
                    }
                }
            }

            if (matchedMapper != null)
            {
                System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(matchedMapper.LocalHost + request.Url);
                webRequest.Method = request.Method;
                webRequest.AllowAutoRedirect = true;

                foreach (var kv in request.Headers)
                {
                    if (kv.Key.Equals("host", StringComparison.OrdinalIgnoreCase))
                    {
                        webRequest.Host = kv.Value;
                    }
                    else if (kv.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                    {
                        webRequest.KeepAlive = "keep-alive".Equals(kv.Value, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (kv.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                    {
                        webRequest.UserAgent = kv.Value;
                    }
                    else if (kv.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                    {
                        webRequest.Accept = kv.Value;
                    }
                    else
                    {
                        webRequest.Headers.Add(kv.Key, kv.Value);
                    }
                }

                webRequest.CookieContainer = new System.Net.CookieContainer();
                foreach (var kv in request.Cookies)
                {
                    webRequest.CookieContainer.Add(new System.Net.Cookie
                    {
                        Name = kv.Key,
                        Value = WebUtility.UrlEncode(kv.Value),
                        Domain=webRequest.Host.Split(':').First(),
                        Path="/"
                    });
                }

                if (request.TimeOut > 0)
                {
                    webRequest.Timeout = request.TimeOut;
                }
                //if (SupportCompression)
                {
                    //webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                    webRequest.Headers.Add(System.Net.HttpRequestHeader.AcceptEncoding, "gzip");
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

                using (System.Net.HttpWebResponse webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse())
                {
                    //int statusCode = (int)webResponse.StatusCode;
                    //if (statusCode >= 200 && statusCode < 400)

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

                        response.ResponseData = contentBuffer;
                    }

                    response.ResponseCode = (int)webResponse.StatusCode;
                    response.ContentType = webResponse.ContentType;

                    //response.Cookies = new Dictionary<string, string>();
                    //if (webResponse.Cookies.Count > 0)
                    //{

                    //    for (int i = 0; i < webResponse.Cookies.Count; i++)
                    //    {
                    //        response.Cookies.Add(webResponse.Cookies[i].Name, webResponse.Cookies[i].Value);
                    //    }
                    //}
                }
            }


            return response;
        }

        public virtual object DoResponse(int funcId, byte[] Param,string clientid)
        {
            if (funcId == Func_WebRequest)
            {
                return DoWebResponse(EntityBufCore.DeSerialize<WebRequest>(Param));
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
            var localhost = System.Net.Dns.GetHostName();
            var addrs = System.Net.Dns.GetHostAddresses(localhost);
            List<System.Net.IPAddress> bindips = new List<System.Net.IPAddress>();
            if (addrs != null && addrs.Length > 0)
            {
                foreach (var addr in addrs)
                {
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        bindips.Add(addr);
                    }
                }
            }

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
