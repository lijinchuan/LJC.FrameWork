using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.KeyWordMatch
{
    public class KeyWordManager
    {
        private KeyWordANF keyWordANF = new KeyWordANF();

        public IEnumerable<KeyWordMatchResult> MatchKeyWord(string text)
        {
            return keyWordANF.MatchKeyWord(text);
        }

        public string Replace(string text)
        {
            return keyWordANF.Replace(text);
        }

        public void AddKeyWord(string keyWord, object tag = null)
        {
            keyWordANF.AddKeyWord(keyWord, tag);
        }

        public void RemoveKeyWord(string keyWord)
        {
            keyWordANF.RemoveKeyWord(keyWord);
        }
    }
}
