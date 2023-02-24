using LJC.FrameWork.SOA.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class ESBClientPoolManager:IDisposable
    {
        /// <summary>
        /// 最大客户端数连接设置
        /// </summary>
        public static readonly uint MAXCLIENTCOUNT = 3;

        private readonly ESBClient[] Clients = null;

        internal static readonly Dictionary<ESBServerConfigItem, ESBClient[]> BaseClients = new Dictionary<ESBServerConfigItem, ESBClient[]>();

        private static System.Timers.Timer backGroundWorker = null;

#pragma warning disable S3963 // "static" fields should be initialized inline
        static ESBClientPoolManager()
        {
            var esbConfig = ESBConfig.ReadConfig();
            var configItems = esbConfig.ESBServerConfigItems ?? new List<ESBServerConfigItem>();
            if (!configItems.Any(p => p.ESBServer == esbConfig.ESBServer && p.ESBPort == esbConfig.ESBPort))
            {
                configItems.Add(new ESBServerConfigItem
                {
                    AutoStart = esbConfig.AutoStart,
                    ESBPort = esbConfig.ESBPort,
                    ESBServer = esbConfig.ESBServer,
                    IsSecurity = esbConfig.IsSecurity,
                    MaxClientCount = esbConfig.MaxClientCount
                });
            }

            foreach(var item in configItems)
            {
                var count = Math.Min(item.MaxClientCount == 0 ? (int)MAXCLIENTCOUNT : item.MaxClientCount, 100);
                List<ESBClient> clients = new List<ESBClient>();
                for(var i = 0; i < count; i++)
                {
                    var newClient = new ESBClient(item.ESBServer, item.ESBPort, item.AutoStart, item.IsSecurity);
                    clients.Add(newClient);
                }
                BaseClients.Add(item, clients.ToArray());
            }

            if (configItems.Count > 1)
            {
                backGroundWorker = Comm.TaskHelper.SetInterval(30000, () =>
                  {
                      RefrashServicesList(true);
                      return false;
                  });
            }
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

        public ESBClientPoolManager(uint clientcount = 0, Func<int, ESBClient> getClient = null)
        {
            if (clientcount == 0)
            {
                clientcount = 2;
            }

            if (MAXCLIENTCOUNT > 0 && clientcount > MAXCLIENTCOUNT)
            {
                clientcount = MAXCLIENTCOUNT;
            }
            if (getClient != null)
            {
                Clients = new ESBClient[clientcount];
                for (int i = 0; i < clientcount; i++)
                {
                    var client = getClient(i);
                    client.Error += client_Error;
                    client.Login(null, null);
                    Clients[i] = client;
                }
            }
        }

        public void Dispose()
        {
            if (Clients != null)
            {
                foreach(var client in Clients)
                {
                    client.Dispose();
                }
            }
        }

        public ESBClient RandClient()
        {
            var idx = new Random(DateTime.Now.Ticks.GetHashCode()).Next(0, Clients.Length);

            var client = Clients[idx];

            return client;
        }

        public static List<ESBClient> FindServices(int serviceNo)
        {
            List<ESBClient> clients = new List<ESBClient>();
            if (BaseClients.Count == 1)
            {
                var values = BaseClients.First().Value;
                var idx = new Random(DateTime.Now.Ticks.GetHashCode()).Next(0, values.Length);
                clients.Add(values[idx]);
            }
            else if (BaseClients.Count > 1)
            {
                RefrashServicesList();
                foreach (var client in BaseClients)
                {
                    if (client.Key.RegisterServiceInfos.Any(p => p.ServiceNo == serviceNo))
                    {
                        var idx = new Random(DateTime.Now.Ticks.GetHashCode()).Next(0, client.Value.Length);
                        clients.Add(client.Value[idx]);
                    }
                }
            }
            return clients;
        }

        private static void RefrashServicesList(bool force = false)
        {
            foreach (var client in BaseClients)
            {
                if (force || client.Key.RegisterServiceInfos == null)
                {
                    var resp = client.Value.First().DoRequest<ListServiceInfosResponse>(Consts.ESBServerServiceNo,
                        Consts.FunNo_ListServiceInfos, new ListServiceInfosRequest());

                    lock (client.Key)
                    {
                        client.Key.RegisterServiceInfos = resp.Services.ToList();
                    }
                }
            }
        }

        public static ESBClient FindService(int serviceNo)
        {
            var findClients = FindServices(serviceNo);
            if (!findClients.Any())
            {
                return null;
            }

            var idx = findClients.Count == 1 ? 0 : new Random(DateTime.Now.Ticks.GetHashCode()).Next(0, findClients.Count);
            return findClients[idx];
        }

        public IEnumerable<ESBClient> EnumClients()
        {
            if (Clients != null && Clients.Length > 0)
            {
                foreach (var item in Clients)
                {
                    yield return item;
                }
            }

            foreach(var client in BaseClients)
            {
                foreach(var item in client.Value)
                {
                    yield return item;
                }
            }
        }

        void client_Error(Exception obj)
        {
            Console.WriteLine("出错:" + obj.Message);
        }

        /// <summary>
        /// 释放客户端
        /// </summary>
        public static void Realse()
        {
            if (backGroundWorker != null)
            {
                backGroundWorker.Dispose();
            }

            foreach(var client in BaseClients)
            {
                foreach (var item in client.Value)
                {
                    using (item)
                    {
                        item.CloseClient();
                    }
                }
            }
        }
    }
}
