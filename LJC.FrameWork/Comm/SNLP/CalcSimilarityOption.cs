using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm.SNLP
{
    public class CalcSimilarityOption
    {
        private double _expectSimilarity = 50;
        /// <summary>
        /// 期望相似度最小
        /// </summary>
        public double ExpectSimilarity
        {
            get
            {
                return _expectSimilarity;
            }
            set
            {
                if (value >= 0 && value <= 100)
                {
                    _expectSimilarity = value;
                }
            }
        }

        public NLPCompareOptions CompareOptions
        {
            get;
            set;
        }
    }
}
