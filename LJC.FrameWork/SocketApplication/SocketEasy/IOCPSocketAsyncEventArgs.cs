using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace LJC.FrameWork.SocketEasy
{
    /// <summary>
    /// 
    /// </summary>
    public class IOCPSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        public static int InstanceCount
        {
            get;
            private set;
        }

        public IOCPSocketAsyncEventArgs()
        {
            InstanceCount++;
        }


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

        /// <summary>
        /// 清理缓存，这个要放在前面
        /// </summary>
        internal void ClearBuffer()
        {
            //这个要放在前面，因为在用的话会报错
            this.SetBuffer(null, 0, 0);
            _bufferIndex = -1;
            BufferLen = 0;
            BufferRev = 0;
        }
    }
}
