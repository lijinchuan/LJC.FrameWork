using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.QuickDataBase
{
    internal class DBOrderby
    {
        public string OrderbyColumnName
        {
            get;
            set;
        }

        private DBOrderbyDirection _orderbyDirection = DBOrderbyDirection.asc;
        public DBOrderbyDirection OrderbyDirection
        {
            get
            {
                return _orderbyDirection;
            }
            set
            {
                _orderbyDirection = value;
            }
        }
    }
}
