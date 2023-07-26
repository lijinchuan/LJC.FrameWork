using LJC.FrameWork.ConfigurationSectionHandler;
using LJC.FrameWork.EntityBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketApplication.SocketSTD
{
    public class SessionClient : SessionMessageApp
    {
        private ConcurrentDictionary<string, AutoReSetEventResult> watingEvents;

        //private static readonly object LockObj = new object();
        private ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim();

        static SessionClient()
        {
        }

        public SessionClient(string serverIP, int serverPort,bool isSecurity, bool startSession=true)
            : base(serverIP, serverPort,isSecurity)
        {
            watingEvents = new ConcurrentDictionary<string, AutoReSetEventResult>();
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
                watingEvents.TryAdd(reqID, autoResetEvent);
                SendMessage(message);
                //new Func<Message, bool>(SendMessage).BeginInvoke(message, null, null);
                WaitHandle.WaitAny(new WaitHandle[] { autoResetEvent }, timeOut);
                AutoReSetEventResult removedicitem = null;
                watingEvents.TryRemove(reqID,out removedicitem);

                if (autoResetEvent.DataException != null)
                {
                    throw autoResetEvent.DataException;
                }

                if (autoResetEvent.IsTimeOut)
                {
                    var ex = new TimeoutException();
                    ex.Data.Add("errorsender", "LJC.FrameWork.SocketApplication.SocketSTD.SessionClient");
                    ex.Data.Add("MessageType", message.MessageHeader.MessageType);
                    ex.Data.Add("TransactionID", message.MessageHeader.TransactionID);
                    ex.Data.Add("ipString", this.ipString);
                    ex.Data.Add("ipPort", this.ipPort);
                    if (message.MessageBuffer != null)
                    {
                        ex.Data.Add("MessageBuffer", Convert.ToBase64String(message.MessageBuffer));
                    }
                    ex.Data.Add("resulttype", typeof(T).FullName);
                    LogManager.LogHelper.Instance.Error("SendMessageAnsy", ex);
                    throw ex;
                }
                else
                {
                    if (autoResetEvent.DataException != null)
                    {
                        throw autoResetEvent.DataException;
                    }
                    try
                    {  
                        T result = EntityBufCore.DeSerialize<T>((byte[])autoResetEvent.WaitResult);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Exception e = new Exception("解析消息体失败：" + reqID, ex);
                        e.Data.Add("messageid", reqID);
                        throw e;
                    }
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
            return message.MessageBuffer;
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


                byte[] result = null;
                Exception innerex = null;

                try
                {

                    result = DoMessage(message);
                }
                catch (Exception ex)
                {
                    innerex = ex;
                }

                AutoReSetEventResult autoEvent=null;
                if (watingEvents.TryGetValue(message.MessageHeader.TransactionID, out autoEvent))
                {
                    autoEvent.WaitResult = result;
                    autoEvent.IsTimeOut = false;
                    autoEvent.DataException = innerex;
                    autoEvent.Set();
                    return;
                }
            }

            base.ReciveMessage(message);
        }
    }
}
