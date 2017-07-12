using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    public class UDPSession : Session
    {
        public SocketEasyUDP.Server.SessionServer SessionServer
        {
            get;
            set;
        }

        public IPEndPoint EndPoint
        {
            get;
            set;
        }

        public System.Threading.ManualResetEventSlim SendMsgFlag = new System.Threading.ManualResetEventSlim();

        public override bool SendMessage(Message msg)
        {
            int trytimes = 0;
            while (true)
            {
                SendMsgFlag.Reset();
                SessionServer.SendMessage(msg, this.EndPoint);
                if (SendMsgFlag.Wait(10))
                {
                    return true;
                }
                else
                {
                    trytimes++;
                    if (trytimes >= 3)
                    {
                        throw new TimeoutException();
                    }
                }
            }
        }
    }
}
