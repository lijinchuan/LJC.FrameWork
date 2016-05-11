using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public interface IDBCachAble : ICachAble
    {
        /// <summary>
        /// 集合key
        /// </summary>
        /// <returns></returns>
        string GetCollectCachKey();
    }
}
