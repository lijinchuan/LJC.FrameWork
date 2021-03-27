using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.CodeExpression
{
    public class CalSignFactory
    {
        private static Dictionary<string, Type> RegisterFunSign = new Dictionary<string, Type>();

        public static void Register(string alias, Type funsignType)
        {
            try
            {
                RegisterFunSign.Add(alias.ToLower(), funsignType);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 保留字
        /// </summary>
        static string ProtectWord = "|fun|";

        private static Func<CalCurrent,CalSign> CreateCalSign(string signExp)
        {
            if (string.Equals(signExp, "and", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new AndSign());
            else if (string.Equals(signExp, "not", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new NotSign());
            else if (string.Equals(signExp, "or", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new OrSign());
            else if (string.Equals(signExp, ":", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new SetValueSign(p));
            else if (string.Equals(signExp, ":=", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new OutputValueSign());
            else if (string.Equals(signExp, "=", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new EqSign());
            else if (string.Equals(signExp, ">", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new Bigersign());
            else if (string.Equals(signExp, ">=", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new BigerEqSign());
            else if (string.Equals(signExp, "<", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new SmallerSign());
            else if (string.Equals(signExp, "<=", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new SmallerEqSign());
            else if (string.Equals(signExp, "+", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new PlusSign());
            else if (string.Equals(signExp, "-", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new MinusSign());
            else if (string.Equals(signExp, "*", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new MultiplieSign());
            else if (string.Equals(signExp, "/", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new DividedSign());
            else if (string.Equals(signExp, "mod", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new ModSign());
            else if (string.Equals(signExp, ",", StringComparison.OrdinalIgnoreCase))
                return new Func<CalCurrent, CalSign>(p => new BeanSign());
            else
            {
                Match mc = Comm.MatchFunctionExpress(signExp);
                //Match mc = new Regex(@"^([A-z]{1}[A-z0-9]*)\((.*)\)$").Match(signExp);
                if (mc.Success)
                {
                    if (Comm.BrackIsMatched(mc.Groups[2].Value, 0, mc.Groups[2].Length))
                    {
                        var fs = CreateFunSign(mc.Groups[1].Value, mc.Groups[2].Value);
                        if (fs == null)
                            throw new ExpressErrorException("不存在的函数" + mc.Groups[1].Value + "！");
                        
                        return fs;
                    }
                }

                //mc = Comm.MatchValNameExpress(signExp);
                if (Comm.IsValName(signExp) && !IsProtectWord(signExp))
                {
                    var fs = CreateFunSign(signExp,null);
                    if (fs != null)
                        return fs;

                    var vs = new Func<CalCurrent, CalSign>(p => new VarSign(signExp,p));
                    return vs;
                }

                return null;
            }

        }

        internal static Func<CalCurrent,FunSign> CreateFunSign(string funName,string paramString)
        {
            if (string.Equals("t", funName, StringComparison.OrdinalIgnoreCase)
                || string.Equals("true", funName, StringComparison.OrdinalIgnoreCase))
            {
                return new Func<CalCurrent, FunSign>(p => new T());
            }

            if (string.Equals("f", funName, StringComparison.OrdinalIgnoreCase)
                || string.Equals("false", funName, StringComparison.OrdinalIgnoreCase))
            {
                return new Func<CalCurrent, FunSign>(p => new F());
            }

            Type fs;
            if (RegisterFunSign.TryGetValue(funName.ToLower(), out fs))
            {
                return new Func<CalCurrent, FunSign>(p =>
                {
                    var r = (FunSign)Activator.CreateInstance(fs, p);
                    r.ParamString = paramString;
                    return r;
                });
            }
            return null;
        }

        private static int count = 0;
        private static ConcurrentDictionary<string, Func<CalCurrent, CalSign>> singdic = new ConcurrentDictionary<string, Func<CalCurrent, CalSign>>();
        internal static bool TryCreateSign(string sign, CalCurrent pool, out CalSign calSign)
        {
            Console.WriteLine((++count) + ":" + sign + ":" + singdic.Count);
            calSign = null;
            Func<CalCurrent, CalSign> f = null;
            if(!singdic.TryGetValue(sign,out f))
            {
                f = CreateCalSign(sign);
                singdic.TryAdd(sign, f);
            }
            
            if (f != null)
            {
                calSign = f(pool);
            }

            return calSign != null;
        }

        internal static bool IsProtectWord(string word)
        {
            return ProtectWord.Contains("|" + word.ToLower() + "|");
        }
    }
}
