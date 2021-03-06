using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.Comm
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// 是否是润年
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static bool IsLeapYear(int year)
        {
            if (year % 4 > 0)
                return false;

            if (year % 100 == 0 && year % 400 > 0)
                return false;

            return true;
        }
        /// <summary>
        /// 求月最后一天
        /// </summary>
        /// <param name="mon"></param>
        /// <returns></returns>
        public static int MonLastDay(int year,int mon)
        {
            if (".1.3.5.7.8.10.12.".Contains("." + mon + "."))
                return 31;

            if (".4.6.9.11.".Contains("." + mon + "."))
                return 30;

            if (IsLeapYear(year))
                return 29;

            return 28;
        }

        public static long GetTimeStamp(DateTime? dt=null)
        {
            return (long)(dt.HasValue?dt.Value:DateTime.Now).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        public static DateTime FromTimeStamp(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(timestamp).ToLocalTime();
        }

        public static DateTime ParseDateTime(string yyyymmdd, string hhmmss)
        {
            if (hhmmss.Length == 1)
            {
                hhmmss = "00000"+hhmmss;
            }
            else if (hhmmss.Length == 4)
            {
                hhmmss = "00" + hhmmss;
            }
            return DateTime.Parse(new Regex(@"^(\d{4})(\d{2})(\d{2})$").Replace(yyyymmdd, "$1-$2-$3")
                + " " + new Regex(@"^(\d{1,2})(\d{2})(\d{2})$").Replace(hhmmss, "$1:$2:$3"));
        }

        public static DateTime ConvertToDateTime(string dateTimeString)
        {
            if (new Regex(@"^\d{8}$").IsMatch(dateTimeString))
                return DateTime.Parse(new Regex(@"^((19|20)\d{2})(\d{2})(\d{2})$").Replace(dateTimeString, "$1-$3-$4"));
            else if (new Regex(@"^\d{14}$").IsMatch(dateTimeString))
                return DateTime.Parse(new Regex(@"^((19|20)\d{2})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})$").Replace(dateTimeString, "$1-$3-$4 $5:$6:$7"));

            throw new Exception(string.Format("无法把{0}转换成时间格式！", dateTimeString));
        }

        /// <summary>
        /// 取当周的第一天，周一为第一天
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetWeekFirstDate(DateTime dateTime)
        {
            var dayofweek = (int)dateTime.DayOfWeek;
            if (dayofweek == 0)
                return dateTime.Date.AddDays(dayofweek - 6);
            return dateTime.Date.AddDays(-dayofweek + 1);
        }

        /// <summary>
        /// 获取周的最后一天，周日
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetWeekLastDate(DateTime dateTime)
        {
            var dayofweek = (int)dateTime.DayOfWeek;
            if (dayofweek == 0)
                return dateTime.Date;
            return dateTime.Date.AddDays(7 - dayofweek);
        }

        /// <summary>
        /// 获取本日期是周几 1-7 代表周一到周日
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int GetDayOfWeek(this DateTime dateTime)
        {
            int day = (int)dateTime.DayOfWeek;
            if (day == 0) return 7;
            return day;
        }
    }
}
