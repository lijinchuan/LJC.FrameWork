using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm.SNLP
{
    public class NLPHelper
    {
        private static Dictionary<char,HashSet<int>> MakeDic(string source)
        {
            var dic = new Dictionary<char, HashSet<int>>();
            var pos = 0;
            foreach(var ch in source)
            {
                HashSet<int> hashSet;
                if (!dic.TryGetValue(ch, out hashSet))
                {
                    hashSet = new HashSet<int>();
                    dic.Add(ch, hashSet);
                }
                hashSet.Add(pos++);
            }
            return dic;
        }

        private static int MatchFrom(string src,int srcStart, Dictionary<char, HashSet<int>> dicTarget, int targetStart)
        {
            var len = 0;
            for(var i = srcStart; i < src.Length; i++)
            {
                var ch = src[i];
                if (dicTarget.ContainsKey(ch) && dicTarget[ch].Contains(targetStart + len))
                {
                    len++;
                }
                else
                {
                    break;
                }
            }
            return len;
        }

        private static List<NLPCompareDetail> BestCompare(string src,int srcStart,string target,int targetStart,
            Dictionary<char,HashSet<int>> dic,
            List<NLPCompareDetail> PreNLPCompareDetails,
            NLPCompareOptions options,
            Dictionary<string, List<NLPCompareDetail>> repeatDic)
        {
            if (PreNLPCompareDetails == null)
            {
                PreNLPCompareDetails = new List<NLPCompareDetail>();
            }
            else
            {
                if (DateTime.Now.Subtract(options.BeinDt).TotalMilliseconds > options.TimeOutMills)
                {
                    throw new TimeoutException();
                }
            }
            if (repeatDic == null)
            {
                repeatDic = new Dictionary<string, List<NLPCompareDetail>>();
                options.RepeatDic = repeatDic;
            }
            var srcLen = src.Length;
            HashSet<int> hashSet;
            var targetLen = target.Length;
            var preSumLen = PreNLPCompareDetails.Sum(p => p.Len);
            for (var i = srcStart; i < srcLen; i++)
            {
                var ch = src[i];
                
                if(dic.TryGetValue(ch,out hashSet))
                {
                    var maxLen = 0;
                    List<NLPCompareDetail> maxTempListLi = default;
                    foreach (var hs in hashSet)
                    {
                        var key = i + "_" + hs;
                        if (repeatDic.ContainsKey(key))
                        {
                            if (repeatDic[key].Sum(p => p.Len) + preSumLen <= maxLen)
                            {
                                continue;
                            }
                        }

                        var len = MatchFrom(src, i, dic, hs);
                        if (len < options.CompareMinLen)
                        {
                            continue;
                        }

                        List<NLPCompareDetail> tempDetails = null;
                        if (hs < targetStart)
                        {
                            tempDetails = new List<NLPCompareDetail>();
                            foreach (var item in PreNLPCompareDetails)
                            {
                                if (item.TargetStart < hs)
                                {
                                    tempDetails.Add(item);
                                }
                            }
                        }
                        else
                        {
                            tempDetails = PreNLPCompareDetails.ToList();
                        }

                        if (repeatDic.ContainsKey(key))
                        {
                            tempDetails.AddRange(repeatDic[key]);
                        }
                        else
                        {
                            
                            tempDetails.Add(new NLPCompareDetail
                            {
                                Len = len,
                                SrcStart = i,
                                TargetStart = hs
                            });

                            tempDetails = BestCompare(src, i + len, target, hs + len, dic, tempDetails,options, repeatDic);
                            var tempDetailsMore = tempDetails.ToList();
                            tempDetailsMore.RemoveAll(p => p.TargetStart < hs);
                            repeatDic.Add(key, tempDetailsMore);
                        }
                        var tempLen = tempDetails.Sum(p => p.Len);
                        if (tempLen > maxLen)
                        {
                            maxLen = tempLen;
                            maxTempListLi = tempDetails;
                        }
                    }
                    if (maxTempListLi==default)
                    {
                        continue;
                    }
                    
                    PreNLPCompareDetails = maxTempListLi;
                    break;
                }

            }
            return PreNLPCompareDetails;
            
        }

        /// <summary>
        /// 比较两个字符串相同的部分，返回接近的最佳匹配结果
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static NLPCompareResult NLPCompare(string src,string target,NLPCompareOptions options)
        {
            var result = new NLPCompareResult();
            options.BeinDt = DateTime.Now;
            var srcLen = src.Length;
            var targetLen = target.Length;
            var longText = srcLen > targetLen ? src : target;
            var shortText = srcLen > targetLen ? target : src;
            result.NLPCompareDetails = BestCompare(shortText, 0, longText, 0, MakeDic(longText), null,options,null);

            return result;
        }

        /// <summary>
        /// 计算文本相似度
        /// </summary>
        public static CalcSimilarityResult CalcSimilarity(string src,string target,CalcSimilarityOption option)
        {
            var result = new CalcSimilarityResult();

            var srcLen = src.Length;
            var targetLen = target.Length;
            var maxLen = Math.Max(srcLen, targetLen);
            var val = Math.Min(srcLen, targetLen) * 100.0 / maxLen;
            if (option.ExpectSimilarity <= val)
            {
                if (option.CompareOptions == null)
                {
                    option.CompareOptions = new NLPCompareOptions();
                }
                var compareResult = NLPCompare(src, target, option.CompareOptions);
                val =Math.Round(compareResult.NLPCompareDetails.Sum(p => p.Len) * 100.0 / maxLen,2);
                result.CalcSimilarityValue = val;
            }
            
            return result;
        }
    }
}
