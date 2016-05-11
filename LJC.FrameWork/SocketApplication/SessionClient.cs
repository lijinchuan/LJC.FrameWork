using LJC.FrameWork.EntityBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketApplication
{
    public class SessionClient : SessionMessageApp
    {
        protected Exception BuzException = null;
        private Dictionary<string, AutoReSetEventResult> watingEvents;

        //private static readonly object LockObj = new object();
        private ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim();

        static SessionClient()
        {

        }

        public SessionClient(string serverIP, int serverPort,bool startSession=true)
            : base(serverIP, serverPort)
        {
            watingEvents = new Dictionary<string, AutoReSetEventResult>();
            if (startSession)
            {
                StartSession();
            }
        }

        /// <summary>
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
                new Func<Message, bool>(SendMessage).BeginInvoke(message, null, null);
                WaitHandle.WaitAny(new WaitHandle[] { autoResetEvent }, timeOut);
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

        /// <summary>
        /// 处理自定义消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual byte[] DoMessage(Message message)
        {
            return null;
        }

        protected sealed override void FormAppMessage(Message message, Session session)
        {
            base.FormAppMessage(message, session);
        }

        protected sealed override bool OnUserLogin(string user, string pwd, out string loginFailReson)
        {
            return base.OnUserLogin(user, pwd, out loginFailReson);
        }

        protected override void ReciveMessage(Message message)
        {
            //base.ReciveMessage(message);

            if (!string.IsNullOrEmpty(message.MessageHeader.TransactionID))
            {
                if (watingEvents.Count == 0)
                    return;

                byte[] result = DoMessage(message);

                AutoReSetEventResult autoEvent = watingEvents.First(p => p.Key == message.MessageHeader.TransactionID).Value;
                if (autoEvent != null)
                {
                    autoEvent.WaitResult = result;
                    autoEvent.IsTimeOut = false;
                    autoEvent.Set();
                    return;
                }
            }

            base.ReciveMessage(message);
        }
    }
}
