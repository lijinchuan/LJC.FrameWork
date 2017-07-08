﻿using LJC.FrameWork.SocketEasyUDP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP.Server
{
    public class SessionServer: ServerBase
    {
        private Dictionary<string, UDPSession> _sessiondic = new Dictionary<string, UDPSession>();
        private Dictionary<string, AutoReSetEventResult> watingEvents = new Dictionary<string, AutoReSetEventResult>();

        public SessionServer(string ip,int port) : base(ip, port)
        {

        }

        public SessionServer(int port) : base(port)
        {

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

        private void App_Login(Message message, UDPSession session)
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
                lock (_sessiondic)
                {
                    if (_sessiondic.ContainsKey(session.SessionID))
                    {
                        _sessiondic.Remove(session.SessionID);
                    }
                    _sessiondic.Add(session.SessionID, session);
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

            responsemsg.SessionID = session.SessionID;
            responsemsg.LoginID = request.LoginID;
            responsemsg.HeadBeatInterVal = headBeatInt;
            responsemsg.SessionTimeOut = headBeatInt * 3;
            responsemsg.LoginFailReson = loginFailMsg;
            LoginSuccessMessage.SetMessageBody(responsemsg);
            SendMessage(LoginSuccessMessage, session.EndPoint);

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

        private void App_LoginOut(Message message, UDPSession session)
        {
            Message msg = new Message(MessageType.LOGOUT);

            session.Socket.SendMessge(msg);
            lock (_sessiondic)
            {
                _sessiondic.Remove(session.SessionID);
            }
            session.IsValid = false;

            Console.WriteLine(string.Format("{0}已退出登陆", session.UserName));
        }

        private void App_HeatBeat(Message message, UDPSession session)
        {
            Message msg = new Message(MessageType.HEARTBEAT);
            msg.SetMessageBody(session.SessionID);

            session.LastSessionTime = DateTime.Now;
            session.SendMessage(msg);

            SocketApplicationComm.Debug(string.Format("{0}发来心跳！", session.SessionID));
        }

        protected virtual void FromSessionMessage(Message message,UDPSession session)
        {

        }

        protected sealed override void FromApp(Message message, EndPoint endpoint)
        {
            var ipendpoint = ((IPEndPoint)endpoint);
            string key = string.Format("{0}_{1}", ipendpoint.Address.ToString(), ipendpoint.Port);
            UDPSession session = null;
            if (_sessiondic.TryGetValue(key, out session))
            {
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
                    //if(!string.IsNullOrWhiteSpace(message.MessageHeader.TransactionID))
                    //{
                    //    Console.WriteLine("收到消息:" + message.MessageHeader.TransactionID);
                    //}

                    FromSessionMessage(message, session);
                }
            }
            else
            {
                if (message.IsMessage(MessageType.LOGIN))
                {
                    App_Login(message, new UDPSession { SessionID = key, SessionServer = this });
                }
                else
                {
                    Message msg = new Message(MessageType.RELOGIN);
                    SendMessage(msg, endpoint);
                    return;
                }
            }
            base.FromApp(message, endpoint);
        }
    }
}