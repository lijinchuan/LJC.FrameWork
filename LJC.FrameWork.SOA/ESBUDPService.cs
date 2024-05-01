using LJC.FrameWork.SocketApplication.SocketEasyUDP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;
using LJC.FrameWork.SOA.Contract;
using LJC.FrameWork.Comm;
using LJC.FrameWork.EntityBuf;

namespace LJC.FrameWork.SOA
{
    public class ESBUDPService: SessionServer
    {
        public Func<int, byte[], string, Dictionary<string, string>, object> DoResponseAction;
        private int _serviceNo;
        public ESBUDPService(int serviceNo,string[] ips,int port) : base(ips, port)
        {
            _serviceNo = serviceNo;
        }


        public string[] BindIps
        {
            get
            {
                return _bindingips;
            }
            internal set
            {
                _bindingips = value;
            }
        }

        public int GetBindUdpPort()
        {
            return this._bindport;
        }

        protected T GetParam<T>(Dictionary<string, string> header, byte[] data)
        {
            var isJson = header?[Consts.HeaderKey_ContentType] == Consts.HeaderValue_ContentType_JSONValue;
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
                    var reqbag = GetParam<SOARedirectRequest>(message.MessageHeader.CustomData, message.MessageBuffer);

                    if (reqbag.ServiceNo != _serviceNo)
                    {
                        throw new Exception(Consts.ERRORSERVICEMSG);
                    }
                    if (DoResponseAction != null)
                    {
                        var obj = DoResponseAction(reqbag.FuncId, reqbag.Param, session.SessionID, message.MessageHeader.CustomData);

                        if (!string.IsNullOrWhiteSpace(message.MessageHeader.TransactionID))
                        {
                            var retmsg = new Message((int)SOAMessageType.DoSOARedirectResponse);
                            retmsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                            SOARedirectResponse resp = new SOARedirectResponse();
                            resp.IsSuccess = true;
                            resp.ResponseTime = DateTime.Now;
                            resp.Result = BuildResult(message.MessageHeader.CustomData, obj);
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
                    var retmsg = new Message((int)SOAMessageType.DoSOARedirectResponse);
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
