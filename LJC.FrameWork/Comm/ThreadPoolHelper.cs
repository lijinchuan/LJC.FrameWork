using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    public static class ThreadPoolHelper
    {
        private const int MinWorkThreads = 100;
        private const int MinCompletionPortThreads = 100;
        static ThreadPoolHelper()
        {
            CheckSetMinThreads(MinWorkThreads, MinCompletionPortThreads);
        }

        public static string PrintDetail()
        {
            //ThreadPool.GetMaxThreads(out int work, out int completionPortNum);
            ThreadPool.GetMinThreads(out int minWork, out int minCompletionPortNum);
            ThreadPool.GetMaxThreads(out int maxWork, out int maxCompletionPortNum);
            ThreadPool.GetAvailableThreads(out int aWork, out int aCompletionPortNum);
            return "MaxThreads(" + (minWork - maxWork + aWork) + "," + (minCompletionPortNum - maxCompletionPortNum + aCompletionPortNum) + ")";
        }

        /// <summary>
        /// 检查最小线程数，如果小于传入的值则设置
        /// </summary>
        /// <param name="workerThreads"></param>
        /// <param name="completionPortThreads"></param>
        /// <returns></returns>
        public static bool CheckSetMinThreads(int workerThreads, int completionPortThreads)
        {
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
            workerThreads = Math.Max(workerThreads, minWorkerThreads);
            completionPortThreads = Math.Max(completionPortThreads, minCompletionPortThreads);
            if (minWorkerThreads != workerThreads || minCompletionPortThreads != completionPortThreads)
            {
                LogManager.LogHelper.Instance.Info($"ThreadPool.SetMinThreads({workerThreads},{completionPortThreads})");
                return ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
            }
            return false;
        }

        //
        // 摘要:
        //     将方法排入队列以便执行。 此方法在有线程池线程变得可用时执行。
        //
        // 参数:
        //   callBack:
        //     一个 System.Threading.WaitCallback，表示要执行的方法。
        //
        // 返回结果:
        //     如果此方法成功排队，则为 true；如果无法将该工作项排队，则引发 System.NotSupportedException。
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     callBack 为 null。
        //
        //   T:System.NotSupportedException:
        //     承载公共语言运行时 (CLR) 的宿主不支持此操作。
        public static bool QueueUserWorkItem(WaitCallback callBack)
        {
            return ThreadPool.QueueUserWorkItem(callBack);
        }
        //
        // 摘要:
        //     将方法排入队列以便执行，并指定包含该方法所用数据的对象。 此方法在有线程池线程变得可用时执行。
        //
        // 参数:
        //   callBack:
        //     System.Threading.WaitCallback，它表示要执行的方法。
        //
        //   state:
        //     包含方法所用数据的对象。
        //
        // 返回结果:
        //     如果此方法成功排队，则为 true；如果无法将该工作项排队，则引发 System.NotSupportedException。
        //
        // 异常:
        //   T:System.NotSupportedException:
        //     承载公共语言运行时 (CLR) 的宿主不支持此操作。
        //
        //   T:System.ArgumentNullException:
        //     callBack 为 null。
        public static bool QueueUserWorkItem(WaitCallback callBack, object state)
        {
            return ThreadPool.QueueUserWorkItem(callBack, state);
        }
    }
}
