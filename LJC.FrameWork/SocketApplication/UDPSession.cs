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

        public override bool SendMessage(Message msg)
        {
            return SessionServer.SendMessage(msg, this.EndPoint);
        }
    }
}
