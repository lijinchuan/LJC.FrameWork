using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.LogManager
{
    public interface ILogWriter
    {
        void WriteLog(string logTit, string logBody, LogCategory category);
    }
}
