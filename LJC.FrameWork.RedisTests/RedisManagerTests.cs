using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LJC.FrameWork.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LJC.FrameWork.Redis.Tests
{
    [TestClass()]
    public class RedisManagerTests
    {
        [TestMethod()]
        public void GetClientTest()
        {
            using(var client=RedisManager.Instance.GetClient())
            {
                client.PublishMessage("KeywordChannel", "asdfsad");
            }
        }
    }
}
