using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.LogManager;
using LJC.FrameWork.SocketApplication.SocketSTD;

namespace LJC.FrameWork.SOA
{
    public class ESBClient:SessionClient
    {
        private static Dictionary<int, List<ESBClientPoolManager>> _esbClientDicManager = new Dictionary<int, List<ESBClientPoolManager>>();
        private static Dictionary<int, List<ESBUdpClient>> _esbUdpClientDic = new Dictionary<int, List<ESBUdpClient>>();

        public static event Action<Contract.SOANoticeClientMessage> OnNotice;

        public ESBClient(string serverIP, int serverPort, bool startSession=true, bool isSecurity=false)
            : base(serverIP, serverPort,isSecurity,startSession)
        {
        }

        //internal ESBClient()
        //    :base(ESBConfig.ReadConfig().ESBServer,ESBConfig.ReadConfig().ESBPort,ESBConfig.ReadConfig().IsSecurity,ESBConfig.ReadConfig().AutoStart)
        //{
            
        //}

        public string GetESBServer()
        {
            return this.ipString;
        }

        public int GetESBPort()
        {
            return this.ipPort;
        }

        private SessionMessageApp clientSession
        {
            get;
            set;
        }

        protected override void ReciveMessage(Message message)
        {
            if (message.IsMessage((int)SOAMessageType.SOANoticeClientMessage))
            {
                var notice = message.GetMessageBody<Contract.SOANoticeClientMessage>();
                if (OnNotice != null)
                {
                    OnNotice.BeginInvoke(notice,null,null);
                }
                return;
            }

            base.ReciveMessage(message);
        }

        internal T DoRequest<T>(int serviceno, int funcid, object param, bool sendAll = false)
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

            if (sendAll)
            {
                msg.AddCustomData("SendAll", "1");
            }

            T result = SendMessageAnsy<T>(msg);
            return result;
        }

        internal T DoRedirectRequest<T>(int messageType, object request)
        {
            Message msg = new Message(messageType);
            msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
            msg.MessageBuffer = EntityBufCore.Serialize(request);

            T result = SendMessageAnsy<T>(msg);
            return result;
        }

        internal T DoRedirectRequest<T>(int serviceno, int funcid,object param)
        {
            SOARedirectRequest request = new SOARedirectRequest();
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

            Message msg = new Message((int)SOAMessageType.DoSOARedirectRequest);
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceId"></param>
        /// <param name="functionId"></param>
        /// <param name="param"></param>
        /// <param name="sendAll">是否发送给所有的服务端</param>
        /// <returns></returns>
        public static T DoSOARequest<T>(int serviceId, int functionId, object param, bool sendAll = false)
        {
            //using (var client = new ESBClient())
            //{
            //    client.StartClient();
            //    client.Error += client_Error;
            //    var result = client.DoRequest<T>(serviceId, functionId, param);

            //    return result;
            //}

            if (serviceId == Consts.ESBServerServiceNo && ESBClientPoolManager.BaseClients.Count > 1)
            {

                throw new ArgumentException("0服务不能使用此方法");
            }

            if (sendAll)
            {
                var clients = ESBClientPoolManager.FindServices(serviceId);
                if (!clients.Any())
                {
                    throw new SOAException("查找服务失败：" + serviceId);
                }
                var i = 0;
                T result = default;
                foreach(var client in clients)
                {
                    if (i++ == 0)
                    {
                        result = client.DoRequest<T>(serviceId, functionId, param, sendAll);
                    }
                    else
                    {
                        _ = client.DoRequest<T>(serviceId, functionId, param, sendAll);
                    }
                }
                return result;
            }
            else
            {
                var client = ESBClientPoolManager.FindService(serviceId);
                if (client == null)
                {
                    throw new SOAException("查找服务失败：" + serviceId);
                }
                var result = client.DoRequest<T>(serviceId, functionId, param);

                return result;
            }
        }

        private static int OrderIp(string ip)
        {
            if (ip.StartsWith("192.168.0."))
            {
                return 0;
            }

            if (ip.StartsWith("192.168.1."))
            {
                return 10;
            }

            if (ip.StartsWith("192.168."))
            {
                return 50;
            }

            if (ip.StartsWith("172."))
            {
                return 100;
            }

            if (ip.StartsWith("10."))
            {
                return 200;
            }

            return 300;
        }

        public static T DoSOARequest2<T>(int serviceId, int functionId, object param, bool sendAll = false)
        {
            if (sendAll)
            {
                return DoSOARequest<T>(serviceId, functionId, param, sendAll);
            }
            List<ESBUdpClient> udpclientlist = null;
            if (!_esbUdpClientDic.TryGetValue(serviceId, out udpclientlist))
            {
                bool takecleint = false;
                lock (_esbUdpClientDic)
                {
                    if (!_esbUdpClientDic.TryGetValue(serviceId, out udpclientlist))
                    {
                        takecleint = true;
                        if (!_esbClientDicManager.ContainsKey(serviceId))
                        {
                            _esbClientDicManager.Add(serviceId, null);
                        }
                        _esbUdpClientDic.Add(serviceId, null);
                    }
                }
                LogHelper.Instance.Debug("takecleint" + serviceId+ "takecleint:"+ takecleint);
                if (takecleint)
                {
                    new Action(() =>
                    {
                        GetRegisterServiceInfoResponse respserviceinfo = null;
                        try
                        {
                            var svcClient = ESBClientPoolManager.FindService(serviceId);
                            if (svcClient == null)
                            {
                                throw new Exception("无可用客户端，服务号:" + serviceId);
                            }

                            respserviceinfo = svcClient.DoRequest<GetRegisterServiceInfoResponse>(Consts.ESBServerServiceNo, Consts.FunNo_GetRegisterServiceInfo, new GetRegisterServiceInfoRequest
                            {
                                ServiceNo = serviceId
                            });

                            LogHelper.Instance.Debug("获取服务信息："+ Comm.JsonUtil<object>.Serialize(respserviceinfo));

                            if (!respserviceinfo.Infos.Any())
                            {
                                throw new Exception("服务" + serviceId + "没有服务提供方");
                            }
                        }
                        catch(Exception ex)
                        {
                            LogHelper.Instance.Error("获取服务信息出错",ex);

                            lock (_esbUdpClientDic)
                            {
                                _esbUdpClientDic.Remove(serviceId);
                            }
                            lock (_esbClientDicManager)
                            {
                                _esbClientDicManager.Remove(serviceId);
                            }

                            return;
                        }
                        
                        if (respserviceinfo.Infos != null && respserviceinfo.Infos.Length > 0)
                        {
                            List<ESBClientPoolManager> poollist = new List<ESBClientPoolManager>();
                            List<ESBUdpClient> udppoollist = new List<ESBUdpClient>();
                            foreach (var info in respserviceinfo.Infos)
                            {
                                if (info.RedirectUdpIps != null)
                                {
                                    foreach (var ip in info.RedirectUdpIps.OrderBy(p=>OrderIp(p)))
                                    {
                                        try
                                        {
                                            var client = new ESBUdpClient(ip, info.RedirectUdpPort);
                                            client.Error += (ex) =>
                                            {
                                                if (ex is System.Net.WebException
                                                ||ex is System.Net.Sockets.SocketException)
                                                {
                                                    client.Dispose();
                                                    lock (_esbUdpClientDic)
                                                    {
                                                        if (_esbUdpClientDic.TryGetValue(serviceId, out List<ESBUdpClient> oldList)
                                                                   && oldList.Any(q => q == client))
                                                        {
                                                            _esbUdpClientDic.Remove(serviceId);
                                                            LogHelper.Instance.Debug("移除UDP直连服务:" + serviceId);
                                                        }
                                                    }
                                                }
                                            };

                                            client.StartClient();
                                            client.Login(null, null);
                                            int trytimes = 0;
                                            var maxTryTimes = 10;
                                            var success = false;
                                            while (trytimes < maxTryTimes)
                                            {
                                                System.Threading.Thread.Sleep(10);
                                                if (client.IsLogin)
                                                {
                                                    var resp = client.DoRedirectRequest<Contract.QueryServiceNoResponse>((int)SOAMessageType.QueryServiceNo, null);
                                                    if (resp.ServiceNo == serviceId)
                                                    {
                                                        success = true;
                                                        udppoollist.Add(client);

                                                        LogHelper.Instance.Debug(string.Format("创建udp客户端成功:{0},端口{1}", ip, info.RedirectUdpPort));
                                                    }
                                                    else
                                                    {

                                                    }
                                                    break;
                                                }
                                                trytimes++;
                                            }
                                            if (!success)
                                            {
                                                client.Dispose();
                                                LogHelper.Instance.Debug(string.Format("创建udp客户端失败:{0},端口{1}", ip, info.RedirectUdpPort));
                                                throw new TimeoutException();
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }

                                if (udppoollist.Count==0&& info.RedirectTcpIps != null&& info.RedirectTcpIps.Any())
                                {
                                    LogHelper.Instance.Debug("RedirectTcpIps不为空:"+string.Join("、",info.RedirectTcpIps));
                                    foreach (var ip in info.RedirectTcpIps.OrderBy(p => OrderIp(p)))
                                    {
                                        try
                                        {
                                            var client = new ESBClient(ip, info.RedirectTcpPort, false);
                                            LogHelper.Instance.Debug("使用:" + ip);
                                            if (client.StartSession())
                                            {
                                                LogHelper.Instance.Debug("StartSession 成功");
                                                var resp = client.DoRedirectRequest<Contract.QueryServiceNoResponse>((int)SOAMessageType.QueryServiceNo, null);
                                                if (resp.ServiceNo == serviceId)
                                                {
                                                    LogHelper.Instance.Debug("验证成功");
                                                    client.Error += (ex) =>
                                                    {
                                                        if (ex is System.Net.WebException
                                                        || ex is System.Net.Sockets.SocketException
                                                        || !client.socketClient.Connected)
                                                        {
                                                            client.CloseClient();
                                                            client.Dispose();
                                                            lock (_esbClientDicManager)
                                                            {
                                                                if (_esbClientDicManager.TryGetValue(serviceId, out List<ESBClientPoolManager> oldList)
                                                                          && oldList.Any(p => p.EnumClients().Any(q => q == client)))
                                                                {
                                                                    LogHelper.Instance.Debug("移除TCP直连服务:" + serviceId);
                                                                    _esbClientDicManager.Remove(serviceId);

                                                                    if (_esbUdpClientDic.TryGetValue(serviceId, out List<ESBUdpClient> clientList) && clientList == null)
                                                                    {
                                                                        lock (_esbUdpClientDic)
                                                                        {
                                                                            _esbUdpClientDic.Remove(serviceId);
                                                                            LogHelper.Instance.Debug("移除UDP直连服务,使TCP重连:" + serviceId);
                                                                        }
                                                                    }

                                                                }
                                                            }
                                                        }
                                                    };

                                                    poollist.Add(new ESBClientPoolManager(0, (idx) =>
                                                    {
                                                        if (idx == 0)
                                                        {
                                                            return client;
                                                        }
                                                        var newclient = new ESBClient(ip, info.RedirectTcpPort, false);
                                                        newclient.StartSession();
                                                        newclient.Error += (ex) =>
                                                        {
                                                            if (ex is System.Net.WebException
                                                            || ex is System.Net.Sockets.SocketException
                                                            || !newclient.socketClient.Connected)
                                                            {
                                                                try
                                                                {
                                                                    newclient.CloseClient();
                                                                    newclient.Dispose();
                                                                }
                                                                catch
                                                                {

                                                                }
                                                            }
                                                        };
                                                        return newclient;
                                                    }));
                                                    LogHelper.Instance.Debug(string.Format("创建tcp客户端成功:{0},端口{1}", ip, info.RedirectTcpPort));
                                                    break;
                                                }
                                                else
                                                {
                                                    client.CloseClient();
                                                    client.Dispose();
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            LogHelper.Instance.Debug(string.Format("创建tcp客户端失败:{0},端口{1}", ip, info.RedirectTcpPort));
                                        }
                                    }
                                }
                            }

                            if (udppoollist.Count>0)
                            {
                                lock (_esbUdpClientDic)
                                {
                                    _esbUdpClientDic[serviceId] = udppoollist;
                                }
                            }
                            if (poollist.Count > 0)
                            {
                                lock (_esbClientDicManager)
                                {
                                    _esbClientDicManager[serviceId] = poollist;
                                }
                            }
                            else if (udppoollist.Count == 0 && respserviceinfo.Infos?.Any(p => p.RedirectTcpIps?.Any() == true) == true)
                            {
                                lock (_esbUdpClientDic)
                                {
                                    _esbUdpClientDic.Remove(serviceId);
                                    LogHelper.Instance.Debug("移除UDP直连服务,使TCP重连:" + serviceId);
                                }
                                if (_esbClientDicManager.TryGetValue(serviceId, out List<ESBClientPoolManager> mans) && mans == null)
                                {
                                    lock (_esbClientDicManager)
                                    {
                                        _esbClientDicManager.Remove(serviceId);
                                    }
                                }
                            }
                        }
                    }).BeginInvoke(null, null);
                }
            }

            if (udpclientlist != null && udpclientlist.Count > 0)
            {
                DateTime start = DateTime.Now;
                var client = udpclientlist.First();
                var ret = client.DoRequest<T>(serviceId, functionId, param);
                LogHelper.Instance.Debug("服务" + serviceId + ",功能：" + functionId + ",UDP直连" + client.Ip + ":" + client.Port + ",耗时" + DateTime.Now.Subtract(start).TotalMilliseconds);

                return ret;
            }
            else
            {
                List<ESBClientPoolManager> poolmanagerlist = null;
                if (_esbClientDicManager.TryGetValue(serviceId, out poolmanagerlist) && poolmanagerlist != null && poolmanagerlist.Count > 0)
                {
                    //Console.WriteLine("直连了");
                    DateTime start = DateTime.Now;
                    var poolmanager = poolmanagerlist.Count == 1 ? poolmanagerlist[0]
                    : poolmanagerlist[new Random().Next(0, poolmanagerlist.Count)];

                    var client = poolmanager.RandClient();

                    var ret = client.DoRedirectRequest<T>(serviceId, functionId, param);

                    LogHelper.Instance.Debug("服务" + serviceId + ",功能：" + functionId + ",直连" + client.ipString + ":" + client.ipPort + ",耗时" + DateTime.Now.Subtract(start).TotalMilliseconds);

                    return ret;
                }
                else
                {
                    DateTime start = DateTime.Now;
                    var ret = DoSOARequest<T>(serviceId, functionId, param);

                    LogHelper.Instance.Debug("服务" + serviceId + ",功能：" + functionId + ",非直连耗时:" + DateTime.Now.Subtract(start).TotalMilliseconds);

                    return ret;
                }
            }
        }

        static void UDPClient_Error(Exception e)
        {
            LogHelper.Instance.Error("UDPSOA请求错误", e);
        }

        static void client_Error(Exception e)
        {
            LogHelper.Instance.Error("SOA请求错误", e);
        }

        public static void Close()
        {
            //_clientmanager.Dispose();
            ESBClientPoolManager.Realse();
            foreach (var man in _esbClientDicManager)
            {
                if (man.Value != null)
                {
                    foreach (var m in man.Value)
                    {
                        try
                        {
                            m.Dispose();
                        }
                        catch
                        {

                        }
                    }
                }
            }

            foreach(var item in _esbUdpClientDic)
            {
                if (item.Value != null)
                {
                    foreach (var c in item.Value)
                    {
                        try
                        {
                            c.Dispose();
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
    }
}
