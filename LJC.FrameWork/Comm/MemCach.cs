using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    [Obsolete("请用LocalCacheManager代替。")]
    public static class MemCach
    {
        private static Dictionary<string, CachItem> CachPool;
        private static ReaderWriterLockSlim rwLock;
        public static int MemSize
        {
            get
            {
                try
                {
                    string file = "memcach.bin";
                    Comm.SerializerHelper.BinarySave(file, CachPool);
                    return (int)CommFun.GetSize(file);
                }
                catch(Exception e)
                {
                    return 0;
                }

            }
        }

        static MemCach()
        {
            CachPool = new Dictionary<string, CachItem>();
            rwLock = new ReaderWriterLockSlim();
            CommFun.SetInterval(5000, ClearTimeOutItem);
        }

        static bool ClearTimeOutItem()
        {
            try
            {
                rwLock.EnterUpgradeableReadLock();
                List<string> removeKey = new List<string>();
                foreach (KeyValuePair<string, CachItem> kv in CachPool)
                {
                    if (kv.Value.CachTime < DateTime.Now)
                    {
                        removeKey.Add(kv.Key);
                    }
                }

                try
                {
                    rwLock.EnterWriteLock();
                    removeKey.ForEach(k =>
                    {
                        CachPool.Remove(k);
                    });
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }

            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }

            return false;
        }

        public static object GetCach(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            try
            {
                rwLock.EnterReadLock();
                KeyValuePair<string, CachItem> kv = CachPool.FirstOrDefault(c => c.Key == key);

                CachItem item = kv.Value;
                if (item == null)
                    return null;

                if (kv.Value.CachTime < DateTime.Now)
                {
                    //CachPool.Remove(key);
                    return null;
                }

                return kv.Value.CachObj;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public static void AddCach(string key, object val, DateTime expirTime)
        {
            try
            {
                rwLock.EnterWriteLock();
                KeyValuePair<string, CachItem> kv = CachPool.FirstOrDefault(c => c.Key == key);
                if (kv.Value != null)
                {
                    CachPool.Remove(key);
                }

                if (expirTime > DateTime.Now)
                    CachPool.Add(key, new CachItem
                    {
                        CachTime = expirTime,
                        CachObj = val
                    });
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static void AddCach(string key, object val, int min = 30)
        {
            try
            {
                rwLock.EnterWriteLock();
                KeyValuePair<string, CachItem> kv = CachPool.FirstOrDefault(c => c.Key == key);

                if (kv.Value != null)
                {
                    CachPool.Remove(key);

                }

                CachPool.Add(key, new CachItem
                {
                    CachTime = DateTime.Now.AddMinutes(min),
                    CachObj = val
                });
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static bool RemoveCachItem(string cachKey)
        {
            try
            {
                rwLock.EnterWriteLock();
                return CachPool.Remove(cachKey);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static void ClearMem()
        {
            try
            {
                rwLock.EnterWriteLock();
                List<string> keys = CachPool.Keys.ToList();

                keys.ForEach(s => CachPool.Remove(s));
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
    }
}
