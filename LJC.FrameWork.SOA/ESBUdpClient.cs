using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class ESBUdpClient:LJC.FrameWork.SocketApplication.SocketEasyUDP.Client.SessionClient
    {
        public ESBUdpClient(string host,int port) : base(host, port)
        {

        }

        internal T DoRequest<T>(int funcid, object param)
        {
            SOARedirectRequest request = new SOARedirectRequest();
            request.FuncId = funcid;
            if (param == null)
            {
                request.Param = null;
            }
            else
            {
                request.Param = EntityBufCore.Serialize(param);
            }

            Message msg = new Message((int)SOAMessageType.DoSOARedirectRequest);
            msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
            msg.MessageBuffer = EntityBufCore.Serialize(request);

            var resp = SendMessageAnsy<SOARedirectResponse>(msg);
            if (resp.IsSuccess)
            {
                return LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<T>(resp.Result);
            }
            else
            {
                throw new Exception(resp.ErrMsg);
            }
        }
    }
}
