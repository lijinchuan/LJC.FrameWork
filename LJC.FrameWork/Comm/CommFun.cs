using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Xml.Serialization;
using System.Timers;
using System.Drawing;
using System.Data;
using LJC.FrameWork.Data.QuickDataBase;

namespace LJC.FrameWork.Comm
{
    public static class CommFun
    {
        /// <summary>
        /// 创建一个表格
        /// </summary>
        /// <param name="colsName">表格字段，可以加//注释</param>
        /// <returns></returns>
        public static DataTable CreateTable(params string[] colsName)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < colsName.Length; i++)
            {
                dt.Columns.Add(colsName[i].Split(new string[] { "//" }, StringSplitOptions.None)[0].Trim());
            }

            return dt;
        }

        /// <summary>
        /// 创建一个表格
        /// </summary>
        /// <param name="colsName">表格字段,可以加//注释</param>
        /// <returns></returns>
        public static DataTable CreateFatTable(params string[] colsName)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < colsName.Length; i++)
            {
                dt.Columns.Add(colsName[i].Split(new string[] { "//" }, StringSplitOptions.None)[0].Trim(), typeof(object));
            }

            return dt;
        }

        public static Match SingleMatch(string regexExp,string source)
        {
            Regex rg = new Regex(regexExp,RegexOptions.IgnoreCase|RegexOptions.Multiline|RegexOptions.IgnorePatternWhitespace);

            MatchCollection ms = rg.Matches(source);
            if (ms.Count != 1)
                return Match.Empty;

            return ms[0];

        }

        public static string SingleMatchResult(string regexExp, string source, int groupIndex)
        {
            Match m = SingleMatch(regexExp, source);

            if (m != null && m.Groups.Count >= groupIndex)
            {
                return m.Groups[groupIndex].Value;
            }

            return string.Empty;
        }

        public static MatchCollection MulitMatch(string regexExp, string source)
        {
            Regex rg = new Regex(regexExp, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

            return rg.Matches(source);
        }

        public static string[] MulitMatchResult(string regexExp, string source)
        {
            List<string> result=new List<string>();

            Regex rg = new Regex(regexExp, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

            MatchCollection matchs = rg.Matches(source);

            if (matchs != null && matchs.Count > 0)
            {
                foreach (Match m in matchs)
                {
                    result.Add(m.Value);
                }
            }

            return result.ToArray();
        }

        
        public static DateTime ConvertToDateTime(string yyyyMMdd, string hhmmss)
        {
            DateTime result = default(DateTime);
            string str= new Regex(@"^(\d{4})(\d{2})(\d{2})").Replace(yyyyMMdd+" "+hhmmss,"$1-$2-$3");
            DateTime.TryParse(str,out result);

            return result;
        }

        /// <summary>
        /// 断言，判断条件是否满足，如果不满足，抛出异常
        /// </summary>
        /// <param name="b"></param>
        public static void Assert(bool b)
        {
            if (!b)
                throw new Exception("条件不满足，无法继续进行下去。");
        }

        [Obsolete("此方法已经转到StringHelper里面")]
        /// <summary>
        /// 最字符串的后几位字符组成的子串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string LastN(this string input, int n)
        {
            if (n <= 0)
                return "";

            if (n >= input.Length)
                return input;

            return input.Substring(input.Length - n, n);
        }

        [Obsolete("废弃掉")]
        /// <summary>
        /// 获取一个唯一的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetAUniqueString()
        {
            DateTime now = DateTime.Now;

            return string.Format("{0}{1}{2}{3}{4}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
        }

        /// <summary>
        /// 取当前程序运行的目录
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentAppForder()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 定时执行
        /// </summary>
        /// <param name="millSecond">间隔时间</param>
        /// <param name="act">执行方法</param>
        /// <param name="canMulti">允许多运行，指上一次没有运行完，能否再次触发运行，默认false</param>
        /// <returns></returns>
        public static Timer SetInterval(int millSecond, Func<bool> act)
        {
            Timer timer = new Timer();
            timer.Interval = millSecond;

            timer.Elapsed += (o, e) => { if (act()) { timer.Stop(); timer.Close(); timer.Dispose(); } };

            timer.Start();

            return timer;
        }

        /// <summary>
        /// 转化为几位有效小数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this decimal data,int point=2)
        {
            return Math.Round(data, point);
        }

        [Obsolete("废弃")]
        /// <summary>
        /// 报警
        /// </summary>
        /// <param name="times">报警次数，每秒一次</param>
        public static void Alert(int times)
        {
            while (times-- > 0)
            {
                Console.Write("\a\a\a");
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static bool IsinheritInterface<T,I>()
        {
            if(!typeof(T).IsClass||!typeof(I).IsInterface)
            {
                return false;
            }

            try
            {
                InterfaceMapping map = typeof(T).GetInterfaceMap(typeof(I));
                return true;
            }
            catch
            {

            }

            return false;
        }

        /// <summary>
        /// 取属性成员
        /// </summary>
        /// <typeparam name="A">属性</typeparam>
        /// <typeparam name="T">类</typeparam>
        /// <returns></returns>
        public static List<A> GetAllPAttr<A, T>()
        {
            string key = "GetAllPAttr_" + typeof(A).FullName + "_" + typeof(T).FullName;
            List<A> result = (List<A>)MemCach.GetCach(key);
            if (result != null)
                return result;
            result = new List<A>();

            PropertyInfo[] propertyinfos = typeof(T).GetProperties();

            propertyinfos.ToList().ForEach(p =>
            {
                object[] obs = p.GetCustomAttributes(typeof(A), true);
                if (obs.Length > 0)
                {
                    A attr = (A)obs[0];
                    PropertyInfo pp = attr.GetType().GetProperty("Property");
                    if (pp != null)
                    {
                        pp.SetValue(attr, p, null);
                    }

                    result.Add((A)obs[0]);
                }
                //else
                //{
                //    A attr = new A();
                //    PropertyInfo pp = attr.GetType().GetProperty("Property");
                //    if (pp != null)
                //    {
                //        pp.SetValue(attr, p, null);
                //    }

                //    result.Add(attr);
                //}
            });

            MemCach.AddCach(key, result);
            return result;
        }

        /// <summary>
        /// 取属性成员
        /// </summary>
        /// <typeparam name="A">属性</typeparam>
        /// <typeparam name="T">类</typeparam>
        /// <returns></returns>
        public static List<ReportAttr> GetQuickDataBaseAttr<T>()
        {
            string key = "GetAllPAttr_" + typeof(ReportAttr).FullName + "_" + typeof(T).FullName;
            List<ReportAttr> result = (List<ReportAttr>)MemCach.GetCach(key);
            if (result != null)
                return result;

            string tabName;
            object[] os = typeof(T).GetCustomAttributes(typeof(ReportAttr), true);
            if (os.Length > 0)
            {
                tabName = ((ReportAttr)os[0]).TableName;
            }
            else
            {
                tabName = typeof(T).Name;
            }

            result = new List<ReportAttr>();

            PropertyInfo[] propertyinfos = typeof(T).GetProperties();

            propertyinfos.ToList().ForEach(p =>
            {
                object[] obs = p.GetCustomAttributes(typeof(ReportAttr), true);
                if (obs.Length > 0)
                {
                    ReportAttr attr = (ReportAttr)obs[0];
                    attr.TableName = tabName;
                    if (string.IsNullOrWhiteSpace(attr.Column))
                    {
                        attr.Column = p.Name;
                    }
                    if (string.IsNullOrWhiteSpace(attr.ColumnName))
                    {
                        attr.ColumnName = p.Name;
                    }
                    PropertyInfo pp = attr.GetType().GetProperty("Property");
                    if (pp != null)
                    {
                        pp.SetValue(attr, p, null);
                    }

                    result.Add((ReportAttr)obs[0]);
                }
            });

            MemCach.AddCach(key, result);
            return result;
        }

        public static long GetSize(string path)
        {
            FileInfo f = new FileInfo(path);

            return f.Length;
        }

       
        public static string GetRuningPath()
        {
            //D:\\Work\\learn\\ATrade_gx\\ATUI2.0\\ATradeUI2.0\\bin\\Debug\\ATradeUI2.0.EXETradeStrategy.xml
            string path = System.Windows.Forms.Application.ExecutablePath;
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;
            return path.Substring(0, path.LastIndexOf('\\') + 1);
        }

        public static decimal TryParseDecimal(string decimalstr)
        {
            decimal tryvalue = default(decimal);
            decimal.TryParse(decimalstr, out tryvalue);
            return tryvalue;
        }

        public static double TryParseDouble(string doublestr)
        {
            double tryvalue = default(double);
            double.TryParse(doublestr, out tryvalue);
            return tryvalue;
        }

    }
}
