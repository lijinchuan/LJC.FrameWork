using Enyim.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.MemCached
{
    public sealed class MemcachedClient:ICachClient
    {
        private IMemcachedClient _client;

        public MemcachedClient(string serverip, int port, string bucket)
        {
            _client = MemcachedHelper.GetClient(serverip, port, bucket);
        }

        public bool Store(StoreMode storemode, string key, object value)
        {
            return _client.Store((Enyim.Caching.Memcached.StoreMode)storemode, key, value);
        }

        public bool Store(StoreMode storemode, string key, object value, DateTime expirsAt)
        {
            return _client.Store((Enyim.Caching.Memcached.StoreMode)storemode, key, value, expirsAt);
        }

        public bool Store(StoreMode storemode, string key, object value, TimeSpan validFor)
        {
            return _client.Store((Enyim.Caching.Memcached.StoreMode)storemode, key, value, validFor);
        }

        public T Get<T>(string key)
        {
            return _client.Get<T>(key);
        }

        public bool Remove(string key)
        {
            return _client.Remove(key);
        }

        public bool KeyExists(string key)
        {
            object obj = null;
            return _client.TryGet(key, out obj);
        }

        public bool TryGet(string key, out object oldval)
        {
            return _client.TryGet(key, out oldval);
        }
    }
}
