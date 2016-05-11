using System;
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

        private static CalSign CreateCalSign(string signExp, CalCurrent pool)
        {
            if (string.Equals(signExp, "and", StringComparison.OrdinalIgnoreCase))
                return new AndSign();
            else if (string.Equals(signExp, "not", StringComparison.OrdinalIgnoreCase))
                return new NotSign();
            else if (string.Equals(signExp, "or", StringComparison.OrdinalIgnoreCase))
                return new OrSign();
            else if (string.Equals(signExp, ":", StringComparison.OrdinalIgnoreCase))
                return new SetValueSign(pool);
            else if (string.Equals(signExp, ":=", StringComparison.OrdinalIgnoreCase))
                return new OutputValueSign();
            else if (string.Equals(signExp, "=", StringComparison.OrdinalIgnoreCase))
                return new EqSign();
            else if (string.Equals(signExp, ">", StringComparison.OrdinalIgnoreCase))
                return new Bigersign();
            else if (string.Equals(signExp, ">=", StringComparison.OrdinalIgnoreCase))
                return new BigerEqSign();
            else if (string.Equals(signExp, "<", StringComparison.OrdinalIgnoreCase))
                return new SmallerSign();
            else if (string.Equals(signExp, "<=", StringComparison.OrdinalIgnoreCase))
                return new SmallerEqSign();
            else if (string.Equals(signExp, "+", StringComparison.OrdinalIgnoreCase))
                return new PlusSign();
            else if (string.Equals(signExp, "-", StringComparison.OrdinalIgnoreCase))
                return new MinusSign();
            else if (string.Equals(signExp, "*", StringComparison.OrdinalIgnoreCase))
                return new MultiplieSign();
            else if (string.Equals(signExp, "/", StringComparison.OrdinalIgnoreCase))
                return new DividedSign();
            else if (string.Equals(signExp, "mod", StringComparison.OrdinalIgnoreCase))
                return new ModSign();
            else if (string.Equals(signExp, ",", StringComparison.OrdinalIgnoreCase))
                return new BeanSign();
            else
            {
                Match mc = Comm.MatchFunctionExpress(signExp);
                //Match mc = new Regex(@"^([A-z]{1}[A-z0-9]*)\((.*)\)$").Match(signExp);
                if (mc.Success)
                {
                    if (Comm.BrackIsMatched(mc.Groups[2].Value, 0, mc.Groups[2].Length))
                    {
                        FunSign fs = CreateFunSign(mc.Groups[1].Value, pool);
                        if (fs == null)
                            throw new ExpressErrorException("不存在的函数" + mc.Groups[1].Value + "！");

                        fs.ParamString = mc.Groups[2].Value;

                        return fs;
                    }
                }

                //mc = Comm.MatchValNameExpress(signExp);
                if (Comm.IsValName(signExp) && !IsProtectWord(signExp))
                {
                    FunSign fs = CreateFunSign(signExp, pool);
                    if (fs != null)
                        return fs;

                    VarSign vs = new VarSign(signExp, pool);
                    return vs;
                }

                return null;
            }

        }

        internal static FunSign CreateFunSign(string funName, CalCurrent pool)
        {
            if (string.Equals("t", funName, StringComparison.OrdinalIgnoreCase)
                || string.Equals("true", funName, StringComparison.OrdinalIgnoreCase))
            {
                return new T();
            }

            if (string.Equals("f", funName, StringComparison.OrdinalIgnoreCase)
                || string.Equals("false", funName, StringComparison.OrdinalIgnoreCase))
            {
                return new F();
            }

            Type fs;
            if (RegisterFunSign.TryGetValue(funName.ToLower(), out fs))
            {
                return (FunSign)Activator.CreateInstance(fs,pool);
            }
            return null;
        }

        internal static bool TryCreateSign(string sign, CalCurrent pool, out CalSign calSign)
        {
            calSign = CreateCalSign(sign, pool);

            return calSign != null;
        }

        internal static bool IsProtectWord(string word)
        {
            return ProtectWord.Contains("|" + word.ToLower() + "|");
        }
    }
}
