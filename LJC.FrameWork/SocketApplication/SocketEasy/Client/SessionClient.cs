using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace LJC.FrameWork.SocketEasy.Client
{
    public class SessionClient:ClientHugeBase
    {
        private System.Timers.Timer timer;
        private Session SessionContext;
        /// <summary>
        /// 是否第一次报超时
        /// </summary>
        private bool isFirstTimeOut = true;

        public event Action SessionTimeOut;
        public event Action SessionResume;
        public event Action LoginFail;
        public event Action LoginSuccess;

        public event Action BeferLogout;

        private string uid;
        private string pwd;

        protected Exception BuzException = null;
        private Dictionary<string, AutoReSetEventResult> watingEvents=new Dictionary<string,AutoReSetEventResult>();

        public SessionClient(string serverip, int serverport, bool startSession)
            : base(serverip, serverport)
        {
            if (startSession)
            {
                StartSession();
            }
        }

        public Session LookUpSession()
        {
            return SessionContext;
        }

        #region

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

            if (timer == null)
            {
                timer = new System.Timers.Timer(SessionContext.HeadBeatInterVal);
                timer.Elapsed += HeartBeat_Elapsed;
                timer.Start();
            }
        }

        private void ReciveHeartBeat(Message message)
        {
            if (SessionContext.IsTimeOut() || !isFirstTimeOut)
            {
                isFirstTimeOut = true;
                SessionContext.IsLogin = true;


                OnSessionResume();

                if (SessionResume != null)
                    SessionResume();
            }
            //SessionContext.LastSessionTime = message.MessageHeader.MessageTime;
            SessionContext.LastSessionTime = DateTime.Now;
        }

        protected virtual void OnLoginSuccess()
        {

        }

        protected virtual void OnLoginFail(string failMsg)
        {
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

            if (!SendMessage(message))
            {
                OnLoginFail("发送请求失败");
                if (LoginFail != null)
                {
                    LoginFail();
                }
            }
        }

        public void Logout()
        {
            try
            {
                if (BeferLogout != null)
                {
                    BeferLogout();
                }
            }
            catch(Exception ex)
            {

            }

            Message msg = new Message(MessageType.LOGOUT);

            stop = true;
            isStartClient = false;
            SendMessage(msg);
        }

        protected sealed override void OnMessage(Message message)
        {
            base.OnMessage(message);

            if (SessionContext != null)
            {
                SessionContext.LastSessionTime = DateTime.Now;
            }

            if (message.IsMessage(MessageType.LOGIN))
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
            else if (message.IsMessage(MessageType.HEARTBEAT))
            {
                ReciveHeartBeat(message);
            }
            else if (message.IsMessage(MessageType.LOGOUT))
            {
                SessionContext.IsLogin = false;
                SessionContext.IsValid = false;
                stop = true;
                isStartClient = false;
                this.timer.Stop();
            }
            else if (message.IsMessage(MessageType.RELOGIN))
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

        public bool StartSession()
        {
            try
            {
                if (!isStartClient && !StartClient())
                    return false;

                return true;
            }
            catch (Exception e)
            {
                OnError(e);

                return false;
            }

        }


        void HeartBeat_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (DateTime.Now.Subtract(SessionContext.LastSessionTime).TotalSeconds < 10)
                    return;

                if (SessionContext.IsTimeOut() && isFirstTimeOut)
                {
                    isFirstTimeOut = false;
                    SessionContext.IsLogin = false;

                    OnSessionTimeOut();

                    if (SessionTimeOut != null)
                    {
                        SessionTimeOut();
                    }
                }

                Message msg = new Message(MessageType.HEARTBEAT);

                SendMessage(msg);
            }
            catch (Exception exp)
            {
                OnError(exp);
            }
        }

        protected virtual void OnSessionTimeOut()
        {

        }

        protected virtual void OnSessionResume()
        {

        }
        #endregion

        #region
        /// 
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

                SendMessage(message);
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
                    throw new Exception(string.Format("请求超时，请求序列号:{0}", reqID));
                }
                else
                {
                    T result = EntityBufCore.DeSerialize<T>((byte[])autoResetEvent.WaitResult);
                    return result;
                }
            }
        }

        protected void ReciveMessage(Message message)
        {
            if (!string.IsNullOrEmpty(message.MessageHeader.TransactionID))
            {
                AutoReSetEventResult autoEvent = null;

                Console.WriteLine("收到消息:" + message.MessageHeader.TransactionID);

                if (watingEvents.TryGetValue(message.MessageHeader.TransactionID,out autoEvent))
                {
                    autoEvent.WaitResult = message.MessageBuffer;
                    autoEvent.IsTimeOut = false;
                    autoEvent.Set();
                    return;
                }
            }
            
        }
        #endregion
    }
}
