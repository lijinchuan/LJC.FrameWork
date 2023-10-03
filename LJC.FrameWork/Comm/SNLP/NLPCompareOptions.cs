using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm.SNLP
{
    public class NLPCompareOptions
    {
        private int _compareMinLen = 2;
        /// <summary>
        /// 对比最新长度
        /// </summary>
        public int CompareMinLen
        {
            get
            {
                return _compareMinLen;
            }
            set
            {
                if (value <= 0)
                {
                    return;
                }
                _compareMinLen = value;
            }
        }

        private int _timeOutMills = 1000;
        /// <summary>
        /// 超时毫秒
        /// </summary>
        public int TimeOutMills
        {
            get
            {
                return _timeOutMills;
            }
            set
            {
                if (value <= 0)
                {
                    return;
                }
                _timeOutMills = value;
            }
        }

        internal int PrepareMaxLen
        {
            get;
            set;
        }

        internal Dictionary<string, Tuple<List<NLPCompareDetail>, int>> RepeatDic
        {
            get;
            set;
        }

        internal DateTime BeinDt
        {
            get;
            set;
        }
    }
}
