using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace LJC.FrameWork.Data.QuickDataBase
{
    [Serializable]
    public class ReportAttr : Attribute
    {

        /// <summary>
        /// 是否分表
        /// </summary>
        public bool IsSplitTable = false;
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName = string.Empty;
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName = string.Empty;
        /// <summary>
        /// 列
        /// </summary>
        public string Column = string.Empty;

        /// <summary>
        /// 是否自增主键
        /// </summary>
        public bool isKey = false;
        /// <summary>
        /// 排序
        /// </summary>
        public int Index = 100;
        /// <summary>
        /// 支持列排序
        /// </summary>
        public bool Sort = false;

        private PropertyInfo _property;
        public PropertyInfo Property
        {
            get
            {
                return _property;
            }
            set
            {
                _property = value;
                if (value != null)
                {
                    PropertyEx = new PropertyInfoEx(value);
                }
            }
        }

        internal PropertyInfoEx PropertyEx
        {
            get;
            private set;
        }

        private bool _isEncry = false;
        /// <summary>
        /// 是否加密
        /// </summary>
        public bool IsEncry
        {
            get
            {
                return _isEncry;
            }
            set
            {
                _isEncry = value;
            }
        }

        public int Width;

    }
}
