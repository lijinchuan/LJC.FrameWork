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
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Restart();
            using(var client=RedisManager.Instance.GetClient())
            {
                client.PublishMessage("KeywordChannel", "asdfsad");
                //var bt = client.Get("x");
            }

            sw.Stop();

            var mill = sw.ElapsedMilliseconds;
        }
    }
}
