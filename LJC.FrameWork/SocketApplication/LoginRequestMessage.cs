using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    internal class LoginRequestMessage
    {
        public string LoginID
        {
            get;
            set;
        }

        public string LoginPwd
        {
            get;
            set;
        }
    }
}
