using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.Data.QuickDataBase;

namespace LJC.FrameWork.LogManager
{
    internal class CategoryLogReader:ILogReader
    {
        LogCategory cateGory;

        public CategoryLogReader(LogCategory cateGory)
        {
            this.cateGory = cateGory;
        }

        public List<Log> ReadLog()
        {
            //return new DataContextMoudle<Log>().WhereEq("Category", cateGory).ExecuteList();
            return DataContextMoudelFactory<Log>.GetDataContext().WhereEq("Category", cateGory).ExecuteList();
        }
    }
}
