using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using LJC.FrameWork.EntityBuf;
using System.Threading;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP.Client
{
    public class SessionClient: ClientBase
    {
        private System.Threading.Timer _heartbeatTimer = null;
        private string uid = string.Empty, pwd = string.Empty;
        private Dictionary<string, AutoReSetEventResult> watingEvents = new Dictionary<string, AutoReSetEventResult>();

        public event Action LoginFail;
        public event Action LoginSuccess;
        protected Exception BuzException = null;
        public volatile bool IsLogin = false;

        private Session SessionContext;

        public SessionClient(string host, int port) : base(host, port)
        {

        }

        protected override void OnError(Exception e)
        {
            if (_stop)
                return;

            base.OnError(e);
        }

        #region login事件
        protected virtual void OnLoginSuccess()
        {
            IsLogin = true;
        }

        protected virtual void OnLoginFail(string failMsg)
        {
        }

        public sealed override bool SendMessage(Message msg, IPEndPoint endpoint)
        {
            if (SessionContext != null)
            {
                SessionContext.LastSessionTime = DateTime.Now;
            }
            return base.SendMessage(msg, endpoint);
        }

        public void Login(string uid, string pwd)
        {
            this.uid = uid;
            this.pwd = pwd;

            Message message = new Message(MessageType.LOGIN);
            LoginRequestMessage msg = new LoginRequestMessage();
            msg.LoginID = this.uid;
            msg.LoginPwd = this.pwd;
            message.SetMessageBody(msg);

            if (!SendMessage(message, null))
            {
                OnLoginFail("发送请求失败");
                if (LoginFail != null)
                {
                    LoginFail();
                }
            }
        }
        #endregion

        private void DoHeartBeat(object obj)
        {
            try
            {
                _heartbeatTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                if (DateTime.Now.Subtract(SessionContext.LastSessionTime).TotalSeconds < 10)
                    return;

                Message msg = new Message(MessageType.HEARTBEAT);

                //SendMessage(msg,null);
                SendMessageNoSure(msg, null);
            }
            catch (Exception exp)
            {
                OnError(exp);
            }
            finally
            {
                if (!_stop)
                {
                    _heartbeatTimer.Change(SessionContext.HeadBeatInterVal, 0);
                }
            }
        }

        private void StartHearteBeat(LoginResponseMessage message)
        {
            SessionContext = new Session();
            SessionContext.UserName = message.LoginID;
            SessionContext.ConnectTime = DateTime.Now;
            SessionContext.SessionID = message.SessionID;
            SessionContext.SessionTimeOut = message.SessionTimeOut;
            SessionContext.HeadBeatInterVal = message.HeadBeatInterVal;
            SessionContext.IsLogin = true;
            SessionContext.IsValid = true;

            if (_heartbeatTimer == null)
            {
                _heartbeatTimer = new System.Threading.Timer(new System.Threading.TimerCallback(DoHeartBeat), null, message.HeadBeatInterVal, 0);
            }
        }

        protected sealed override void OnMessage(Message message)
        {
            base.OnMessage(message);

            if (message.IsMessage((int)MessageType.LOGIN))
            {
                LoginResponseMessage loginMsg = message.GetMessageBody<LoginResponseMessage>();
                if (loginMsg.LoginResult)
                {
                    StartHearteBeat(loginMsg);
                    OnLoginSuccess();

                    if (LoginSuccess != null)
                    {
                        LoginSuccess();
                    }
                }
                else
                {
                    OnLoginFail(loginMsg.LoginFailReson);
                    if (LoginFail != null)
                    {
                        LoginFail();
                    }
                }
            }
            else if (message.IsMessage((int)MessageType.HEARTBEAT))
            {
                SessionContext.LastSessionTime = DateTime.Now;
            }
            else if (message.IsMessage(MessageType.LOGOUT))
            {
                SessionContext.IsLogin = false;
                SessionContext.IsValid = false;
                _stop = true;
            }
            else if (message.IsMessage((int)MessageType.RELOGIN))
            {
                if (SessionContext == null)
                {
                    throw new Exception("请先调用login方法。");
                }
                SessionContext.IsLogin = false;
                Login(uid, pwd);
            }
            else
            {
                ReciveMessage(message);
            }
            
        }

        protected void ReciveMessage(Message message)
        {
            if (!string.IsNullOrEmpty(message.MessageHeader.TransactionID))
            {
                AutoReSetEventResult autoEvent = null;

                Console.WriteLine("收到消息:" + message.MessageHeader.TransactionID);

                if (watingEvents.TryGetValue(message.MessageHeader.TransactionID, out autoEvent))
                {
                    autoEvent.WaitResult = message.MessageBuffer;
                    autoEvent.IsTimeOut = false;
                    autoEvent.Set();
                    return;
                }
            }

        }

        /// 需要实现DoMessage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public T SendMessageAnsy<T>(Message message, int timeOut = 30000)
        {
            if (string.IsNullOrEmpty(message.MessageHeader.TransactionID))
                throw new Exception("消息没有设置唯一的序号。无法进行同步。");

            string reqID = message.MessageHeader.TransactionID;

            using (AutoReSetEventResult autoResetEvent = new AutoReSetEventResult(reqID))
            {
                watingEvents.Add(reqID, autoResetEvent);
                BuzException = null;

                SendMessage(message,null);
                //ThreadPool.QueueUserWorkItem(new WaitCallback(o => { SendMessage((Message)o); }), message);
                //new Func<Message, bool>(SendMessage).BeginInvoke(message, null, null);

                autoResetEvent.WaitOne(timeOut);
                //WaitHandle.WaitAny(new WaitHandle[] { autoResetEvent }, timeOut);

                watingEvents.Remove(reqID);

                if (BuzException != null)
                {
                    throw BuzException;
                }

                if (autoResetEvent.IsTimeOut)
                {
                    throw new TimeoutException(string.Format("请求超时，请求序列号:{0}", reqID));
                }
                else
                {
                    T result = EntityBufCore.DeSerialize<T>((byte[])autoResetEvent.WaitResult);
                    return result;
                }
            }
        }

        public bool SendFile(string localfile,int sendsplitcount,Action<double> process)
        {
            int count = sendsplitcount <= 0 ? 1024 * 100 : sendsplitcount;
            string filename = System.IO.Path.GetFileName(localfile);
            byte[] buffer = new byte[count];
            using (System.IO.FileStream fs = new System.IO.FileStream(localfile, System.IO.FileMode.Open))
            {
                var total = fs.Length;
                var sendbytes = 0;
                while (true)
                {
                    var len = fs.Read(buffer, 0, count);
                    if (len > 0)
                    {
                        SendFileMessage filemsg = new SendFileMessage() { FileBytes = len < count ? buffer.Take(len).ToArray() : buffer, FileName = filename };
                        Message msg = new Message(MessageType.SENDFILE);
                        msg.MessageHeader.TransactionID = SocketApplicationComm.GetSeqNum();
                        msg.SetMessageBody(filemsg);
                        if (!SendMessageAnsy<SendFileECHOMessage>(msg).IsSuccess)
                        {
                            return false;
                        }
                        sendbytes += len;
                        if (process != null)
                        {
                            process(Math.Round((sendbytes*1.0 / total), 4));
                        }
                    }
                    else
                    {
                        if (process != null)
                        {
                            process(1);
                        }
                        break;
                    }
                }
            }

            return true;
        }

        protected override void DisposeManagedResource()
        {
            base.DisposeManagedResource();
        }

        protected override void DisposeUnManagedResource()
        {
            if (_heartbeatTimer != null)
            {
                _heartbeatTimer.Dispose();
            }
            base.DisposeUnManagedResource();
        }
    }
}
