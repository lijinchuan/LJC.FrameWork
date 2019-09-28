using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    public class ProcessTraceUtil
    {
        private static ConcurrentDictionary<int, Queue<Tuple<string, long>>> TraceDic = new ConcurrentDictionary<int, Queue<Tuple<string, long>>>();
        private static int _maxTraceId = 1;
        private const string _traceIDKey = "__pt_traceid";

        private static int GetTraceID()
        {
            var obj = CallContext.LogicalGetData(_traceIDKey);
            if (obj == null)
            {
                return 0;
            }

            return (int)obj;
        }

        private static int CreateTraceID()
        {
            var traceid = Interlocked.Increment(ref _maxTraceId);
            if (traceid > int.MaxValue - 10000)
            {
                _maxTraceId = 0;
            }
            CallContext.LogicalSetData(_traceIDKey, traceid);
            //ExecutionContext.RestoreFlow();
            return traceid;
        }

        public static void StartTrace()
        {
            try
            {
                var traceid = CreateTraceID();
                Queue<Tuple<string, long>> queue = null;
                if (!TraceDic.TryGetValue(traceid, out queue))
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
                var traceid = GetTraceID();
                if (traceid == 0)
                {
                    return;
                }
                TraceDic[traceid].Enqueue(new Tuple<string, long>(message, Environment.TickCount & Int32.MaxValue));
            }
            catch { }
        }

        private static long GetMem(string munit)
        {
            long mem = GC.GetTotalMemory(false);
            if (!string.IsNullOrWhiteSpace(munit))
            {
                munit = munit.ToLower();
                switch (munit)
                {
                    case "k":
                    case "kb":
                        {
                            mem /= 1024;
                            break;
                        }
                    case "m":
                    case "mb":
                        {
                            mem /= (1024 * 1024);
                            break;
                        }
                    case "g":
                    case "gb":
                        {
                            mem /= (1024 * 1024 * 1024);
                            break;
                        }
                }
            }
            return mem;
        }

        public static void TraceMem(string message, string munit = "k")
        {
            try
            {
                var traceid = GetTraceID();
                if (traceid == 0)
                {
                    return;
                }
                TraceDic[traceid].Enqueue(new Tuple<string, long>(string.Format("[mem:{1}{2}字节]{0}", message, GetMem(munit), munit), Environment.TickCount & Int32.MaxValue));
            }
            catch
            {

            }
        }

        public static long GetTraceTickes()
        {
            try
            {
                return Environment.TickCount & Int32.MaxValue - TraceDic[GetTraceID()].First().Item2;
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

                var traceid = GetTraceID();
                if (traceid > 0)
                {

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
                    TraceDic.TryRemove(traceid, out queue);
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
