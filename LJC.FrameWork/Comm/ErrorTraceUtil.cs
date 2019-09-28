using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    public static class ErrorTraceUtil
    {
        private static ConcurrentDictionary<int, Stack> ErrorTraceDic = new ConcurrentDictionary<int, Stack>();
        private static int _maxTraceId = 1;
        private const string _traceIDKey = "__et_traceid";

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
                Stack stack = null;
                if (!ErrorTraceDic.TryGetValue(traceid,out stack))
                {
                    stack = new Stack();
                    stack.Push("(");
                    ErrorTraceDic.TryAdd(traceid, stack);
                }
                else
                {
                    stack.Push("(");
                }
            }
            catch { }
        }

        public static void ClearError()
        {
            try
            {
                Stack stack;
                var traceid = GetTraceID();
                ErrorTraceDic.TryRemove(traceid, out stack);
            }
            catch { }
        }

        public static void EndTrace()
        {
            try
            {
                var traceid = GetTraceID();
                Stack stack = null;
                if (ErrorTraceDic.TryGetValue(traceid, out stack))
                {
                    while (stack.Count > 0 && stack.Pop().ToString() != "(")
                    {
                    }
                }
            }
            catch { }
        }

        public static void SetErrorCode(string errorCode)
        {
            try
            {
                var traceid = GetTraceID();
                Stack stack = null;
                if (ErrorTraceDic.TryGetValue(traceid, out stack))
                {
                    stack.Push(errorCode);
                }
            }
            catch { }
        }

        /// <summary>
        /// 得到最后的错误代码，如果为空，则无错误
        /// </summary>
        /// <returns></returns>
        public static string GetLastErrorCode()
        {
            try
            {
                var traceid = GetTraceID();
                Stack stack = null;
                if (ErrorTraceDic.TryGetValue(traceid, out stack))
                {
                    if (stack.Count > 0)
                    {
                        var code = stack.Peek().ToString();
                        if (code.Equals("("))
                            return string.Empty;

                        return code;
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
