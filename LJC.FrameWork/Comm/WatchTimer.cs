using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace LJC.FrameWork.Comm
{
    public class WatchTimer
    {
        private System.Timers.Timer _timer;
        /// <summary>
        /// 毫秒
        /// </summary>
        private int _timeOut = 1000;
        private bool _isLive = false;
        private long _timeFlow = 0L;
        private bool _isFinished = true;

        private event Action _onTimeOut;

        public WatchTimer(int timeOut)
        {
            _timeOut = timeOut;
            _timer = new System.Timers.Timer(timeOut / 3);
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
        }

        public void SetTimeOutCallBack(Action action)
        {
            if (_onTimeOut != null)
            {
                _timeFlow = 0L;
            }
            else
            {
                _onTimeOut += action;
            }
            if (_isLive)
                return;

            Start();
        }

        public void ReSetTimeOutCallBack(Action action)
        {
            if (_onTimeOut != null)
            {
                _onTimeOut -= _onTimeOut;
            }
            _onTimeOut += action;
            Start();
        }


        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_timeFlow >= _timeOut)
            {
                if (_onTimeOut != null && _isLive)
                {
                    try
                    {
                        Reset();
                        _isFinished = false;
                        _onTimeOut();
                    }
                    catch
                    {

                    }
                    finally
                    {
                        _isFinished = true;
                    }
                }
            }
            else
            {
                _timeFlow += _timeOut / 3;
            }
        }

        public void Start()
        {
            if (!_isFinished)
                return;
            if (_isLive)
                return;
            _timer.Start();
            _isLive = true;
        }

        public void Stop()
        {
            Reset();
        }

        public void Restart()
        {
            Reset();
            _timer.Start();
        }

        public void ClearTimeOut()
        {
            _timeFlow = 0;
        }

        private void Reset()
        {
            _timer.Stop();
            _isLive = false;
            _timeFlow = 0L;
        }
    }
}
