using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using LJC.FrameWork.SocketApplication;
using System.Net;
using System.Threading;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.SocketEasyUDP.Server
{
    public class ServerBase:UDPSocketBase
    {
        Socket __s = null;
        protected string[] _bindingips = null;
        protected int _bindport = 0;
        private bool _isBindIp = false;

        static BufferPollManager _buffermanager = new BufferPollManager(1000, MAX_PACKAGE_LEN);

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

                        int len = 0;
                        byte[] buffer = null;
                        int offset = 0;
                        var bufferindex= _buffermanager.GetBuffer();
                        if (bufferindex != -1)
                        {
                            buffer = _buffermanager.Buffer;
                            offset = _buffermanager.GetOffset(bufferindex);
                            len = __s.ReceiveFrom(buffer, offset, MAX_PACKAGE_LEN, SocketFlags.None, ref remote);
                        }
                        else
                        {
                            buffer = new byte[MAX_PACKAGE_LEN];
                            len = __s.ReceiveFrom(buffer, ref remote);
                        }

                        if (len > 8)
                        {
                            var segmentid=BitConverter.ToInt64(buffer, offset);
                            Console.WriteLine(Environment.TickCount + ":收包:" + len + "，发确认:" + segmentid);
                            SendEcho(remote, segmentid);
                            if (bufferindex == -1)
                            {
                                OnSocket(remote, buffer);
                            }
                            else
                            {
                                OnSocket(remote, bufferindex, len);
                            }
                        }
                        else
                        {
                            
                            try
                            {
                                var locker = GetSendMsgLocker((IPEndPoint)remote);
                                if (locker != null)
                                {
                                    var segmentid = BitConverter.ToInt64(buffer, offset);
                                    Console.WriteLine(Environment.TickCount + ":收确认:" + segmentid);
                                    if (locker.SegmentId == segmentid)
                                    {
                                        locker.Set();
                                    }
                                }
                            }
                            finally
                            {
                                if (bufferindex != -1)
                                {
                                    _buffermanager.RealseBuffer(bufferindex);
                                }
                            }
                        }
                    }
                }).BeginInvoke(null, null);
        }

        protected virtual void FromApp(Message message,EndPoint endpoint)
        {
        }

        private void OnSocket(object endpoint, int bufferindex,int len)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((o) =>
            {
                var bytes = new byte[len];
                try
                {
                    var offset = _buffermanager.GetOffset(bufferindex);
                    for (int i = 0; i < len; i++)
                    {
                        bytes[i] = _buffermanager.Buffer[offset + i];
                    }
                }
                finally
                {
                    _buffermanager.RealseBuffer(bufferindex);
                }
                var mergebuffer = MargeBag(bytes);
                if (mergebuffer != null)
                {
                    var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(mergebuffer);
                    FromApp(message, (EndPoint)endpoint);
                }
               
            }));
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
