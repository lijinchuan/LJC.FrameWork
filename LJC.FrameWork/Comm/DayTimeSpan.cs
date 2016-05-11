using LJC.FrameWork.Data.QuickDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    /// <summary>
    /// 表示一天内两个时间点之间的时间段
    /// </summary>
    public class DayTimeSpan
    {
        private Pair<DayTimePoint, DayTimePoint> _tpPair;

        /// <summary>
        /// 开始时间点
        /// </summary>
        public DayTimePoint TimeBegin
        {
            get
            {
                return _tpPair.First;
            }
        }

        /// <summary>
        /// 结束时间点
        /// </summary>
        public DayTimePoint TimeEnd
        {
            get
            {
                return _tpPair.Second;
            }
        }

        public DayTimeSpan(DayTimePoint s, DayTimePoint e)
        {
            if (s > e)
            {
                throw new Exception("TimeSpan构造错误，TimeSpan只支持同一天的一个时间段，并且开始时间点不能大于结束时间点。");
            }

            _tpPair = new Pair<DayTimePoint, DayTimePoint>(s, e);
        }

        /// <summary>
        /// 时间是否落在时间段内
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public bool IsTimeInSpan(DateTime dt)
        {
            DayTimePoint tempTP = new DayTimePoint(dt.Hour, dt.Minute);

            return tempTP >= TimeBegin && tempTP <= TimeEnd;
        }
    }
}
