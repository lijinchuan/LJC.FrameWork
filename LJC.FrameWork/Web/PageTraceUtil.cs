using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LJC.FrameWork.Web
{
    public static class PageTraceUtil
    {
        private static ConcurrentDictionary<string, Queue<Tuple<string, long>>> TraceDic = new ConcurrentDictionary<string, Queue<Tuple<string, long>>>();
        private const string TraceIDName = "_traceid";
        private const string SessionIDName = "_sessionid";

        public static void StartTrace(this HttpContext httpcontext)
        {
            try
            {
                string traceid = Guid.NewGuid().ToString();
                httpcontext.Items.Add(TraceIDName, traceid);
                Queue<Tuple<string, long>> queue = null;
                if (!TraceDic.TryGetValue(traceid,out queue))
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
                var context = System.Web.HttpContext.Current;
                var dic=TraceDic[context.Items[TraceIDName].ToString()];
                dic.Enqueue(new Tuple<string, long>(message, Environment.TickCount));

                if (!context.Items.Contains(SessionIDName) && context.Session != null)
                {
                    context.Items.Add(SessionIDName, context.Session.SessionID);
                }
            }
            catch { }
        }

        public static long GetTraceTickes()
        {
            try
            {
                return Environment.TickCount - TraceDic[System.Web.HttpContext.Current.Items[TraceIDName].ToString()].First().Item2;
            }
            catch
            {
                return 0;
            }
        }

        public static string PrintTrace(this HttpContext httpcontext)
        {
            string traceid = null;
            try
            {
                StringBuilder sb = new StringBuilder(string.Format("sessionid:{0},clientip:{1} ", httpcontext.Items.Contains(SessionIDName) ? httpcontext.Items[SessionIDName].ToString() : string.Empty, HttpUtil.GetRemoteIp(httpcontext)));

                traceid = httpcontext.Items[TraceIDName].ToString();

                var queue = TraceDic[traceid];

                long timeline = 0;
                while (queue.Count > 0)
                {
                    var tp = queue.Dequeue();
                    if (timeline == 0)
                    {
                        timeline = tp.Item2;
                    }
                    sb.AppendLine(string.Format("{0}ms:  {1}", tp.Item2 - timeline, tp.Item1));
                }

                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                if(traceid!=null)
                {
                    Queue<Tuple<string, long>> oldqueue = null;
                    TraceDic.TryRemove(traceid, out oldqueue);
                }
            }
        }
    }
}
