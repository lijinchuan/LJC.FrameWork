using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using LJC.FrameWork.LogManager;

namespace LJC.FrameWork.SOA
{
    public class ESBServer:SessionServer
    {
        private static object LockObj = new object();
        private List<ESBServiceInfo> ServiceContainer = new List<ESBServiceInfo>();
        private Dictionary<string,Session> ClientSessionList = new Dictionary<string,Session>();
        private static ReaderWriterLockSlim ConatinerLock = new ReaderWriterLockSlim();

        public ESBServer(int port)
            : base(port)
        {
            ServerModeNeedLogin = false;
        }

        protected override byte[] DoMessage(Message message)
        {
            if (message.IsMessage((int)SOAMessageType.DoSOATransferResponse))
            {
                var resp = message.GetMessageBody<SOATransferResponse>();
                if (!resp.IsSuccess)
                {
                    Logger.TextLog("DoMessage失败", string.Empty, LogCategory.Other);
                    return null;
                }

                //这里要改进下
                return LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(resp.Result);
            }
            return base.DoMessage(message);
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
                    Logger.DebugTextLog(string.Format("SOA响应耗时,请求序列号:{0},服务号:{1},功能号:{2}",
                        response.ClientTransactionID, toulp.Item1, toulp.Item2)
                    , DateTime.Now.Subtract(session.BusinessTimeStamp).TotalMilliseconds + "毫秒",
                    LogCategory.SOA);
                }
                else
                {
                    Logger.TextLog(string.Format("DoTransferResponse(SOATransferResponse response)失败,请求序列号:{0}",
                    response.ClientTransactionID),
                        "找不到会话ID" + response.ClientId, LogCategory.Other);
                }
            }
            catch (Exception ex)
            {
                Logger.TextLog(string.Format("DoTransferResponse(SOATransferResponse response)失败,请求序列号:{0}",
                    response.ClientTransactionID),
                        ex, LogCategory.Other);
            }
        }

        internal void DoTransferRequest(Session session,string msgTransactionID,SOARequest request)
        {
            session.BusinessTimeStamp = DateTime.Now;
            session.Tag = new Tuple<int, int>(request.ServiceNo, request.FuncId);

            SOAResponse resp = new SOAResponse();
            Message msgRet = new Message((int)SOAMessageType.DoSOAResponse);
            msgRet.MessageHeader.TransactionID = msgTransactionID;
            resp.IsSuccess = true;

            //查询服务
            var serviceInfos= ServiceContainer.FindAll(p => p.ServiceNo.Equals(request.ServiceNo));
            if (serviceInfos == null||serviceInfos.Count==0)
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
                    SOATransferRequest transferrequest = new SOATransferRequest();
                    transferrequest.ClientId = session.SessionID;
                    transferrequest.FundId = request.FuncId;
                    transferrequest.Param = request.Param;
                    transferrequest.ClientTransactionID = msgTransactionID;

                    Message msg = new Message((int)SOAMessageType.DoSOATransferRequest);
                    msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
                    msg.SetMessageBody(transferrequest);

                    try
                    {
                        ConatinerLock.EnterWriteLock();
                        ClientSessionList.Add(session.SessionID, session);
                    }
                    finally
                    {
                        ConatinerLock.ExitWriteLock();
                    }

                    if (serviceInfo.Session.SendMessage(msg))
                    {
                        Logger.DebugTextLog(string.Format("发送SOA请求,请求序列:{0},服务号:{1},功能号:{2}",
                            msgTransactionID, request.ServiceNo, request.FuncId), string.Empty, LogCategory.Other);
                        return;
                    }
                    else
                    {
                        try
                        {
                            ConatinerLock.EnterWriteLock();
                            ClientSessionList.Remove(session.SessionID);
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

            msgRet.SetMessageBody(resp);
            session.Socket.SendMessge(msgRet);

            Logger.TextLog(string.Format("SOA请求失败,服务可能未注册,请求序列号:{0},服务号:{1},功能号:{2}",
                msgTransactionID, request.ServiceNo, request.FuncId)
                , DateTime.Now.Subtract(session.BusinessTimeStamp).TotalMilliseconds + "毫秒",
                LogCategory.SOA);
        }

        protected sealed override void FormAppMessage(Message message, Session session)
        {
            if (message.IsMessage((int)SOAMessageType.RegisterService))
            {
                Message msg = new Message((int)SOAMessageType.RegisterService);
                msg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                try
                {
                    var req = message.GetMessageBody<RegisterServiceRequest>();

                    lock (LockObj)
                    {
                        ServiceContainer.RemoveAll(p => p.Session.IPAddress.Equals(session.IPAddress)&&p.ServiceNo.Equals(req.ServiceNo));

                        ServiceContainer.Add(new ESBServiceInfo
                        {
                            ServiceNo = req.ServiceNo,
                            Session = session,
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
                        ErrMsg=ex.Message,
                        IsSuccess=false
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
                        IsSuccess=true
                    });
                }
                catch (Exception e)
                {
                    msg.SetMessageBody(new UnRegisterServiceResponse
                    {
                        IsSuccess=false,
                        ErrMsg=e.Message,
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
                session.Close();
                return;
            }
            base.FormAppMessage(message, session);
        }
    }
}
