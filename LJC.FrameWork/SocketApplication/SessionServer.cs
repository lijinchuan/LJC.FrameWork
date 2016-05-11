using LJC.FrameWork.EntityBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketApplication
{
    public class SessionServer:SessionMessageApp
    {
        private Dictionary<string, AutoReSetEventResult> watingEvents;

        //private static readonly object LockObj = new object();
        private ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim();

        static SessionServer()
        {

        }

        public SessionServer(int serverPort)
            : base(serverPort)
        {
            watingEvents = new Dictionary<string, AutoReSetEventResult>();
        }

        public T SendMessageAnsy<T>(Session s,Message message, int timeOut = 60000)
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
                        T result = EntityBufCore.DeSerialize<T>((byte[])autoResetEvent.WaitResult);
                        return result;
                    }
                }
                else
                {
                    throw new Exception("发送失败。");
                }
            }
        }

        /// <summary>
        /// 处理自定义消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual byte[] DoMessage(Message message)
        {
            return null;
        }


        protected sealed override void OnLoginFail(string failMsg)
        {
            base.OnLoginFail(failMsg);
        }

        protected sealed override void OnLoginSuccess()
        {
            base.OnLoginSuccess();
        }

        protected sealed override void OnSessionResume()
        {
            base.OnSessionResume();
        }

        protected sealed override void OnSessionTimeOut()
        {
            base.OnSessionTimeOut();
        }

        protected sealed override void ReciveMessage(Message message)
        {
            base.ReciveMessage(message);
        }

        protected override void FormAppMessage(Message message, Session session)
        {
            //base.ReciveMessage(message);
            byte[] result = DoMessage(message);

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

            base.FormAppMessage(message, session);
        }

        
    }
}
