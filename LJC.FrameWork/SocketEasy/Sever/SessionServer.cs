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
    public class SessionServer:ServerBase/*ServerHugeBase*/
    {
        protected Dictionary<string, Session> appSockets;

        private Dictionary<string, AutoReSetEventResult> watingEvents;

        private ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim();

        public SessionServer(string[] ips, int port)
            : base(ips, port)
        {
            appSockets = new Dictionary<string, Session>();
            watingEvents = new Dictionary<string, AutoReSetEventResult>();
        }

        public SessionServer(int port)
            : base(null, port)
        {
            appSockets = new Dictionary<string, Session>();
            watingEvents = new Dictionary<string, AutoReSetEventResult>();
        }

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
                lock (appSockets)
                {
                    if (appSockets.ContainsKey(session.SessionID))
                    {
                        appSockets.Remove(session.SessionID);
                    }
                    appSockets.Add(session.SessionID, session);
                }
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
            lock (appSockets)
            {
                appSockets.Remove(session.SessionID);
            }
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

        #region 同步消息

        public T SendMessageAnsy<T>(Session s, Message message, int timeOut = 60000)
        {
            if (string.IsNullOrEmpty(message.MessageHeader.TransactionID))
                throw new Exception("消息没有设置唯一的序号。无法进行同步。");

            string reqID = message.MessageHeader.TransactionID;

            using (AutoReSetEventResult autoResetEvent = new AutoReSetEventResult(reqID))
            {
                watingEvents.Add(reqID, autoResetEvent);
                if (s.Socket.SendMessge(message))
                {
                    WaitHandle.WaitAny(new WaitHandle[] { autoResetEvent }, timeOut);

                    watingEvents.Remove(reqID);

                    if (autoResetEvent.IsTimeOut)
                    {
                        throw new Exception("请求超时");
                    }
                    else
                    {
                        T result = EntityBufCore.DeSerialize<T>((byte[])autoResetEvent.WaitResult,SocketApplicationComm.IsMessageCompress);
                        return result;
                    }
                }
                else
                {
                    throw new Exception("发送失败。");
                }
            }
        }

        protected void FormAppMessage(Message message, Session session)
        {
            //base.ReciveMessage(message);
            byte[] result = message.MessageBuffer;

            if (result != null && !string.IsNullOrEmpty(message.MessageHeader.TransactionID))
            {
                if (watingEvents.Count == 0)
                    return;

                AutoReSetEventResult autoEvent = watingEvents.First(p => p.Key == message.MessageHeader.TransactionID).Value;
                if (autoEvent != null)
                {
                    autoEvent.WaitResult = result;
                    autoEvent.IsTimeOut = false;
                    autoEvent.Set();
                    return;
                }
            }
        }
        #endregion
    }
}
