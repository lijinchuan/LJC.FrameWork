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
        protected string[] _bindingips = null;
        protected int _bindport = 0;
        private bool _isBindIp = false;

        private object _bindlocker = new object();

        private void BindIps()
        {
            if (!_isBindIp)
            {
                lock (_bindlocker)
                {
                    if (!_isBindIp)
                    {
                        __s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        __s.UseOnlyOverlappedIO = true;
                        if (_bindingips == null)
                        {
                            __s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, _bindport));
                        }
                        else
                        {
                            //foreach (var ip in _bindingips)
                            //{
                            //    __s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), _bindport));
                            //}

                            __s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, _bindport));
                        }
                    }
                }
            }
        }

        public ServerBase(int port)
        {
            //__s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //__s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port));

            _bindport = port;
        }

        public ServerBase(string[] ips, int port)
        {
            //__s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //__s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));

            _bindingips = ips;
            _bindport = port;
        }

        public void StartServer()
        {
            BindIps();

            new Action(() =>
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
                }).BeginInvoke(null, null);
        }

        protected virtual void FromApp(Message message,EndPoint endpoint)
        {
        }

        private void OnSocket(object endpoint,byte[] bytes)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((o) =>
                {
                    //Console.WriteLine("收到消息:" + bytes.Length);
                    var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(bytes);
                    FromApp(message, (EndPoint)endpoint);
                }));
        }

        public override bool SendMessage(Message msg, EndPoint endpoint)
        {
            var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);
            foreach (var segment in SplitBytes(bytes))
            {
                lock (__s)
                {
                    __s.SendTo(segment, SocketFlags.None, endpoint);
                }
            }

            return true;
        }


        protected override void DisposeUnManagedResource()
        {
            if (__s != null)
            {
                __s.Close();
                __s.Dispose();
            }
            base.DisposeUnManagedResource();
        }
    }
}
