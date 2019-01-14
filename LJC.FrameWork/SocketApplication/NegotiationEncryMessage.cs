using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    /// <summary>
    /// 协商加密消息
    /// 客户端生成一对公/私钥，将公钥发给服务器，服务器负责生成非对称加密密钥，发给客户端
    /// </summary>
    public class NegotiationEncryMessage
    {
        /// <summary>
        /// 公钥
        /// </summary>
        public string PublicKey
        {
            get;
            set;
        }

        public string EncryKey
        {
            get;
            set;
        }
    }
}
