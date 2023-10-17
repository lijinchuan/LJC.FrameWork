using LJC.FrameWork.LogManager;
using LJC.FrameWork.Net.HTTP.Server;
using LJC.FrameWork.SOA.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    internal static class SimulateServerManager
    {
        /// <summary>
        /// 网站服务端口
        /// </summary>
        internal static string DefaltWebPort = System.Configuration.ConfigurationManager.AppSettings["webwerverport"];

        internal static Func<WebRequest, WebResponse> TransferRequest;

        internal static Func<IEnumerable<WebMapper>> GetWebMapperList;

        private static object rwLocker = new object();

        private static Dictionary<int, SimulateServer> SimulateServers = new Dictionary<int, SimulateServer>();

        public static void AddDefaultServer()
        {
            if (!string.IsNullOrWhiteSpace(DefaltWebPort))
            {
                var webport = int.Parse(DefaltWebPort);
                AddSimulateServer(webport);

                LogHelper.Instance.Info("web默认服务开启:" + webport);
            }

        }

        public static void AddSimulateServer(int port)
        {
            lock (rwLocker)
            {
                if (SimulateServers.ContainsKey(port))
                {
                    return;
                }
                var server = new SimulateServer(port);
                server.StartServer();
                SimulateServers.Add(port, server);

                LogHelper.Instance.Info("web服务开启:" + port);
            }
        }

        public static void RemoveSimulateServer(int port)
        {
            lock (rwLocker)
            {
                if (SimulateServers.TryGetValue(port, out SimulateServer simulateServer))
                {
                    SimulateServers.Remove(port);
                    simulateServer.Dispose();
                }
            }
        }

        public static List<WebMapper> ListWeb()
        {
            return GetWebMapperList().ToList();
        }

    }
}
