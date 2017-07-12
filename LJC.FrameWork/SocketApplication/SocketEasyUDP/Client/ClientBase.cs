using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketEasyUDP.Client
{
    public class ClientBase:UDPSocketBase
    {
        private UdpClient _udpClient;
        private System.Net.IPEndPoint _serverPoint = null;
        protected volatile bool _stop = true;
        private volatile bool _isstartclient = false;

        public ClientBase(string host,int port)
        {
            _udpClient = new UdpClient();
            _udpClient.Connect(host, port);
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1024 * 1000);
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1024 * 1000);
            _serverPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), port);
        }


        public override bool SendMessage(SocketApplication.Message msg, EndPoint endpoint)
        {
            var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);

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
            _stop = false;
            new Action(() =>
                {
                    while (!_stop)
                    {
                        try
                        {
                            _isstartclient = true;
                            var bytes = _udpClient.Receive(ref _serverPoint);

                            OnMessage(bytes);
                        }
                        catch (ObjectDisposedException ex)
                        {
                            OnError(ex);
                        }
                        catch(Exception ex)
                        {
                            OnError(ex);
                        }
                    }
                }).BeginInvoke(null, null);

            while (true)
            {
                if (_isstartclient)
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }

        protected virtual void OnMessage(Message message)
        {
        }
        

        private void OnMessage(byte[] data)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((o) =>
            {
                var margebytes = MargeBag(data);
                if (margebytes != null)
                {
                    var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(margebytes);
                    OnMessage(message);
                }
            }));
        }

        protected override void DisposeManagedResource()
        {
            base.DisposeManagedResource();
        }

        protected override void DisposeUnManagedResource()
        {
            if (_udpClient != null)
            {
                _udpClient.Close();
            }

            base.DisposeUnManagedResource();
        }
    }
}
