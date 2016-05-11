using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class SingleTimer : System.Timers.Timer
    {
        /// <summary>
        /// 错误之后再次重试时间
        /// </summary>
        public int ErrorInterval = 5000;

        public SingleTimer()
            : base()
        {

        }


        public SingleTimer(int millsec)
            : base(millsec)
        {

        }

        private DateTime _lastStartTime = DateTime.MinValue;
        public DateTime LastStartTime
        {
            get
            {
                return _lastStartTime;
            }
            set
            {
                _lastStartTime = value;
            }
        }

        private DateTime _lastFinishTime = DateTime.MinValue;


        public DateTime LastFinishTime
        {
            get
            {
                return _lastFinishTime;
            }
            internal set
            {
                _lastFinishTime = value;
            }
        }

        private DateTime _lastSuccessTime = DateTime.MinValue;
        /// <summary>
        /// 上次成功时间
        /// </summary>
        public DateTime LastSuccessTime
        {
            get
            {
                return _lastSuccessTime;
            }
            set
            {
                _lastSuccessTime = value;
            }
        }

        private int _errorTimes = 0;
        /// <summary>
        /// 错误次数
        /// </summary>
        public int ErrorTimes
        {
            get
            {
                return _errorTimes;
            }
            set
            {
                _errorTimes = value;
            }
        }

        private int _errorTryTimes = 1;
        public int ErrorTryTimes
        {
            get
            {
                return _errorTryTimes;
            }
            set
            {
                _errorTryTimes = value;
            }
        }

        public bool IsRuning
        {
            get;
            internal set;
        }

        public Exception LastError
        {
            get;
            internal set;
        }

        internal void Success()
        {
            this.LastError = null;
            this.ErrorTimes = 0;
            this.LastFinishTime = DateTime.Now;
            this.LastSuccessTime = DateTime.Now;
        }

        internal void Error(Exception ex)
        {
            this.LastError = ex;
            this.LastFinishTime = DateTime.Now;
            this.ErrorTimes++;
        }

        internal void Restart(double intelval)
        {
            this.Interval = Math.Max(1, intelval);
            this.IsRuning = false;
            this.Start();
        }

        internal void Kill()
        {
            Stop();
            Close();
            Dispose();
        }
    }
}
