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

namespace LJC.FrameWork.SOA
{
    public class ESBServer:SocketEasy.Sever.SessionServer
    {
        private static object LockObj = new object();
        protected List<ESBServiceInfo> ServiceContainer = new List<ESBServiceInfo>();
        private Dictionary<string,Session> ClientSessionList = new Dictionary<string,Session>();
        private static ReaderWriterLockSlim ConatinerLock = new ReaderWriterLockSlim();

        public ESBServer(int port)
            : base(port)
        {
            ServerModeNeedLogin = false;
        }

        internal void DoTransferResponse(SOATransferResponse response)
        {
            try
            {
                Session session = null;

                ConatinerLock.EnterReadLock();
                ClientSessionList.TryGetValue(response.ClientId, out session);
                ConatinerLock.ExitReadLock();

                if (session != null)
                {
                    ConatinerLock.EnterWriteLock();
                    ClientSessionList.Remove(response.ClientId);
                    ConatinerLock.ExitWriteLock();

                    SOAResponse resp = new SOAResponse();
                    Message msgRet = new Message((int)SOAMessageType.DoSOAResponse);
                    msgRet.MessageHeader.TransactionID = response.ClientTransactionID;
                    resp.IsSuccess = response.IsSuccess;
                    resp.ErrMsg = response.ErrMsg;
                    resp.Result = response.Result;

                    msgRet.SetMessageBody(resp);
                    session.Socket.SendMessge(msgRet);

                    var toulp = (Tuple<int, int>)session.Tag;

                    if (SocketApplicationEnvironment.TraceMessage)
                    {
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
                session.Socket.SendMessge(msgRet);
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
                            }
                            serviceInfo = ServiceContainer.FindAll(p => p.ServiceNo.Equals(request.ServiceNo)).LastOrDefault();
                            if(serviceInfo==null)
                            {
                                throw new Exception(string.Format("{0}服务未注册,服务超过30秒无应答。", request.ServiceNo));
                            }
                        }

                        string clientid = SocketApplicationComm.GetSeqNum();
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
                            ClientSessionList.Add(clientid, session);
                        }
                        finally
                        {
                            ConatinerLock.ExitWriteLock();
                        }

                        if (serviceInfo.Session.SendMessage(msg))
                        {
                            LogHelper.Instance.Debug(string.Format("发送SOA请求,请求序列:{0},服务号:{1},功能号:{2}",
                                msgTransactionID, request.ServiceNo, request.FuncId));
                            return;
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
                        }
                        //var result = SendMessageAnsy<byte[]>(serviceInfo.Session, msg);

                        //resp.Result = result;
                    }
                    catch (Exception ex)
                    {
                        resp.IsSuccess = false;
                        resp.ErrMsg = ex.Message;
                    }
                }

                if (!resp.IsSuccess)
                {
                    msgRet.SetMessageBody(resp);
                    session.Socket.SendMessge(msgRet);

                    LogHelper.Instance.Error(string.Format("SOA请求失败,服务可能未注册,请求序列号:{0},服务号:{1},功能号:{2},耗时:{3}",
                        msgTransactionID, request.ServiceNo, request.FuncId,DateTime.Now.Subtract(session.BusinessTimeStamp).TotalMilliseconds + "毫秒"));
                }
            }
        }

        protected override void FormApp(Message message, Session session)
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
                        ServiceContainer.RemoveAll(p => p.Session.IPAddress.Equals(session.IPAddress) && p.ServiceNo.Equals(req.ServiceNo));

                        ServiceContainer.Add(new ESBServiceInfo
                        {
                            ServiceNo = req.ServiceNo,
                            Session = session,
                            RedirectTcpIps=req.RedirectTcpIps,
                            RedirectTcpPort=req.RedirectTcpPort,
                            RedirectUdpIps=req.RedirectUdpIps,
                            RedirectUdpPort=req.RedirectUdpPort
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
                session.Socket.SendMessge(msg);
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
                        ServiceContainer.RemoveAll(p => p.Session.IPAddress.Equals(session.IPAddress) && p.ServiceNo.Equals(req.ServiceNo));
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
                session.Socket.SendMessge(msg);
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

            base.FormApp(message, session);
        }
    }
}
