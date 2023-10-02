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
            List<NLPCompareDetail> PreNLPCompareDetails)
        {
            if (PreNLPCompareDetails == null)
            {
                PreNLPCompareDetails = new List<NLPCompareDetail>();
            }
            var srcLen = src.Length;
            for(var i = srcStart; i < srcLen; i++)
            {
                var ch = src[i];
                HashSet<int> hashSet;
                if(dic.TryGetValue(ch,out hashSet))
                {
                    var maxLen = 0;
                    List<NLPCompareDetail> maxTempListLi = default;
                    foreach (var hs in hashSet)
                    {
                        var len = MatchFrom(src, i, dic, hs);
                        if (len < 4)
                        {
                            continue;
                        }
                        var tempDetails = PreNLPCompareDetails.ToList();
                       
                        if (hs < targetStart)
                        {
                            tempDetails.RemoveAll(p => p.TargetStart >= hs);
                        }
                        tempDetails.Add(new NLPCompareDetail
                        {
                            Len = len,
                            SrcStart = i,
                            TargetStart = hs
                        });
                        
                        tempDetails = BestCompare(src, i + len, target, hs + len, dic, tempDetails);
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

        public static NLPCompareResult NLPCompare(string src,string target)
        {
            var result = new NLPCompareResult();
            result.NLPCompareDetails = BestCompare(src, 0, target, 0, MakeDic(target), null);

            return result;
        }
    }
}
