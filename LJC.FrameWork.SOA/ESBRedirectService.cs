using LJC.FrameWork.SocketEasy.Sever;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class ESBRedirectService : SessionServer
    {
        public Func<int, byte[], object> DoResponseAction;

        public ESBRedirectService(string[] ips, int port)
            : base(ips, port)
        {
            ServerModeNeedLogin = false;
        }

        public string[] GetBindIps()
        {
            return this.bindIpArray;
        }

        public int GetBindTcpPort()
        {
            return this.ipPort;
        }

        protected override void FormApp(SocketApplication.Message message, SocketApplication.Session session)
        {
            if (message.IsMessage((int)SOAMessageType.DoSOARedirectRequest))
            {
                try
                {
                    if (DoResponseAction != null)
                    {
                        var reqbag = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<SOARedirectRequest>(message.MessageBuffer);
                        var obj = DoResponseAction(reqbag.FuncId, reqbag.Param);

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
                }
            }
            else
            {
                base.FormApp(message, session);
            }
        }
    }
}
