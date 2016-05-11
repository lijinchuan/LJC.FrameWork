using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.Data.QuickDataBase;

namespace LJC.FrameWork.LogManager
{
    internal class LevelLogReader:ILogReader
    {
        LogLevel level;

        public LevelLogReader(LogLevel level)
        {
            this.level = level;
        }

        public List<Log> ReadLog()
        {
            //return new DataContextMoudle<Log>().WhereBigerEq("Level", level).ExecuteList();
            return DataContextMoudelFactory<Log>.GetDataContext().WhereBigerEq("Level", level).ExecuteList();
        }
    }
}
