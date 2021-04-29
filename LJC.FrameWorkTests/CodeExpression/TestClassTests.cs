using Microsoft.VisualStudio.TestTools.UnitTesting;
using LJC.FrameWork.CodeExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.Tests
{
    [TestClass()]
    public class TestClassTests
    {
        [TestMethod()]
        public void TestDateAddTest()
        {
            new TestClass().TestDateAdd();
        }
    }
}