using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    public enum MessageType:uint
    {
        HEARTBEAT,
        TEST_REQUEST,
        RESEND_REQUEST,
        REJECT,
        SEQUENCE_RESET,
        LOGIN,
        LOGOUT,
        INDICATION_OF_INTEREST,
        ADVERTISEMENT,
        EXECUTION_REPORT,
        ORDER_CANCEL_REJECT,
        UAN,
        UAP,
        /// <summary>
        /// 重登陆
        /// </summary>
        RELOGIN
    }
}
