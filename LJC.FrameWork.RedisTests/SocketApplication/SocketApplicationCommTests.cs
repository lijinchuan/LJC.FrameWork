using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LJC.FrameWork.SocketApplication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace LJC.FrameWork.SocketApplication.Tests
{
    [TestClass()]
    public class SocketApplicationCommTests
    {
        [TestMethod()]
        public void SendMessgeTest()
        {
            System.Net.Sockets.Socket s = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            var msg = new Message();
            msg.MessageHeader = new MessageHeader
            {
                MessageTime=DateTime.Now,
                MessageType=0,
                TransactionID="1"
            };
            msg.SetMessageBody("hello word");
            s.SendMessage(msg, string.Empty);
        }
    }
}
