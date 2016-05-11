using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    public enum FieldEnum : int
    {
        SessionID = 200,
        HeadBeatInterVal,
        SessionTimeOut,
        /// <summary>
        /// 登陆ID
        /// </summary>
        LoginID,
        /// <summary>
        /// 登陆密码
        /// </summary>
        LoginPwd,
        /// <summary>
        /// 0-成功，1-失败
        /// </summary>
        LoginResult,
        LoginFailReson,
    }
}
