using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    public interface IExpressPart
    {
        int ModeID
        {
            get;
            set;
        }

        /// <summary>
        /// 表达式所在行数
        /// </summary>
        int CodeLine
        {
            get;
            set;
        }

        /// <summary>
        /// 表达式中开始位置
        /// </summary>
        int StartIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 表达式中结束位置
        /// </summary>
        int EndIndex
        {
            get;
            set;
        }
    }
}
