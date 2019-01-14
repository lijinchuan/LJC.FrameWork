using LJC.FrameWork.SocketApplication.SocketEasyUDP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;

namespace LJC.FrameWork.SOA
{
    public class ESBUDPService: SessionServer
    {
        public Func<int, byte[],string, object> DoResponseAction;

        public ESBUDPService(string[] ips,int port) : base(ips, port)
        {

        }


        public string[] GetBindIps()
        {
            return this._bindingips;
        }

        public int GetBindUdpPort()
        {
            return this._bindport;
        }

        protected override void FromSessionMessage(Message message, UDPSession session)
        {
            if (message.IsMessage((int)SOAMessageType.DoSOARedirectRequest))
            {
                try
                {
                    if (DoResponseAction != null)
                    {
                        var reqbag = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<SOARedirectRequest>(message.MessageBuffer);
                        var obj = DoResponseAction(reqbag.FuncId, reqbag.Param,session.SessionID);

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

                    session.SendMessage(retmsg);
                }
            }
            else
            {
                base.FromSessionMessage(message, session);
            }
        }
    }
}
