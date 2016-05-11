using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    public class CalCurrent
    {

        private Dictionary<string, CalResult> _varDataPool = new Dictionary<string, CalResult>();
        /// <summary>
        /// 变量存储器
        /// </summary>
        public Dictionary<string, CalResult> VarDataPool
        {
            get
            {
                return _varDataPool;
            }
        }

        private int _currentIndex = -1;
        public int CurrentIndex
        {
            get
            {
                return _currentIndex;
            }
            set
            {
                _currentIndex = value;
            }
        }

        private int _currentBound = -1;
        public int CurrentBound
        {
            get
            {
                return _currentBound;
            }
            set
            {
                _currentBound = value;
            }
        }

        public void Clear()
        {
            _varDataPool.Clear();
            CurrentIndex = -1;
        }

        public object RuntimeParam
        {
            get;
            set;
        }
    }
}
