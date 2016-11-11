using LJC.FrameWork.Comm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace LJC.FrameWork.SocketApplication
{
    public class SessionMessageApp : MessageApp
    {
        private Timer timer;
        private Session SessionContext;
        /// <summary>
        /// 是否第一次报超时
        /// </summary>
        private bool isFirstTimeOut = true;
        protected Dictionary<string, Session> appSockets;

        public event Action SessionTimeOut;
        public event Action SessionResume;
        public event Action LoginFail;
        public event Action LoginSuccess;

        public event Action BeferLogout;

        private string uid;
        private string pwd;

        public SessionMessageApp(string ip, int port)
            : base(ip, port)
        {

        }

        public SessionMessageApp(int port)
            : base(string.Empty, port)
        {
            appSockets = new Dictionary<string, Session>();
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
            SessionContext.SessionID = SocketApplicationComm.GetSeqNum();
            SessionContext.SessionTimeOut = message.SessionTimeOut;
            SessionContext.HeadBeatInterVal = message.HeadBeatInterVal;
            SessionContext.IsLogin = true;
            SessionContext.IsValid = true;

            if (timer == null)
            {
                timer = new Timer(SessionContext.HeadBeatInterVal);
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

        protected virtual void ReciveMessage(Message message)
        {

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

        #region 服务器模式

        private void App_HeatBeat(Message message, Session session)
        {
            Message msg = new Message(MessageType.HEARTBEAT);
            msg.SetMessageBody(session.SessionID);

            //s.Send(msg.ToBytes());
            session.LastSessionTime = DateTime.Now;
            session.Socket.SendMessge(message);

            SocketApplicationComm.Debug(string.Format("{0}发来心跳！", session.SessionID));
        }

        /// <summary>
        /// 服务器处理客户端登陆请求
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        protected virtual bool OnUserLogin(string user, string pwd, out string loginFailReson)
        {
            loginFailReson = string.Empty;
            return true;
        }

        private void App_Login(Message message, Session session)
        {
            Exception ex = null;
            LoginRequestMessage request = message.GetMessageBody<LoginRequestMessage>();
            //string uid = message.Get<string>(FieldEnum.LoginID);
            //string pwd = message.Get<string>(FieldEnum.LoginPwd);

            Message LoginSuccessMessage = new Message(MessageType.LOGIN);
            LoginResponseMessage responsemsg = new LoginResponseMessage();

            string loginFailMsg = string.Empty;
            bool canLogin = false;
            try
            {
                canLogin = OnUserLogin(request.LoginID, request.LoginPwd, out loginFailMsg);
            }
            catch (Exception e)
            {
                ex = e;
                loginFailMsg = "服务器出错";
            }
            if (canLogin)
            {
                responsemsg.LoginResult = true;
                session.IsLogin = true;
                session.UserName = request.LoginID;

                //session.Socket = s;
                //session.IPAddress = ((System.Net.IPEndPoint)s.RemoteEndPoint).Address.ToString();
                appSockets.Add(session.SessionID, session);
                Console.WriteLine("{0}成功登陆", request.LoginID);
            }
            else
            {
                responsemsg.LoginResult = false;
            }

            string heartBeatConfig = ConfigHelper.AppConfig("HeartBeat");
            //int headBeatInt = int.Parse(ConfigurationManager.AppSettings["HeartBeat"]);
            int headBeatInt;
            if (!int.TryParse(heartBeatConfig, out headBeatInt))
            {
                headBeatInt = 5000;
            }

            responsemsg.SessionID= session.SessionID;
            responsemsg.LoginID = request.LoginID;
            responsemsg.HeadBeatInterVal = headBeatInt;
            responsemsg.SessionTimeOut = headBeatInt * 3;
            responsemsg.LoginFailReson = loginFailMsg;
            LoginSuccessMessage.SetMessageBody(responsemsg);
            session.Socket.SendMessge(LoginSuccessMessage);

            if (!canLogin)
            {
                session.Close();
                Console.WriteLine("{0}登录失败", request.LoginID);
            }

            if (ex != null)
            {
                throw ex;
            }
        }

        private void App_LoginOut(Message message, Session session)
        {
            Message msg = new Message(MessageType.LOGOUT);

            session.Socket.SendMessge(msg);
            appSockets.Remove(session.SessionID);
            session.IsValid = false;

            Console.WriteLine(string.Format("{0}已退出登陆", session.UserName));
        }

        protected virtual void FormAppMessage(Message message, Session session)
        {

        }

        private bool _serverModeNeedLogin = true;
        /// <summary>
        /// 如果是服务端模式，是否可以登录
        /// </summary>
        protected bool ServerModeNeedLogin
        {
            get
            {
                return _serverModeNeedLogin;
            }
            set
            {
                _serverModeNeedLogin = value;
            }
        }
        /// <summary>
        /// 从客户端发来的消息
        /// </summary>
        /// <param name="s"></param>
        /// <param name="message"></param>
        protected override void FormApp(Message message, Session session)
        {
            session.LastSessionTime = DateTime.Now;

            if (ServerModeNeedLogin && !session.IsLogin && !message.IsMessage(MessageType.LOGIN))
            {
                Message msg = new Message(MessageType.RELOGIN);
                session.Socket.SendMessge(msg);
                return;
            }

            if (message.IsMessage(MessageType.LOGIN))
            {
                App_Login(message, session);
            }
            if (message.IsMessage(MessageType.LOGOUT))
            {
                App_LoginOut(message, session);
            }
            else if (message.IsMessage(MessageType.HEARTBEAT))
            {
                App_HeatBeat(message, session);
            }
            else
            {
                FormAppMessage(message, session);
            }

            base.FormApp(message, session);
        }
        #endregion
    }
}
