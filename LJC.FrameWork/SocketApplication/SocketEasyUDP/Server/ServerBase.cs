using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using LJC.FrameWork.SocketApplication;
using System.Net;
using System.Threading;

namespace LJC.FrameWork.SocketEasyUDP.Server
{
    public class ServerBase:UDPSocketBase
    {
        Socket __s = null;
        protected string[] _bindingips = null;
        protected int _bindport = 0;
        private bool _isBindIp = false;

        Dictionary<string, SendMsgManualResetEventSlim> _sendMsgLockerSlim = new Dictionary<string, SendMsgManualResetEventSlim>();

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
                        __s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1024 * 1000);
                        __s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1024 * 1000);
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

        protected void SendEcho(EndPoint remote,long segmentid)
        {
            var buffer = BitConverter.GetBytes(segmentid);
            __s.SendTo(buffer, remote);
        }

        SendMsgManualResetEventSlim GetSendMsgLocker(IPEndPoint endpoint,bool create=true)
        {
            var key = endpoint.Address.ToString() + "_" + endpoint.Port;

            SendMsgManualResetEventSlim locker = null;
            if(!_sendMsgLockerSlim.TryGetValue(key,out locker))
            {
                if (create)
                {
                    lock (_sendMsgLockerSlim)
                    {
                        if (!_sendMsgLockerSlim.TryGetValue(key, out locker))
                        {
                            locker = new SendMsgManualResetEventSlim();
                            _sendMsgLockerSlim.Add(key, locker);
                        }
                    }
                }
            }

            return locker;
        }

        public void StartServer()
        {
            BindIps();

            new Action(() =>
                {
                    while (true)
                    {
                        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                        EndPoint remote = (EndPoint)sender;

                        var buffer = new byte[MAX_PACKAGE_LEN];
                        int len = __s.ReceiveFrom(buffer, ref remote);

                        if (len > 8)
                        {
                            SendEcho(remote, BitConverter.ToInt64(buffer, 0));

                            OnSocket(remote, buffer);
                        }
                        else
                        {
                            var locker = GetSendMsgLocker((IPEndPoint)remote);
                            if (locker != null)
                            {
                                var segmentid = BitConverter.ToInt64(buffer, 0);
                                if (locker.SegmentId == segmentid)
                                {
                                    locker.Set();
                                }
                            }
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
                    var mergebuffer = MargeBag(bytes);
                    if (mergebuffer != null)
                    {
                        var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(mergebuffer);
                        FromApp(message, (EndPoint)endpoint);
                    }
                }));
        }

        public override bool SendMessage(Message msg, EndPoint endpoint)
        {
            var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);
            int trytemes = 0;
            foreach (var segment in SplitBytes(bytes))
            {
                trytemes = 0;
                lock (__s)
                {
                    var lockflag = this.GetSendMsgLocker((IPEndPoint)endpoint);
                    while (true)
                    {
                        var segmentid = BitConverter.ToInt64(segment, 0);
                        lockflag.SegmentId = segmentid;
                        lockflag.Reset();
                        __s.SendTo(segment, SocketFlags.None, endpoint);
                        if (lockflag.Wait(TimeOutMillSec))
                        {
                            break;
                        }
                        else
                        {
                            if (trytemes++ >= TimeOutTryTimes)
                            {
                                throw new TimeoutException();
                            }
                        }
                    }
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
