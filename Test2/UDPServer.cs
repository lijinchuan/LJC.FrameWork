using LJC.FrameWork.SocketApplication.SocketEasyUDP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Test2
{
    public class UDPServer:SessionServer
    {
        public UDPServer():base(39990)
        {

        }

        protected override void FromSessionMessage(LJC.FrameWork.SocketApplication.Message message, LJC.FrameWork.SocketApplication.UDPSession session)
        {
            base.FromSessionMessage(message, session);
        }
    }
}
