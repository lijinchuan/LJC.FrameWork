using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    public static class ErrorTraceUtil
    {
        private static ConcurrentDictionary<int, Stack> ErrorTraceDic = new ConcurrentDictionary<int, Stack>();

        public static void StartTrace()
        {
            try
            {
                var traceid = Thread.CurrentThread.ManagedThreadId;
                Stack stack = null;
                if (!ErrorTraceDic.TryGetValue(traceid,out stack))
                {
                    stack = new Stack();
                    stack.Push("(");
                    ErrorTraceDic.TryAdd(traceid, stack);
                }
                else
                {
                    stack.Clear();
                    stack.Push("(");
                }
            }
            catch { }
        }

        public static void EndTrace()
        {
            try
            {
                var traceid = Thread.CurrentThread.ManagedThreadId;
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
                var traceid = Thread.CurrentThread.ManagedThreadId;
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
                var traceid = Thread.CurrentThread.ManagedThreadId;
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
