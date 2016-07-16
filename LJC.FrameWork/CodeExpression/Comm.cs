using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.CodeExpression
{
    public static class Comm
    {
        public static uint CallCount_ToDecimal = 0;

        public static object Parse(string val)
        {
            double result1;
            if (double.TryParse(val, out result1))
            {
                return result1;
            }

            bool result2;
            if (bool.TryParse(val, out result2))
            {
                return result2;
            }

            return val;
        }

        public static Type GetType(string val)
        {

            return Parse(val).GetType();
        }

        //public static decimal ToDecimal(this object o, int poit = 2)
        //{
        //    if (o == null)
        //        return 0.00M;

        //    decimal d;
        //    //if (o is RunintimeValue)
        //    //{
        //    //    var ro = ((RunintimeValue)o).Invoke();
        //    //    if (decimal.TryParse(ro.ToString(), out d))
        //    //    {
        //    //        return decimal.Parse(d.ToString("f" + poit));
        //    //    }
        //    //}
        //    //else
        //    //{
        //    if (decimal.TryParse(o.ToString(), out d))
        //    {
        //        return decimal.Parse(d.ToString("f" + poit));
        //    }
        //    //}

        //    return 0M;

        //}

        public static double ToDouble(this object o, uint point = 2)
        {
            ++CallCount_ToDecimal;

            if (o == null)
                return 0.00;

            if (o is double)
            {
                double orial = (double)o;
                var pow = Math.Pow(10, point);
                return (orial * pow) / pow;
            }

            double d;
            if (double.TryParse(o.ToString(), out d))
            {
                return double.Parse(d.ToString("f" + point));
            }

            return 0.00;

        }

        //public static int ToInt(this object o)
        //{
        //    if (o == null)
        //        return 0;
        //    return int.Parse(o.ToString());
        //}

        public static bool ToBool(this object o)
        {
            if (o == null)
                return false;

            if (o is Boolean)
            {
                return (bool)o;
            }

            bool b;

            //if (o is RunintimeValue)
            //{
            //    var ro = ((RunintimeValue)o).Invoke();
            //    if (bool.TryParse(ro.ToString(), out b))
            //    {
            //        return b;
            //    }
            //}
            //else
            //{

            if (bool.TryParse(o.ToString(), out b))
            {
                return b;
            }
            //}

            return false;
        }

        public static object[] ToArr(this object o)
        {
            if (o == null)
                return new object[] { };

            if (o is object[])
                return (object[])o;

            return new object[] { o };
        }

        private static Regex regexFunctionExpress = new Regex(@"^([A-z]{1}[A-z0-9]*)\((.*)\)$");
        public static Match MatchFunctionExpress(string express)
        {
            return regexFunctionExpress.Match(express);
        }

        //private static Regex regexValNameExpress = new Regex(@"^([A-z]{1}[A-z0-9]*)$");
        //public static Match MatchValNameExpress(string express)
        //{
        //    return regexValNameExpress.Match(express);
        //}

        public static bool IsValName(string express)
        {
            if (string.IsNullOrEmpty(express))
                return false;

            var firstChar = express[0];
            if (firstChar < 'A' || firstChar > 'z')
                return false;

            int len = express.Length;
            for (int i = 1; i < len; i++)
            {
                char ch = express[i];
                if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'z'))
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        public static bool IsCharOrNum(this char ch)
        {
            if (ch >= '0' && ch <= '9')
                return true;

            if (ch >= 'A' && ch <= 'z')
                return true;

            return false;
        }

        public static bool IsNum(this char ch)
        {
            return ch >= '0' && ch <= '9';
        }


        public static bool BrackIsMatched(string express, int start, int end)
        {
            int brackets = 0;
            for (int k = start; k < end; k++)
            {
                if (express[k] == '(')
                {
                    brackets++;
                }

                if (express[k] == ')' && brackets == 0)
                    return false;

                if (express[k] == ')' && brackets > 0)
                {
                    brackets--;
                }

            }

            return brackets == 0;
        }

        /// <summary>
        /// 去括号 
        /// </summary>
        /// <returns></returns>
        public static string TrimBrackets(this string express)
        {
            for (int i = 0, j = express.Length - 1; ; i = 0, j = express.Length - 1)
            {
                if (express[i] == '(' && express[j] == ')')
                {
                    if (BrackIsMatched(express, i + 1, j))
                    {
                        express = express.Remove(j, 1);
                        express = express.Remove(i, 1);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return express;
        }
    }
}
