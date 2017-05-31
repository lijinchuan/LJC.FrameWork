using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.ResourcePool
{
    public class ResourceHelper<T> where T : class
    {
        // Fields
        private static ResourceHelper<T> _Helper;
        private static PoolManager<T> PoolManager;

        // Methods
        static ResourceHelper()
        {
            ResourceHelper<T>._Helper = null;
            ResourceHelper<T>.PoolManager = null;
        }

        public Resource<T> GetResource(Func<T> resMaker)
        {
            ResourceHelper<T>.PoolManager.CreateInstance = resMaker;
            return ResourceHelper<T>.PoolManager.Get();
        }

        // Properties
        public static ResourceHelper<T> Instance
        {
            get
            {
                if ((ResourceHelper<T>._Helper == null) || (null == ResourceHelper<T>.PoolManager))
                {
                    int num2;
                    int result = 10;
                    if (ConfigurationManager.AppSettings.AllKeys.Contains<string>("MaxPoolSize"))
                    {
                        int.TryParse(ConfigurationManager.AppSettings["MaxPoolSize"], out result);
                    }
                    int? nullable = null;
                    if (ConfigurationManager.AppSettings.AllKeys.Contains<string>("PoolTimeout") && int.TryParse(ConfigurationManager.AppSettings["PoolTimeout"], out num2))
                    {
                        nullable = new int?(num2);
                    }
                    ResourceHelper<T>._Helper = new ResourceHelper<T>();
                    PoolConfig config2 = new PoolConfig();
                    config2.MaxPoolSize = new int?(result);
                    config2.PoolTimeout = nullable;
                    PoolConfig config = config2;
                    ResourceHelper<T>.PoolManager = new PoolManager<T>(config);
                }
                return ResourceHelper<T>._Helper;
            }
        }
    }
}
