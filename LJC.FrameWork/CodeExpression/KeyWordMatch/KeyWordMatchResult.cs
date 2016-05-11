using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression.KeyWordMatch
{
    public class KeyWordMatchResult
    {
        public string KeyWordMatched
        {
            get;
            set;
        }

        public int PostionStart
        {
            get;
            set;
        }

        public int PostionEnd
        {
            get;
            set;
        }

        public object Tag
        {
            get;
            set;
        }
    }
}
