using Microsoft.VisualStudio.TestTools.UnitTesting;
using LJC.FrameWork.SOA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Tests
{
    [TestClass()]
    public class WebTransferSvcHelperTests
    {
        [TestMethod()]
        public void RelaceLocationTest()
        {
            var x = WebTransferSvcHelper.RelaceLocation("http://127.0.0.1:8080/webapp/", "118.24.243.32:8080", "http://127.0.0.1:8080/webapp");
        }
    }
}