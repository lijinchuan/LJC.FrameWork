using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Redis
{
    public class RedisConfig
    {
        private Int64 _defaultDB = 0;
        public Int64 DefaultDB
        {
            get
            {
                return _defaultDB;
            }
            set
            {
                _defaultDB = value;
            }
        }

        private int _maxReadPool = 5;
        public int MaxReadPoolSize
        {
            get
            {
                return _maxReadPool;
            }
            set
            {
                if (value < 1)
                    return;
                _maxReadPool = value;
            }
        }

        private int _maxWritePool = 5;
        public int MaxWritePoolSize
        {
            get
            {
                return _maxWritePool;
            }
            set
            {
                if (value < 1)
                    return;
                _maxWritePool = value;
            }
        }
    }
}
