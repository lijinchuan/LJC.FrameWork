using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LJC.FrameWork.Comm.Coroutine
{
    internal class CoroutineWorker : IDisposable
    {
        private List<CoroutineUnitBag> unitstemp = null;
        private List<CoroutineUnitBag> units = null;
        private Thread thread = null;
        private volatile bool running = false;
        private DateTime turnstart = DateTime.MaxValue;
        private CoroutineUnitBag currentUnit = null;
        private int sleepms = 1;
        private const int maxsleepms = 100;
        private int maxCpu = 10;

        public DateTime Turnstart
        {
            get
            {
                return turnstart;
            }
            private set
            {
                turnstart = value;
            }
        }

        public CoroutineUnitBag GetCurrentUnitBag()
        {
            return currentUnit;
        }

        public CoroutineWorker(int maxcpu)
        {
            this.maxCpu = maxcpu;
            unitstemp = new List<CoroutineUnitBag>();
            units = new List<CoroutineUnitBag>();

            MakeThread();
        }

        private void MakeThread()
        {
            lock (this)
            {
                ThreadStart ts = new ThreadStart(() =>
                    {
                        try
                        {
                            Loop();
                        }
                        catch (ThreadAbortException ex)
                        {
                            new Action(() => MakeThread()).BeginInvoke(null, null);
                        }
                    }
                    );

                if (thread == null)
                {
                    thread = new Thread(ts);
                }
                else
                {
                    int trycount = 0;
                    int maxtrycount = 10;
                    while (trycount++ < maxtrycount)
                    {
                        if (thread.ThreadState == ThreadState.Aborted || thread.ThreadState == ThreadState.Stopped)
                        {
                            thread = new Thread(ts);
                            break;
                        }
                        else
                        {
                            Console.WriteLine(thread.ThreadState);
                            Thread.Sleep(100 * trycount);
                        }
                    }

                    if (trycount > maxtrycount)
                    {
                        throw new TimeoutException("MakeThread");
                    }
                }

                if (running)
                {
                    thread.Start();
                }
            }
        }

        public void Add(CoroutineUnitBag bag)
        {
            if (bag != null)
            {
                lock (unitstemp)
                {
                    unitstemp.Add(bag);
                }

                if (!running)
                {
                    lock (this)
                    {
                        if (!running)
                        {
                            running = true;
                            MakeThread();
                        }
                    }
                }
            }
        }

        public void Run()
        {
            if (running)
            {
                return;
            }

            running = true;
            thread.Start();
        }

        public bool HasWork()
        {
            return running && (units.Count > 0 || unitstemp.Count > 0);
        }

        private void Loop()
        {
            bool isfirst = true;
            bool isdone = true;
            List<CoroutineUnitBag> leftlist = new List<CoroutineUnitBag>();

            while (running)
            {
                DateTime timestart = DateTime.Now;
                lock (this)
                {
                    turnstart = DateTime.MaxValue;
                    currentUnit = null;
                    if (!isfirst)
                    {
                        units = leftlist;
                        leftlist = new List<CoroutineUnitBag>();
                    }
                    else
                    {
                        isfirst = false;
                    }

                    lock (unitstemp)
                    {
                        units.AddRange(unitstemp);
                        unitstemp.Clear();
                    }
                }

                if (units.Count == 0)
                {
                    if (sleepms < maxsleepms)
                    {
                        Thread.Sleep(sleepms++);
                        continue;
                    }
                    else
                    {
                        sleepms = 1;
                        running = false;
                        break;
                    }
                }
                else
                {
                    sleepms = 1;
                }

                foreach (var bag in units)
                {
                    isdone = true;

                    if (!bag.HasError)
                    {
                        turnstart = DateTime.Now;
                        currentUnit = bag;
                        bag.Exceute();
                        {
                            try
                            {
                                if (!bag.IsDone() && !bag.IsSuccess())
                                {
                                    if (bag.IsTimeOut())
                                    {
                                        throw new TimeoutException();
                                    }
                                    isdone = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                bag.HasError = true;
                                bag.Error = ex;
                                isdone = true;
                            }
                        }
                    }
                    turnstart = DateTime.MaxValue;
                    currentUnit = null;

                    if (isdone)
                    {
                        try
                        {
                            bag.CallBack(new CoroutineCallBackEventArgs
                            {
                                CoroutineUnit = bag.CUnit,
                                Error = bag.Error,
                                HasError = bag.HasError
                            });
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        leftlist.Add(bag);
                    }
                }

                if (leftlist.Count > 0)
                {
                    var ms = DateTime.Now.Subtract(timestart).TotalMilliseconds;
                    Thread.Sleep((int)(100.0 / Math.Max(ms, 1) / maxCpu));
                }
            }
        }

        public void Interrupt(CoroutineUnitBag forUnit)
        {
            var time=this.turnstart;
            if (forUnit != null && forUnit == currentUnit && !forUnit.HasError)
            {
                forUnit.HasError = true;
                forUnit.Error = new TimeoutException("检查到长时间运行任务需要中断执行,线程号:" + this.thread.ManagedThreadId + ",任务id:" + forUnit.Id + "," + forUnit.CUnit.ToString() + ",执行时间:" + (DateTime.Now.Subtract(time).TotalMilliseconds+",还有任务数:"+this.units.Count));

                lock (this)
                {
                    this.thread.Abort();
                }
            }
        }

        public void Stop()
        {
            running = false;
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
                running = false;
            }

            disposed = true;
        }
    }
}
