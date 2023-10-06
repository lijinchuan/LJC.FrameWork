using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm.SNLP
{
    public class NLPHelper
    {
        private static HashSet<Char> BiaoDianHash = new HashSet<char>(new char[] { ',',';','。','?','!'});
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

        private static int MatchFrom(string src, int srcStart, string target, int targetStart)
        {
            if (srcStart > 0 && targetStart > 0)
            {
                if (src[srcStart - 1] == target[targetStart - 1])
                {
                    return 1;
                }
            }

            var start = srcStart;
            var srclen = src.Length;
            var targetlen = target.Length;

            while (srcStart < srclen && targetStart < targetlen)
            {
                if (src[srcStart] != target[targetStart])
                {
                    break;
                }
                srcStart++;
                targetStart++;

            }

            return srcStart - start;
        }


        private static List<NLPCompareDetail> BestCompare(string src, int srcStart, string target, Dictionary<char, HashSet<int>> dic
            , NLPCompareOptions options)
        {
            HashSet<int> hash;

            List<NLPCompareDetail> list = new List<NLPCompareDetail>();
            for(var i = srcStart; i < src.Length; i++)
            {
                var ch = src[i];
                if (BiaoDianHash.Contains(ch))
                {
                    continue;
                }
                if(dic.TryGetValue(ch,out hash))
                {
                    foreach(var hs in hash)
                    {
                        var len = MatchFrom(src, i, target, hs);
                        if (len >= options.CompareMinLen)
                        {
                            list.Add(new NLPCompareDetail
                            {
                                SrcStart=i,
                                TargetStart=hs,
                                Len=len
                            });
                        }
                    }
                }
            }
            var ordeList= list.OrderByDescending(p=>p.Len).ToList();
            var resultList = new List<NLPCompareDetail>();
            foreach (var item in ordeList)
            {
                if (resultList.All(p => (p.SrcStart >= item.SrcStart+item.Len || p.SrcStart + p.Len <= item.SrcStart)
                &&(p.TargetStart >= item.TargetStart + item.Len || p.TargetStart + p.Len <= item.TargetStart)))
                {
                    resultList.Add(item);
                }
            }
            return resultList;
        }

        /// <summary>
        /// 比较两个字符串相同的部分，返回接近的最佳匹配结果
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static NLPCompareResult NLPCompare(string src, string target, NLPCompareOptions options)
        {
            var result = new NLPCompareResult();
            options.BeinDt = DateTime.Now;
            var srcLen = src.Length;
            var targetLen = target.Length;
            var longText = srcLen > targetLen ? src : target;
            var shortText = srcLen > targetLen ? target : src;

            result.NLPCompareDetails = BestCompare(shortText, 0, longText, MakeDic(longText),options);

            result.UseMills = (int)DateTime.Now.Subtract(options.BeinDt).TotalMilliseconds;
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
                result.UseMills = compareResult.UseMills;
            }
            
            return result;
        }
    }
}
