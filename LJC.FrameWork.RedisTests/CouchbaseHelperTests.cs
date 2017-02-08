using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LJC.FrameWork.Couchbase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace LJC.FrameWork.Couchbase.Tests
{
    [TestClass()]
    public class CouchbaseHelperTests
    {
        [TestMethod()]
        public void GetClientTest()
        {
            //new CouchbaseHelper().GetClient("couchbase1");

            //var bb = CouchbaseHelper.Store(CouchbaseHelper.GetClient("couchbase1"), StoreMode.Add, "test_1", "中国人民");

            //var ss = CouchbaseHelper.Get<string>(CouchbaseHelper.GetClient("couchbase1"), "test_1");

            var ss = CouchbaseHelper.Get<string>(CouchbaseHelper.GetClient("127.0.0.1",""), "test_1");
        }
    }
}
