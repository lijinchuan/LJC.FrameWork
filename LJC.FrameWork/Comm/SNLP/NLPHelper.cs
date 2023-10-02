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
            if (DateTime.Now.Subtract(options.BeinDt).TotalMilliseconds > options.TimeOutMills)
            {
                throw new TimeoutException();
            }
            if (PreNLPCompareDetails == null)
            {
                PreNLPCompareDetails = new List<NLPCompareDetail>();
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

        public static NLPCompareResult NLPCompare(string src,string target,NLPCompareOptions options)
        {
            var result = new NLPCompareResult();
            options.BeinDt = DateTime.Now;
            result.NLPCompareDetails = BestCompare(src, 0, target, 0, MakeDic(target), null,options,null);

            return result;
        }
    }
}
