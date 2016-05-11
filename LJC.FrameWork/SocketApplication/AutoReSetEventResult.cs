using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketApplication
{
    internal class AutoReSetEventResult : EventWaitHandle
    {
        public AutoReSetEventResult(string reqID)
            : base(false, EventResetMode.ManualReset)
        {
            ReqID = reqID;
        }

        public string ReqID
        {
            get;
            private set;
        }

        /// <summary>
        /// 等待返回的结果
        /// </summary>
        public object WaitResult
        {
            get;
            set;
        }

        private bool _isTimeOut = true;
        public bool IsTimeOut
        {
            get
            {
                return _isTimeOut;
            }
            internal set
            {
                _isTimeOut = value;
            }
        }

        //new public void Set()
        //{
        //    base.Set();
        //    this.Close();
        //}
    }
}
