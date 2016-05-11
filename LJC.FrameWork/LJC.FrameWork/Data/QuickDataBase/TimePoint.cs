using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.QuickDataBase
{
    [Serializable]
    /// <summary>
    /// 时间点
    /// </summary>
    public class TimePoint
    {
        /// <summary>
        /// 时
        /// </summary>
        public int hour
        {
            get;
            set;
        }

        /// <summary>
        /// 分
        /// </summary>
        public int min
        {
            get;
            set;
        }

        public TimePoint(int h, int m)
        {
            hour = h;
            min = m;
        }

        public static bool operator <(TimePoint ts, DateTime t)
        {
            return ts.hour < t.Hour ||
                (ts.hour == t.Hour && ts.min < t.Minute)
                || (ts.hour == t.Hour && ts.min == t.Minute && t.Second > 0);
        }

        public static bool operator ==(TimePoint ts, DateTime t)
        {
            return ts.hour == t.Hour && ts.min == t.Minute && t.Second == 0;
        }

        public static bool operator <=(TimePoint ts, DateTime t)
        {
            return ts < t || ts == t;
        }

        public static bool operator !=(TimePoint ts, DateTime t)
        {
            return !(ts == t);
        }

        public static bool operator >(TimePoint ts, DateTime t)
        {
            return ts.hour > t.Hour
                || (ts.hour == t.Hour && ts.min > t.Minute);
        }

        public static bool operator >=(TimePoint ts, DateTime t)
        {
            return ts > t || ts == t;
        }

        public static bool operator >(TimePoint ts1, TimePoint ts2)
        {
            return ts1.hour > ts2.hour ||
                (ts1.hour == ts2.hour && ts1.min > ts2.min);
        }

        public static bool operator >=(TimePoint ts1, TimePoint ts2)
        {
            return ts1 == ts2 || ts1 > ts2;
        }

        public static bool operator <(TimePoint ts1, TimePoint ts2)
        {
            return ts1 != ts2 && !(ts1 > ts2);
        }

        public static bool operator <=(TimePoint ts1, TimePoint ts2)
        {
            return ts1 == ts2 || ts1 < ts2;
        }

        public static bool operator ==(TimePoint ts1, TimePoint ts2)
        {
            return (ts1.hour == ts2.hour && ts1.min == ts2.min);
        }

        public static bool operator !=(TimePoint ts1, TimePoint ts2)
        {
            return !(ts1 == ts2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TimePoint))
                return false;

            if (this == (TimePoint)obj)
                return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (this.hour + "" + this.min).GetHashCode();
        }
    }
}
