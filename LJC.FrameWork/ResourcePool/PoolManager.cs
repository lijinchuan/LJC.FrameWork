using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.ResourcePool
{
    public class PoolManager<T> where T : class
    {
        // Fields
        private static Resource<T>[] _AllAllocateRes;
        private static int _PoolIndex;
        private static int _PoolSize;
        private static int? _PoolTimeout;
        private static int _RecheckPoolAfterMs;

        // Methods
        static PoolManager()
        {
            PoolManager<T>._AllAllocateRes = null;
            PoolManager<T>._PoolSize = 10;
            PoolManager<T>._PoolTimeout = null;
            PoolManager<T>._RecheckPoolAfterMs = 0x186a0;
            PoolManager<T>._PoolIndex = 0;
        }

        public PoolManager()
            : this(new PoolConfig { MaxPoolSize = 10, PoolTimeout = 0xea60 })
        {
        }

        public PoolManager(PoolConfig config)
        {
            if (config.MaxPoolSize.HasValue)
            {
                PoolManager<T>._PoolSize = config.MaxPoolSize.Value;
            }
            PoolManager<T>._PoolTimeout = config.PoolTimeout;
            PoolManager<T>._AllAllocateRes = new Resource<T>[PoolManager<T>._PoolSize];
        }

        public Resource<T> Get()
        {
            Resource<T> resource2;
            Resource<T>[] resourceArray = null;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(resourceArray = PoolManager<T>._AllAllocateRes, ref lockTaken);
                Resource<T> resource = null;
                while ((resource = this.GetAnValidRes()) == null)
                {
                    if (PoolManager<T>._PoolTimeout.HasValue)
                    {
                        if (!Monitor.Wait(PoolManager<T>._AllAllocateRes, PoolManager<T>._PoolTimeout.Value))
                        {
                            throw new TimeoutException("在指定的时间内未获取到资源！");
                        }
                    }
                    else
                    {
                        Monitor.Wait(PoolManager<T>._AllAllocateRes, PoolManager<T>._RecheckPoolAfterMs);
                    }
                }
                PoolManager<T>._PoolIndex++;
                Status status = new Status();
                status.IsUsing = true;
                resource.Status = status;
                resource2 = resource;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(resourceArray);
                }
            }
            return resource2;
        }

        private Resource<T> GetAnValidRes()
        {
            int index = PoolManager<T>._PoolIndex % PoolManager<T>._PoolSize;
            for (int i = index; i < PoolManager<T>._PoolSize; i++)
            {
                Resource<T> resource = PoolManager<T>._AllAllocateRes[index];
                if (!(((resource == null) || resource.Status.IsUsing) || resource.Status.HasException))
                {
                    return resource;
                }
                if ((resource == null) || resource.Status.HasException)
                {
                    if (null == this.CreateInstance)
                    {
                        throw new Exception("无法构造资源的新实例！");
                    }
                    resource = new Resource<T>();
                    resource.Current = this.CreateInstance();
                    Status status = new Status();
                    status.IsActive = true;
                    status.HasException = false;
                    status.IsUsing = false;
                    resource.Status = status;
                    PoolManager<T>._AllAllocateRes[index] = resource;
                    return resource;
                }
                if (resource.Status.IsUsing)
                {
                }
            }
            return null;
        }

        // Properties
        public Func<T> CreateInstance
        {
            get;
            set;
        }
    }


}
