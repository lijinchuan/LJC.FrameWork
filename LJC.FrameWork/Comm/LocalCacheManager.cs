using LJC.FrameWork.LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    public static class LocalCacheManager<T>
    {
        private static Dictionary<string, object> _lockDiction = new Dictionary<string, object>();
        private static Dictionary<string, CacheItem<T>> _cacheDiction = new Dictionary<string, CacheItem<T>>();
        private static object _cacheLock = new object();
        private static Timer _backgroundtasktimer;

        private static string _LocalCached = AppDomain.CurrentDomain.BaseDirectory + "\\LocalCached\\";
        private static string _WorkCached;

        static LocalCacheManager()
        {
            _backgroundtasktimer = new Timer(new TimerCallback(CheckCachedData), null, 1000, 1000);

            #region 创建工作文件夹
            if (!System.IO.Directory.Exists(_LocalCached))
            {
                System.IO.Directory.CreateDirectory(_LocalCached);
            }

            _WorkCached = _LocalCached + "\\" + HashEncrypt.MD5_JS(typeof(T).FullName) + "\\";
            if (!System.IO.Directory.Exists(_WorkCached))
            {
                System.IO.Directory.CreateDirectory(_WorkCached);
            }
            #endregion
        }

        private static void CheckExpirsForDel()
        {
            List<string> delKeys = null;
            lock (_cacheLock)
            {
                delKeys = _cacheDiction.Where(p => p.Value.Expired < DateTime.Now && p.Value.RefrashFunc == null).Select(p => p.Key).ToList();
            }

            if (delKeys.Count > 0)
            {
                foreach (var key in delKeys)
                {
                    lock (_cacheLock)
                    {
                        _lockDiction.Remove(key);
                        _cacheDiction.Remove(key);
                    }
                    //LogHelper.Instance.Info("移除缓存:" + key);
                }
            }
        }

        private static void CheckExpirsForUpdate()
        {
            List<KeyValuePair<string, CacheItem<T>>> items = null;
            lock (_cacheLock)
            {
                items = _cacheDiction.Where(p => p.Value.Expired < DateTime.Now && p.Value.RefrashFunc != null).ToList();
            }

            var expiredItems = items
                .Select(p => new
                {
                    p.Key,
                    p.Value.CachMinis,
                    fun = p.Value.RefrashFunc
                }
                ).ToList();

            foreach (var x in expiredItems)
            {
                try
                {
                    var result = x.fun();

                    lock (_cacheLock)
                    {
                        _cacheDiction.Remove(x.Key);
                        if (!object.Equals(default(T), result))
                        {
                            _cacheDiction.Add(x.Key, new CacheItem<T>
                            {
                                Item = result,
                                Expired = DateTime.Now.AddMinutes(x.CachMinis),
                                RefrashFunc = x.fun,
                            });
                        }
                    }

                    new Action(() =>
                    {
                        string cachfile = _WorkCached + x.Key + ".dat";
                        SerializerHelper.BinarySave(cachfile, result);

                    }).BeginInvoke(null, null);

                    //LogHelper.Instance.Info("后台更新缓存完成:" + x.Key);
                }
                catch (Exception ex)
                {
                    //throw ex;
                    LogHelper.Instance.Error("更新缓存" + x.Key + "出错", ex);
                }
            }
        }

        private static void CheckCachedData(object param)
        {
            _backgroundtasktimer.Change(Timeout.Infinite, Timeout.Infinite);
            try
            {
                //删除过期数据
                CheckExpirsForDel();

                //刷新数据
                CheckExpirsForUpdate();

            }
            finally
            {
                _backgroundtasktimer.Change(1000, 1000);
            }
        }

        /// <summary>
        /// 从缓存里面取数据，且会检查过期时间，如果过期了，用刷新方法刷新数据，
        /// </summary>
        /// <param name="key">缓存的Key</param>
        /// <param name="reflashFunc">刷新方法</param>
        /// <param name="cachedMins">缓存时间</param>
        /// <returns></returns>
        public static T Find(string key, Func<T> reflashFunc, int cachedMins = 10)
        {
            if (string.IsNullOrWhiteSpace(key) || reflashFunc == null)
            {
                if (reflashFunc != null)
                    return reflashFunc();

                return default(T);
            }

            CacheItem<T> val = null;

            if (_cacheDiction.TryGetValue(key, out val) && val != null && val.Expired < DateTime.Now)
            {
                return val.Item;
            }

            object locker;
            lock (_cacheLock)
            {
                if (!_lockDiction.TryGetValue(key, out locker))
                {
                    locker = new object();
                    _lockDiction.Add(key, locker);
                }
            }

            lock (locker)
            {
                if (!_cacheDiction.TryGetValue(key, out val) || val == null || val.Expired < DateTime.Now)
                {

                    val = new CacheItem<T>();
                    val.Item = reflashFunc();

                    if (!object.Equals(val.Item, default(T)))
                    {
                        lock (_cacheLock)
                        {
                            _cacheDiction.Remove(key);
                            _cacheDiction.Add(key, val);
                        }
                    }
                }
            }
            return val.Item;
        }

        /// <summary>
        /// 从缓存里面刷新数据，但不检查过期，会把刷新方法加入到后台定时检查更新缓存数据
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="reflashFunc">刷新方法</param>
        /// <param name="cachedMins">缓存时间</param>
        /// <returns></returns>
        /// 
        public static T FindCache(string key, Func<T> reflashFunc, int cachedMins = 10)
        {
            if (string.IsNullOrWhiteSpace(key) || reflashFunc == null)
            {
                if (reflashFunc != null)
                    return reflashFunc();

                return default(T);
            }

            CacheItem<T> val = null;
            if (_cacheDiction.TryGetValue(key, out val) && val != null)
            {
                return val.Item;
            }

            object locker;
            lock (_cacheLock)
            {
                if (!_lockDiction.TryGetValue(key, out locker))
                {
                    locker = new object();
                    _lockDiction.Add(key, locker);
                }
            }

            lock (locker)
            {
                try
                {
                    if (!_cacheDiction.TryGetValue(key, out val) || val == null)
                    {
                        string cachfile = _WorkCached + key + ".dat";
                        val = new CacheItem<T>();
                        val.RefrashFunc = reflashFunc;
                        val.CachMinis = cachedMins;

                        val.Item = (T)SerializerHelper.BinaryGet(cachfile);

                        if (object.Equals(val.Item, default(T)) && !object.Equals(val.Item = reflashFunc(), default(T)))
                        {
                            new Action(() => SerializerHelper.BinarySave(cachfile, val.Item)).BeginInvoke(null, null);
                        }

                        if (!object.Equals(val.Item, default(T)))
                        {
                            lock (_cacheLock)
                            {
                                _cacheDiction.Remove(key);
                                _cacheDiction.Add(key, val);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                    //LogHelper.Instance.Error("[FindCache]error:" + key, ex);
                }
            }
            return val.Item;
        }
    }
}
