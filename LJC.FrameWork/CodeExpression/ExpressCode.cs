using LJC.FrameWork.CodeExpression.Sign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.CodeExpression
{
    public class ExpressCode
    {
        public List<BinTree<IExpressPart>> ExpressTreesBack
        {
            get;
            private set;
        }
        private List<BinTree<IExpressPart>> expressTrees = null;
        /// <summary>
        /// 变量存储
        /// </summary>
        public CalCurrent CalCurrent
        {
            get;
            private set;
        }

        private string code;
        public ExpressCode(string code)
        {
            CalCurrent = new CalCurrent();
            this.code = code;
        }

        private ExpressCode()
        {
            CalCurrent = new CalCurrent();
        }

        private void AnalyseExpress()
        {
            ExpressTreesBack = new List<BinTree<IExpressPart>>();
            string[] subExpress = code.Split(new string[] { "\r\n", "\n", ";" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < subExpress.Length; i++)
            {
#if DEBUG
                string exp = subExpress[i].Trim();
                ExpressTreesBack.Add(ResolveExpress(exp, i).Pepare());
#else
                try
                {
                    string exp = subExpress[i].Trim();
                    ExpressTreesBack.Add(ResolveExpress(exp, i).Pepare());
                }
                catch (ExpressErrorException e)
                {
                    e.ErrerLine = i + 1;
                    throw e;
                }
                catch (Exception e)
                {
                    throw new Exception("第" + (i + 1) + "行：" + e.Message);
                }
#endif

            }
        }

        public CalResult CallResult()
        {
            if (this.ExpressTreesBack == null)
            {
                AnalyseExpress();
            }

            this.expressTrees = this.ExpressTreesBack.Select(p => p).ToList();

            for (int i = 0; i < expressTrees.Count; i++)
            {
#if DEBUG
                CallResult1(expressTrees[i]);
#else
                try
                {
                    CallResult1(expressTrees[i]);
                }
                catch (ExpressErrorException e)
                {
                    e.ErrerLine = i + 1;
                    throw e;
                }
                catch (Exception e)
                {
                    throw new Exception("第" + (i + 1) + "行：" + e.Message);
                }
#endif

            }

            return null;
        }

        public CalResult CallResult(object param)
        {
            this.CalCurrent.RuntimeParam = param;
            return CallResult();
        }

        /// <summary>
        /// 计算表达式的值
        /// </summary>
        /// <param name="express"></param>
        /// <returns></returns>
        private CalResult CallResult1(BinTree<IExpressPart> expressBinTree)
        {
            CalResult result = new CalResult();

            if (expressBinTree.TreeNode is SetValueSign)
            {
                var partResult = CallResult(expressBinTree.RightTree);
                result.ResultType = partResult.ResultType;
                if (partResult.Results != null)
                {
                    result.Results = partResult.Results;
                }
                else
                {
                    result.Result = partResult.Result;
                }

                var key = (expressBinTree.LeftTree.TreeNode as VarSign).VarName;
                this.CalCurrent.VarDataPool.Add(key, result);
            }
            else if (expressBinTree.TreeNode is OutputValueSign)
            {
                var partResult = CallResult(expressBinTree.RightTree);
                result.ResultType = partResult.ResultType;
                if (partResult.Results != null)
                {
                    result.Results = partResult.Results;
                }
                else
                {
                    result.Result = partResult.Result;
                }
            }
            else
            {
                CallResult(expressBinTree);
            }

            return result;
        }

        private CalResult CallResult(BinTree<IExpressPart> calBinTree)
        {
            if (calBinTree.LeftTree == null && calBinTree.RightTree == null)
            {
                if (calBinTree.TreeNode == null)
                {
                    return new CalResult
                    {
                        Result = ""
                    };
                }
                if (calBinTree.TreeNode is CalExpress)
                {
                    CalExpress ce = calBinTree.TreeNode as CalExpress;

                    return new CalResult
                    {
                        Result = ce.Value,
                        //ResultType = Comm.GetType(ce.Express)
                    };
                }

            }

            if (calBinTree.TreeNode is IIteratorCollectionFun && this.CalCurrent.CurrentIndex != -1)
            {
                var indx = this.CalCurrent.CurrentIndex;
                (calBinTree.TreeNode as CalSign).OnBeginOperator += () =>
                {
                    this.CalCurrent.CurrentIndex = indx;
                };
                this.CalCurrent.CurrentIndex = -1;
            }

            if (!(calBinTree.TreeNode is CalSign))
            {
                throw new ExpressErrorException(string.Format("未能计算的表达式：{0}。", calBinTree.TreeNode));
            }

            var calSign = (calBinTree.TreeNode as CalSign);
            //var time1 = DateTime.Now;

            CalResult leftResult = null;
            CalResult rightResult = null;
            
            if (calBinTree.LeftTree != null)
            {
                //如果左边是赋值的则不调取值方法
                if (calBinTree.TreeNode is SetValueSign
                    || calBinTree.TreeNode is OutputValueSign)
                    ((VarSign)calBinTree.LeftTree.TreeNode).IsSetValue = true;

                leftResult = CallResult(calBinTree.LeftTree);
            }


            if (calBinTree.RightTree != null)
            {
                if (calBinTree.TreeNode is ConditionSign)
                {
                    if (leftResult.Results != null)
                    {
                        rightResult = new CalResult();
                        if (rightResult.Results == null)
                        {
                            rightResult.Results = new object[leftResult.Results.Length];
                        }
                        var lastCurrentIndex = this.CalCurrent.CurrentIndex;
                        for (var i = 0; i < leftResult.Results.Length; i++)
                        {
                            if (lastCurrentIndex == -1)
                            {
                                this.CalCurrent.CurrentIndex = i;
                            }
                            else
                            {
                                i = lastCurrentIndex;
                            }
                            var lr = leftResult.Results[i];
                            if (!(lr is bool))
                            {
                                throw new ExpressErrorException("if then的条件表达式条件必须为bool值！");
                            }
                            CalResult rr = null;
                            if (lr.ToBool())
                            {
                                if (calBinTree.RightTree.TreeNode is ElseSign)
                                {
                                    rr = CallResult(calBinTree.RightTree.LeftTree);
                                }
                                else
                                {
                                    rr = CallResult(calBinTree.RightTree);
                                }
                            }
                            else
                            {
                                if (calBinTree.RightTree.TreeNode is ElseSign)
                                {
                                    rr = CallResult(calBinTree.RightTree.RightTree);
                                }
                                else
                                {
                                    rr = null;
                                }
                            }
                            if (rr != null && rr.Results != null)
                            {
                                throw new Exception("条件表达式右值不能是集合");
                            }
                            rightResult.Results[i] = rr == null ? null : rr.Result;
                            if (lastCurrentIndex > -1)
                            {
                                break;
                            }
                        }
                        if (lastCurrentIndex == -1)
                            this.CalCurrent.CurrentIndex = lastCurrentIndex;
                    }
                    else
                    {
                        if (!(leftResult.Result is bool))
                            throw new ExpressErrorException("if then的条件表达式条件必须为bool值！");

                        if (leftResult.Result.ToBool())
                        {
                            if (calBinTree.RightTree.TreeNode is ElseSign)
                            {
                                rightResult = CallResult(calBinTree.RightTree.LeftTree);
                            }
                            else
                            {
                                rightResult = CallResult(calBinTree.RightTree);
                            }
                        }
                        else
                        {
                            if (calBinTree.RightTree.TreeNode is ElseSign)
                            {
                                rightResult = CallResult(calBinTree.RightTree.RightTree);
                            }
                            else
                            {
                                rightResult = new CalResult
                                {
                                    Result = null,
                                    ResultType = typeof(bool)
                                };
                            }
                        }

                    }

                }
                else if (calBinTree.TreeNode is LoopSign)
                {
                    var sign = calBinTree.TreeNode as LoopSign;
                    leftResult = CallResult(calBinTree.LeftTree);

                    while ((bool)leftResult.Result)
                    {
                        rightResult = CallResult(calBinTree.RightTree);
                        leftResult = CallResult(calBinTree.LeftTree);
                    }
                }
                else if (calBinTree.TreeNode is LoopLeftSign)
                {
                    var sign = calBinTree.TreeNode as LoopLeftSign;
                    leftResult = CallResult(calBinTree.LeftTree);
                    if ((bool)leftResult.Result)
                    {
                        rightResult = CallResult(calBinTree.RightTree);
                    }
                }
                else if (calBinTree.TreeNode is ForSign)
                {
                    //var forsign = calBinTree.TreeNode as ForSign;

                    rightResult = CallResult(calBinTree.RightTree);
                }
                else
                {
                    rightResult = CallResult(calBinTree.RightTree);
                }

            }

            CalSign cs = calBinTree.TreeNode as CalSign;
            cs.LeftVal = leftResult;
            cs.RightVal = rightResult;

            var result = cs.Operate();
            //calSign.ExeTicks += DateTime.Now.Subtract(time1).TotalMilliseconds;
            return result;
        }

#region
        private BinTree<IExpressPart> ResolveExpress(string express,int line)
        {
            return ResolveExpress2(ResolveCalStep(express,line),line);
        }

        private BinTree<IExpressPart> ResolveExpress2(IExpressPart[] ss,int line)
        {
            try
            {

                BinTree<IExpressPart> expressTree = new BinTree<IExpressPart>();

                if (ss.Length == 1 && ss[0] is CalExpress)
                {
                    if (ss[0] is StringSign)
                    {
                        expressTree.TreeNode = ss[0];
                        return expressTree;
                    }

                    IExpressPart[] ios = ResolveCalStep(((CalExpress)ss[0]).Express, line);

                    if (ios == null || ios.Length < 1)
                        return expressTree;

                    if (ios.Length == 1)
                    {
                        if (ios[0] is CalExpress)
                        {
                            expressTree.TreeNode = (CalExpress)ios[0];
                            return expressTree;
                        }
                        else if (ios[0] is VarSign)
                        {
                            expressTree.TreeNode = ios[0];
                            return expressTree;
                        }
                    }
                    else if (ios.Length > 1)
                    {
                        ss = ios;
                    }

                }
                else if (ss.Length == 1 && ss[0] is VarSign)
                {
                    expressTree.TreeNode = ss[0];
                    return expressTree;
                }

                if (ss.Length > 1)
                {
                    var first = ss.Where(s => s is CalSign).OrderBy((o) => (o as CalSign).Priority).FirstOrDefault();
                    if (first == null)
                    {
                        throw new Exception("解析表达式树失败");
                    }
                    int Priority = ((CalSign)first).Priority;
                    CalSign cs = (CalSign)ss.Where(s => s is CalSign).Where(w => (w as CalSign).Priority == Priority).OrderByDescending(o => o.ModeID).Take(1).FirstOrDefault();
                    if (cs != default(CalSign))
                    {
                        expressTree.TreeNode = cs;

                        expressTree.LeftTree = ResolveExpress2(ss.Where(w => w.ModeID < cs.ModeID).ToArray(), line);
                        expressTree.LeftTree.FatherNode = expressTree;
                        expressTree.RightTree = ResolveExpress2(ss.Where(w => w.ModeID > cs.ModeID).ToArray(), line);
                        expressTree.RightTree.FatherNode = expressTree;

                    }
                }

                return expressTree;

            }
            catch (Exception exx)
            {
                throw exx;
            }
        }

        /// <summary>
        /// 分解过程,每次分解一行代码
        /// </summary>
        /// <param name="express"></param>
        /// <returns></returns>
        private IExpressPart[] ResolveCalStep(string express, int line)
        {
            if (string.IsNullOrWhiteSpace(express))
                return null;

            if (express.StartsWith("--"))
            {
                return null;
            }

            //express = new Regex(@";;{1,}").Replace(express, ";");
            //这里有问题，比如((5+3)/2)
            //express = TrimBrackets(express);

            List<IExpressPart> arry = new List<IExpressPart>();
            Stack<string> workstack = new Stack<string>();


            int bracket = 0;
            int pointStart = 0;
            int modelID = 0;

            int ifcount = 0;
            int elsePosit = 0;
            int thenPosit = 0;
            IExpressPart ep = null;

            int forcount = 0;
            int forPosit = 0;
            int stepPosit = 0;
            int toPosit = 0;
            int beginPosit = 0;

            int endcount = 0;

            int expressLen = express.Length;
            var expressCharArray = express.ToArray();
            var isinstring = false;
            for (int i = 0; i <= expressLen; i++)
            {

                if (i == expressLen
                    || (bracket == 0 && i > 0 && !IsCanGroup(expressCharArray[i - 1], expressCharArray[i], ref isinstring))
                    )
                {
                    string es = express.Substring(pointStart, i - pointStart);

                    es = es.TrimBrackets();
                    //检查是否是保留字
                    //if (CalSignFactory.IsProtectWord(es))
                    //{
                    //    throw new ExpressErrorException(es + "是保留字！");
                    //}
                    if (i < expressLen && es[0] == '\'' && expressCharArray[i] == '\'' && ifcount == 0)
                    {
                        i++;
                        StringSign cexp = new StringSign(es.Trim('\''), ++modelID);
                        cexp.StartIndex = pointStart;
                        cexp.EndIndex = i;
                        cexp.CodeLine = line;
                        arry.Add(cexp);
                    }
                    else if (es[0] == '\'' && expressCharArray[i] != '\'' && ifcount == 0)
                    {
                        throw new ExpressErrorException("字符串分析错误");
                    }
                    else if (es.Equals("if", StringComparison.OrdinalIgnoreCase))
                    {
                        if (forcount == 0)
                        {
                            if (ifcount == 0)
                            {
                                ep = new ConditionSign();
                                modelID += 2;
                                ep.CodeLine = line;
                                ep.ModeID = modelID;
                                ep.StartIndex = pointStart;
                                ep.EndIndex = i;
                                arry.Add(ep);
                            }

                            ifcount++;
                        }
                        workstack.Push("if");
                        endcount++;
                    }
                    //else if (es.ToLower() == "then")
                    else if (es.Equals("then", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ifcount == 1)
                        {
                            //先加上左边的条件
                            if (i - 6 - ep.EndIndex <= 0)
                                throw new ExpressErrorException("if缺少条件表达式！", line, ep.EndIndex, i, ep);
                            CalExpress cexp = new CalExpress(express.Substring(ep.EndIndex + 1, i - 6 - ep.EndIndex).TrimBrackets(), ep.ModeID - 1);
                            cexp.StartIndex = ep.EndIndex + 1;
                            cexp.EndIndex = i - 4;
                            cexp.CodeLine = line;
                            arry.Add(cexp);

                            thenPosit = i - 4;
                        }
                    }
                    //else if (es.ToLower() == "else")
                    else if (es.Equals("else", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ifcount == 0 && forcount == 0)
                        {
                            throw new ExpressErrorException("表达式错误，else缺少if条件！", line, elsePosit, i, ep);
                        }

                        if (thenPosit == 0 && forcount == 0)
                        {
                            throw new ExpressErrorException("表达式错误，else前要有then！", line, thenPosit, i, ep);
                        }

                        if (ifcount == 1)
                        {
                            elsePosit = i - 4;
                        }
                    }
                    //else if (es.ToLower() == "end")
                    else if (es.Equals("end", StringComparison.OrdinalIgnoreCase))
                    {
                        if (workstack.Count == 0)
                        {
                            throw new ExpressErrorException("end匹配失败！", line);
                        }
                        endcount--;
                        var word = workstack.Pop();

                        if (endcount == 0 && ifcount > 0)
                        {
                            if (ifcount == 1 && thenPosit == 0)
                                throw new ExpressErrorException("if 表达式错误，缺少then！", line, i, i + 3, ep);

                            if (ifcount == 1)
                            {
                                if (elsePosit > 0)
                                {
                                    modelID += 2;
                                    ElseSign elseSign = new ElseSign();
                                    elseSign.StartIndex = elsePosit;
                                    elseSign.EndIndex = elsePosit + 4;
                                    elseSign.ModeID = modelID;
                                    elseSign.CodeLine = line;
                                    arry.Add(elseSign);

                                    //左边为真条件
                                    if (elsePosit - thenPosit - 6 <= 0)
                                        throw new ExpressErrorException("if else 表达式中缺少条件为真的表达式！", line, thenPosit - 1, thenPosit, ep);

                                    CalExpress cexp0 = new CalExpress(express.Substring(thenPosit + 5, elsePosit - thenPosit - 6).Trim().TrimBrackets(), modelID - 1);
                                    cexp0.StartIndex = thenPosit + 5;
                                    cexp0.EndIndex = elsePosit - thenPosit - 6;
                                    cexp0.CodeLine = line;
                                    arry.Add(cexp0);

                                    if (i - elsePosit - 9 <= 0)
                                        throw new ExpressErrorException("if else 表达式中缺少条件为假的表达式！", line, elsePosit + 4, elsePosit + 5, ep);

                                    //右边为假条件
                                    CalExpress cexp = new CalExpress(express.Substring(elsePosit + 5, i - elsePosit - 9).Trim().TrimBrackets(), ++modelID);
                                    cexp.StartIndex = elsePosit + 5;
                                    cexp.EndIndex = i - 4;
                                    cexp.CodeLine = line;
                                    arry.Add(cexp);
                                }
                                else
                                {
                                    //加上右边的操作
                                    if (i - thenPosit - 9 <= 0)
                                        throw new ExpressErrorException("if表达式中缺少条件表达式！", line, thenPosit + 4, thenPosit + 5, ep);

                                    //右边条件
                                    CalExpress cexp = new CalExpress(express.Substring(thenPosit + 5, i - thenPosit - 9).TrimBrackets(), ++modelID);
                                    cexp.StartIndex = thenPosit + 5;
                                    cexp.EndIndex = i - 4;
                                    arry.Add(cexp);
                                }
                            }
                        }
                        else if (endcount == 0 && forcount > 0)
                        {
                            if (forcount == 1)
                            {
                                if (beginPosit == 0)
                                {
                                    throw new ExpressErrorException("for表达式中缺少begin表达式！", line);
                                }

                                //改写成SET
                                //提取变量
                                var forparam = express.Substring(forPosit + 4, (stepPosit == 0 ? toPosit : stepPosit) - 5).Trim().TrimBrackets();
                                if (forparam.IndexOf(':') == -1)
                                {
                                    throw new ExpressErrorException("for表达式开始必须是一个赋值表达式！", line);
                                }
                                var tempvarname = forparam.Split(':')[0];
                                CalExpress cexpset = new CalExpress(forparam, modelID++);

                                var forsign = new ForSign();
                                forsign.StartIndex = forPosit;
                                forsign.EndIndex = forPosit + 3;
                                forsign.CodeLine = line;
                                forsign.ModeID = modelID++;

                                var limitexp = express.Substring(toPosit + 3, beginPosit - toPosit - 4).Trim().TrimBrackets();
                                var stepexp = "1";
                                if (stepPosit > 0)
                                {
                                    stepexp = express.Substring(stepPosit + 5, toPosit - stepPosit - 6).Trim().TrimBrackets();
                                }
                                var beginexp = express.Substring(beginPosit + 6, i - beginPosit - 10);

                                modelID += 2;
                                LoopLeftSign loopLeftSign = new LoopLeftSign();
                                loopLeftSign.ModeID = modelID;

                                var checkexpress = new CalExpress($"{tempvarname}<={limitexp}", loopLeftSign.ModeID - 1);
                                var doexpress = new CalExpress($"{beginexp}", loopLeftSign.ModeID + 1);

                                LoopSign loopSign = new LoopSign();
                                modelID += 2;
                                loopSign.ModeID = modelID;

                                modelID++;
                                //CalExpress condition = new CalExpress($"if {tempvarname}<={limitexp} then {beginexp} else {tempvarname}:{tempvarname}+{stepexp} end", modelID++);
                                var loopright = new CalExpress($"{tempvarname}:={tempvarname}+{stepexp}", modelID);

                                arry.Clear();
                                arry.Add(cexpset);
                                arry.Add(forsign);
                                arry.Add(loopSign);
                                arry.Add(loopLeftSign);
                                arry.Add(checkexpress);
                                arry.Add(doexpress);
                                arry.Add(loopright);
                            }
                        }
                        if (word == "if" && ifcount > 0) ifcount--;
                        if (word == "for" && forcount > 0) forcount--;
                    }
                    else if (es.Equals("while", StringComparison.OrdinalIgnoreCase))
                    {

                    }
                    else if (es.Equals("do", StringComparison.OrdinalIgnoreCase))
                    {

                    }
                    else if (es.Equals("endwhile", StringComparison.OrdinalIgnoreCase))
                    {

                    }
                    else if (es.Equals("for", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ifcount == 0)
                        {
                            if (forcount == 0)
                            {
                                forPosit = i - 3;
                            }

                            forcount++;
                        }
                        workstack.Push("for");
                        endcount++;
                    }
                    else if (es.Equals("step", StringComparison.OrdinalIgnoreCase))
                    {

                        if (forcount == 0 && ifcount == 0)
                        {
                            throw new ExpressErrorException("表达式错误，step缺少for条件！", line);
                        }

                        if (toPosit > 0 && ifcount == 0)
                        {
                            throw new ExpressErrorException("表达式错误，step要在to条件之前！", line);
                        }

                        if (forcount == 1)
                        {
                            stepPosit = i - 4;
                        }
                    }
                    else if (es.Equals("to", StringComparison.OrdinalIgnoreCase))
                    {
                        if (forcount == 0 && ifcount == 0)
                        {
                            throw new ExpressErrorException("表达式错误，to缺少for条件！", line);
                        }

                        if (forcount == 1)
                        {
                            toPosit = i - 2;
                        }
                    }
                    else if (es.Equals("begin", StringComparison.OrdinalIgnoreCase))
                    {
                        if (forcount == 0 && ifcount == 0)
                        {
                            throw new ExpressErrorException("表达式错误，begin缺少for条件！", line, elsePosit, i, ep);
                        }

                        if (toPosit == 0 && ifcount == 0)
                        {
                            throw new ExpressErrorException("表达式错误，for begin缺少to条件！", line, elsePosit, i, ep);
                        }

                        beginPosit = i - 5;
                    }
                    else if (!string.IsNullOrWhiteSpace(es) && es != ";" && ifcount == 0)
                    {
                        CalSign cs;
                        if (CalSignFactory.TryCreateSign(es, this.CalCurrent, out cs))
                        {
                            cs.StartIndex = pointStart;
                            cs.EndIndex = i;
                            if (cs is FunSign)
                            {
                                FunSign fs = cs as FunSign;

                                arry.Add(new CalExpress(fs.FunName, ++modelID));
                                fs.ModeID = ++modelID;
                                arry.Add(fs);
                                if (!string.IsNullOrWhiteSpace(fs.ParamString))
                                    arry.Add(new CalExpress(fs.ParamString, ++modelID));

                            }
                            else
                            {
                                cs.ModeID = ++modelID;
                                arry.Add(cs);
                            }
                        }
                        else
                        {
                            CalExpress cexp = new CalExpress(es, ++modelID);
                            cexp.StartIndex = pointStart;
                            cexp.EndIndex = i;
                            arry.Add(cexp);
                        }

                    }

                    pointStart = i;
                }



                if (i < expressLen)
                {
                    if (!isinstring)
                    {
                        if (expressCharArray[i] == '(')
                            bracket++;
                        else if (expressCharArray[i] == ')')
                            bracket--;
                    }
                }

                if (i == expressLen && bracket > 0)
                {
                    throw new ExpressErrorException("表达式错误，缺少右)！", line, expressLen, expressLen + 1, ep);
                }

                if (i == expressLen && ifcount > 0)
                {
                    throw new ExpressErrorException("if表达式错误！", line, expressLen, expressLen + 1, ep);
                }

                if (i == expressLen && endcount != 0)
                {
                    throw new ExpressErrorException("end表达式错误！", line, expressLen, expressLen + 1, ep);
                }

            }

            return arry.OrderBy(c => c.ModeID).ToArray();
        }

        //要考虑负数的
        private static bool IsCanGroup(char first, char second,ref bool isinstring)
        {
            if (first == '\'')
            {
                if (!isinstring)
                {
                    isinstring = true;
                }
                return true;
            }

            if (second == '\'')
            {
                if (first == '\\')
                {
                    return true;
                }
                isinstring = false;
                return false;
            }


            if (isinstring)
            {
                return true;
            }

            if (second == ' ')
                return false;

            if (second == ';' || second == ',')
                return false;

            
            //string s = (first + second).ToString().Trim();

            //if (s.Length == 0)
            //    return true;
            if (first == ' ' && second == ' ')
                return true;


            //if (s.Length == 1)
            //    return false;
            if (first == ' ' || second == ' ')
                return false;

            //if (RegexCharAndNum.IsMatch(s))
            //    return true;
            if (first.IsCharOrNum() && second.IsCharOrNum())
                return true;


            //if (RegexPlusMinusMultiDivided.IsMatch(second.ToString()))
            //    return false;
            if (second == '+' || second == '-' || second == '*' || second == '/')
                return false;

            //if (s.Equals(":="))
            //    return true;
            if (first == ':' && second == '=')
                return true;

            //if (RegexCharOrNumAndLeftBrackets.IsMatch(s))
            //    return true;
            if (first.IsCharOrNum() && second == '(')
                return true;

            //if (RegexNumNum.IsMatch(s))
            //    return true;
            if ((first.IsNum() && second == '.') || (first == '.' && second.IsNum()))
                return true;

            //if (RegexBigerOrSmallEq.IsMatch(s))
            //    return true;
            if ((first == '>' || first == '<') && second == '=')
                return true;

            //if (RegexNumCharAndSign.IsMatch(s))
            //    return false;
            if ((first.IsCharOrNum() && (second == '>' || second == '<' || second == '='))
                || (second.IsCharOrNum() && (first == '>' || first == '<' || first == '=')))
            {
                return false;
            }

            return false;
        }

#endregion

    }
}
