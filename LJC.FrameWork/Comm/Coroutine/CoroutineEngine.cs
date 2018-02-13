using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LJC.FrameWork.Comm.Coroutine
{
    /// <summary>
    /// 协程库
    /// </summary>
    public class CoroutineEngine:IDisposable
    {   //
        private CoroutineWorker[] works = null;
        private long id = 0;
        private int workthreads = 1;
        private Timer checkTimer;
        private int maxTurnsMs = 1500;
        //最大cpu使用率,100-100%,
        private int maxCpu = 100;

        public static CoroutineEngine DefaultCoroutineEngine = null;

        static CoroutineEngine()
        {
            DefaultCoroutineEngine = new CoroutineEngine(4);
            DefaultCoroutineEngine.Run();
        }

        public CoroutineEngine(int workthreads, int maxcpu = 10)
        {
            if (workthreads <= 0 || workthreads > 100)
            {
                workthreads = 16;
            }
            if (maxcpu <= 0)
            {
                maxcpu = 10;
            }
            else if (maxcpu >= 100)
            {
                maxcpu = 100;
            }

            this.workthreads = workthreads;
            this.maxCpu = maxcpu;
            works = new CoroutineWorker[workthreads];

            for (int i = 0; i < workthreads; i++)
            {
                works[i] = new CoroutineWorker(maxCpu);
            }

            checkTimer = new Timer(new TimerCallback((o) => CheckWorker()), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        public int MaxTurnsMs
        {
            get
            {
                return maxTurnsMs;
            }
            set
            {
                if (value > 1)
                {
                    maxTurnsMs = 1;
                }
            }
        }

        private void CheckWorker()
        {
            checkTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            try
            {
                var now = DateTime.Now;
                for (int i = 0; i < works.Length;i++ )
                {
                    var work = works[i];
                    if (work.HasWork() && now.Subtract(work.Turnstart).TotalMilliseconds > MaxTurnsMs)
                    {
                        DateTime dt = DateTime.Now;
                        
                        work.Interrupt(work.GetCurrentUnitBag());
                    }
                }
            }
            finally
            {
                checkTimer.Change(10, System.Threading.Timeout.Infinite);
            }
        }

        public void Run()
        {
            for (int i = 0; i < workthreads; i++)
            {
                works[i].Run();
            }
            checkTimer.Change(10, Timeout.Infinite);
        }

        public void Dispatcher(ICoroutineUnit unit)
        {
            if (unit != null)
            {
                var newid = System.Threading.Interlocked.Increment(ref id);
                CoroutineUnitBag bag = new CoroutineUnitBag(newid, unit);
                var work=works[newid % workthreads];
                work.Add(bag);
            }
        }

        public bool HasWork()
        {
            return works.Any(p => p.HasWork());
        }

        public void Close()
        {
            checkTimer.Change(Timeout.Infinite, Timeout.Infinite);
            foreach (var work in works)
            {
                work.Stop();
            }
        }

        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                checkTimer.Dispose();
            }

            disposed = true;
        }
    }
}
