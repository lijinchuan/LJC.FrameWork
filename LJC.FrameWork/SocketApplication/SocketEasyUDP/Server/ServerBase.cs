using LJC.FrameWork.Comm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP.Server
{
    public class ServerBase : UDPSocketBase
    {
        Socket __s = null;
        protected string[] _bindingips = null;
        protected int _bindport = 0;
        private bool _isBindIp = false;

        protected const ushort MAX_PACKAGE_LEN = 65507; //65507 1472 548

        static BufferPollManager _buffermanager = new BufferPollManager(1000, MAX_PACKAGE_LEN);

        Dictionary<string, SendMsgManualResetEventSlim> _sendMsgLockerSlim = new Dictionary<string, SendMsgManualResetEventSlim>();
        Dictionary<string, PipelineManualResetEventSlim> _pipelineSlimDic = new Dictionary<string, PipelineManualResetEventSlim>();
        Dictionary<string, AutoReSetEventResult> _resetevent = new Dictionary<string, AutoReSetEventResult>();
        Dictionary<string, ushort> _MTUDic = new Dictionary<string, ushort>();

        private object _bindlocker = new object();

        static ServerBase()
        {
            System.Threading.ThreadPool.SetMinThreads(2000, 2000);
            //System.Threading.ThreadPool.SetMaxThreads(2000, 2000);
        }

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

        protected void SendEcho(EndPoint remote, long bagid)
        {
            var buffer = BitConverter.GetBytes(bagid);
            __s.SendTo(buffer, remote);
        }

        private UDPRevResultMessage QuestionBag(long bagid,IPEndPoint endpoint)
        {
            int trytimes = 0;
            Message question = new Message(MessageType.UDPQUERYBAG);
            question.SetMessageBody(new UDPRevResultMessage
            {
                BagId = bagid
            });

            string key = string.Format("{0}:{1}:{2}", endpoint.Address.ToString(), endpoint.Port, bagid);
            var wait = new AutoReSetEventResult(string.Empty);
            _resetevent.Add(key, wait);

            try
            {
                while (true)
                {
                    wait.IsTimeOut = true;
                    wait.Reset();
                    try
                    {
                        SendMessageNoSure(question, endpoint);
                        wait.WaitOne(100);
                        if (!wait.IsTimeOut)
                        {
                            return (UDPRevResultMessage)wait.WaitResult;
                        }
                        else
                        {
                            throw new TimeoutException();
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        trytimes++;
                        if (trytimes >= 10)
                        {
                            throw ex;
                        }
                    }
                }
            }
            finally
            {
                _resetevent.Remove(key);
            }
        }

        private void CreateMessagePipeline(IPEndPoint endpoint, PipelineManualResetEventSlim slim, long bagid,string pipelinekey)
        {
            new Action(() =>
            {
                int trytimes = 0;
                try
                {
                    while (true)
                    {
                        slim.Reset();
                        slim.Wait(30000);

                        if (!slim.IsTimeOut)
                        {
                            var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(slim.MsgBuffer);
                            DispatchMessage(message, endpoint);
                            break;
                        }
                        else
                        {
                            trytimes++;
                            if (trytimes >= TimeOutTryTimes)
                            {
                                throw new TimeoutException();
                            }
                        }
                    }
                }
                finally
                {
                    _pipelineSlimDic.Remove(pipelinekey);
                    ClearTempBag(bagid, endpoint);
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

        private void DispatchMessage(Message message, IPEndPoint endpoint)
        {
            if (message.IsMessage(MessageType.UDPQUERYBAG))
            {
                UDPRevResultMessage revmsg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<UDPRevResultMessage>(message.MessageBuffer);

                var respmsg = new Message(MessageType.UDPANSWERBAG);
                bool isreved = false;
                revmsg.Miss = GetMissSegment(revmsg.BagId, endpoint, out isreved);
                revmsg.IsReved = isreved;
                respmsg.SetMessageBody(revmsg);

                SendMessage(respmsg, endpoint);
            }
            else if (message.IsMessage(MessageType.UDPANSWERBAG))
            {
                UDPRevResultMessage revmsg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<UDPRevResultMessage>(message.MessageBuffer);
                AutoReSetEventResult wait = null;
                string key = string.Format("{0}:{1}:{2}", endpoint.Address.ToString(), endpoint.Port, revmsg.BagId);
                if (_resetevent.TryGetValue(key, out wait))
                {
                    wait.WaitResult = revmsg;
                    wait.IsTimeOut = false;
                    wait.Set();
                }
            }
            else if (message.IsMessage(MessageType.UPDSETMTU))
            {
                var mtu = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<UDPSetMTUMessage>(message.MessageBuffer).MTU;
                if (mtu < MTU_MIN)
                {
                    mtu = MTU_MIN;
                }
                if (mtu > MTU_MAX)
                {
                    mtu = MTU_MAX;
                }

                lock (_MTUDic)
                {
                    var key = endpoint.Address.ToString();
                    if (_MTUDic.ContainsKey(key))
                    {
                        _MTUDic[key] = mtu;
                    }
                    else
                    {
                        _MTUDic.Add(key, mtu);
                    }
                }
            }
            else if (message.IsMessage(MessageType.UDPCLEARBAGID))
            {
                ClearTempData(endpoint);
            }
            else
            {
                FromApp(message, endpoint);
            }
        }

        protected virtual void FromApp(Message message, IPEndPoint endpoint)
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
                var ipendpoint = (IPEndPoint)endpoint;
                var bagid = GetBagId(bytes);
                string pipelinekey = string.Format("{0}:{1}:{2}", ipendpoint.Address.ToString(), ipendpoint.Port, bagid);
                var mergebuffer = MargeBag(bytes, (IPEndPoint)endpoint);
                if (mergebuffer != null)
                {
                    
                    Console.WriteLine("收包:"+bagid);
                    SendEcho((EndPoint)endpoint, bagid);

                    if (mergebuffer.Length <= bytes.Length)
                    {
                        var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(mergebuffer);
                        DispatchMessage(message, (IPEndPoint)endpoint);
                    }
                    else
                    {
                        PipelineManualResetEventSlim slim = null;
                        //通知管道
                        if (_pipelineSlimDic.TryGetValue(pipelinekey, out slim))
                        {
                            slim.MsgBuffer = mergebuffer;
                            slim.Set();
                        }
                    }
                }
                else
                {
                    //创建管道
                    PipelineManualResetEventSlim slim = null;
                    if (!_pipelineSlimDic.TryGetValue(pipelinekey, out slim))
                    {
                        lock (_pipelineSlimDic)
                        {
                            if (!_pipelineSlimDic.TryGetValue(pipelinekey, out slim))
                            {
                                slim = new PipelineManualResetEventSlim();
                                slim.BagId = bagid;
                                CreateMessagePipeline((IPEndPoint)endpoint, slim, bagid, pipelinekey);
                                _pipelineSlimDic.Add(pipelinekey, slim);
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

        public ushort GetClientMTU(IPEndPoint endpoint)
        {
            var key = endpoint.Address.ToString();
            ushort mtu=0;
            if (_MTUDic.TryGetValue(key, out mtu))
            {
                return mtu;
            }
            return MAX_PACKAGE_LEN;
        }

        public void SendMessageNoSure(Message msg,IPEndPoint endpoint)
        {
            var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);
            var segments = SplitBytes(bytes,GetClientMTU(endpoint)).ToArray();
            lock (__s)
            {
                for (var i = 0; i < segments.Length; i++)
                {
                    var segment = segments[i];
                    __s.SendTo(segment, SocketFlags.None, endpoint);
                }
            }
        }

        public override bool SendMessage(Message msg, IPEndPoint endpoint)
        {
            var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);
            var segments = SplitBytes(bytes, GetClientMTU(endpoint)).ToArray();
            var bagid = GetBagId(segments.First());
            int[] sended = segments.Select(p => 0).ToArray();
            int trytimes = 0;
            LogManager.LogHelper.Instance.Info("发消息:" + bagid + ",长度:" + bytes.Length);
            while (true)
            {
                lock (__s)
                {
                    var lockflag = this.GetSendMsgLocker((IPEndPoint)endpoint);
                    lockflag.BagId = bagid;
                    lockflag.Reset();
                    for (var i = 0; i < segments.Length; i++)
                    {
                        if (sended[i] != 0)
                        {
                            continue;
                        }
                        var segment = segments[i];
                        __s.SendTo(segment, SocketFlags.None, endpoint);
                        sended[i] = 1;
                    }
                    lockflag.Wait(3000);
                    if (!lockflag.IsTimeOut)
                    {
                        LogManager.LogHelper.Instance.Info("发消息:" + bagid + "成功");
                        return true;
                    }
                }

                if (trytimes++ >= TimeOutTryTimes)
                {
                    LogManager.LogHelper.Instance.Info("发消息:" + bagid + "超时，重试次数:" + trytimes);
                    throw new TimeoutException();
                }

                LogManager.LogHelper.Instance.Info("发消息:" + bagid + "需要重试,请求重发包");
                var revmsg = QuestionBag(bagid, endpoint);
                if (revmsg.IsReved)
                {
                    LogManager.LogHelper.Instance.Info("发消息:" + bagid + "请求重发包，返回完成");
                    return true;
                }

                if (revmsg.Miss != null && revmsg.Miss.Length > 0)
                {
                    LogManager.LogHelper.Instance.Info("发消息:" + bagid + "请求重发包，返回缺少包数量："+revmsg.Miss.Length);
                    foreach (var i in revmsg.Miss)
                    {
                        sended[i] = 0;
                    }
                }
                else if (revmsg.Miss == null)
                {
                    LogManager.LogHelper.Instance.Info("发消息:" + bagid + "请求重发包，返回完全没收到");
                    for (int i = 0; i < sended.Length; i++)
                    {
                        sended[i] = 0;
                    }
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
