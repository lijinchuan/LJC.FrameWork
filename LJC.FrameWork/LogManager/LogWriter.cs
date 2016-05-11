using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.Data.QuickDataBase;

namespace LJC.FrameWork.LogManager
{
    internal abstract  class LogWriter
    {
        public static long LogToDB(Log log)
        {
            //new DataContextMoudle<Log>(log).Add();
            return DataContextMoudelFactory<Log>.GetDataContext(log).Add();
        }
    }
}
