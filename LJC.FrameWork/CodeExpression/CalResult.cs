using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    public class CalResult
    {
        /// <summary>
        /// 表达式结果字符串
        /// </summary>
        public object Result
        {
            get;
            set;
        }

        /// <summary>
        /// 如果是单个的操作，放单个的数，如0，1，也可能是数组，比如逗号操作
        /// 也可以放当天的数据，比如一个交易日的收盘价
        /// </summary>
        public Type ResultType
        {
            get;
            set;
        }

        /// <summary>
        /// 存放运算的集合，比如很多天的收盘价
        /// </summary>
        public object[] Results
        {
            get;
            set;
        }
    }
}
