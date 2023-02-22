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
        public static uint MAXCLIENTCOUNT = 0;

        private ESBClient[] Clients = null;

        private static Dictionary<ESBServerConfigItem, ESBClient[]> BaseClients = new Dictionary<ESBServerConfigItem, ESBClient[]>();

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
                var count = Math.Min(item.MaxClientCount == 0 ? 2 : item.MaxClientCount, 100);
                List<ESBClient> clients = new List<ESBClient>();
                for(var i = 0; i < count; i++)
                {
                    clients.Add(new ESBClient(item.ESBServer, item.ESBPort, item.AutoStart, item.IsSecurity));
                }
                BaseClients.Add(item, clients.ToArray());
            }
        }

        public ESBClientPoolManager(uint clientcount=2,Func<int,ESBClient> getClient=null)
        {
            if (clientcount == 0)
            {
                clientcount = 2;
            }

            if (MAXCLIENTCOUNT > 0 && clientcount > MAXCLIENTCOUNT)
            {
                clientcount = MAXCLIENTCOUNT;
            }

            Clients = new ESBClient[clientcount];
            for (int i = 0; i < clientcount; i++)
            {
                var client = getClient == null ? new ESBClient() : getClient(i);
                client.Error += client_Error;
                client.Login(null, null);
                Clients[i] = client;
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

        public IEnumerable<ESBClient> EnumClients()
        {
            if (Clients != null && Clients.Length > 0)
            {
                foreach (var item in Clients)
                {
                    yield return item;
                }
            }
        }

        void client_Error(Exception obj)
        {
            Console.WriteLine("出错:" + obj.Message);
        }
    }
}
