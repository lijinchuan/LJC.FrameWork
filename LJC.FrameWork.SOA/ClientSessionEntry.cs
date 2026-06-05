using System;
using System.Collections.Generic;
using LJC.FrameWork.SocketApplication;
using System.Threading;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.SOA
{
    /// <summary>
    /// 封装 ClientSessionList 的条目，替代原来的 object[] 结构
    /// Callback 可以是：
    /// - Session （同步转发回客户端）
    /// - Action<byte[],bool,int,string,Dictionary<string,string>> （流式回调）
    /// - Action<WebResponse> （回调）
    /// - AutoResetEvent （等待同线程返回）
    /// - AutoReSetEventResult<byte[]> （等待并拿结果）
    /// </summary>
    public class ClientSessionEntry
    {
        public string TransactionId { get; set; }
        // Strongly-typed callbacks instead of object
        public Session SessionCallback { get; set; }
        public Action<byte[], bool, int, string, Dictionary<string, string>> StreamCallbackBytes { get; set; }
        public AutoResetEvent WaitEvent { get; set; }
        public AutoReSetEventResult<byte[]> WaitResult { get; set; }
        public ESBServiceInfo ServiceInfo { get; set; }
        public int FuncId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public Contract.WebResponse LastWebResponse { get; set; }

        /// <summary>
        /// 接力时间
        /// </summary>
        public DateTime ContinueTime
        {
            get;
            set;
        } = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public int LastTrunkNo
        {
            get;
            set;
        }

        /// <summary>
        /// 客户端确认的TrunkNo
        /// </summary>
        public int AckTrunkNo
        {
            get;
            set;
        } = -1;

        /// <summary>
        /// 尝试释放/唤醒等待对象（如果存在），并标记超时（如果是 AutoReSetEventResult）
        /// </summary>
        public void ReleaseWaiterAsTimeout()
        {
            try
            {
                if (WaitResult != null)
                {
                    try { WaitResult.IsTimeOut = true; } catch { }
                    try { WaitResult.Dispose(); } catch { }
                }
                else if (WaitEvent != null)
                {
                    try { WaitEvent.Set(); } catch { }
                    try { WaitEvent.Dispose(); } catch { }
                }
            }
            catch
            {
                // swallow
            }
        }
    }
}
