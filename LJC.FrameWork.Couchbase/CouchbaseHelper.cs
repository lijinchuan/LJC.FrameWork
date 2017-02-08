using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CB = Couchbase;

namespace LJC.FrameWork.Couchbase
{
    public static class CouchbaseHelper
    {
        static ConcurrentDictionary<string, CB.CouchbaseClient> ClientDic = new ConcurrentDictionary<string, CB.CouchbaseClient>();

        public static CB.CouchbaseClient GetClient(string sectionname)
        {
            CB.CouchbaseClient client = null;
            if (!ClientDic.TryGetValue(sectionname, out client))
            {
                if (string.IsNullOrWhiteSpace(sectionname))
                {
                    throw new ArgumentNullException("clientname");
                }

                client = new CB.CouchbaseClient(sectionname);
                ClientDic.TryAdd(sectionname, client);
            }

            return client;
        }

        public static bool Store(this CB.CouchbaseClient client, StoreMode storemode, string key, object value)
        {
            return client.Store((Enyim.Caching.Memcached.StoreMode)storemode, key, value);
        }

        public static bool Store(this CB.CouchbaseClient client, StoreMode storemode, string key, object value, DateTime expirsAt)
        {
            return client.Store((Enyim.Caching.Memcached.StoreMode)storemode, key, value, expirsAt);
        }

        public static bool Store(this CB.CouchbaseClient client, StoreMode storemode, string key, object value, TimeSpan validFor)
        {
            return client.Store((Enyim.Caching.Memcached.StoreMode)storemode, key, value, validFor);
        }

        public static T Get<T>(this CB.CouchbaseClient client,string key)
        {
            return client.Get<T>(key);
        }

        public static bool Remove(this CB.CouchbaseClient client,string key)
        {
            return client.Remove(key);
        }
    }
}
