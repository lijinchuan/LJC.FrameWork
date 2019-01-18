using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    public class MySession:LJC.FrameWork.SocketEasy.Sever.SessionServer
    {
        public MySession()
            : base(5555)
        {

        }

        protected override void FromApp(LJC.FrameWork.SocketApplication.Message message, LJC.FrameWork.SocketApplication.Session session)
        {
            if(message.MessageHeader.MessageType==10240)
            {
                var msg = new LJC.FrameWork.SocketApplication.Message
                {
                    MessageHeader = message.MessageHeader,
                };
                msg.MessageHeader.MessageTime = DateTime.Now;
                msg.SetMessageBody(message.MessageHeader.TransactionID);
                session.SendMessage(msg);

                //Console.WriteLine("收到消息:" + msg.MessageHeader.TransactionID);

                return;
            }

            base.FromApp(message, session);
            
        }

        protected override void OnError(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
