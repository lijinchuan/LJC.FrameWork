using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.QuickDataBase
{
    /// <summary>
    /// 时间段
    /// </summary>
    public class TimeSpan
    {
        private Pair<TimePoint, TimePoint> _tpPair;

        /// <summary>
        /// 开始时间点
        /// </summary>
        public TimePoint TimeBegin
        {
            get
            {
                return _tpPair.First;
            }
        }

        /// <summary>
        /// 结束时间点
        /// </summary>
        public TimePoint TimeEnd
        {
            get
            {
                return _tpPair.Second;
            }
        }

        public TimeSpan(TimePoint s, TimePoint e)
        {
            if (s > e)
            {
                throw new Exception("TimeSpan构造错误，TimeSpan只支持同一天的一个时间段，并且开始时间点不能大于结束时间点。");
            }

            _tpPair = new Pair<TimePoint, TimePoint>(s, e);
        }

        /// <summary>
        /// 时间是否落在时间段内
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public bool IsTimeInSpan(DateTime dt)
        {
            TimePoint tempTP = new TimePoint(dt.Hour, dt.Minute);

            return tempTP >= TimeBegin && tempTP <= TimeEnd;
        }
    }
}
