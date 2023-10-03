using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm.SNLP
{
    public class CalcSimilarityResult
    {
        /// <summary>
        /// 计算的相关度结果
        /// </summary>
        public double CalcSimilarityValue
        {
            get;
            set;
        }

        public int UseMills
        {
            get;
            internal set;
        }
    }
}
