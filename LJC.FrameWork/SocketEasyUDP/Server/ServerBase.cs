using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using LJC.FrameWork.SocketApplication;
using System.Net;

namespace LJC.FrameWork.SocketEasyUDP.Server
{
    public class ServerBase:UDPSocketBase
    {
        Socket __s = null;
        Dictionary<string, Socket> _connectDic = new Dictionary<string, Socket>();

        public ServerBase(int port)
        {
            __s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            __s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port));
        }

        public ServerBase(string ip, int port)
        {
            __s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            __s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));
        }

        public void StartServer()
        {
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)sender;

                var buffer = new byte[MAX_PACKAGE_LEN];
                int len = __s.ReceiveFrom(buffer, ref Remote);

                var mergebuffer = MargeBag(buffer);
                if (mergebuffer != null)
                {
                    OnSocket(Remote, mergebuffer);
                }
            }
        }

        private void OnSocket(object endpoint,byte[] bytes)
        {
            //__s.SendTo(LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(message), Remote);

            var message = new Message();
            message.MessageHeader.MessageType = (int)MessageType.REJECT;
            message.MessageHeader.MessageTime = DateTime.Now;

            Console.WriteLine(bytes.Length);
        }

        public override bool SendMessage(Message msg)
        {
            var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);
            foreach (var segment in SplitBytes(bytes))
            {
                lock (__s)
                {
                    __s.Send(bytes, SocketFlags.None);
                }
            }

            return true;
        }
    }
}
