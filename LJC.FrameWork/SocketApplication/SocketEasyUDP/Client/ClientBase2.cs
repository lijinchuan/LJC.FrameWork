using LJC.FrameWork.SocketEasyUDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP.Client
{
    public class ClientBase2: UDPSocketBase2
    {
        private UdpClient _udpClient;
        private System.Net.IPEndPoint _serverPoint = null;
        protected volatile bool _stop = true;
        private volatile bool _isstartclient = false;

        SendMsgManualResetEventSlim _sendmsgflag = new SendMsgManualResetEventSlim();
        Dictionary<long, PipelineManualResetEventSlim> _pipelineSlimDic = new Dictionary<long, PipelineManualResetEventSlim>();
        Dictionary<long, AutoReSetEventResult> _resetevent = new Dictionary<long, AutoReSetEventResult>();

        public ClientBase2(string host, int port)
        {
            _udpClient = new UdpClient();
            _udpClient.Connect(host, port);
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1024 * 1000);
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1024 * 1000);
            _serverPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), port);
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
            var segments = SplitBytes(bytes).ToArray();
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                _udpClient.Send(segment, segment.Length);
            }
        }

        public override bool SendMessage(Message msg, EndPoint endpoint)
        {
            var bytes = LJC.FrameWork.EntityBuf.EntityBufCore.Serialize(msg);
            var segments = SplitBytes(bytes).ToArray();
            var bagid = GetBagId(segments.First());
            int[] sended = segments.Select(p => 0).ToArray();
            int trytimes = 0;
            LogManager.LogHelper.Instance.Info("发消息:" + bagid + ",长度:" + bytes.Length);
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
            new Action(() =>
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
                            Console.WriteLine(Environment.TickCount + ":收确认:" + bagid);
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
            if (message.IsMessage(MessageType.UDPQUERYBAG))
            {
                UDPRevResultMessage revmsg = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<UDPRevResultMessage>(message.MessageBuffer);

                var respmsg = new Message(MessageType.UDPANSWERBAG);
                revmsg.Miss = GetMissSegment(revmsg.BagId).ToArray();
                revmsg.IsReved = revmsg.Miss != null && revmsg.Miss.Length == 0;
                respmsg.SetMessageBody(revmsg);

                SendMessage(respmsg,null);
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
        }

        private void CreateMessagePipeline(PipelineManualResetEventSlim slim, long bagid)
        {
            new Action(() =>
            {
                slim.Reset();
                slim.Wait(30000);

                if (!slim.IsTimeOut)
                {
                    var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(slim.MsgBuffer);
                    OnMessage(message);
                }
                else
                {
                    ClearTempBag(bagid);
                    Console.Write("接收超时:" + bagid);
                    OnError(new TimeoutException("接收超时"));
                }

            }).BeginInvoke(null, null);
        }

        private void OnMessage(byte[] data)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((o) =>
            {
                var margebytes = MargeBag(data);
                if (margebytes != null)
                {
                    var bagid = GetBagId(data);
                    SendEcho(bagid);
                    if (data.Length >= margebytes.Length)
                    {
                        var message = LJC.FrameWork.EntityBuf.EntityBufCore.DeSerialize<Message>(margebytes);
                        OnMessage(message);
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
            if (_udpClient != null)
            {
                _udpClient.Close();
            }

            base.DisposeUnManagedResource();
        }
    }
}
