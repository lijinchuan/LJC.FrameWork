using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.LogManager
{
    public interface ILogReader
    {
        List<Log> ReadLog();
    }
}
