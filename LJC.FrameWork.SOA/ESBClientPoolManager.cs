﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class ESBClientPoolManager:IDisposable
    {
        private ESBClient[] Clients = null;

        public ESBClientPoolManager(uint clientcount=5,Func<ESBClient> getClient=null)
        {
            if(clientcount==0)
            {
                clientcount = 5;
            }

            Clients = new ESBClient[clientcount];
            for (uint i = 0; i < clientcount; i++)
            {
                var client = getClient == null ? new ESBClient() : getClient();
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

        void client_Error(Exception obj)
        {
            Console.WriteLine("出错:" + obj.Message);
        }
    }
}
