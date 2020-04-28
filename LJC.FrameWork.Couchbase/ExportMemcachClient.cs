using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.MemCached
{
    public sealed class ExportMemcachClient : ICachClient
    {
        private static Dictionary<string, ICachClient> clients = new Dictionary<string, ICachClient>();
        private ICachClient _client = null;
        public ExportMemcachClient(string dll, string serverip, int port, string bucket)
        {
            var key = $"{dll}_{serverip}:{port}_{bucket}";
            ICachClient cachClient = null;
            if(clients.TryGetValue(key,out cachClient) && cachClient != null)
            {
                _client = cachClient;
                return;
            }

            lock (clients)
            {
                if (clients.TryGetValue(key, out cachClient) && cachClient != null)
                {
                    _client = cachClient;
                    return;
                }

                var assembly = System.Reflection.Assembly.LoadFrom(dll);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetInterface(nameof(ICachClient)) != null)
                    {
                        var constructor = type.GetConstructor(new[] { typeof(string), typeof(int), typeof(string) });
                        if (constructor == null)
                        {
                            throw new Exception("没有合适的构造函数");
                        }
                        var obj = constructor.Invoke(new object[] { serverip, port, bucket });
                        _client = (ICachClient)obj;
                        break;
                    }
                    else
                    {
                        throw new Exception("没有实现ICachClient");
                    }
                }

                if (_client == null)
                {
                    throw new Exception("初始化失败，找不到对象");
                }

                clients.Add(key, _client);
            }
        }

        public T Get<T>(string key)
        {
            return _client.Get<T>(key);
        }

        public bool KeyExists(string key)
        {
            return _client.KeyExists(key);
        }

        public bool Remove(string key)
        {
            return _client.Remove(key);
        }

        public bool Store(StoreMode storemode, string key, object value)
        {
            return _client.Store(storemode, key, value);
        }

        public bool Store(StoreMode storemode, string key, object value, DateTime expirsAt)
        {
            return _client.Store(storemode, key, value, expirsAt);
        }

        public bool Store(StoreMode storemode, string key, object value, TimeSpan validFor)
        {
            return _client.Store(storemode, key, value, validFor);
        }

        public bool TryGet(string key, out object oldval)
        {
            return _client.TryGet(key, out oldval);
        }
    }
}
