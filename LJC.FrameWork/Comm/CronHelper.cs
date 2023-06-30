using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    /// <summary>
    /// 在week上使用  5L表示本月最后一个星期五
    ///               7L表示本月最后一个星期天
    ///               
    /// 在week上使用  7#3表示每月的第三个星期天
    ///               2#4表示每月的第四个星期二
    /// </summary>
    public class CronHelper
    {
        class Cron
        {
            private int[] seconds = new int[60];
            private int[] minutes = new int[60];
            private int[] hours = new int[24];
            private int[] days = new int[31];
            private int[] month = new int[12];
            private int[] weeks = new int[7];
            //2019-2099年
            private int[] year = new int[80];

            public int[] Seconds { get => seconds; set => seconds = value; }
            public int[] Minutes { get => minutes; set => minutes = value; }
            public int[] Hours { get => hours; set => hours = value; }
            public int[] Days { get => days; set => days = value; }
            public int[] Month { get => month; set => month = value; }
            public int[] Weeks { get => weeks; set => weeks = value; }
            public int[] Year { get => year; set => year = value; }

            public Cron()
            {
                for (int i = 0; i < 60; i++)
                {
                    seconds[i] = 0;
                    minutes[i] = 0;
                }
                for (int i = 0; i < 24; i++)
                {
                    hours[i] = 0;
                }
                for (int i = 0; i < 31; i++)
                {
                    days[i] = 0;
                }
                for (int i = 0; i < 12; i++)
                {
                    month[i] = 0;
                }
                for (int i = 0; i < 7; i++)
                {
                    weeks[i] = 0;
                }
                for (int i = 0; i < 80; i++)
                {
                    year[i] = 0;
                }
            }

            public void Init()
            {
                for (int i = 0; i < 7; i++)
                {
                    weeks[i] = 0;
                }
                for (int i = 0; i < 31; i++)
                {
                    days[i] = 0;
                }
            }
        }

        /// <summary>
        /// Cron表达式转换(默认开始时间为当前)
        /// </summary>
        /// <param name="cron">表达式</param>
        /// <returns>最近5次要执行的时间</returns>
        public static List<DateTime> CronToDateTime(string cron)
        {
            try
            {
                List<DateTime> lits = new List<DateTime>();
                Cron c = new Cron();
                string[] arr = cron.Split(' ');
                Seconds(c, arr[0]);
                Minutes(c, arr[1]);
                Hours(c, arr[2]);
                Month(c, arr[4]);
                if (arr.Length < 7)
                {
                    Year(c, null);
                }
                else
                {
                    Year(c, arr[6]);
                }
                DateTime now = DateTime.Now;
                int addtime = 1;
                while (true)
                {
                    if (Check(c, now, addtime))
                    {
                        if (arr[3] != "?")
                        {
                            Days(c, arr[3], DateTime.DaysInMonth(now.Year, now.Month), now);
                            int DayOfWeek = (((int)now.DayOfWeek) + 6) % 7;
                            if (c.Days[now.Day - 1] == 1 && c.Weeks[DayOfWeek] == 1)
                            {
                                var dt = GetDateTime(now, addtime);
                                lits.Add(dt);
                            }
                        }
                        else
                        {
                            Weeks(c, arr[5], DateTime.DaysInMonth(now.Year, now.Month), now);
                            int DayOfWeek = (((int)now.DayOfWeek) + 6) % 7;
                            if (c.Days[now.Day - 1] == 1 && c.Weeks[DayOfWeek] == 1)
                            {
                                var dt = GetDateTime(now, addtime);
                                lits.Add(dt);
                            }
                        }
                    }
                    if (lits.Count >= 5)
                    {
                        break;
                    }
                    c.Init();
                    if (!arr[1].Contains('-') && !arr[1].Contains(',') && !arr[1].Contains('*') && !arr[1].Contains('/'))
                    {
                        if (now.Minute == int.Parse(arr[1]))
                        {
                            addtime = 3600;
                        }
                    }
                    else if (arr[0] == "0" && now.Second == 0)
                    {
                        addtime = 60;
                    }
                    now = now.AddSeconds(addtime);
                }
                return lits;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Cron表达式转换（自定义开始时间）
        /// </summary>
        /// <param name="cron">表达式</param>
        /// <param name="now">开始时间</param>
        /// <returns>最近5次要执行的时间</returns>
        public static List<DateTime> CronToDateTime(string cron, DateTime now)
        {
            try
            {
                List<DateTime> lits = new List<DateTime>();
                Cron c = new Cron();
                string[] arr = cron.Split(' ');
                Seconds(c, arr[0]);
                Minutes(c, arr[1]);
                Hours(c, arr[2]);
                Month(c, arr[4]);
                if (arr.Length < 7)
                {
                    Year(c, null);
                }
                else
                {
                    Year(c, arr[6]);
                }
                int addtime = 1;
                while (true)
                {
                    if (c.Seconds[now.Second] == 1 && c.Minutes[now.Minute] == 1 && c.Hours[now.Hour] == 1 && c.Month[now.Month - 1] == 1 && c.Year[now.Year - 2019] == 1)
                    {
                        if (arr[3] != "?")
                        {
                            Days(c, arr[3], DateTime.DaysInMonth(now.Year, now.Month), now);
                            int DayOfWeek = (((int)now.DayOfWeek) + 6) % 7;
                            if (c.Days[now.Day - 1] == 1 && c.Weeks[DayOfWeek] == 1)
                            {
                                lits.Add(now);
                            }
                        }
                        else
                        {
                            Weeks(c, arr[5], DateTime.DaysInMonth(now.Year, now.Month), now);
                            int DayOfWeek = (((int)now.DayOfWeek) + 6) % 7;
                            if (c.Days[now.Day - 1] == 1 && c.Weeks[DayOfWeek] == 1)
                            {
                                lits.Add(now);
                            }
                        }
                    }
                    if (lits.Count >= 5)
                    {
                        break;
                    }
                    c.Init();
                    if (!arr[1].Contains('-') && !arr[1].Contains(',') && !arr[1].Contains('*') && !arr[1].Contains('/'))
                    {
                        if (now.Minute == int.Parse(arr[1]))
                        {
                            addtime = 3600;
                        }
                    }
                    else if (arr[0] == "0" && now.Second == 0)
                    {
                        addtime = 60;
                    }
                    now = now.AddSeconds(addtime);
                }
                return lits;
            }
            catch
            {
                return null;
            }
        }

        private static bool Check(Cron c, DateTime now, int addtime)
        {
            if (addtime >= 3600)
            {
                return (c.Hours[now.Hour] == 1 && c.Month[now.Month - 1] == 1 && c.Year[now.Year - 2019] == 1);
            }
            else if (addtime >= 60)
            {
                return (c.Minutes[now.Minute] == 1 && c.Hours[now.Hour] == 1 && c.Month[now.Month - 1] == 1 && c.Year[now.Year - 2019] == 1);
            }
            return (c.Seconds[now.Second] == 1 && c.Minutes[now.Minute] == 1 && c.Hours[now.Hour] == 1 && c.Month[now.Month - 1] == 1 && c.Year[now.Year - 2019] == 1);
        }

        private static DateTime GetDateTime(DateTime now, int addseconds)
        {
            var dt = now;
            if (addseconds >= 3600)
            {
                dt = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            }
            else if (addseconds >= 60)
            {
                dt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            }
            return dt;
        }

        /// <summary>
        /// Cron表达式转换(默认开始时间为当前)
        /// </summary>
        /// <param name="cron">表达式</param>
        /// <returns>最近要执行的时间字符串</returns>
        public static DateTime? GetNextDateTime(string cron)
        {
            try
            {
                DateTime now = DateTime.Now;
                string[] arr = cron.Split(' ');
                if (IsOrNoOne(cron))
                {
                    string date = arr[6] + "/" + arr[4] + "/" + arr[3] + " " + arr[2] + ":" + arr[1] + ":" + arr[0];
                    if (DateTime.Compare(Convert.ToDateTime(date), now) >= 0)
                    {
                        return DateTime.Parse(date);
                    }
                    else
                    {
                        return null;
                    }
                }
                Cron c = new Cron();
                Seconds(c, arr[0]);
                Minutes(c, arr[1]);
                Hours(c, arr[2]);
                Month(c, arr[4]);
                if (arr.Length < 7)
                {
                    Year(c, null);
                }
                else
                {
                    Year(c, arr[6]);
                }
                int addtime = 1;
                while (true)
                {
                    if (Check(c, now, addtime))
                    {
                        if (arr[3] != "?")
                        {
                            Days(c, arr[3], DateTime.DaysInMonth(now.Year, now.Month), now);
                            int DayOfWeek = (((int)now.DayOfWeek) + 6) % 7;
                            if (c.Days[now.Day - 1] == 1 && c.Weeks[DayOfWeek] == 1)
                            {
                                var dt = GetDateTime(now, addtime);
                                return dt;
                            }
                        }
                        else
                        {
                            Weeks(c, arr[5], DateTime.DaysInMonth(now.Year, now.Month), now);
                            int DayOfWeek = (((int)now.DayOfWeek) + 6) % 7;
                            if (c.Days[now.Day - 1] == 1 && c.Weeks[DayOfWeek] == 1)
                            {
                                var dt = GetDateTime(now, addtime);
                                return dt;
                            }
                        }
                    }
                    c.Init();
                    if (!arr[1].Contains('-') && !arr[1].Contains(',') && !arr[1].Contains('*') && !arr[1].Contains('/'))
                    {
                        if (now.Minute == int.Parse(arr[1]))
                        {
                            addtime = 3600;
                        }
                    }
                    else if (arr[0] == "0" && now.Second == 0)
                    {
                        addtime = 60;
                    }
                    now = now.AddSeconds(addtime);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Cron表达式转换（自定义开始时间）
        /// </summary>
        /// <param name="cron">表达式</param>
        /// <param name="now">开始时间</param>
        /// <returns>最近要执行的时间字符串</returns>
        public static DateTime? GetNextDateTime(string cron, DateTime now)
        {
            try
            {
                var startNow = now;
                string[] arr = cron.Split(' ');
                if (IsOrNoOne(cron))
                {
                    string date = arr[6] + "/" + arr[4] + "/" + arr[3] + " " + arr[2] + ":" + arr[1] + ":" + arr[0];
                    if (DateTime.Compare(Convert.ToDateTime(date), now) > 0)
                    {
                        return DateTime.Parse(date);
                    }
                    else
                    {
                        return null;
                    }
                }
                Cron c = new Cron();
                Seconds(c, arr[0]);
                Minutes(c, arr[1]);
                Hours(c, arr[2]);
                Month(c, arr[4]);
                if (arr.Length < 7)
                {
                    Year(c, null);
                }
                else
                {
                    Year(c, arr[6]);
                }
                int addtime = 1;
                while (true)
                {
                    if (Check(c, now, addtime))
                    {
                        if (arr[3] != "?")
                        {
                            Days(c, arr[3], DateTime.DaysInMonth(now.Year, now.Month), now);
                            int DayOfWeek = (((int)now.DayOfWeek) + 6) % 7;
                            if (c.Days[now.Day - 1] == 1 && c.Weeks[DayOfWeek] == 1)
                            {
                                var dt = GetDateTime(now, addtime);
                                if (dt != startNow)
                                {
                                    return dt;
                                }
                            }
                        }
                        else
                        {
                            Weeks(c, arr[5], DateTime.DaysInMonth(now.Year, now.Month), now);
                            int DayOfWeek = (((int)now.DayOfWeek) + 6) % 7;
                            if (c.Days[now.Day - 1] == 1 && c.Weeks[DayOfWeek] == 1)
                            {
                                var dt = GetDateTime(now, addtime);
                                if (dt != startNow)
                                {
                                    return dt;
                                }
                            }
                        }
                    }
                    c.Init();
                    if (!arr[1].Contains('-') && !arr[1].Contains(',') && !arr[1].Contains('*') && !arr[1].Contains('/'))
                    {
                        if (now.Minute == 0 && arr[1] == "0")
                        {
                            addtime = 3600;
                        }
                    }
                    else if (arr[0] == "0" && now.Second == 0)
                    {
                        addtime = 60;
                    }
                    now = now.AddSeconds(addtime);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Cron表达式转换成中文描述
        /// </summary>
        /// <param name="cronExp"></param>
        /// <returns></returns>
        public static string TranslateToChinese(string cronExp)
        {
            if (cronExp == null || cronExp.Length < 1)
            {
                return "cron表达式为空";
            }
            string[] tmpCorns = cronExp.Split(' ');
            StringBuilder sBuffer = new StringBuilder();
            if (tmpCorns.Length == 6)
            {
                //解析月
                if (!tmpCorns[4].Equals("*"))
                {
                    sBuffer.Append(tmpCorns[4]).Append("月");
                }
                else
                {
                    sBuffer.Append("每月");
                }
                //解析周
                if (!tmpCorns[5].Equals("*") && !tmpCorns[5].Equals("?"))
                {
                    char[] tmpArray = tmpCorns[5].ToCharArray();
                    foreach (char tmp in tmpArray)
                    {
                        switch (tmp)
                        {
                            case '1':
                                sBuffer.Append("星期天");
                                break;
                            case '2':
                                sBuffer.Append("星期一");
                                break;
                            case '3':
                                sBuffer.Append("星期二");
                                break;
                            case '4':
                                sBuffer.Append("星期三");
                                break;
                            case '5':
                                sBuffer.Append("星期四");
                                break;
                            case '6':
                                sBuffer.Append("星期五");
                                break;
                            case '7':
                                sBuffer.Append("星期六");
                                break;
                            case '-':
                                sBuffer.Append("至");
                                break;
                            default:
                                sBuffer.Append(tmp);
                                break;
                        }
                    }
                }

                //解析日
                if (!tmpCorns[3].Equals("?"))
                {
                    if (!tmpCorns[3].Equals("*"))
                    {
                        sBuffer.Append(tmpCorns[3]).Append("日");
                    }
                    else
                    {
                        sBuffer.Append("每日");
                    }
                }

                //解析时
                if (!tmpCorns[2].Equals("*"))
                {
                    sBuffer.Append(tmpCorns[2]).Append("时");
                }
                else
                {
                    sBuffer.Append("每时");
                }

                //解析分
                if (!tmpCorns[1].Equals("*"))
                {
                    sBuffer.Append(tmpCorns[1]).Append("分");
                }
                else
                {
                    sBuffer.Append("每分");
                }

                //解析秒
                if (!tmpCorns[0].Equals("*"))
                {
                    sBuffer.Append(tmpCorns[0]).Append("秒");
                }
                else
                {
                    sBuffer.Append("每秒");
                }
            }
            return sBuffer.ToString();
        }

        #region 初始化Cron对象
        private static void Seconds(Cron c, string str)
        {
            if (str == "*")
            {
                for (int i = 0; i < 60; i++)
                {
                    c.Seconds[i] = 1;
                }
            }
            else if (str.Contains('-'))
            {
                int begin = int.Parse(str.Split('-')[0]);
                int end = int.Parse(str.Split('-')[1]);
                for (int i = begin; i <= end; i++)
                {
                    c.Seconds[i] = 1;
                }
            }
            else if (str.Contains('/'))
            {
                int begin = int.Parse(str.Split('/')[0]);
                int interval = int.Parse(str.Split('/')[1]);
                while (true)
                {
                    c.Seconds[begin] = 1;
                    if ((begin + interval) >= 60)
                        break;
                    begin += interval;
                }
            }
            else if (str.Contains(','))
            {

                for (int i = 0; i < str.Split(',').Length; i++)
                {
                    c.Seconds[int.Parse(str.Split(',')[i])] = 1;
                }
            }
            else
            {
                c.Seconds[int.Parse(str)] = 1;
            }
        }
        private static void Minutes(Cron c, string str)
        {
            if (str == "*")
            {
                for (int i = 0; i < 60; i++)
                {
                    c.Minutes[i] = 1;
                }
            }
            else if (str.Contains('-'))
            {
                int begin = int.Parse(str.Split('-')[0]);
                int end = int.Parse(str.Split('-')[1]);
                for (int i = begin; i <= end; i++)
                {
                    c.Minutes[i] = 1;
                }
            }
            else if (str.Contains('/'))
            {
                int begin = int.Parse(str.Split('/')[0]);
                int interval = int.Parse(str.Split('/')[1]);
                while (true)
                {
                    c.Minutes[begin] = 1;
                    if ((begin + interval) >= 60)
                        break;
                    begin += interval;
                }
            }
            else if (str.Contains(','))
            {

                for (int i = 0; i < str.Split(',').Length; i++)
                {
                    c.Minutes[int.Parse(str.Split(',')[i])] = 1;
                }
            }
            else
            {
                c.Minutes[int.Parse(str)] = 1;
            }
        }
        private static void Hours(Cron c, string str)
        {
            if (str == "*")
            {
                for (int i = 0; i < 24; i++)
                {
                    c.Hours[i] = 1;
                }
            }
            else if (str.Contains('-'))
            {
                int begin = int.Parse(str.Split('-')[0]);
                int end = int.Parse(str.Split('-')[1]);
                for (int i = begin; i <= end; i++)
                {
                    c.Hours[i] = 1;
                }
            }
            else if (str.Contains('/'))
            {
                int begin = int.Parse(str.Split('/')[0]);
                int interval = int.Parse(str.Split('/')[1]);
                while (true)
                {
                    c.Hours[begin] = 1;
                    if ((begin + interval) >= 24)
                        break;
                    begin += interval;
                }
            }
            else if (str.Contains(','))
            {

                for (int i = 0; i < str.Split(',').Length; i++)
                {
                    c.Hours[int.Parse(str.Split(',')[i])] = 1;
                }
            }
            else
            {
                c.Hours[int.Parse(str)] = 1;
            }
        }
        private static void Month(Cron c, string str)
        {
            if (str == "*")
            {
                for (int i = 0; i < 12; i++)
                {
                    c.Month[i] = 1;
                }
            }
            else if (str.Contains('-'))
            {
                int begin = int.Parse(str.Split('-')[0]);
                int end = int.Parse(str.Split('-')[1]);
                for (int i = begin; i <= end; i++)
                {
                    c.Month[i - 1] = 1;
                }
            }
            else if (str.Contains('/'))
            {
                int begin = int.Parse(str.Split('/')[0]);
                int interval = int.Parse(str.Split('/')[1]);
                while (true)
                {
                    c.Month[begin - 1] = 1;
                    if ((begin + interval) >= 12)
                        break;
                    begin += interval;
                }
            }
            else if (str.Contains(','))
            {

                for (int i = 0; i < str.Split(',').Length; i++)
                {
                    c.Month[int.Parse(str.Split(',')[i]) - 1] = 1;
                }
            }
            else
            {
                c.Month[int.Parse(str) - 1] = 1;
            }
        }
        private static void Year(Cron c, string str)
        {
            if (str == null || str == "*")
            {
                for (int i = 0; i < 80; i++)
                {
                    c.Year[i] = 1;
                }
            }
            else if (str.Contains('-'))
            {
                int begin = int.Parse(str.Split('-')[0]);
                int end = int.Parse(str.Split('-')[1]);
                for (int i = begin - 2019; i <= end - 2019; i++)
                {
                    c.Year[i] = 1;
                }
            }
            else
            {
                c.Year[int.Parse(str) - 2019] = 1;
            }
        }
        private static void Days(Cron c, string str, int len, DateTime now)
        {
            for (int i = 0; i < 7; i++)
            {
                c.Weeks[i] = 1;
            }
            if (str == "*" || str == "?")
            {
                for (int i = 0; i < len; i++)
                {
                    c.Days[i] = 1;
                }
            }
            else if (str.Contains('-'))
            {
                int begin = int.Parse(str.Split('-')[0]);
                int end = int.Parse(str.Split('-')[1]);
                for (int i = begin; i <= end; i++)
                {
                    c.Days[i - 1] = 1;
                }
            }
            else if (str.Contains('/'))
            {
                int begin = int.Parse(str.Split('/')[0]);
                int interval = int.Parse(str.Split('/')[1]);
                while (true)
                {
                    c.Days[begin - 1] = 1;
                    if ((begin + interval) >= len)
                        break;
                    begin += interval;
                }
            }
            else if (str.Contains(','))
            {
                for (int i = 0; i < str.Split(',').Length; i++)
                {
                    c.Days[int.Parse(str.Split(',')[i]) - 1] = 1;
                }
            }
            else if (str.Contains('L'))
            {
                int i = str.Replace("L", "") == "" ? 0 : int.Parse(str.Replace("L", ""));
                c.Days[len - 1 - i] = 1;
            }
            else if (str.Contains('W'))
            {
                c.Days[len - 1] = 1;
            }
            else
            {
                c.Days[int.Parse(str) - 1] = 1;
            }
        }
        private static void Weeks(Cron c, string str, int len, DateTime now)
        {
            if (str == "*" || str == "?")
            {
                for (int i = 0; i < 7; i++)
                {
                    c.Weeks[i] = 1;
                }
            }
            else if (str.Contains('-'))
            {
                int begin = int.Parse(str.Split('-')[0]);
                int end = int.Parse(str.Split('-')[1]);
                for (int i = begin; i <= end; i++)
                {
                    c.Weeks[i - 1] = 1;
                }
            }
            else if (str.Contains(','))
            {
                for (int i = 0; i < str.Split(',').Length; i++)
                {
                    c.Weeks[int.Parse(str.Split(',')[i]) - 1] = 1;
                }
            }
            else if (str.Contains('L'))
            {
                int i = str.Replace("L", "") == "" ? 0 : int.Parse(str.Replace("L", ""));
                if (i == 0)
                {
                    c.Weeks[6] = 1;
                }
                else
                {
                    c.Weeks[i - 1] = 1;
                    c.Days[GetLastWeek(i, now) - 1] = 1;
                    return;
                }
            }
            else if (str.Contains('#'))
            {
                int i = int.Parse(str.Split('#')[0]);
                int j = int.Parse(str.Split('#')[1]);
                c.Weeks[i - 1] = 1;
                c.Days[GetWeek(i - 1, j, now)] = 1;
                return;
            }
            else
            {
                c.Weeks[int.Parse(str) - 1] = 1;
            }
            //week中初始化day，则说明day没要求
            for (int i = 0; i < len; i++)
            {
                c.Days[i] = 1;
            }
        }
        #endregion

        #region 方法

        public static bool IsOrNoOne(string cron)
        {
            if (cron.Contains('-') || cron.Contains(',') || cron.Contains('/') || cron.Contains('*'))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 获取最后一个星期几的day
        /// </summary>
        /// <param name="i">星期几</param>
        /// <param name="now"></param>
        /// <returns></returns>
        private static int GetLastWeek(int i, DateTime now)
        {
            DateTime d = now.AddDays(1 - now.Day).Date.AddMonths(1).AddSeconds(-1);
            int DayOfWeek = ((((int)d.DayOfWeek) + 6) % 7) + 1;
            int a = DayOfWeek >= i ? DayOfWeek - i : 7 + DayOfWeek - i;
            return DateTime.DaysInMonth(now.Year, now.Month) - a;
        }
        /// <summary>
        /// 获取当月第几个星期几的day
        /// </summary>
        /// <param name="i">星期几</param>
        /// <param name="j">第几周</param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static int GetWeek(int i, int j, DateTime now)
        {
            int day = 0;
            DateTime d = new DateTime(now.Year, now.Month, 1);
            int DayOfWeek = ((((int)d.DayOfWeek) + 6) % 7) + 1;
            if (i >= DayOfWeek)
            {
                day = (7 - DayOfWeek + 1) + 7 * (j - 2) + i;
            }
            else
            {
                day = (7 - DayOfWeek + 1) + 7 * (j - 1) + i;
            }
            return day;
        }
        #endregion

    }
}
