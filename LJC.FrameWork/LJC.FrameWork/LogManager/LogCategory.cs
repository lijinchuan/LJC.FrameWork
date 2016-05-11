using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LJC.FrameWork.LogManager
{
    public enum LogCategory
    {
        [Description("交易类")]
        Trade,
        [Description("历史行情类")]
        HisQuote,
        [Description("实时行情类")]
        RealQuote,
        [Description("登陆类")]
        Login,
        [Description("数据计算类")]
        DataCalculate,
        [Description("股票信息")]
        StockInfo,
        [Description("Fix")]
        FixApi,
        [Description("未分类")]
        Other = 99
    }
}
