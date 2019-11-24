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

namespace LJC.FrameWork.SOA
{
    public class ESBServer:SocketEasy.Sever.SessionServer
    {
        private static object LockObj = new object();
        protected List<ESBServiceInfo> ServiceContainer = new List<ESBServiceInfo>();
        internal Dictionary<string, Session[]> ClientSessionList = new Dictionary<string, Session[]>();
        internal static ReaderWriterLockSlim ConatinerLock = new ReaderWriterLockSlim();

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
                        sb.Append("<tr><th>ID</th><th>服务器地址</th><th>TCP直连</th><th>UDP直连</th></tr>");
                        foreach (var item in gp)
                        {
                            if (DateTime.Now.Subtract(item.Session.LastSessionTime).TotalMinutes > 1)
                            {
                                lock (this._esb.ServiceContainer)
                                {
                                    _esb.ServiceContainer.Remove(item);
                                    item.Session.Close();
                                }
                            }

                            sb.AppendFormat("<tr><td>{0}</td><td>{1}:{2}</td><td>{3}</td><td>{4}</td></tr>", item.Session.SessionID, item.Session.IPAddress, item.Session.Port,
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
                    if (!clienthash.Contains(item.Value[0].SessionID)|| !clienthash.Contains(item.Value[1].SessionID))
                    {
                        lock (_esb.ClientSessionList)
                        {
                            _esb.ClientSessionList.Remove(item.Key);
                        }
                    }

                    sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}:{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td></tr>", item.Key, item.Value[0].SessionID, item.Value[0].IPAddress, item.Value[0].Port,
                        item.Value[0].ConnectTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        item.Value[0].LastSessionTime.ToString("yyyy-MM-dd HH:mm:ss"), Math.Round(item.Value[0].LastSessionTime.Subtract(item.Value[0].ConnectTime).TotalMinutes, 3),
                        item.Value[0].BytesSend, item.Value[0].BytesRev);
                }
                sb.Append("</table>");

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
            }
        }

        internal System.Collections.Concurrent.ConcurrentDictionary<string, Session> GetConnectedList()
        {
            return this._connectSocketDic;
        }

        internal void DoTransferResponse(SOATransferResponse response)
        {
            try
            {
                Session[] session = null;

                ConatinerLock.EnterReadLock();
                ClientSessionList.TryGetValue(response.ClientTransactionID, out session);
                ConatinerLock.ExitReadLock();

                if (session != null)
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
                    session[0].SendMessage(msgRet);

                    var toulp = (Tuple<int, int>)session[0].Tag;

                    if (SocketApplicationEnvironment.TraceMessage)
                    {
                        LogHelper.Instance.Debug(string.Format("SOA响应耗时,请求序列号:{0},服务号:{1},功能号:{2},用时:{3},结果:{4}",
                            response.ClientTransactionID, toulp.Item1, toulp.Item2, DateTime.Now.Subtract(session[0].BusinessTimeStamp).TotalMilliseconds + "毫秒",
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
                default:
                    {
                        throw new NotImplementedException(string.Format("未实现的功能:{0}", funcId));
                    }
            }
        }

        internal void DoTransferRequest(Session session, string msgTransactionID, SOARequest request)
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

                if (resp.IsSuccess)
                {
                    ESBServiceInfo serviceInfo = null;
                    if (serviceInfos.Count == 1)
                    {
                        serviceInfo = serviceInfos[0];
                    }
                    else
                    {
                        Random rd = new Random();
                        var idx = rd.Next(1, serviceInfos.Count + 1);
                        serviceInfo = serviceInfos[idx - 1];
                    }

                    try
                    {
                        if(DateTime.Now.Subtract(serviceInfo.Session.LastSessionTime).TotalSeconds>30)
                        {
                            lock (LockObj)
                            {
                                ServiceContainer.Remove(serviceInfo);
                                serviceInfo.Session.Close();
                            }
                            serviceInfo = ServiceContainer.FindAll(p => p.ServiceNo.Equals(request.ServiceNo)).LastOrDefault();
                            if(serviceInfo==null)
                            {
                                throw new Exception(string.Format("{0}服务可能不可用,30秒无应答。", request.ServiceNo));
                            }
                        }

                        string clientid = session.SessionID;
                        SOATransferRequest transferrequest = new SOATransferRequest();
                        transferrequest.ClientId = clientid;
                        transferrequest.FundId = request.FuncId;
                        transferrequest.Param = request.Param;
                        transferrequest.ClientTransactionID = msgTransactionID;

                        Message msg = new Message((int)SOAMessageType.DoSOATransferRequest);
                        msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
                        msg.SetMessageBody(transferrequest);

                        try
                        {
                            ConatinerLock.EnterWriteLock();
                            ClientSessionList.Add(msgTransactionID,new Session[2] { session, serviceInfo.Session });
                        }
                        finally
                        {
                            ConatinerLock.ExitWriteLock();
                        }

                        if (serviceInfo.Session.SendMessage(msg))
                        {
                            //LogHelper.Instance.Debug(string.Format("发送SOA请求,请求序列:{0},服务号:{1},功能号:{2}",
                            //    msgTransactionID, request.ServiceNo, request.FuncId));
                            return;
                        }
                        else
                        {
                            try
                            {
                                ConatinerLock.EnterWriteLock();
                                ClientSessionList.Remove(msgTransactionID);
                            }
                            finally
                            {
                                ConatinerLock.ExitWriteLock();
                            }
                        }
                        //var result = SendMessageAnsy<byte[]>(serviceInfo.Session, msg);

                        //resp.Result = result;
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);

                        resp.IsSuccess = false;
                        resp.ErrMsg = ex.Message;
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
                        var remlist=ServiceContainer.Where(p => p.Session.IPAddress.Equals(session.IPAddress) && p.Session.Port.Equals(session.Port) && p.ServiceNo.Equals(req.ServiceNo)).ToList();
                        foreach(var item in remlist)
                        {
                            item.Session.Close();
                            ServiceContainer.Remove(item);
                        }

                        ServiceContainer.Add(new ESBServiceInfo
                        {
                            ServiceNo = req.ServiceNo,
                            Session = session,
                            RedirectTcpIps = req.RedirectTcpIps,
                            RedirectTcpPort = req.RedirectTcpPort,
                            RedirectUdpIps = req.RedirectUdpIps,
                            RedirectUdpPort = req.RedirectUdpPort
                        });
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
                        ServiceContainer.RemoveAll(p => p.Session.IPAddress.Equals(session.IPAddress) && p.Session.Port.Equals(session.Port) && p.ServiceNo.Equals(req.ServiceNo));
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
                var req = message.GetMessageBody<SOARequest>();
                DoTransferRequest(session, message.MessageHeader.TransactionID, req);
                return;
            }
            else if (message.IsMessage((int)SOAMessageType.DoSOATransferResponse))
            {
                var resp = message.GetMessageBody<SOATransferResponse>();
                DoTransferResponse(resp);
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
                        NoticeBody=req.NoticeBody,
                        NoticeType=req.NoticeType,
                        ServiceNo=service==null?-1:service.ServiceNo
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
                            FailList=faillist.ToArray(),
                            SuccList=succlist.ToArray(),
                            IsDone=true
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
