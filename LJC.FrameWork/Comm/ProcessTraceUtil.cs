using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    public class ProcessTraceUtil
    {
        private static ConcurrentDictionary<int, Queue<Tuple<string, long>>> TraceDic = new ConcurrentDictionary<int, Queue<Tuple<string, long>>>();

        public static void StartTrace()
        {
            try
            {
                var traceid = Thread.CurrentThread.ManagedThreadId;
                Queue<Tuple<string, long>> queue = null;
                if (!TraceDic.ContainsKey(traceid))
                {
                    queue = new Queue<Tuple<string, long>>();
                    TraceDic.TryAdd(traceid, queue);
                }
                else
                {
                    queue.Clear();
                }
            }
            catch { }
        }

        public static void Trace(string message)
        {
            try
            {
                TraceDic[Thread.CurrentThread.ManagedThreadId].Enqueue(new Tuple<string, long>(message, Environment.TickCount));
            }
            catch { }
        }

        public static string PrintTrace()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                var traceid = Thread.CurrentThread.ManagedThreadId;

                var queue = TraceDic[traceid];

                long timeline = 0;
                while (queue.Count > 0)
                {
                    var tp = queue.Dequeue();
                    if (timeline == 0)
                    {
                        timeline = tp.Item2;
                    }
                    sb.AppendLine(string.Format("{0}:  {1}ms", tp.Item1, tp.Item2 - timeline));
                }

                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
