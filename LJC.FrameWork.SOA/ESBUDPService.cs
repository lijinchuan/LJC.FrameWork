using LJC.FrameWork.SocketApplication.SocketEasyUDP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;
using LJC.FrameWork.SOA.Contract;

namespace LJC.FrameWork.SOA
{
    public class ESBUDPService: SessionServer
    {
        public Func<int, byte[],string, object> DoResponseAction;
        private int _serviceNo;
        public ESBUDPService(int serviceNo,string[] ips,int port) : base(ips, port)
        {
            _serviceNo = serviceNo;
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
                            throw new Exception(Consts.MISSINGFUNCTION);
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

                    if (ex.Message.Contains(Consts.ERRORSERVICEMSG))
                    {
                        session.Close(Consts.ERRORSERVICEMSG);
                    }
                }
            }
            else
            {
                base.FromSessionMessage(message, session);
            }
        }
    }
}
