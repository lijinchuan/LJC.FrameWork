using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using LJC.FrameWork.LogManager;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.SOA.Contract;
using System.Text.RegularExpressions;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.SOA
{
    public class ESBServer:SocketEasy.Sever.SessionServer
    {
        private static object LockObj = new object();
        protected List<ESBServiceInfo> ServiceContainer = new List<ESBServiceInfo>();
        //internal Dictionary<string, Session[]> ClientSessionList = new Dictionary<string, Session[]>();
        internal Dictionary<string, object[]> ClientSessionList = new Dictionary<string, object[]>();
        internal static ReaderWriterLockSlim ConatinerLock = new ReaderWriterLockSlim();

        /// <summary>
        /// 管理页面端口
        /// </summary>
        private string ManagerWebPortStr = System.Configuration.ConfigurationManager.AppSettings["esbmanport"];
        

        #region 管理web
        public class DefaultHander : LJC.FrameWork.Net.HTTP.Server.IRESTfulHandler
        {
            private ESBServer _esb = null;
            public DefaultHander(ESBServer esb)
            {
                _esb = esb;
            }

            public bool Process(LJC.FrameWork.Net.HTTP.Server.HttpServer server, LJC.FrameWork.Net.HTTP.Server.HttpRequest request, LJC.FrameWork.Net.HTTP.Server.HttpResponse response, Dictionary<string, string> param)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"<html>
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
            <meta charset = ""UTF-8"" >
                     <title>ESB实时数据</title >
                   </head ><body>");
                sb.Append(@"<style type=""text/css"">
        table{
            border:solid 1px lightblue; width:100%;
        }
        th{
            height:30px;
        }

        td{
            border-top:dashed 1px #ccc;
            height:22px;
        }

    </style>
                ");
                sb.AppendFormat("当前时间:{0}<br/><br/>",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendFormat("线程池参数:{0}<br/>", ThreadPoolHelper.PrintDetail());
                var servicelist = _esb.ServiceContainer.Select(p => p).ToList();
                sb.AppendFormat("当前注册了{0}个服务实例<br/>", servicelist.Count);
                if (servicelist.Count > 0)
                {
                    sb.AppendFormat("<table>");
                    sb.AppendFormat("<tr><th>服务号</th><th>服务实例</th></tr>");
                    foreach (var gp in servicelist.GroupBy(p => p.ServiceNo))
                    {
                        sb.AppendFormat("<tr>");
                        sb.AppendFormat("<td>{0}</td>", gp.Key);
                        sb.Append("<td>");
                        sb.Append("<table>");
                        sb.Append("<tr><th>服务名称</th><th>端点名称</th><th>ID</th><th>服务器地址</th><th>TCP直连</th><th>UDP直连</th></tr>");
                        foreach (var item in gp)
                        {
                            if (DateTime.Now.Subtract(item.Session.LastSessionTime).TotalMinutes > 1)
                            {
                                lock (this._esb.ServiceContainer)
                                {
                                    _esb.ServiceContainer.Remove(item);
                                    item.Session.Close("on resp over 1 mins");
                                }
                            }

                            sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}:{4}</td><td>{5}</td><td>{6}</td></tr>",
                                item.ServiceName,item.EndPointName,item.Session.SessionID, item.Session.IPAddress, item.Session.Port,
                                item.RedirectTcpIps == null ? "" : (string.Join(",", item.RedirectTcpIps) + ":" + item.RedirectTcpPort),
                                item.RedirectUdpIps == null ? "" : (string.Join(",", item.RedirectUdpIps) + ":" + item.RedirectUdpPort));
                        }
                        sb.Append("</table>");
                        sb.Append("</td>");
                        sb.AppendFormat("</tr>");
                    }
                    sb.AppendFormat("</table>");
                }

                //客户端
                sb.Append("<br/>");
                var clients = _esb.GetConnectedList().Select(p => p).ToList();
                sb.AppendFormat("当前连接了{0}个客户端", clients.Count);
                sb.Append("<table>");
                sb.Append("<tr>");
                sb.AppendFormat("<th>clientid</th><th>地址</th><th>连接时间</th><th>上次心跳时间</th><th>连接时长(分钟)</th><th>发送字节</th><th>接收字节</th>");
                sb.Append("</tr>");
                HashSet<string> clienthash = new HashSet<string>();
                foreach (var item in clients)
                {
                    if (!clienthash.Contains(item.Key))
                    {
                        clienthash.Add(item.Key);
                    }

                    sb.AppendFormat("<tr><td>{0}</td><td>{1}:{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td></tr>", item.Key, item.Value.IPAddress, item.Value.Port,
                        item.Value.ConnectTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        item.Value.LastSessionTime.ToString("yyyy-MM-dd HH:mm:ss"), Math.Round(item.Value.LastSessionTime.Subtract(item.Value.ConnectTime).TotalMinutes, 3),
                        item.Value.BytesSend, item.Value.BytesRev);
                }
                if (clients.Count > 0)
                {
                    sb.AppendFormat("<tr><td colspan='5'>{0}</td><td>{1}kb</td><td>{2}kb</td></tr>", "合计", (clients.Sum(p => p.Value.BytesSend) / 1024), (clients.Sum(p => p.Value.BytesRev) / 1024));
                }
                sb.Append("</table>");
                sb.Append("<br/>");
                //最快的节点
                sb.AppendFormat("功能耗时统计(服务器会选择功能总耗时少的作为最快的选择)");
                sb.AppendFormat("<table>");
                sb.AppendFormat("<tr><td>服务号</td><td>功能耗时</td></tr>");

                foreach (var item in servicelist.GroupBy(p => p.ServiceNo).Where(p => p.Count() > 1))
                {
                    StringBuilder sbb = new StringBuilder();
                    sbb.Append("<table>");
                    sbb.Append("<tr><th>服务名称</th><th>端点名称</th><th>端口地址</th><th>功能耗时统计</th></tr>");
                    foreach (var s in item)
                    {
                        sbb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", s.ServiceName, s.EndPointName, s.Session.IPAddress + ":" + s.Session.Port,
                           "<table><tr><td>功能号</td><td>用时秒数</td></tr>" + string.Join("", s.FunctionUsedSecs.Select(k => "<tr><td>" + k.Key + "</td><td>" + k.Value+ "</td></tr>")) + "</table>");
                    }
                    sbb.Append("</table>");
                    
                    sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", item.Key, sbb.ToString());
                }

                sb.AppendFormat("</table>");
                //客户端
                sb.Append("<br/>");
                var liveclients = _esb.ClientSessionList.Select(p => p).ToList();
                sb.AppendFormat("当前活跃{0}个客户端", liveclients.Count);
                sb.Append("<table>");
                sb.Append("<tr>");
                sb.AppendFormat("<th>任务ID</th><th>clientid</th><th>地址</th><th>连接时间</th><th>上次心跳时间</th><th>连接时长(分钟)</th><th>发送字节</th><th>接收字节</th>");
                sb.Append("</tr>");
                foreach (var item in liveclients)
                {
                    var session = item.Value[0] as Session;
                    if (session == null)
                    {
                        continue;
                    }
                    var seviceInfo = (ESBServiceInfo)item.Value[1];
                    if (!clienthash.Contains(session.SessionID)|| !clienthash.Contains(seviceInfo.Session.SessionID))
                    {
                        lock (_esb.ClientSessionList)
                        {
                            _esb.ClientSessionList.Remove(item.Key);
                        }
                    }

                    sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}:{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td></tr>", item.Key, session.SessionID, session.IPAddress, session.Port,
                        session.ConnectTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        session.LastSessionTime.ToString("yyyy-MM-dd HH:mm:ss"), Math.Round(session.LastSessionTime.Subtract(session.ConnectTime).TotalMinutes, 3),
                        session.BytesSend, session.BytesRev);
                }
                sb.Append("</table>");

                sb.Append("</body></html>");
                response.Content = sb.ToString();
                return true;
            }
        }
        #endregion

        public ESBServer(int port)
            : base(port)
        {
            ServerModeNeedLogin = false;

            if (!string.IsNullOrWhiteSpace(ManagerWebPortStr))
            {
                var manport = int.Parse(ManagerWebPortStr);

                LJC.FrameWork.Net.HTTP.Server.HttpServer manhttpserver = new Net.HTTP.Server.HttpServer(new Net.HTTP.Server.Server(manport));
                manhttpserver.Handlers.Add(new LJC.FrameWork.Net.HTTP.Server.RESTfulApiHandlerBase(LJC.FrameWork.Net.HTTP.Server.HMethod.GET, "/esb/index", new List<string>() { }, new DefaultHander(this)));

                LogHelper.Instance.Info("管理WEB服务开启:" + manport);
            }

            SimulateServerManager.TransferRequest = DoWebRequest;
            SimulateServerManager.AddDefaultServer();
        }

        internal System.Collections.Concurrent.ConcurrentDictionary<string, Session> GetConnectedList()
        {
            return this._connectSocketDic;
        }

        internal void DoTransferResponse(SOATransferResponse response)
        {
            try
            {
                object[] carrayObjs = null;

                ConatinerLock.EnterReadLock();
                ClientSessionList.TryGetValue(response.ClientTransactionID, out carrayObjs);
                ConatinerLock.ExitReadLock();

                if (carrayObjs != null)
                {
                    ConatinerLock.EnterWriteLock();
                    ClientSessionList.Remove(response.ClientTransactionID);
                    ConatinerLock.ExitWriteLock();

                    SOAResponse resp = new SOAResponse();
                    Message msgRet = new Message((int)SOAMessageType.DoSOAResponse);
                    msgRet.MessageHeader.TransactionID = response.ClientTransactionID;
                    resp.IsSuccess = response.IsSuccess;
                    resp.ErrMsg = response.ErrMsg;
                    resp.Result = response.Result;

                    msgRet.SetMessageBody(resp);
                    var session = (Session)carrayObjs[0];
                    var serviceInfo = (ESBServiceInfo)carrayObjs[1];
                    var funid = (int)carrayObjs[2];
                    var startTime = (DateTime)carrayObjs[3];
                    var useSecs = (int)Math.Round(DateTime.Now.Subtract(startTime).TotalSeconds, 0);
                    session.SendMessage(msgRet);

                    serviceInfo.FunctionUsedSecs.AddOrUpdate(funid, useSecs, (key, old) => old - 30 + useSecs);

                    if (SocketApplicationEnvironment.TraceMessage)
                    {
                        var toulp = (Tuple<int, int>)session.Tag;
                        LogHelper.Instance.Debug(string.Format("SOA响应耗时,请求序列号:{0},服务号:{1},功能号:{2},用时:{3},结果:{4}",
                            response.ClientTransactionID, toulp.Item1, toulp.Item2, DateTime.Now.Subtract(session.BusinessTimeStamp).TotalMilliseconds + "毫秒",
                            Convert.ToBase64String(response.Result)));
                    }
                }
                else
                {
                    Exception ex = new Exception(string.Format("DoTransferResponse(SOATransferResponse response)失败,请求序列号:{0}",response.ClientTransactionID));
                    ex.Data.Add("response.ClientId", response.ClientId);

                    LogHelper.Instance.Error("DoTransferResponse出错", ex);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("请求序列号", response.ClientTransactionID);

                LogHelper.Instance.Error("DoTransferResponse出错", ex);
            }
        }

        internal void DoTransferWebResponse(SOATransferWebResponse response)
        {
            try
            {
                object[] carrayObjs = null;

                ConatinerLock.EnterReadLock();
                ClientSessionList.TryGetValue(response.ClientTransactionID, out carrayObjs);
                ConatinerLock.ExitReadLock();

                if (carrayObjs != null)
                {
                    ConatinerLock.EnterWriteLock();
                    ClientSessionList.Remove(response.ClientTransactionID);
                    ConatinerLock.ExitWriteLock();

                    if (response.Result != null)
                    {
                        carrayObjs[carrayObjs.Length - 1] = EntityBufCore.DeSerialize<WebResponse>(response.Result);
                    }

                    var ae = (carrayObjs[0] as AutoResetEvent);
                    ae.Set();
                }
                else
                {
                    Exception ex = new Exception(string.Format("DoTransferWebResponse(TransferWebResponse response)失败,请求序列号:{0}", response.ClientTransactionID));
                    ex.Data.Add("response.ClientId", response.ClientId);

                    LogHelper.Instance.Error("DoTransferResponse出错", ex);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("请求序列号", response.ClientTransactionID);

                LogHelper.Instance.Error("DoTransferResponse出错", ex);
            }
        }

        public virtual object DoRequest(int funcId,byte[] param)
        {
            switch(funcId)
            {
                case Consts.FunNo_ECHO:
                    {
                        return new SOAServerEchoResponse
                        {
                            Ok=true
                        };
                    }
                case Consts.FunNo_Environment:
                    {
                        return new SOAServerEnvironmentResponse
                        {
                            MachineName=Environment.MachineName,
                            OSVersion=Environment.OSVersion.VersionString,
                            ProcessorCount=Environment.ProcessorCount,
                        };
                    }
                case Consts.FunNo_ExistsAServiceNo:
                    {
                        int serviceno=EntityBuf.EntityBufCore.DeSerialize<int>(param);
                        return ServiceContainer.Exists(p => p.ServiceNo == serviceno);
                    }
                case Consts.FunNo_GetRegisterServiceInfo:
                    {
                        var req=EntityBuf.EntityBufCore.DeSerialize<GetRegisterServiceInfoRequest>(param);
                        GetRegisterServiceInfoResponse resp = new GetRegisterServiceInfoResponse();

                        resp.ServiceNo = req.ServiceNo;
                        resp.Infos = ServiceContainer.Where(p => p.ServiceNo.Equals(req.ServiceNo)).Select(p => new RegisterServiceInfo
                        {
                            ServiceNo=p.ServiceNo,
                            RedirectTcpIps=p.RedirectTcpIps,
                            RedirectTcpPort=p.RedirectTcpPort,
                            RedirectUdpIps=p.RedirectUdpIps,
                            RedirectUdpPort=p.RedirectUdpPort,
                        }).ToArray();

                        return resp;
                    }
                case Consts.FunNo_ListServiceInfos:
                    {
                        var req = EntityBuf.EntityBufCore.DeSerialize<ListServiceInfosRequest>(param);
                        ListServiceInfosResponse resp = new ListServiceInfosResponse();

                        resp.Services = ServiceContainer.Select(p => new RegisterServiceInfo
                        {
                            ServiceNo = p.ServiceNo,
                            RedirectTcpIps = p.RedirectTcpIps,
                            RedirectTcpPort = p.RedirectTcpPort,
                            RedirectUdpIps = p.RedirectUdpIps,
                            RedirectUdpPort = p.RedirectUdpPort,
                        }).ToArray();

                        return resp;

                    }
                default:
                    {
                        throw new NotImplementedException(string.Format("未实现的功能:{0}", funcId));
                    }
            }
        }

        internal WebResponse DoWebRequest(WebRequest webRequest)
        {
            var list = ServiceContainer.ToList();
            ESBServiceInfo serviceInfo = null;
            WebMapper webMapper = null;
            foreach (var item in list.Where(p => p.WebMappers != null && p.WebMappers.Any()))
            {
                webMapper = WebTransferSvcHelper.Find(webRequest, item.WebMappers);
                if (webMapper != null)
                {
                    serviceInfo = item;
                    break;
                }
            }

            if (webMapper == null)
            {
                return null;
            }

            try
            {
                if (DateTime.Now.Subtract(serviceInfo.Session.LastSessionTime).TotalSeconds > 30)
                {
                    lock (LockObj)
                    {
                        ServiceContainer.Remove(serviceInfo);
                        serviceInfo.Session.Close("no resp over 30s");

                        return null;
                    }
                }

                string clientid = Guid.NewGuid().ToString("N");
                SOATransferWebRequest transferrequest = new SOATransferWebRequest();
                transferrequest.ClientId = clientid;
                transferrequest.FundId = 0;
                transferrequest.Param = EntityBuf.EntityBufCore.Serialize(webRequest);

                transferrequest.ClientTransactionID = clientid;

                using (AutoResetEvent autoResetEvent = new AutoResetEvent(false))
                {
                    var carrayObj = new object[] { autoResetEvent, serviceInfo, 0, DateTime.Now, null };
                    try
                    {
                        ConatinerLock.EnterWriteLock();
                        ClientSessionList.Add(clientid, carrayObj);
                    }
                    finally
                    {
                        ConatinerLock.ExitWriteLock();
                    }

                    Message msg = new Message((int)SOAMessageType.SOATransferWebRequest);
                    msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
                    msg.SetMessageBody(transferrequest);

                    if (serviceInfo.Session.SendMessage(msg))
                    {
                        //if (sendAll)
                        //{
                        //    LogHelper.Instance.Debug(string.Format("发送SOA请求,请求序列:{0},服务号:{1},功能号:{2},subMsgTransactionID:{3}",
                        //        msgTransactionID, request.ServiceNo, request.FuncId,subMsgTransactionID));
                        //}
                        if (autoResetEvent.WaitOne(webRequest.TimeOut))
                        {
                            return (WebResponse)carrayObj.Last();
                        }
                        else
                        {
                            try
                            {
                                ConatinerLock.EnterWriteLock();
                                ClientSessionList.Remove(clientid);
                            }
                            finally
                            {
                                ConatinerLock.ExitWriteLock();
                            }
                            throw new TimeoutException();
                        }
                    }
                    else
                    {
                        try
                        {
                            ConatinerLock.EnterWriteLock();
                            ClientSessionList.Remove(clientid);

                            return null;
                        }
                        finally
                        {
                            ConatinerLock.ExitWriteLock();
                        }
                    }
                }
                //var result = SendMessageAnsy<byte[]>(serviceInfo.Session, msg);

                //resp.Result = result;
            }
            catch (Exception ex)
            {
                OnError(ex);

                return null;
            }
        }

        internal void DoTransferRequest(Session session, string msgTransactionID, SOARequest request,bool sendAll=false)
        {
            session.BusinessTimeStamp = DateTime.Now;
            session.Tag = new Tuple<int, int>(request.ServiceNo, request.FuncId);

            SOAResponse resp = new SOAResponse();
            Message msgRet = new Message((int)SOAMessageType.DoSOAResponse);
            msgRet.MessageHeader.TransactionID = msgTransactionID;
            resp.IsSuccess = true;

            //调用本地的方法
            if (request.ServiceNo == 0)
            {
                try
                {
                    var obj = DoRequest(request.FuncId, request.Param);
                    resp.Result = EntityBuf.EntityBufCore.Serialize(obj);
                }
                catch (Exception ex)
                {
                    resp.IsSuccess = false;
                    resp.ErrMsg = ex.Message;
                }
                
                msgRet.SetMessageBody(resp);
                session.SendMessage(msgRet);
            }
            else
            {
                //查询服务
                var serviceInfos = ServiceContainer.FindAll(p => p.ServiceNo.Equals(request.ServiceNo));
                if (serviceInfos == null || serviceInfos.Count == 0)
                {
                    resp.IsSuccess = false;
                    resp.ErrMsg = string.Format("{0}服务未注册。", request.ServiceNo);
                }
                else
                {
                    List<ESBServiceInfo> callList = new List<ESBServiceInfo>();
                    if (resp.IsSuccess)
                    {
                        if (sendAll && serviceInfos.Count > 1)
                        {
                            callList.AddRange(serviceInfos);
                        }
                        else
                        {
                            ESBServiceInfo serviceInfo = null;
                            if (serviceInfos.Count == 1)
                            {
                                serviceInfo = serviceInfos[0];
                                serviceInfo.FunctionUsedSecs.AddOrUpdate(request.FuncId, 30, (key, old) => old + 30);
                            }
                            else
                            {
                                serviceInfo = serviceInfos.FirstOrDefault(p => !p.FunctionUsedSecs.ContainsKey(request.FuncId));
                                if (serviceInfo == null)
                                {
                                    serviceInfo = serviceInfos.OrderBy(p => p.FunctionUsedSecs[request.FuncId]).First();
                                    serviceInfo.FunctionUsedSecs.AddOrUpdate(request.FuncId, 30, (key, old) => old + 30);
                                }
                                else
                                {
                                    //后添加的，默认也是最快的
                                    var minSecs = serviceInfos.Where(p => p != serviceInfo).Min(p => p.FunctionUsedSecs.ContainsKey(request.FuncId) ? p.FunctionUsedSecs[request.FuncId] : long.MaxValue);
                                    if (minSecs != long.MaxValue)
                                    {
                                        serviceInfo.FunctionUsedSecs.AddOrUpdate(request.FuncId, minSecs + 30, (key, old) => old + 30);
                                    }
                                    else
                                    {
                                        serviceInfo.FunctionUsedSecs.AddOrUpdate(request.FuncId, 30, (key, old) => old + 30);
                                    }
                                }

                                //Random rd = new Random();
                                //var idx = rd.Next(1, serviceInfos.Count + 1);
                                //serviceInfo = serviceInfos[idx - 1];
                            }
                            callList.Add(serviceInfo);
                        }

                        try
                        {
                            var canUsedList = new List<ESBServiceInfo>();
                            foreach (var serviceInfo in callList)
                            {
                                if (DateTime.Now.Subtract(serviceInfo.Session.LastSessionTime).TotalSeconds > 30)
                                {
                                    lock (LockObj)
                                    {
                                        ServiceContainer.Remove(serviceInfo);
                                        serviceInfo.Session.Close("no resp over 30s");
                                    }
                                    if (sendAll)
                                    {
                                        throw new Exception("some service instance is down");
                                    }
                                }
                                else
                                {
                                    canUsedList.Add(serviceInfo);
                                }
                            }

                            if (!canUsedList.Any())
                            {
                                var canUseInfo = ServiceContainer.FindAll(p => p.ServiceNo.Equals(request.ServiceNo)).LastOrDefault();
                                if (canUseInfo == null)
                                {
                                    throw new Exception(string.Format("{0}服务可能不可用,30秒无应答。", request.ServiceNo));
                                }
                                canUsedList.Add(canUseInfo);
                            }

                            for (var i = 0; i < canUsedList.Count; i++)
                            {

                                var serviceInfo = canUsedList[i];
                                string clientid = session.SessionID;
                                SOATransferRequest transferrequest = new SOATransferRequest();
                                transferrequest.ClientId = clientid;
                                transferrequest.FundId = request.FuncId;
                                transferrequest.Param = request.Param;
                                var isLast = i == canUsedList.Count - 1;
                                var subMsgTransactionID = msgTransactionID;
                                if (!isLast)
                                {
                                    subMsgTransactionID += "#" + i;
                                }

                                transferrequest.ClientTransactionID = subMsgTransactionID;
                                try
                                {
                                    ConatinerLock.EnterWriteLock();
                                    ClientSessionList.Add(subMsgTransactionID, new object[] { session, serviceInfo, request.FuncId, DateTime.Now });
                                }
                                finally
                                {
                                    ConatinerLock.ExitWriteLock();
                                }

                                Message msg = new Message((int)SOAMessageType.DoSOATransferRequest);
                                msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
                                msg.SetMessageBody(transferrequest);

                                if (serviceInfo.Session.SendMessage(msg))
                                {
                                    //if (sendAll)
                                    //{
                                    //    LogHelper.Instance.Debug(string.Format("发送SOA请求,请求序列:{0},服务号:{1},功能号:{2},subMsgTransactionID:{3}",
                                    //        msgTransactionID, request.ServiceNo, request.FuncId,subMsgTransactionID));
                                    //}
                                    if (isLast)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        ConatinerLock.EnterWriteLock();
                                        ClientSessionList.Remove(subMsgTransactionID);
                                    }
                                    finally
                                    {
                                        ConatinerLock.ExitWriteLock();
                                    }
                                }
                                //var result = SendMessageAnsy<byte[]>(serviceInfo.Session, msg);

                                //resp.Result = result;
                            }
                        }
                        catch (Exception ex)
                        {
                            OnError(ex);

                            resp.IsSuccess = false;
                            resp.ErrMsg = ex.Message;
                        }
                    }
                }

                if (!resp.IsSuccess)
                {
                    msgRet.SetMessageBody(resp);
                    session.SendMessage(msgRet);
                }
            }
        }

        protected override void FromApp(Message message, Session session)
        {
            if (message.IsMessage((int)SOAMessageType.RegisterService))
            {
                Message msg = new Message((int)SOAMessageType.RegisterService);
                msg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                try
                {
                    var req = message.GetMessageBody<RegisterServiceRequest>();

                    if (req.ServiceNo == 0)
                        throw new NotSupportedException("不允许使用服务号:0");

                    lock (LockObj)
                    {
                        var remlist = ServiceContainer.Where(p => p.Session.SessionID != session.SessionID && p.Session.IPAddress.Equals(session.IPAddress) && p.Session.Port.Equals(session.Port) && p.ServiceNo.Equals(req.ServiceNo)).ToList();
                        foreach (var item in remlist)
                        {
                            item.Session.Close($"remove same service instance,new sessionid:{session.SessionID}");
                            ServiceContainer.Remove(item);
                        }

                        if (!ServiceContainer.Any(p => p.Session.SessionID == session.SessionID && p.ServiceNo == req.ServiceNo))
                        {
                            var webMappersData = message.GetCustomData(nameof(ServiceConfig.WebMappers));
                            List<WebMapper> webMappers = new List<WebMapper>();
                            if (!string.IsNullOrWhiteSpace(webMappersData))
                            {
                                webMappers = Comm.SerializerHelper.DeserializerXML<List<WebMapper>>(webMappersData);

                                foreach (var port in webMappers.Where(p => p.MappingPort > 0).Select(p => p.MappingPort).Distinct())
                                {
                                    SimulateServerManager.AddSimulateServer(port);
                                }
                            }

                            ServiceContainer.Add(new ESBServiceInfo
                            {
                                ServiceNo = req.ServiceNo,
                                Session = session,
                                EndPointName = message.GetCustomData("EndPointName") ?? string.Empty,
                                ServiceName = message.GetCustomData("ServiceName") ?? string.Empty,
                                RedirectTcpIps = req.RedirectTcpIps,
                                RedirectTcpPort = req.RedirectTcpPort,
                                RedirectUdpIps = req.RedirectUdpIps,
                                RedirectUdpPort = req.RedirectUdpPort,
                                WebMappers = webMappers
                            });
                        }
                    }

                    msg.SetMessageBody(new RegisterServiceResponse
                    {
                        IsSuccess = true
                    });
                }
                catch (Exception ex)
                {
                    msg.SetMessageBody(new RegisterServiceResponse
                    {
                        ErrMsg = ex.Message,
                        IsSuccess = false
                    });
                }
                session.SendMessage(msg);
                return;
            }
            else if (message.IsMessage((int)SOAMessageType.UnRegisterService))
            {
                Message msg = new Message((int)SOAMessageType.UnRegisterService);
                msg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                try
                {
                    var req = message.GetMessageBody<UnRegisterServiceRequest>();
                    lock (LockObj)
                    {
                        var remList = ServiceContainer.Where(p => p.Session.IPAddress.Equals(session.IPAddress) && p.Session.Port.Equals(session.Port) && p.ServiceNo.Equals(req.ServiceNo));
                        List<int> mapPorts = new List<int>(); 
                        foreach (var item in remList)
                        {
                            if (item.WebMappers != null && item.WebMappers.Any())
                            {
                                mapPorts.AddRange(item.WebMappers.Where(p => p.MappingPort > 0).Select(p => p.MappingPort));
                            }
                            ServiceContainer.Remove(item);
                        }
                        foreach(var port in mapPorts.Distinct())
                        {
                            if (ServiceContainer.Any(p => p.WebMappers != null && p.WebMappers.Any() && p.WebMappers.Any(q => q.MappingPort == port)))
                            {
                                continue;
                            }
                            SimulateServerManager.RemoveSimulateServer(port);
                        }
                    }
                    msg.SetMessageBody(new UnRegisterServiceResponse
                    {
                        IsSuccess = true
                    });
                }
                catch (Exception e)
                {
                    msg.SetMessageBody(new UnRegisterServiceResponse
                    {
                        IsSuccess = false,
                        ErrMsg = e.Message,
                    });
                }
                session.SendMessage(msg);
                return;
            }
            else if (message.IsMessage((int)SOAMessageType.DoSOARequest))
            {
                var isSendAll = message.GetCustomData("SendAll") == "1";
                var req = message.GetMessageBody<SOARequest>();
                DoTransferRequest(session, message.MessageHeader.TransactionID, req, isSendAll);
                return;
            }
            else if (message.IsMessage((int)SOAMessageType.DoSOATransferResponse))
            {
                var resp = message.GetMessageBody<SOATransferResponse>();
                DoTransferResponse(resp);
                return;
            }
            else if (message.IsMessage((int)SOAMessageType.SOATransferWebResponse))
            {
                var resp = message.GetMessageBody<SOATransferWebResponse>();
                DoTransferWebResponse(resp);
                return;
            }
            else if (message.IsMessage((int)SOAMessageType.SOANoticeRequest))
            {
                var req = message.GetMessageBody<Contract.SOANoticeRequest>();
                if (req.ReciveClients != null && req.ReciveClients.Length > 0)
                {
                    Session s = null;
                    Message notice = new Message((int)SOAMessageType.SOANoticeClientMessage);
                    var service = this.ServiceContainer.FirstOrDefault(p => p.Session == session);

                    notice.SetMessageBody(new Contract.SOANoticeClientMessage
                    {
                        NoticeBody = req.NoticeBody,
                        NoticeType = req.NoticeType,
                        ServiceNo = service == null ? -1 : service.ServiceNo
                    });
                    List<string> succlist = new List<string>();
                    List<string> faillist = new List<string>();
                    foreach (var cid in req.ReciveClients)
                    {
                        //Console.WriteLine("给用户发通知:"+cid);
                        if (this.GetConnectedList().TryGetValue(cid, out s))
                        {
                            try
                            {
                                if (s.SendMessage(notice))
                                {
                                    succlist.Add(cid);
                                }
                                else
                                {
                                    faillist.Add(cid);
                                }
                            }
                            catch
                            {
                                faillist.Add(cid);
                            }
                        }
                        else
                        {
                            faillist.Add(cid);
                        }
                    }
                    if (req.NeedResult)
                    {
                        var respmesg = new Message((int)SOAMessageType.SOANoticeResponse);
                        respmesg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                        respmesg.SetMessageBody(new Contract.SOANoticeResponse
                        {
                            FailList = faillist.ToArray(),
                            SuccList = succlist.ToArray(),
                            IsDone = true
                        });
                        session.SendMessage(respmesg);
                    }
                }
                return;
            }

            base.FromApp(message, session);
        }
    }
}
