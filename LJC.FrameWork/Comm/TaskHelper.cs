using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LJC.FrameWork.Comm
{
    public class TaskHelper
    {
        public static void RunTask<Tinput>(List<Tinput> data, int openTaskNum, Action<object> fun)
        {
            if (data == null || data.Count == 0)
                return;

            if (openTaskNum < 1)
                openTaskNum = 1;

            if (openTaskNum > data.Count)
            {
                openTaskNum = data.Count;
            }

            var percount = (int)Math.Ceiling(data.Count * 1.0 / openTaskNum);

            Task[] taskList = new Task[openTaskNum];

            for (int i = 0; i < openTaskNum; i++)
            {
                var subdata = data.Skip(i * percount).Take(percount);
                taskList[i] = Task.Factory.StartNew(fun, subdata.ToList());
            }

            if (taskList.Length > 0)
            {
                Task.WaitAll(taskList, -1);
            }
        }

        public static void RunTask<Tinput, ToutPut>(List<Tinput> data, int openTaskNum, Func<object, ToutPut> fun)
        {
            if (data == null || data.Count == 0)
                return;

            if (openTaskNum < 1)
                openTaskNum = 1;

            if (openTaskNum > data.Count)
            {
                openTaskNum = data.Count;
            }

            var percount = (int)Math.Ceiling(data.Count * 1.0 / openTaskNum);

            Task[] taskList = new Task[openTaskNum];

            for (int i = 0; i < openTaskNum; i++)
            {
                var subdata = data.Skip(i * percount).Take(percount);
                taskList[i] = Task.Factory.StartNew<ToutPut>(fun, subdata.ToList());
            }

            if (taskList.Length > 0)
            {
                Task.WaitAll(taskList);
            }
        }

        /// <summary>
        /// 定时执行
        /// </summary>
        /// <param name="millSecond">间隔时间</param>
        /// <param name="act">执行方法，返回值为是否，如果返回了true，则会停止timer，否则会一直运行。</param>
        /// <param name="runintime">是否马上运行一次，默认true</param>
        /// <returns></returns>
        public static Timer SetInterval(int millSecond, Func<bool> act, int errtrytimes = 1, bool runintime = true)
        {
            SingleTimer timer = runintime ? new SingleTimer() : new SingleTimer(millSecond);
            timer.ErrorTryTimes = errtrytimes;

            timer.Elapsed += (o, e) =>
            {
                lock (timer)
                {
                    if (timer.IsRuning)
                        return;
                    timer.IsRuning = true;
                    timer.Stop();
                }
                DateTime starttime = DateTime.Now;
                timer.LastStartTime = starttime;

                bool actresult = false;
                try
                {
                    actresult = act();
                    timer.Success();
                }
                catch (Exception ex)
                {
                    timer.Error(ex);
                    if (timer.ErrorTimes < timer.ErrorTryTimes)
                    {
                        timer.Restart(timer.ErrorInterval);
                        return;
                    }
                    else
                    {
                        LogManager.LogHelper.Instance.Error("timer中断", ex);
                        timer.LastError = ex;
                        timer.Kill();
                        return;
                    }
                }

                if (actresult)
                {
                    timer.Kill();
                }
                else
                {
                    timer.Restart(millSecond - DateTime.Now.Subtract(starttime).TotalMilliseconds);
                }


                timer.LastFinishTime = DateTime.Now;
            };

            timer.Start();

            return timer;
        }

        /// <summary>
        /// 每天定时执行一次任务
        /// </summary>
        /// <param name="startTimePoint">运行时间点</param>
        /// <param name="act">程序代码</param>
        /// <param name="errtry">错误重试次数</param>
        /// <param name="runintime">是否立刻运行一次，如果不是，则下一次运行，默认false</param>
        /// <returns></returns>
        public static Timer SetInterval(DayTimePoint startTimePoint, Func<bool> act, int errtrytimes = 1, bool runintime = false)
        {
            SingleTimer timer = new SingleTimer(1);
            timer.ErrorTryTimes = errtrytimes;

            timer.Elapsed += (o, e) =>
            {
                lock (timer)
                {
                    if (timer.IsRuning)
                        return;

                    if (timer.LastStartTime == DateTime.MinValue && DateTime.Now < startTimePoint)
                        return;

                    if (!runintime && timer.LastStartTime == DateTime.MinValue && DateTime.Now.Subtract(startTimePoint.ToDateTime()).TotalHours > 1)
                    {
                        return;
                    }
                    timer.IsRuning = true;
                    timer.Stop();

                }
                DateTime starttime = DateTime.Now;
                timer.LastStartTime = starttime;

                bool actresult = false;
                try
                {
                    actresult = act();
                    timer.Success();
                }
                catch (Exception ex)
                {
                    timer.Error(ex);
                    if (timer.ErrorTimes < timer.ErrorTryTimes)
                    {
                        timer.Restart(timer.ErrorInterval);
                        return;
                    }
                    else
                    {
                        timer.LastError = ex;
                        timer.Kill();
                        return;
                    }
                }

                if (actresult)
                {
                    timer.Kill();
                }
                else
                {
                    timer.Restart(startTimePoint.GetNextTimePoint(starttime).Subtract(DateTime.Now).TotalMilliseconds);
                }

                timer.LastFinishTime = DateTime.Now;
            };

            timer.Start();

            return timer;
        }
    }
}
