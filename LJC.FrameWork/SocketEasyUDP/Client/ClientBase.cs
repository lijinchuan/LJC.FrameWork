using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace LJC.FrameWork.SocketEasyUDP.Client
{
    public class ClientBase:UDPSocketBase
    {
        private UdpClient _udpClient;
        private System.Net.IPEndPoint _serverPoint = null;

        public ClientBase(string host,int port)
        {
            _udpClient = new UdpClient();
            _udpClient.Connect(host, port);
            _serverPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), port);
        }


        public override bool SendMessage(SocketApplication.Message msg)
        {
            var bytes=LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);

            foreach (var segment in SplitBytes(bytes))
            {
                lock (_udpClient.Client)
                {
                    _udpClient.Send(segment, segment.Length);
                }
            }

            return true;
        }

        public void StartClient()
        {
            new Action(() =>
                {
                    while (true)
                    {
                        var bytes = _udpClient.Receive(ref _serverPoint);

                        var margebytes = MargeBag(bytes);
                        if (margebytes != null)
                        {
                            OnMessage(margebytes);
                        }
                    }
                }).BeginInvoke(null, null);
        }

        public void OnMessage(byte[] data)
        {
            var msg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(data);

            Console.WriteLine(msg.MessageHeader.MessageType.ToString());
        }
    }
}
