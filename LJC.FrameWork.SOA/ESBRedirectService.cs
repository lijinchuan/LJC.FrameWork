using LJC.FrameWork.SOA.Contract;
using LJC.FrameWork.SocketApplication;
using LJC.FrameWork.SocketEasy.Sever;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class ESBRedirectService : SessionServer
    {
        public Func<int, byte[], string,object> DoResponseAction;

        private int _serviceNo;

        public ESBRedirectService(int serviceNo,string[] ips, int port)
            : base(ips, port)
        {
            ServerModeNeedLogin = false;
            _serviceNo = serviceNo;
        }

        public string[] GetBindIps()
        {
            return this.bindIpArray;
        }

        public int GetBindTcpPort()
        {
            return this.ipPort;
        }

        protected override void FromApp(SocketApplication.Message message, SocketApplication.Session session)
        {
            if (message.IsMessage((int)SOAMessageType.QueryServiceNo))
            {
                var responseMsg = new Message((int)SOAMessageType.QueryServiceNo);
                responseMsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                QueryServiceNoResponse responseBody = new QueryServiceNoResponse();
                responseBody.ServiceNo = _serviceNo;

                responseMsg.SetMessageBody(responseBody);
                session.SendMessage(responseMsg);

                return;
            }
            else if (message.IsMessage((int)SOAMessageType.DoSOARedirectRequest))
            {
                try
                {
                    var reqbag = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<SOARedirectRequest>(message.MessageBuffer);
                    if (reqbag.ServiceNo != _serviceNo)
                    {
                        throw new Exception(Consts.ERRORSERVICEMSG);
                    }
                    if (DoResponseAction != null)
                    {
                        var obj = DoResponseAction(reqbag.FuncId, reqbag.Param, session.SessionID);

                        if (!string.IsNullOrWhiteSpace(message.MessageHeader.TransactionID))
                        {
                            var retmsg = new SocketApplication.Message((int)SOAMessageType.DoSOARedirectResponse);
                            retmsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                            SOARedirectResponse resp = new SOARedirectResponse();
                            resp.IsSuccess = true;
                            resp.ResponseTime = DateTime.Now;
                            resp.Result = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(obj);
                            retmsg.SetMessageBody(resp);

                            session.SendMessage(retmsg);
                        }
                        else
                        {
                            throw new Exception("服务未实现");
                        }
                    }
                    else
                    {
                        throw new Exception("服务无法处理");
                    }
                }
                catch (Exception ex)
                {
                    var retmsg = new SocketApplication.Message((int)SOAMessageType.DoSOARedirectResponse);
                    retmsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                    SOARedirectResponse resp = new SOARedirectResponse();
                    resp.IsSuccess = false;
                    resp.ResponseTime = DateTime.Now;
                    resp.ErrMsg = ex.ToString();
                    retmsg.SetMessageBody(resp);

                    try
                    {
                        session.SendMessage(retmsg);
                    }
                    catch (Exception exx)
                    {
                        OnError(exx);
                    }

                    if (ex.Message.Contains(Consts.ERRORSERVICEMSG))
                    {
                        session.Close(Consts.ERRORSERVICEMSG);
                    }
                }
            }
            else
            {
                base.FromApp(message, session);
            }
        }
    }
}
