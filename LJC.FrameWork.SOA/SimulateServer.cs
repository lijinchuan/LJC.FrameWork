using LJC.FrameWork.Net.HTTP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class SimulateServer:IDisposable
    {
        private HttpServer manhttpserver = null;
        private int _port = 0;

        public SimulateServer(int port)
        {
            _port = port;
        }

        private void InitServer()
        {
            manhttpserver.Handlers.Add(new ApiSimulateHandler());
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool StartServer()
        {
            if (manhttpserver == null)
            {
                manhttpserver = new HttpServer(new Server(_port));
                InitServer();
                return true;
            }
            else if (manhttpserver.Server.Port != _port)
            {
                manhttpserver.Server.Close();

                manhttpserver = new HttpServer(new Server(_port));

                InitServer();
                return true;
            }

            return false;
        }

        public bool Stop()
        {
            if (manhttpserver != null && manhttpserver.Server != null)
            {
                manhttpserver.Server.Close();
                manhttpserver = null;
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
