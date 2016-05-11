using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;

namespace LJC.FrameWork.Redis
{
    /// <summary>
    /// Redis
    /// </summary>
    public class RedisManager
    {
        private static Dictionary<string, RedisManager> _InstancePoll = new Dictionary<string, RedisManager>();

        private static RedisManager _Instance = null;
        //private static string[] readWriteHosts;
        private string[] readWriteHosts;

        private long _defaultdb = 0;
        /// <summary>
        /// redis连接池
        /// </summary>
        //private static PooledRedisClientManager redisClientManager = null;
        private PooledRedisClientManager redisClientManager = null;
        /// <summary>
        /// 并发锁
        /// </summary>
        private static object initLock = new object();
        private static ReaderWriterLockSlim RWLock = new ReaderWriterLockSlim();

        private RedisManager()
        {
            if (redisClientManager == null)
            {
                string hosts = ConfigurationManager.AppSettings["Host_Redis"];
                if (string.IsNullOrWhiteSpace(hosts))
                    throw new Exception("未配置host服务器地址和端口号，请在配置文件中添加[Host_Redis]配置。");
                readWriteHosts = new string[] { hosts };

                redisClientManager = new PooledRedisClientManager(readWriteHosts, readWriteHosts,
                    new RedisClientManagerConfig()
                    {
                        MaxReadPoolSize = 10,
                        MaxWritePoolSize = 10,
                        AutoStart = true
                    });
                redisClientManager.SocketReceiveTimeout = 60000;
                redisClientManager.SocketSendTimeout = 60000;
            }
        }

        private RedisManager(string[] readWriteHosts, RedisConfig cfg)
        {
            if (redisClientManager == null)
            {
                this.readWriteHosts = readWriteHosts;

                var confg = new RedisClientManagerConfig()
                {
                    MaxReadPoolSize = cfg.MaxReadPoolSize,
                    MaxWritePoolSize = cfg.MaxWritePoolSize,
                    AutoStart = true,
                };

                if (cfg.DefaultDB != 0)
                {
                    confg.DefaultDb = cfg.DefaultDB;
                }

                redisClientManager = new PooledRedisClientManager(readWriteHosts, readWriteHosts,
                    confg
                    );

                redisClientManager.SocketReceiveTimeout = 60000;
                redisClientManager.SocketSendTimeout = 60000;
            }
        }

        [Obsolete("用GetClient方法代替")]
        public static RedisManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (initLock)
                    {
                        _Instance = new RedisManager();
                    }
                }

                return _Instance;
            }
        }

        public RedisClient GetClient()
        {
            var client = redisClientManager.GetClient() as RedisClient;
            return client;
        }

        private static bool Eq(string[] s1, string[] s2)
        {
            if (s1 != null && s2 != null)
            {
                var ss1 = s1.Distinct().OrderBy(p => p).ToArray();
                var ss2 = s2.Distinct().OrderBy(p => p).ToArray();
                if (ss1.Length == ss2.Length)
                {
                    for (int i = 0; i < ss1.Length; i++)
                    {
                        if (!ss1[i].Equals(ss2[i]))
                            return false;
                    }
                    return true;
                }
            }

            return false;
        }

        public static RedisClient GetClient(string redisInstance, RedisConfig cfg)
        {
            RedisManager man;

            try
            {
                RWLock.EnterUpgradeableReadLock();
                if (_InstancePoll.TryGetValue(redisInstance, out man))
                {
                    return man.GetClient() as RedisClient;
                }

                try
                {
                    RWLock.EnterWriteLock();
                    string hosts = ConfigurationManager.AppSettings[redisInstance];
                    if (string.IsNullOrWhiteSpace(hosts))
                        throw new Exception("未配置host服务器地址和端口号，请在配置文件中添加[" + redisInstance + "]配置。");

                    var readWriteHosts = hosts.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);

                    man = _InstancePoll.FirstOrDefault(p => p.Value._defaultdb == cfg.DefaultDB && Eq(p.Value.readWriteHosts, readWriteHosts)).Value;

                    if (man == null)
                    {
                        man = new RedisManager(readWriteHosts, cfg);
                    }

                    _InstancePoll.Add(redisInstance, man);

                    return man.GetClient() as RedisClient;
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }
            }
            finally
            {
                RWLock.ExitUpgradeableReadLock();
            }
        }

        public static RedisClient GetClient(string redisInstance, long defaultdb = 0)
        {
            return GetClient(redisInstance, new RedisConfig
            {
                DefaultDB = defaultdb
            });
        }

        public RedisClient GetSubscribeClient()
        {
            var clinet = redisClientManager.GetReadOnlyClient() as RedisClient;
            clinet.ReceiveTimeout = 0;
            return clinet;
        }

        public IRedisClient GetIRedisClient()
        {
            return redisClientManager.GetClient();
        }

        public RedisClientFactory GetFactory()
        {
            return redisClientManager.RedisClientFactory as RedisClientFactory;
        }
    }
}
