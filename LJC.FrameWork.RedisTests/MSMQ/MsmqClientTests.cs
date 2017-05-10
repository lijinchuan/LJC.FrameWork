using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LJC.FrameWork.MSMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace LJC.FrameWork.MSMQ.Tests
{
    [TestClass()]
    public class MsmqClientTests
    {
        [TestMethod()]
        public void SplitMessageTest()
        {
            var msg = "12345678911abasdgaefefsf";

            var list = MsmqClient.SplitMessage(msg).ToList();
        }
    }
}
