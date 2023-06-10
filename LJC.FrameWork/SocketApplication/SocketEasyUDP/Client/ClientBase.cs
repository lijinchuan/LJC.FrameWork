using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP.Client
{
    public class ClientBase: UDPSocketBase
    {
        private UdpClient _udpClient;
        private System.Net.IPEndPoint _serverPoint = null;
        protected volatile bool _stop = true;
        private volatile bool _isstartclient = false;

        protected ushort _max_package_len = 1207; //65507 1472 548

        SendMsgManualResetEventSlim _sendmsgflag = new SendMsgManualResetEventSlim();
        Dictionary<long, PipelineManualResetEventSlim> _pipelineSlimDic = new Dictionary<long, PipelineManualResetEventSlim>();
        Dictionary<long, AutoReSetEventResult> _resetevent = new Dictionary<long, AutoReSetEventResult>();

        private Thread _revicethread = null;

        public ClientBase(string host, int port)
        {
            _udpClient = new UdpClient();
            _udpClient.Connect(host, port);
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1024 * 1000);
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1024 * 1000);
            _serverPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), port);
        }

        public string Ip
        {
            get
            {
                return _serverPoint.Address.ToString();
            }
        }

        public int Port
        {
            get
            {
                return _serverPoint.Port;
            }
        }

        public bool SetMTU(ushort mtu)
        {
            if (mtu < MTU_MIN)
            {
                mtu = MTU_MIN;
            }
            if (mtu > MTU_MAX)
            {
                mtu = MTU_MAX;
            }
            _max_package_len = mtu;
            return SendMTU();
        }

        public bool SendMTU()
        {
            Message msg = new Message(MessageType.UPDSETMTU);
            msg.SetMessageBody(new UDPSetMTUMessage
            {
                MTU = _max_package_len
            });
            if (!SendMessage(msg, null))
            {
                return false;
            }

            return true;
        }

        public bool ClearTempData()
        {
            Message msg = new Message(MessageType.UDPCLEARBAGID);
            if (!SendMessage(msg, null))
            {
                return false;
            }

            return true;
        }

        private UDPRevResultMessage QuestionBag(long bagid, EndPoint endpoint)
        {
            int trytimes = 0;
            Message question = new Message(MessageType.UDPQUERYBAG);
            question.SetMessageBody(new UDPRevResultMessage
            {
                BagId = bagid
            });

            var wait = new AutoReSetEventResult(string.Empty);
            _resetevent.Add(bagid, wait);

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
                        _resetevent.Remove(bagid);
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

        public void SendMessageNoSure(Message msg, EndPoint endpoint)
        {
            var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);
            var segments = SplitBytes(bytes, _max_package_len).ToArray();
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                _udpClient.Send(segment, segment.Length);
            }
        }

        public override bool SendMessage(Message msg, IPEndPoint endpoint)
        {
            try
            {
                var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);
                var segments = SplitBytes(bytes, _max_package_len).ToArray();
                var bagid = GetBagId(segments.First());
                int[] sended = segments.Select(p => 0).ToArray();
                int trytimes = 0;
                Console.WriteLine("发消息:" + bagid + ",长度:" + bytes.Length);
                while (true)
                {
                    lock (_udpClient.Client)
                    {
                        _sendmsgflag.BagId = bagid;
                        _sendmsgflag.Reset();
                        for (var i = 0; i < segments.Length; i++)
                        {
                            if (sended[i] != 0)
                            {
                                continue;
                            }
                            var segment = segments[i];
                            _udpClient.Send(segment, segment.Length);
                            sended[i] = 1;
                        }
                        Console.WriteLine("等待信号");
                        _sendmsgflag.Wait(1000);
                        if (!_sendmsgflag.IsTimeOut)
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
                        LogManager.LogHelper.Instance.Info("发消息:" + bagid + "请求重发包，返回缺少包数量：" + revmsg.Miss.Length);
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
            catch (Exception ex)
            {
                OnError(ex);
                return false;
            }
        }

        protected void SendEcho(long bagid)
        {
            //Message echo = new Message(MessageType.UDPECHO);
            //var buffer = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(echo);
            var buffer = BitConverter.GetBytes(bagid);
            _udpClient.Send(buffer, buffer.Length);
        }

        public void StartClient()
        {
            _stop = false;

            if (this._revicethread == null)
            {
                this._revicethread = new Thread(new ThreadStart(()=>
                {
                    while (!_stop)
                    {
                        try
                        {
                            _isstartclient = true;
                           
                            var bytes = _udpClient.Receive(ref _serverPoint);
                            var bagid = BitConverter.ToInt64(bytes, 0);
                            if (bytes.Length > 8)
                            {
                                OnMessage(bytes);
                            }
                            else
                            {
                                Console.WriteLine((Environment.TickCount & Int32.MaxValue) + ":收确认:" + bagid);
                                if (_sendmsgflag.BagId == bagid)
                                {
                                    _sendmsgflag.Set();
                                }
                            }
                        }
                        catch (ObjectDisposedException ex)
                        {
                            OnError(ex);
                        }
                        catch (Exception ex)
                        {
                            OnError(ex);
                        }
                    }
                }));

                this._revicethread.Start();
            }

            while (true)
            {
                if (_isstartclient)
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }

        private void DispatchMessage(Message message)
        {
            if (message.IsMessage(MessageType.UDPQUERYBAG))
            {
                UDPRevResultMessage revmsg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<UDPRevResultMessage>(message.MessageBuffer);

                var respmsg = new Message(MessageType.UDPANSWERBAG);
                bool isreved = false;
                revmsg.Miss = GetMissSegment(revmsg.BagId, null, out isreved);
                revmsg.IsReved = isreved;
                respmsg.SetMessageBody(revmsg);

                SendMessage(respmsg, null);
            }
            else if (message.IsMessage(MessageType.UDPANSWERBAG))
            {
                UDPRevResultMessage revmsg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<UDPRevResultMessage>(message.MessageBuffer);
                AutoReSetEventResult wait = null;
                if (_resetevent.TryGetValue(revmsg.BagId, out wait))
                {
                    wait.WaitResult = revmsg;
                    wait.IsTimeOut = false;
                    wait.Set();
                }
            }
            else
            {
                OnMessage(message);
            }
        }

        protected virtual void OnMessage(Message message)
        {
            
        }

        private void CreateMessagePipeline(PipelineManualResetEventSlim slim, long bagid)
        {
            new Action(() =>
            {
                try
                {
                    slim.Reset();
                    slim.Wait(30000);

                    if (!slim.IsTimeOut)
                    {
                        var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(slim.MsgBuffer);
                        DispatchMessage(message);
                    }
                    else
                    {
                        
                        Console.Write("接收超时:" + bagid);
                        OnError(new TimeoutException("接收超时"));
                    }
                }
                finally
                {
                    ClearTempBag(bagid, null);
                    _pipelineSlimDic.Remove(bagid);
                }

            }).BeginInvoke(null, null);
        }

        private void OnMessage(byte[] data)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((o) =>
            {
                var margebytes = MargeBag(data,null);
                if (margebytes != null)
                {
                    var bagid = GetBagId(data);
                    SendEcho(bagid);
                    if (data.Length >= margebytes.Length)
                    {
                        var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(margebytes);
                        DispatchMessage(message);
                    }
                    else
                    {
                        //发送管道通知
                        PipelineManualResetEventSlim slim = null;
                        //通知管道
                        if (_pipelineSlimDic.TryGetValue(bagid, out slim))
                        {
                            slim.MsgBuffer = margebytes;
                            slim.Set();
                        }
                    }
                }
                else
                {
                    //创建管道
                    var bagid = GetBagId(data);
                    PipelineManualResetEventSlim slim = null;
                    if (!_pipelineSlimDic.TryGetValue(bagid, out slim))
                    {
                        lock (_pipelineSlimDic)
                        {
                            if (!_pipelineSlimDic.TryGetValue(bagid, out slim))
                            {
                                slim = new PipelineManualResetEventSlim();
                                slim.BagId = bagid;
                                _pipelineSlimDic.Add(bagid, slim);
                                CreateMessagePipeline(slim, bagid);
                            }
                        }
                    }
                }
            }));
        }

        protected override void DisposeManagedResource()
        {
            base.DisposeManagedResource();
        }

        protected override void DisposeUnManagedResource()
        {
            if (this._revicethread != null)
            {
                this._revicethread.Abort();
            }
            if (_udpClient != null)
            {
                _udpClient.Client.Dispose();
                _udpClient.Close();
            }

            base.DisposeUnManagedResource();
        }
    }
}
