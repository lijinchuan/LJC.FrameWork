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

        /// <summary>
        /// 注入变量
        /// </summary>
        /// <param name="key"></param>
        /// <param name="calResult"></param>
        public void InjectVar(string key,CalResult calResult)
        {
            VarDataPool.Add(key, calResult);
        }

        public CalResult GetVar(string key)
        {
            if (VarDataPool.ContainsKey(key))
            {
                return VarDataPool[key];
            }

            return null;
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
