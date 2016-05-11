using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    public class ExpressErrorException : Exception
    {

        public ExpressErrorException(string errMsg, int line = 0, int col = 0, int col2 = 0, IExpressPart errExp = null)
            : base(errMsg)
        {
            ErrerLine = line;
            ErrorCol = col;
            ErrorCol2 = col2;
            ErrorExpress = errExp;
        }
        /// <summary>
        /// 错误行
        /// </summary>
        public int ErrerLine
        {
            get;
            set;
        }

        /// <summary>
        /// 错误开始列
        /// </summary>
        public int ErrorCol
        {
            get;
            set;
        }

        /// <summary>
        /// 错误结束列
        /// </summary>
        public int ErrorCol2
        {
            get;
            set;
        }

        /// <summary>
        /// 错误的表达式
        /// </summary>
        public IExpressPart ErrorExpress
        {
            get;
            set;
        }
    }
}
