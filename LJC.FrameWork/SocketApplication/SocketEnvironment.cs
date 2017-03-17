using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    public static class SocketApplicationEnvironment
    {
        public const string TraceSocketDataConfigKey = "TraceSocketDataBag";
        public const string TraceMessageConfigKey = "TraceMessage";

        /// <summary>
        /// 跟踪数据包的发送
        /// </summary>
        public static bool TraceSocketDataBag
        {
            get
            {
                return LJC.FrameWork.Comm.ConfigHelper.AppConfig(TraceSocketDataConfigKey).Equals("T", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static bool TraceMessage
        {
            get
            {
                return LJC.FrameWork.Comm.ConfigHelper.AppConfig(TraceMessageConfigKey).Equals("T", StringComparison.OrdinalIgnoreCase);
            }
        }


    }
}
