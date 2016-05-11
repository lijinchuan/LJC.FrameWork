using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LJC.FrameWork.LogManager
{
    [Serializable]
    public enum LogLevel
    {
        [Description("文本方式记录")]
        Text,
        [Description("调试")]
        Debug,
        [Description("一般")]
        Normal,
        [Description("即时")]
        Real,
        [Description("错误")]
        Error,
        [Description("严重")]
        Serious,
        [Description("致命")]
        Fatal
    }
}
