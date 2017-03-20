using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Web
{
    public class PageTraceUtil
    {
        private static ConcurrentDictionary<string, Queue<Tuple<string, long>>> TraceDic = new ConcurrentDictionary<string, Queue<Tuple<string, long>>>();

        public static void StartTrace()
        {
            try
            {
                var traceid = System.Web.HttpContext.Current.Session.SessionID;
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

                Trace("start");
            }
            catch { }
        }

        public static void Trace(string message)
        {
            try
            {
                TraceDic[System.Web.HttpContext.Current.Session.SessionID].Enqueue(new Tuple<string, long>(message, Environment.TickCount));
            }
            catch { }
        }

        public static long GetTraceTickes()
        {
            try
            {
                return Environment.TickCount - TraceDic[System.Web.HttpContext.Current.Session.SessionID].First().Item2;
            }
            catch
            {
                return 0;
            }
        }

        public static string PrintTrace()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                var traceid = System.Web.HttpContext.Current.Session.SessionID;

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
