using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using LJC.FrameWork.Comm.SNLP;

namespace LJC.FrameWork.Comm.SNLP
{
    public class NLPHelper
    {
        private static readonly HashSet<char> BiaoDianHash = new HashSet<char>(new char[] { ',', ';', '。', '?', '!' });
        // use shared LruCache from Comm namespace (moved to LJC.FrameWork.Comm.LruCache.cs)

        // LRU cache for suffix automatons per target to avoid unbounded memory growth
        // LRU cache: use bytes-based capacity (e.g., 16 MB)
        private const long SamCacheCapacityBytes = 16L * 1024 * 1024;
        private static readonly LruCache<string, SuffixAutomaton> SamCache = new LruCache<string, SuffixAutomaton>(SamCacheCapacityBytes);
        // per-target build locks to avoid duplicate concurrent construction
        private static readonly ConcurrentDictionary<string, object> SamBuildLocks = new ConcurrentDictionary<string, object>();

        // Suffix Automaton implementation for target string using compact transition storage
        private class SuffixAutomaton
        {
            public class State
            {
                public int Len;
                public int Link;
                public int Head; // index to first edge in edges arrays, -1 if none
                public int FirstPos; // position in original string where this state's longest substring ends
            }

            private List<State> st = new List<State>();
            private int last;

            // per-state sparse transitions used when not using dense; switched to dictionary for faster lookup
            // we keep compact arrays during construction for efficient cloning, but maintain per-state dictionaries
            private List<char> edgeChar = new List<char>();
            private List<int> edgeTo = new List<int>();
            private List<int> edgeNext = new List<int>();
            private List<Dictionary<char, int>> stateTrans = new List<Dictionary<char, int>>();

            // dense transitions for small alphabets
            private Dictionary<char, int> charToIdx;
            private int[][] denseTrans; // [state][charIndex] -> dest state or -1
            // threshold to decide whether to build dense transition table (protect against huge allocations)
            private const long DenseMemoryThresholdBytes = 4 * 1024 * 1024; // 4 MB

            public SuffixAutomaton(string s)
            {
                // determine unique chars
                s = s ?? string.Empty;
                var uniq = new HashSet<char>(s);
                // conservative dense decision: only enable if estimated memory <= threshold
                // estimated bytes ~ states_estimate(<=2*len) * alpha * 4 ~= 8 * len * alpha
                bool useDense = uniq.Count > 0 && uniq.Count <= 128 && (8L * s.Length * uniq.Count) <= DenseMemoryThresholdBytes;
                if (useDense)
                {
                    // build mapping
                    charToIdx = new Dictionary<char, int>(uniq.Count);
                    int idx = 0;
                    foreach (var ch in uniq)
                    {
                        charToIdx[ch] = idx++;
                    }
                }

                // initialize
                st.Add(new State { Len = 0, Link = -1, Head = -1, FirstPos = -1 });
                stateTrans.Add(new Dictionary<char, int>());
                last = 0;
                for (int i = 0; i < (s?.Length ?? 0); i++)
                {
                    Extend(s[i], i);
                }

                // if using dense, build dense table once and free compact lists to reduce memory
                if (charToIdx != null)
                {
                    BuildDenseTrans();
                    // free compact edge lists to allow GC reclaiming large temporary lists
                    edgeChar = null;
                    edgeTo = null;
                    edgeNext = null;
                }
            }

            private void BuildDenseTrans()
            {
                int nStates = st.Count;
                int alpha = charToIdx.Count;
                denseTrans = new int[nStates][];
                for (int i = 0; i < nStates; i++)
                {
                    var arr = new int[alpha];
                    for (int j = 0; j < alpha; j++) arr[j] = -1;
                    denseTrans[i] = arr;
                }

                for (int from = 0; from < nStates; from++)
                {
                    // prefer per-state dictionary if available
                    if (from < stateTrans.Count && stateTrans[from] != null)
                    {
                        foreach (var kv in stateTrans[from])
                        {
                            if (charToIdx.TryGetValue(kv.Key, out int ci))
                            {
                                denseTrans[from][ci] = kv.Value;
                            }
                        }
                    }
                    else
                    {
                        int e = st[from].Head;
                        while (e != -1)
                        {
                            // edge lists are present during construction
                            char c = edgeChar[e];
                            if (charToIdx.TryGetValue(c, out int ci))
                            {
                                denseTrans[from][ci] = edgeTo[e];
                            }
                            e = edgeNext[e];
                        }
                    }
                }

                // free compact edge lists to reduce memory if needed (keep for safety)
            }

            private int AddEdge(int from, char c, int to)
            {
                // compact representation used during construction
                int idx = edgeChar.Count;
                edgeChar.Add(c);
                edgeTo.Add(to);
                edgeNext.Add(st[from].Head);
                st[from].Head = idx;
                // maintain per-state dictionary for fast lookup
                var trans = stateTrans[from];
                trans[c] = to;
                return idx;
            }

            // return edge index or -1
            private int GetEdgeIndex(int from, char c)
            {
                // if compact lists are gone, no index-based access available
                if (edgeChar == null) return -1;
                // prefer dictionary lookup
                Dictionary<char, int> trans = null;
                if (from < stateTrans.Count) trans = stateTrans[from];
                if (trans != null && trans.TryGetValue(c, out int dest))
                {
                    // need to find edge index corresponding to dest; fall back to scanning
                    int e = st[from].Head;
                    while (e != -1)
                    {
                        if (edgeChar[e] == c && edgeTo[e] == dest) return e;
                        e = edgeNext[e];
                    }
                }
                return -1;
            }

            private int GetEdgeDest(int from, char c)
            {
                if (denseTrans != null)
                {
                    if (!charToIdx.TryGetValue(c, out int ci)) return -1;
                    return denseTrans[from][ci];
                }

                // try per-state dictionary first
                Dictionary<char, int> trans = null;
                if (from < stateTrans.Count) trans = stateTrans[from];
                if (trans != null && trans.TryGetValue(c, out int destState)) return destState;

                if (edgeChar == null) return -1;
                int idx = GetEdgeIndex(from, c);
                return idx == -1 ? -1 : edgeTo[idx];
            }

            private void SetEdgeDestByIndex(int edgeIndex, int dest)
            {
                edgeTo[edgeIndex] = dest;
                if (denseTrans != null)
                {
                    char c = edgeChar[edgeIndex];
                    if (charToIdx.TryGetValue(c, out int ci))
                    {
                        // find from state: need to locate which state's chain contains this edge
                        // to avoid expensive reverse mapping, skip updating dense here; dense built after construction only
                    }
                }
            }

            private void CloneEdgesTo(int srcStateIndex, int dstStateIndex)
            {
                int e = st[srcStateIndex].Head;
                // iterate edges and add duplicates to dst in same order
                var temp = new List<int>();
                while (e != -1)
                {
                    temp.Add(e);
                    e = edgeNext[e];
                }
                for (int i = temp.Count - 1; i >= 0; i--)
                {
                    int ei = temp[i];
                    AddEdge(dstStateIndex, edgeChar[ei], edgeTo[ei]);
                }
                // clone per-state dictionary if exists
                Dictionary<char, int> srcTrans = null;
                if (srcStateIndex < stateTrans.Count) srcTrans = stateTrans[srcStateIndex];
                if (srcTrans != null)
                {
                    var newDict = new Dictionary<char, int>(srcTrans);
                    // ensure list capacity
                    while (stateTrans.Count <= dstStateIndex) stateTrans.Add(null);
                    stateTrans[dstStateIndex] = newDict;
                }
            }

            private void Extend(char c, int pos)
            {
                int cur = st.Count;
                st.Add(new State { Len = st[last].Len + 1, Link = 0, Head = -1, FirstPos = pos });
                // ensure stateTrans entry for the new state
                stateTrans.Add(new Dictionary<char, int>());
                int p = last;
                while (p != -1 && GetEdgeDest(p, c) == -1)
                {
                    AddEdge(p, c, cur);
                    p = st[p].Link;
                }
                if (p == -1)
                {
                    st[cur].Link = 0;
                }
                else
                {
                    int q = GetEdgeDest(p, c);
                    if (st[p].Len + 1 == st[q].Len)
                    {
                        st[cur].Link = q;
                    }
                    else
                    {
                        int clone = st.Count;
                        st.Add(new State { Len = st[p].Len + 1, Link = st[q].Link, Head = -1, FirstPos = st[q].FirstPos });
                        stateTrans.Add(new Dictionary<char, int>());
                        // copy edges of q to clone
                        CloneEdgesTo(q, clone);

                        // redirect edges that pointed to q on char c to clone
                        while (p != -1)
                        {
                            int ei = GetEdgeIndex(p, c);
                            if (ei == -1 || edgeTo[ei] != q) break;
                            SetEdgeDestByIndex(ei, clone);
                            // update per-state dictionary if present
                            var transp = stateTrans[p];
                            if (transp != null && transp.TryGetValue(c, out int dest) && dest == q)
                            {
                                transp[c] = clone;
                            }
                            p = st[p].Link;
                        }

                        st[q].Link = st[cur].Link = clone;
                        // update stateTrans for cur to reflect existing transitions moved via clone (if any)
                        if (stateTrans.Count > cur)
                        {
                            var curTrans = stateTrans[cur];
                            if (curTrans != null)
                            {
                                // no immediate action needed; cloneEdges already copied
                            }
                        }
                    }
                }
                last = cur;
            }

            public int StatesCount => st.Count;

            // Longest match ending at each position of source
            // Returns arrays: matchLenEndingAt[i] = length of longest substring of target that ends at source index i
            // and endStateIndex[i] = index of the automaton state for that match (can be used to derive target end pos)
            public void LongestMatchesEndingAt(string source, out int[] matchLenEndingAt, out int[] endStateIndex)
            {
                int n = source.Length;
                matchLenEndingAt = new int[n];
                endStateIndex = new int[n];

                int v = 0; // current state
                int l = 0; // current matched length
                for (int i = 0; i < n; i++)
                {
                    char c = source[i];
                    int dest = GetEdgeDest(v, c);
                    if (dest != -1)
                    {
                        v = dest;
                        l++;
                    }
                    else
                    {
                        while (v != -1 && GetEdgeDest(v, c) == -1)
                        {
                            v = st[v].Link;
                        }
                        if (v == -1)
                        {
                            v = 0;
                            l = 0;
                        }
                        else
                        {
                            l = st[v].Len + 1;
                            v = GetEdgeDest(v, c);
                        }
                    }

                    matchLenEndingAt[i] = l;
                    endStateIndex[i] = v;
                }
            }

            public int GetStateFirstPos(int stateIndex)
            {
                if (stateIndex >= 0 && stateIndex < st.Count)
                {
                    return st[stateIndex].FirstPos;
                }
                return -1;
            }
        }

        private static List<NLPCompareDetail> BestCompare(string src, int srcStart, string target, Dictionary<char, List<int>> dic
            , NLPCompareOptions options)
        {
            var list = new List<NLPCompareDetail>();
            if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(target) || options == null)
            {
                return list;
            }

            var srcLen = src.Length;

            // Build or get suffix automaton for target from LRU cache with per-key build lock to avoid duplicate builds
            SuffixAutomaton sam;
            bool shouldCache = target.Length <= 20000; // avoid caching extremely large automata
            if (!SamCache.TryGet(target, out sam))
            {
                var keyLock = SamBuildLocks.GetOrAdd(target, _ => new object());
                lock (keyLock)
                {
                    // double-check
                    if (!SamCache.TryGet(target, out sam))
                    {
                        sam = new SuffixAutomaton(target);
                        if (shouldCache)
                        {
                            try
                            {
                                // estimate size: states * average transition size
                                long estimatedSize = EstimateSamSizeBytesLocal();
                                SamCache.Add(target, sam, estimatedSize);
                            }
                            catch
                            {
                                // ignore cache add errors
                            }
                        }
                    }
                }
                // remove build lock to avoid unbounded growth
                SamBuildLocks.TryRemove(target, out _);
            }

            // compute longest match ending at each position of src
            sam.LongestMatchesEndingAt(src, out var matchLenEndingAt, out var endStateIndex);

            // helper to estimate SAM size in bytes
            long EstimateSamSizeBytesLocal()
            {
                try
                {
                    int states = sam.StatesCount;
                    long trans = 0;
                    var field = typeof(SuffixAutomaton).GetField("stateTrans", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        var stList = field.GetValue(sam) as System.Collections.IList;
                        if (stList != null)
                        {
                            for (int i = 0; i < stList.Count; i++)
                            {
                                var dict = stList[i] as System.Collections.IDictionary;
                                if (dict != null) trans += dict.Count;
                            }
                        }
                    }
                    if (trans == 0) trans = Math.Max(1, states);
                    long size = states * 24L + trans * 24L + target.Length * 2L;
                    return size;
                }
                catch
                {
                    return Math.Max(1024, target.Length * 2);
                }
            }

            // Collect maximal matches (for each end position, take its maximal length)
            for (int end = 0; end < srcLen; end++)
            {
                // timeout support
                if (options.TimeOutMills > 0)
                {
                    var elapsed = (DateTime.Now - options.BeinDt).TotalMilliseconds;
                    if (elapsed > options.TimeOutMills)
                    {
                        break;
                    }
                }

                int len = matchLenEndingAt[end];
                if (len < options.CompareMinLen)
                {
                    continue;
                }

                int start = end - len + 1;
                if (start < 0)
                {
                    continue;
                }

                // skip if starting char is punctuation
                if (BiaoDianHash.Contains(src[start]))
                {
                    continue;
                }

                int stateIdx = endStateIndex[end];
                int targetEndPos = sam.GetStateFirstPos(stateIdx);
                if (targetEndPos < 0)
                {
                    continue;
                }
                int targetStart = targetEndPos - len + 1;
                if (targetStart < 0)
                {
                    continue;
                }

                list.Add(new NLPCompareDetail
                {
                    SrcStart = start,
                    TargetStart = targetStart,
                    Len = len
                });
            }

            if (list.Count == 0)
            {
                return list;
            }

            // remove duplicates and conflicting matches similar to original logic
            list.Sort((a, b) => b.Len.CompareTo(a.Len));

            var resultList = new List<NLPCompareDetail>(list.Count);
            foreach (var item in list)
            {
                bool conflict = false;
                for (int j = 0; j < resultList.Count; j++)
                {
                    var p = resultList[j];

                    bool srcOverlap = !(p.SrcStart >= item.SrcStart + item.Len - 1 || p.SrcStart + p.Len - 1 <= item.SrcStart);
                    bool targetOverlap = !(p.TargetStart >= item.TargetStart + item.Len - 1 || p.TargetStart + p.Len - 1 <= item.TargetStart);
                    bool orderMismatch = !((p.SrcStart > item.SrcStart && p.TargetStart > item.TargetStart) || (p.SrcStart < item.SrcStart && p.TargetStart < item.TargetStart));

                    if ((srcOverlap && targetOverlap) || (!orderMismatch && (srcOverlap || targetOverlap)))
                    {
                        conflict = true;
                        break;
                    }
                }

                if (!conflict)
                {
                    resultList.Add(item);
                }
            }

            resultList.Sort((a, b) => a.SrcStart.CompareTo(b.SrcStart));
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
            if (options == null)
            {
                options = new NLPCompareOptions();
            }

            options.BeinDt = DateTime.Now;

            var dic = new Dictionary<char, List<int>>();
            result.NLPCompareDetails = BestCompare(src, 0, target, dic, options);

            result.UseMills = (int)(DateTime.Now - options.BeinDt).TotalMilliseconds;
            return result;
        }

        /// <summary>
        /// 计算文本相似度
        /// </summary>
        public static CalcSimilarityResult CalcSimilarity(string src, string target, CalcSimilarityOption option)
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
                val = Math.Round(compareResult.NLPCompareDetails.Sum(p => p.Len) * 100.0 / maxLen, 2);
                result.CalcSimilarityValue = val;
                result.UseMills = compareResult.UseMills;
            }

            return result;
        }
    }
}
