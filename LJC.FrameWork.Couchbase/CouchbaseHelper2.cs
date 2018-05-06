using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CB = Couchbase;

namespace LJC.FrameWork.Couchbase
{
    public static class CouchbaseHelper2
    {
        static ConcurrentDictionary<string, PreCouchBaseClient> ClientDic = new ConcurrentDictionary<string, PreCouchBaseClient>();

        public static PreCouchBaseClient GetClient(string sectionname)
        {
            PreCouchBaseClient client = null;
            if (!ClientDic.TryGetValue(sectionname, out client))
            {
                if (string.IsNullOrWhiteSpace(sectionname))
                {
                    throw new ArgumentNullException("clientname");
                }

                client = new PreCouchBaseClient(sectionname);
                ClientDic.TryAdd(sectionname, client);
            }
            return client;
        }

        public static PreCouchBaseClient GetClient(string serverip, string bucket)
        {
            PreCouchBaseClient client = null;
            string key = serverip + bucket;
            if (!ClientDic.TryGetValue(key, out client))
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentNullException("serverip&bucket");
                }

                CB.Configuration.CouchbaseClientConfiguration config = new CB.Configuration.CouchbaseClientConfiguration();
                if (!string.IsNullOrWhiteSpace(bucket))
                {
                    config.Bucket = bucket;
                }
                config.SocketPool.MaxPoolSize = 10;
                config.SocketPool.MinPoolSize = 5;
                config.Urls.Add(new Uri(string.Format("http://{0}:8091/pools", serverip)));
                client = new PreCouchBaseClient(config);

                ClientDic.TryAdd(key, client);
            }

            return client;
        }

        public static PreCouchBaseClient GetClient(string serverip, int port, string bucket)
        {
            PreCouchBaseClient client = null;
            string key = serverip + bucket;
            if (!ClientDic.TryGetValue(key, out client))
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentNullException("serverip&bucket");
                }

                CB.Configuration.CouchbaseClientConfiguration config = new CB.Configuration.CouchbaseClientConfiguration();
                if (!string.IsNullOrWhiteSpace(bucket))
                {
                    config.Bucket = bucket;
                }
                config.SocketPool.MaxPoolSize = 10;
                config.SocketPool.MinPoolSize = 5;
                config.Urls.Add(new Uri(string.Format("http://{0}:{1}/pools", serverip, port)));
                client = new PreCouchBaseClient(config);

                ClientDic.TryAdd(key, client);
            }

            return client;
        }

        public static bool Store(this PreCouchBaseClient client, StoreMode storemode, string key, object value)
        {
            return client.Store((Enyim.Caching.Memcached.StoreMode)storemode, key, value);
        }

        public static bool Store(this PreCouchBaseClient client, StoreMode storemode, string key, object value, DateTime expirsAt)
        {
            return client.Store((Enyim.Caching.Memcached.StoreMode)storemode, key, value, expirsAt);
        }

        public static bool Store(this PreCouchBaseClient client, StoreMode storemode, string key, object value, TimeSpan validFor)
        {
            return client.Store((Enyim.Caching.Memcached.StoreMode)storemode, key, value, validFor);
        }

        public static T Get<T>(this PreCouchBaseClient client, string key)
        {
            return client.Get<T>(key);
        }

        public static bool Remove(this PreCouchBaseClient client, string key)
        {
            return client.Remove(key);
        }
    }
}
