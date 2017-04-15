using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class LockHerper
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, object> _lockerdic = new System.Collections.Concurrent.ConcurrentDictionary<string, object>();
        private static object _locker = new object();

        public static object GetLocker(string lockname)
        {
            if(string.IsNullOrWhiteSpace(lockname))
            {
                throw new ArgumentException("lockname");
            }
            object obj=null;
            if(_lockerdic.TryGetValue(lockname, out obj))
            {
                return obj;
            }
            lock (_locker)
            {
                if (_lockerdic.TryGetValue(lockname, out obj))
                {
                    return obj;
                }

                obj = new object();
                if (_lockerdic.TryAdd(lockname, obj))
                {
                    return obj;
                }
                throw new Exception("添加锁失败："+lockname);
            }
        }
    }
}
