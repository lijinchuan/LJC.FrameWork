using LJC.FrameWork.Comm;
using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketEasy.Sever
{
    public class SessionServer:/*ServerBase*/ServerHugeBase
    {
        private Dictionary<string, AutoReSetEventResult> watingEvents;

        private ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim();

        public SessionServer(string[] ips, int port)
            : base(ips, port)
        {
            watingEvents = new Dictionary<string, AutoReSetEventResult>();
        }

        public SessionServer(int port)
            : base(null, port)
        {
            watingEvents = new Dictionary<string, AutoReSetEventResult>();
        }

        #region 服务器模式

        private void App_HeatBeat(Message message, Session session)
        {
            Message msg = new Message(MessageType.HEARTBEAT);
            msg.SetMessageBody(session.SessionID);

            //s.Send(msg.ToBytes());
            session.LastSessionTime = DateTime.Now;
            session.SendMessage(msg);

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
            session.SendMessage(LoginSuccessMessage);

            if (!canLogin)
            {
                session.Close("login error", true);
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

            session.SendMessage(msg);
            session.IsValid = false;

            Console.WriteLine(string.Format("{0}已退出登陆", session.UserName));
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
        protected override void FromApp(Message message, Session session)
        {
            if (ServerModeNeedLogin && !session.IsLogin && !message.IsMessage(MessageType.LOGIN))
            {
                Message msg = new Message(MessageType.RELOGIN);
                session.SendMessage(msg);
                return;
            }

            if (message.IsMessage(MessageType.LOGIN))
            {
                App_Login(message, session);
            }
            else if (message.IsMessage(MessageType.LOGOUT))
            {
                App_LoginOut(message, session);
            }
            else if (message.IsMessage(MessageType.HEARTBEAT))
            {
                App_HeatBeat(message, session);
            }

            base.FromApp(message, session);
        }

        #endregion
    }
}
