using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.EntityDataBase
{
    public class IndexItem
    {
        public string Field
        {
            get;
            set;
        }

        /// <summary>
        /// 方向,1-正向 -1-逆向
        /// </summary>
        public byte Direction
        {
            get;
            set;
        }
    }
}
