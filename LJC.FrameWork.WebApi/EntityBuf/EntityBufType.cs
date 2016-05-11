using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.WebApi
{
    public class EntityBufType
    {
        private LJC.FrameWork.EntityBuf.EntityType _entityType = LJC.FrameWork.EntityBuf.EntityType.UNKNOWN;
        public LJC.FrameWork.EntityBuf.EntityType EntityType
        {
            get
            {
                return _entityType;
            }
            set
            {
                _entityType = value;
            }
        }

        /// <summary>
        /// 类名，可以是数组，类或者列表
        /// </summary>
        public Type ValueType
        {
            get;
            set;
        }

        /// <summary>
        /// 类名，只是类
        /// </summary>
        public Type ClassType
        {
            get;
            set;
        }

        public object DefaultValue
        {
            get;
            set;
        }

        /// <summary>
        /// 属性
        /// </summary>
        public PropertyInfoEx Property
        {
            get;
            set;
        }
    }
}
