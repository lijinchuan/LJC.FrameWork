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

        internal int BufferLen
        {
            get;
            set;
        }

        internal int BufferRev
        {
            get;
            set;
        }

        private int _bufferIndex = -1;
        internal int BufferIndex
        {
            get
            {
                return _bufferIndex;
            }
            set
            {
                _bufferIndex = value;
            }
        }

        internal void ClearBuffer()
        {
            _bufferIndex = -1;
            this.SetBuffer(null, 0, 0);
            BufferLen = 0;
            BufferRev = 0;
        }
    }
}
