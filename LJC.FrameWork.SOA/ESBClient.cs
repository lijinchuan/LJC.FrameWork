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
        private static Dictionary<int, List<ESBClientPoolManager>> _escClientDicManager = new Dictionary<int, List<ESBClientPoolManager>>();

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

        internal T DoRequest<T>(int serviceno, int funcid, object param)
        {
            SOARequest request = new SOARequest();
            request.ServiceNo = serviceno;
            request.FuncId = funcid;
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

        internal T DoRequest<T>(int funcid,object param)
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

            Message msg = new Message((int)SOAMessageType.DoSOARequest);
            msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
            msg.MessageBuffer = EntityBufCore.Serialize(request);

            T result = SendMessageAnsy<T>(msg);
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
                    throw BuzException;
                }
                return resp.Result;
            }
            else if (message.IsMessage((int)SOAMessageType.DoSOARedirectResponse))
            {
                var resp = message.GetMessageBody<SOARedirectResponse>();
                if (!resp.IsSuccess)
                {
                    BuzException = new Exception(resp.ErrMsg);
                    //这里最好抛出错误来
                    throw BuzException;
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

        public static T DoSOARequest2<T>(int serviceId, int functionId, object param)
        {
            List<ESBClientPoolManager> poolmanagerlist = null;
            if (!_escClientDicManager.TryGetValue(serviceId,out poolmanagerlist))
            {
                bool takecleint = false;
                lock (_escClientDicManager)
                {
                    if (!_escClientDicManager.TryGetValue(serviceId, out poolmanagerlist))
                    {
                        takecleint = true;
                        _escClientDicManager.Add(serviceId, null);
                    }
                }

                if (takecleint)
                {
                    new Action(() =>
                    {
                        var respserviceinfo = DoSOARequest<GetRegisterServiceInfoResponse>(Consts.ESBServerServiceNo, Consts.FunNo_GetRegisterServiceInfo, new GetRegisterServiceInfoRequest
                        {
                            ServiceNo = serviceId
                        });

                        if (respserviceinfo.Infos != null && respserviceinfo.Infos.Length > 0)
                        {
                            List<ESBClientPoolManager> poollist = new List<ESBClientPoolManager>();
                            foreach (var info in respserviceinfo.Infos)
                            {
                                foreach (var ip in info.RedirectTcpIps)
                                {
                                    try
                                    {
                                        var client = new ESBClient(ip, info.RedirectTcpPort, false);
                                        client.Error += (ex) =>
                                         {
                                             if(ex is System.Net.WebException)
                                             {
                                                 client.CloseClient();
                                                 client.Dispose();
                                                 lock (_escClientDicManager)
                                                 {
                                                     _escClientDicManager.Remove(serviceId);
                                                 }
                                             }
                                         };
                                        client.StartClient();
                                        poollist.Add(new ESBClientPoolManager(5, () => client));
                                        break;
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            if (poollist != null)
                            {
                                lock (_escClientDicManager)
                                {
                                    _escClientDicManager[serviceId] = poollist;
                                }
                            }
                        }
                    }).BeginInvoke(null, null);
                }
            }

            if (poolmanagerlist == null||poolmanagerlist.Count==0)
            {
                return DoSOARequest<T>(serviceId, functionId, param);
            }
            else
            {
                var poolmanager = poolmanagerlist.Count == 1 ? poolmanagerlist[0]
                    : poolmanagerlist[new Random().Next(0, poolmanagerlist.Count)];

                return poolmanager.RandClient().DoRequest<T>(functionId, param);
            }
        }

        static void client_Error(Exception e)
        {
            LogHelper.Instance.Error("SOA请求错误", e);
        }
    }
}
