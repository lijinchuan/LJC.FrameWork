using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    /// <summary>
    /// 系统内置的消息类型，占0-99编号
    /// </summary>
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
        RELOGIN,
        /// <summary>
        /// 致命错误
        /// </summary>
        FATAL,
        UDPECHO,
        /// <summary>
        /// UPD询问包数据
        /// </summary>
        UDPQUERYBAG,
        /// <summary>
        /// UPD回应查询包数据
        /// </summary>
        UDPANSWERBAG,
        /// <summary>
        /// 设置MTU参数
        /// </summary>
        UPDSETMTU,
        /// <summary>
        /// 发送文件
        /// </summary>
        SENDFILE,
        SENDFILEECHO,
        /// <summary>
        /// 清理bagid
        /// </summary>
        UDPCLEARBAGID,
        /// <summary>
        /// 断点续传检查
        /// </summary>
        SENDFILRESENDCHECK,
    }
}
