using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    public class AutoReSetEventResult<T> : IDisposable
    {
        private EventWaitHandle _waitObj = new EventWaitHandle(false, EventResetMode.ManualReset);
        public EventWaitHandle WaitObj
        {
            get
            {
                return _waitObj;
            }
        }

        private Lazy<EventWaitHandle> _waitObj2;
        public EventWaitHandle WaitObj2
        {
            get
            {
                return _waitObj2.Value;
            }
        }

        public AutoReSetEventResult()
        {
            _waitObj2 = new Lazy<EventWaitHandle>(() =>
                {
                    return new EventWaitHandle(false, EventResetMode.ManualReset);
                });
        }

        /// <summary>
        /// 等待返回的结果
        /// </summary>
        public T WaitResult
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
            set
            {
                _isTimeOut = value;
            }
        }

        public void Set()
        {
            this._waitObj.Set();
        }


        public void Dispose()
        {
            this._waitObj.Close();
        }
    }
}
