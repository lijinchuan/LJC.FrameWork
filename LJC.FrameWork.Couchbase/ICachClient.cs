using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.MemCached
{
    public interface ICachClient
    {
        bool Store(StoreMode storemode, string key, object value);

        bool Store(StoreMode storemode, string key, object value, DateTime expirsAt);

        bool Store(StoreMode storemode, string key, object value, TimeSpan validFor);

        T Get<T>(string key);

        bool Remove(string key);

        bool KeyExists(string key);

        bool TryGet(string key, out object oldval);
    }
}
