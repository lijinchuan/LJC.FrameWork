using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.CodeExpression.KeyWordMatch
{
    internal class KeyWordANF
    {
        private ReaderWriterLockSlim dicLock = new ReaderWriterLockSlim();
        //private KeyWordDic[] dics = new KeyWordDic[char.MaxValue + 1];
        private KeyWordDic[] dics = new KeyWordDic[char.MaxValue + 1];
        private static Dictionary<string, object> DicTag = new Dictionary<string, object>();

        public void AddKeyWord(string word,object tag=null)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;

            try
            {
                dicLock.EnterWriteLock();
                word = word.Trim();

                if (tag != null && !DicTag.ContainsKey(word))
                {
                    DicTag.Add(word, tag);
                }

                var dic = dics[word[0]];
                if (dic == null)
                    dic = new KeyWordDic(word[0]);

                if (word.Length == 1)
                {
                    dic.HasEndChar = true;
                    return;
                }

                KeyWordDic innerDic = null;

                if (dic.ContainsKey(word[1]))
                {
                    innerDic = dic[word[1]];
                }
                else
                {
                    innerDic = dic.Add(word[1]);
                }

                for (int i = 2; i < word.Length; i++)
                {
                    var ch = word[i];
                    if (!innerDic.ContainsKey(ch))
                    {
                        innerDic = innerDic.Add(ch);
                    }
                    else
                    {
                        innerDic = innerDic[ch];
                    }
                }

                innerDic.HasEndChar = true;
                dics[word[0]] = dic;
            }
            finally
            {
                dicLock.ExitWriteLock();
            }
        }

        public void RemoveKeyWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;

            try
            {
                dicLock.EnterWriteLock();

                var dic = dics[word[0]];
                if (dic == null)
                    return;

                if(DicTag.ContainsKey(word))
                {
                    DicTag.Remove(word);
                }

                if (!dic.ContainsKey(word[0]))
                {
                    return;
                }

                var innerDic = dic[word[0]];
                KeyWordDic dicRemove = dic;
                char removeKey = word[0];

                for (int i = 1; i < word.Length; i++)
                {
                    var ch = word[i];
                    if (!innerDic.ContainsKey(ch))
                    {
                        return;
                    }

                    if (innerDic.Count > 1)
                    {
                        removeKey = ch;
                        dicRemove = innerDic;
                    }

                    innerDic = innerDic[ch];
                }


                //检查是否结束了
                if (innerDic.Keys().Count > 0 && innerDic.HasEndChar)
                {
                    if (innerDic.Keys().Count == 1)
                        dicRemove.Remove(removeKey);
                    else
                        innerDic.HasEndChar = false;
                }

            }
            finally
            {
                dicLock.ExitWriteLock();
            }
        }

        //private KeyWordANFStatus CheckChar(char chr, ref KeyWordDic lastDic)
        //{
        //    if (lastDic != null)
        //    {
        //        bool isacc = lastDic.HasEndChar;
        //        if (lastDic.TryGetValue(chr, out lastDic))
        //        {
        //            if (lastDic.HasEndChar)
        //            {

        //                lastDic = null;

        //                if (isacc)
        //                    return KeyWordANFStatus.accept2;

        //                return KeyWordANFStatus.accept;
        //            }
        //            else
        //            {
        //                if (isacc)
        //                    return KeyWordANFStatus.acceptwating;

        //                return KeyWordANFStatus.wating;
        //            }
        //        }

        //        if (isacc)
        //            return KeyWordANFStatus.accept1;
        //    }

        //    if (dics[chr] != null)
        //    {
        //        lastDic = dics[chr];
        //    }

        //    return KeyWordANFStatus.abort;

        //}

        private KeyWordANFStatus CheckChar(char chr, ref KeyWordDic lastDic, KeyWordANFStatus lastStatu)
        {
            if (lastDic != null)
            {
                bool isacc = lastStatu != KeyWordANFStatus.accept && lastDic.HasEndChar;
                if (lastDic.TryGetValue(chr, out lastDic))
                {
                    if (lastDic.HasEndChar)
                    {

                        //lastDic = null;

                        if (isacc)
                        {
                            //lastDic = null;
                            return KeyWordANFStatus.accept2;
                        }

                        return KeyWordANFStatus.accept;
                    }
                    else
                    {
                        if (isacc)
                            return KeyWordANFStatus.acceptwating;

                        return KeyWordANFStatus.wating;
                    }
                }

                if (isacc)
                {
                    lastDic = null;
                    return KeyWordANFStatus.accept1;
                }
            }

            if (dics[chr] != null)
            {
                lastDic = dics[chr];
            }

            return KeyWordANFStatus.abort;

        }

        public string Replace(string text)
        {
            StringBuilder sb = new StringBuilder();
            int lastPostion = 0;
            foreach (var s in MatchKeyWord(text))
            {
                if (lastPostion < s.PostionStart)
                    sb.Append(text.Substring(lastPostion, s.PostionStart - lastPostion));

                lastPostion = s.PostionEnd + 1;
            }
            return sb.ToString();
        }

        public IEnumerable<KeyWordMatchResult> MatchKeyWord(string text)
        {
            if (text == null)
            {
                yield break;
            }

            KeyWordDic innerDic = null;
            int iStart = 0;
            KeyWordANFStatus statu = KeyWordANFStatus.abort;
            string substr;

            try
            {
                dicLock.EnterReadLock();
                var tlen = text.Length;
                for (int i = 0; i < tlen; i++)
                {
                    if (innerDic == null)
                    {
                        innerDic = dics[text[i]];
                        iStart = i;
                    }
                    else
                    {
                        statu = CheckChar(text[i], ref innerDic, statu);
                        if (statu == KeyWordANFStatus.abort)
                        {
                            //if (innerDic == null)
                            //    iStart = i + 1;
                            //else
                            {
                                int j = 1;
                                for (; j < i - iStart; j++)
                                {
                                    if (dics[text[iStart + j]] != null)
                                    {
                                        break;
                                    }
                                }
                                iStart += j;
                                i = iStart;
                                innerDic = dics[text[i]];
                            }
                        }
                        else if (statu == KeyWordANFStatus.accept1)
                        {
                            var wm = text[i - 1].ToString();
                            object tag = null;
                            if(DicTag.ContainsKey(wm))
                            {
                                tag = DicTag[wm];
                            }
                            yield return new KeyWordMatchResult
                            {
                                KeyWordMatched = wm,
                                PostionStart = iStart,
                                PostionEnd = i,
                                Tag=tag,
                            };

                            i--;
                            iStart = i;
                        }
                        else if (statu == KeyWordANFStatus.accept2)
                        {
                            var wm = text[i - 1].ToString();
                            object tag = null;
                            if (DicTag.ContainsKey(wm))
                            {
                                tag = DicTag[wm];
                            }
                            yield return new KeyWordMatchResult
                            {
                                KeyWordMatched = text[i - 1].ToString(),
                                PostionStart = iStart,
                                PostionEnd = i,
                                Tag=tag,
                            };

                            wm = text.Substring(iStart, 2);
                            tag = null;
                            if (DicTag.ContainsKey(wm))
                            {
                                tag = DicTag[wm];
                            }
                            yield return new KeyWordMatchResult
                            {
                                KeyWordMatched = text.Substring(iStart, 2),
                                PostionStart = iStart,
                                PostionEnd = i,
                                Tag=tag,
                            };
                            iStart = i;
                        }
                        else if (statu == KeyWordANFStatus.acceptwating)
                        {
                            var wm = text[i - 1].ToString();
                            object tag = null;
                            if (DicTag.ContainsKey(wm))
                            {
                                tag = DicTag[wm];
                            }

                            yield return new KeyWordMatchResult
                            {
                                KeyWordMatched = text[i - 1].ToString(),
                                PostionStart = iStart,
                                PostionEnd = i,
                                Tag=tag,
                            };
                            iStart = i - 1;
                        }
                        else if (statu == KeyWordANFStatus.accept)
                        {
                            substr = text.Substring(iStart, i - iStart + 1);
                            object tag = null;
                            if (DicTag.ContainsKey(substr))
                            {
                                tag = DicTag[substr];
                            }

                            yield return new KeyWordMatchResult
                            {
                                KeyWordMatched = substr,
                                PostionStart = iStart,
                                PostionEnd = i,
                                Tag=tag,
                            };

                            //for (int j = 1; j < substr.Length; j++)
                            //{
                            //    innerDic = dics[substr[j]];
                            //    if (innerDic != null)
                            //    {
                            //        i = iStart + j;
                            //        iStart = i;
                            //        break;
                            //    }
                            //}
                        }
                    }

                }
            }
            finally
            {
                dicLock.ExitReadLock();
            }
        }
    }
}
