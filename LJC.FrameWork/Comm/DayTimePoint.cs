using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    [Serializable]
    /// <summary>
    /// 一天内的某个时间点
    /// </summary>
    public class DayTimePoint
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

        public DayTimePoint(int h, int m)
        {
            if (h < 0 || h > 23)
                throw new ArgumentException("h");

            if (m < 0 || m > 59)
                throw new ArgumentException("m");

            hour = h;
            min = m;
        }

        public DateTime GetNextTimePoint(DateTime? now = null)
        {
            if (now == null)
                return DateTime.Now.Date.AddDays(1).AddHours(this.hour).AddMinutes(this.min);

            return now.Value.Date.AddDays(1).AddHours(this.hour).AddMinutes(this.min);
        }

        public static bool operator <(DayTimePoint ts, DateTime t)
        {
            return ts.hour < t.Hour ||
                (ts.hour == t.Hour && ts.min < t.Minute)
                || (ts.hour == t.Hour && ts.min == t.Minute && t.Second > 0);
        }

        public static bool operator <(DateTime t, DayTimePoint ts)
        {
            return ts > t;
        }

        public static bool operator ==(DayTimePoint ts, DateTime t)
        {
            return ts.hour == t.Hour && ts.min == t.Minute && t.Second == 0;
        }

        public static bool operator ==(DateTime t, DayTimePoint ts)
        {
            return ts == t;
        }

        public static bool operator <=(DayTimePoint ts, DateTime t)
        {
            return ts < t || ts == t;
        }

        public static bool operator <=(DateTime t, DayTimePoint ts)
        {
            return ts >= t;
        }

        public static bool operator !=(DayTimePoint ts, DateTime t)
        {
            return !(ts == t);
        }

        public static bool operator !=(DateTime t, DayTimePoint ts)
        {
            return !(ts == t);
        }

        public static bool operator >(DayTimePoint ts, DateTime t)
        {
            return ts.hour > t.Hour
                || (ts.hour == t.Hour && ts.min > t.Minute);
        }

        public static bool operator >(DateTime t, DayTimePoint ts)
        {
            return ts < t;
        }

        public static bool operator >=(DayTimePoint ts, DateTime t)
        {
            return ts > t || ts == t;
        }

        public static bool operator >=(DateTime t, DayTimePoint ts)
        {
            return ts <= t;
        }

        public static bool operator >(DayTimePoint ts1, DayTimePoint ts2)
        {
            return ts1.hour > ts2.hour ||
                (ts1.hour == ts2.hour && ts1.min > ts2.min);
        }

        public static bool operator >=(DayTimePoint ts1, DayTimePoint ts2)
        {
            return ts1 == ts2 || ts1 > ts2;
        }

        public static bool operator <(DayTimePoint ts1, DayTimePoint ts2)
        {
            return ts1 != ts2 && !(ts1 > ts2);
        }

        public static bool operator <=(DayTimePoint ts1, DayTimePoint ts2)
        {
            return ts1 == ts2 || ts1 < ts2;
        }

        public static bool operator ==(DayTimePoint ts1, DayTimePoint ts2)
        {
            return (ts1.hour == ts2.hour && ts1.min == ts2.min);
        }

        public static bool operator !=(DayTimePoint ts1, DayTimePoint ts2)
        {
            return !(ts1 == ts2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is DayTimePoint))
                return false;

            if (this == (DayTimePoint)obj)
                return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (this.hour + "" + this.min).GetHashCode();
        }

        public DateTime ToDateTime()
        {
            return DateTime.Now.Date.AddHours(hour).AddMinutes(min);
        }
    }
}
