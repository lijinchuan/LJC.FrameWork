using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.LogManager;

namespace LJC.FrameWork.SOA
{
    public class ESBClient:SessionClient
    {
        private static ESBClientPoolManager _clientmanager = new ESBClientPoolManager();

        internal ESBClient(string serverIP, int serverPort,bool startSession=true)
            : base(serverIP, serverPort,startSession)
        {
            
        }

        internal ESBClient()
            :base(ESBConfig.ReadConfig().ESBServer,ESBConfig.ReadConfig().ESBPort,ESBConfig.ReadConfig().AutoStart)
        {
            
        }

        private SessionMessageApp clientSession
        {
            get;
            set;
        }

        internal T DoRequest<T>(int serviceno, int fundid, object param)
        {
            SOARequest request = new SOARequest();
            request.ServiceNo = serviceno;
            request.FuncId = fundid;
            if (param == null)
            {
                request.Param = null;
            }
            else
            {
                request.Param = EntityBufCore.Serialize(param);
            }

            Message msg = new Message((int)SOAMessageType.DoSOARequest);
            msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
            msg.MessageBuffer = EntityBufCore.Serialize(request);

            T result= SendMessageAnsy<T>(msg);
            return result;
        }

        protected override byte[] DoMessage(Message message)
        {
            if (message.IsMessage((int)SOAMessageType.DoSOAResponse))
            {
                var resp= message.GetMessageBody<SOAResponse>();
                if (!resp.IsSuccess)
                {
                    BuzException = new Exception(resp.ErrMsg);
                    //这里最好抛出错误来
                    return null;
                }
                return resp.Result;
            }
            return base.DoMessage(message);
        }

        public static T DoSOARequest<T>(int serviceId,int functionId,object param)
        {
            //using (var client = new ESBClient())
            //{
            //    client.StartClient();
            //    client.Error += client_Error;
            //    var result = client.DoRequest<T>(serviceId, functionId, param);

            //    return result;
            //}

            var result =_clientmanager.RandClient().DoRequest<T>(serviceId, functionId, param);

            return result;
        }

        static void client_Error(Exception e)
        {
            LogHelper.Instance.Error("SOA请求错误", e);
        }
    }
}
