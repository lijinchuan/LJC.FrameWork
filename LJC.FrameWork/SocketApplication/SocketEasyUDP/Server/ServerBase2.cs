﻿using LJC.FrameWork.Comm;
using LJC.FrameWork.SocketEasyUDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP.Server
{
    public class ServerBase2 : UDPSocketBase2
    {
        Socket __s = null;
        protected string[] _bindingips = null;
        protected int _bindport = 0;
        private bool _isBindIp = false;

        static BufferPollManager _buffermanager = new BufferPollManager(1000, MAX_PACKAGE_LEN);

        Dictionary<string, SendMsgManualResetEventSlim> _sendMsgLockerSlim = new Dictionary<string, SendMsgManualResetEventSlim>();
        Dictionary<long, PipelineManualResetEventSlim> _pipelineSlimDic = new Dictionary<long, PipelineManualResetEventSlim>();

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

        public ServerBase2(int port)
        {
            //__s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //__s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port));

            _bindport = port;
        }

        public ServerBase2(string[] ips, int port)
        {
            //__s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //__s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));

            _bindingips = ips;
            _bindport = port;
        }

        protected void SendEcho(EndPoint remote, long bagid)
        {
            var buffer = BitConverter.GetBytes(bagid);
            __s.SendTo(buffer, remote);
        }

        private void CreateMessagePipeline(EndPoint endpoint, PipelineManualResetEventSlim slim, long bagid)
        {
            new Action(() =>
            {
                slim.Reset();
                slim.Wait(10000);

                if (!slim.IsTimeOut)
                {
                    var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(slim.MsgBuffer);
                    FromApp(message, (EndPoint)endpoint);
                }
                else
                {
                    Console.Write("接收超时:"+bagid);
                }

            }).BeginInvoke(null, null);
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
                    var bufferindex = _buffermanager.GetBuffer();
                    if (bufferindex == -1)
                    {
                        throw new Exception("没有可用的接收缓存");
                    }

                    if (bufferindex != -1)
                    {
                        buffer = _buffermanager.Buffer;
                        offset = _buffermanager.GetOffset(bufferindex);
                        len = __s.ReceiveFrom(buffer, offset, MAX_PACKAGE_LEN, SocketFlags.None, ref remote);
                    }

                    if (len > 8)
                    {
                        OnSocket(remote, bufferindex, len);
                    }
                    else
                    {
                        try
                        {
                            var locker = GetSendMsgLocker((IPEndPoint)remote);
                            if (locker != null)
                            {
                                var bagid = BitConverter.ToInt64(buffer, offset);
                                //Console.WriteLine(Environment.TickCount + ":收确认:" + segmentid);
                                if (locker.BagId == bagid)
                                {
                                    locker.Set();
                                }
                            }
                        }
                        finally
                        {
                            _buffermanager.RealseBuffer(bufferindex);
                        }
                    }
                }
            }).BeginInvoke(null, null);
        }

        protected virtual void FromApp(Message message, EndPoint endpoint)
        {
        }

        private void OnSocket(object endpoint, int bufferindex, int len)
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
                    var bagid = GetBagId(bytes);
                    Console.WriteLine("收包:"+bagid);
                    SendEcho((EndPoint)endpoint, bagid);

                    if (mergebuffer.Length == bytes.Length)
                    {
                        var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(mergebuffer);
                        FromApp(message, (EndPoint)endpoint);
                    }
                    else
                    {
                        PipelineManualResetEventSlim slim = null;
                        //通知管道
                        if(_pipelineSlimDic.TryGetValue(bagid,out slim))
                        {
                            slim.MsgBuffer = mergebuffer;
                            slim.Set();
                        }
                    }
                }
                else
                {
                    //创建管道
                    var bagid = GetBagId(bytes);
                    PipelineManualResetEventSlim slim = null;
                    if (!_pipelineSlimDic.TryGetValue(bagid, out slim))
                    {
                        lock (_pipelineSlimDic)
                        {
                            if (!_pipelineSlimDic.TryGetValue(bagid, out slim))
                            {
                                slim = new PipelineManualResetEventSlim();
                                slim.BagId = bagid;
                                CreateMessagePipeline((EndPoint)endpoint, slim, bagid);
                                _pipelineSlimDic.Add(bagid, slim);
                            }
                        }
                    }
                }

            }));
        }

        SendMsgManualResetEventSlim GetSendMsgLocker(IPEndPoint endpoint, bool create = true)
        {
            var key = endpoint.Address.ToString() + "_" + endpoint.Port;

            SendMsgManualResetEventSlim locker = null;
            if (!_sendMsgLockerSlim.TryGetValue(key, out locker))
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

        public override bool SendMessage(Message msg, EndPoint endpoint)
        {
            var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);
            var segments = SplitBytes(bytes).ToArray();
            var bagid = GetBagId(segments.First());
            lock (__s)
            {
                var lockflag = this.GetSendMsgLocker((IPEndPoint)endpoint);
                lockflag.BagId = bagid;
                lockflag.Reset();
                for (var i = 0; i < segments.Length; i++)
                {
                    var segment = segments[i];
                    __s.SendTo(segment, SocketFlags.None, endpoint);
                }
                lockflag.Wait(10000);
                if (!lockflag.IsTimeOut)
                {
                    return true;
                }
                else
                {
                    throw new TimeoutException();
                }
            }
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
