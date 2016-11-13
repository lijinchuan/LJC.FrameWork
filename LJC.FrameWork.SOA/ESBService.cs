using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;
using System.Threading;
using LJC.FrameWork.LogManager;

namespace LJC.FrameWork.SOA
{
    public class ESBService:SessionClient,IService
    {
        public ESBService(string serverIP, int serverPort,int sNo)
            : base(serverIP, serverPort)
        {
            this.ServiceNo = sNo;
            this.BeferLogout += this.UnRegisterService;
            this.OnClientReset += ESBService_OnClientReset;
        }

        void ESBService_OnClientReset()
        {
            while (true)
            {
                try
                {
                    if (RegisterService())
                    {
                        LogHelper.Instance.Info("连接重置后注册服务成功");
                        break;
                    }
                    else
                    {
                        LogHelper.Instance.Info("连接重置后注册服务失败");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("连接重置后注册服务失败", ex);
                }
                Thread.Sleep(3000);
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void StartService()
        {
            while (!StartClient())
            {
                Thread.Sleep(1000);
            }
            Login(null, null);
        }

        public ESBService(int sNo)
            :base(ESBConfig.ReadConfig().ESBServer,ESBConfig.ReadConfig().ESBPort)
        {
            this.ServiceNo = sNo;
            this.BeferLogout += this.UnRegisterService;
        }

        public int ServiceNo
        {
            get;
            private set;
        }

        protected sealed override void ReciveMessage(Message message)
        {
            if (message.IsMessage((int)SOAMessageType.DoSOATransferRequest))
            {
                SOATransferRequest request = null;
                try
                {
                    var responseMsg = new Message((int)SOAMessageType.DoSOATransferResponse);
                    responseMsg.MessageHeader.TransactionID = message.MessageHeader.TransactionID;
                    SOATransferResponse responseBody = new SOATransferResponse();
                    request = message.GetMessageBody<SOATransferRequest>();
                    responseBody.ClientTransactionID = request.ClientTransactionID;
                    responseBody.ClientId = request.ClientId;

                    LogHelper.Instance.Debug(string.Format("接收服务请求,请求号:{0}", request.ClientTransactionID));

                    try
                    {
                        var result = DoResponse(request.FundId, request.Param);
                        responseBody.Result = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(result);
                        responseBody.IsSuccess = true;

                        LogHelper.Instance.Debug(string.Format("处理请求：请求号:{0},服务号:{1},功能号:{2},结果:{3}",
                            request.ClientTransactionID, ServiceNo, request.FundId, LJC.FrameWork.Comm.SerializerHelper.SerializerToXML(result)));
                            ;
                    }
                    catch (Exception ex)
                    {
                        responseBody.IsSuccess = false;
                        responseBody.ErrMsg = ex.Message;

                        LogHelper.Instance.Error(string.Format("服务转发出错,请求号:{0},服务号:{1},功能号:{2}",
                            request.ClientTransactionID, ServiceNo, request.FundId), ex);

                    }

                    responseMsg.SetMessageBody(responseBody);
                    //SendMessage(responseMsg);

                    this.SendMessage(responseMsg);
                    ////这里不关闭，会有线程泄露
                    //MessageApp newClient = new MessageApp(this.ipString, this.ipPort, false);
                    //newClient.Error += (e) =>
                    //    {
                    //         newClient.Dispose();
                    //    };
                    //if (newClient.StartClient())
                    //{
                    //    if (!newClient.SendMessage(responseMsg))
                    //    {
                    //        Logger.DebugTextLog(string.Format("服务转发出错,请求号:{0},服务号:{1},功能号:{2}",
                    //        request == null ? "0" : request.ClientTransactionID,
                    //        ServiceNo, request == null ? 0 : request.FundId), "发送消息出错",
                    //        LogCategory.Other);
                    //    }

                        
                    //}
                    //else
                    //{
                    //    Logger.DebugTextLog(string.Format("服务转发出错,原因是StartClient()方法失败,请求号:{0},服务号:{1},功能号:{2}",
                    //        request == null ? "0" : request.ClientTransactionID,
                    //        ServiceNo, request == null ? 0 : request.FundId), "发送消息出错",
                    //        LogCategory.Other);
                    //}

                    //newClient.Dispose();

                    return;
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error(string.Format("服务转发出错,请求号:{0},服务号:{1},功能号:{2}",
                        request == null ? "0" : request.ClientTransactionID,
                        ServiceNo, request == null ? 0 : request.FundId), ex);

                    return;
                }
            }

            base.ReciveMessage(message);
        }

        protected override byte[] DoMessage(LJC.FrameWork.SocketApplication.Message message)
        {
            if (message.IsMessage((int)SOAMessageType.RegisterService))
            {
                return message.MessageBuffer;
            }
            else if (message.IsMessage((int)SOAMessageType.UnRegisterService))
            {
                return message.MessageBuffer;
            }
            return base.DoMessage(message);
        }

        public virtual object DoResponse(int funcId, byte[] Param)
        {
            return null;
        }

        public bool RegisterService()
        {
            if (this.ServiceNo < 0)
                throw new Exception("注册服务失败：服务号不能为负数");

            Message msg = new Message((int)SOAMessageType.RegisterService);
            msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
            RegisterServiceRequest req = new RegisterServiceRequest();
            req.ServiceNo = this.ServiceNo;
            msg.SetMessageBody(req);

           bool boo= SendMessageAnsy<RegisterServiceResponse>(msg).IsSuccess;

           return boo;
        }

        public void UnRegisterService()
        {
            Message msg = new Message((int)SOAMessageType.UnRegisterService);
            msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
            UnRegisterServiceRequest req = new UnRegisterServiceRequest();
            req.ServiceNo = this.ServiceNo;
            msg.SetMessageBody(req);

            bool boo = SendMessageAnsy<UnRegisterServiceResponse>(msg).IsSuccess;

            if (boo)
            {
                Console.WriteLine("取消注册成功");
            }

        }

        protected override void OnLoginSuccess()
        {
            base.OnLoginSuccess();
            while (true)
            {
                try
                {
                    if (RegisterService())
                    {
                        LogHelper.Instance.Info("注册服务成功");
                        break;
                    }
                    else
                    {
                        LogHelper.Instance.Info("注册服务失败");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("注册服务失败", ex);
                }
                Thread.Sleep(3000);
            }
        }

        protected override void OnError(Exception e)
        {
            base.OnError(e);
        }

        protected override void OnSessionResume()
        {
            base.OnSessionResume();

            while (true)
            {
                try
                {
                    if (RegisterService())
                    {
                        LogHelper.Instance.Info("连接恢复后注册服务成功");
                        break;
                    }
                    else
                    {
                        LogHelper.Instance.Info("连接恢复后注册服务失败");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("连接恢复后注册服务失败", ex);
                }

                Thread.Sleep(3000);
            }
        }
    }
}
