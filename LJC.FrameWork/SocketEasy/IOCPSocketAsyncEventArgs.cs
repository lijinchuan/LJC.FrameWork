using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace LJC.FrameWork.SocketEasy
{
    public class IOCPSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        private bool _isReadPackLen = false;
        internal bool IsReadPackLen
        {
            get
            {
                return _isReadPackLen;
            }
            set
            {
                _isReadPackLen = value;
            }
        }

        internal void ClearBuffer()
        {
            this.SetBuffer(null, 0, 0);
        }
    }
}
