using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Comm
{
    public class BufferPollManager
    {
        private byte[] _buffer = null;
        private int _bufferIndex = -1;
        private Stack<int> _poll = null;
        private int _blockNum = 0;
        private int _blockSize = 0;

        public byte[] Buffer
        {
            get
            {
                return _buffer;
            }
        }

        public int BlockSize
        {
            get
            {
                return _blockSize;
            }
        }

        public int GetOffset(int bufferindex)
        {
            return bufferindex * _blockSize;
        }

        public BufferPollManager(int blocknum,int blocksize)
        {
            if(blocknum<=0)
            {
                throw new ArgumentOutOfRangeException("blocknum");
            }

            if(blocksize<=0)
            {
                throw new ArgumentOutOfRangeException("blocksize");
            }

            _blockNum = blocknum;
            _blockSize = blocksize;
            _buffer = new byte[_blockNum * _blockSize];

            _poll = new Stack<int>();
        }

        public int GetBuffer()
        {
            lock(this)
            {
                if(_poll.Count>0)
                {
                    return _poll.Pop();
                }
            }

            if (_bufferIndex < _blockNum)
            {
                var newbufferindex = Interlocked.Increment(ref _bufferIndex);
                if (newbufferindex >= _blockNum)
                {
                    return -1;
                }

                return newbufferindex;
            }

            return -1;
        }

        public void RealseBuffer(int bufferindex)
        {
            if(bufferindex<0)
            {
                return;
            }

            lock(this)
            {
                _poll.Push(bufferindex);
            }
        }
    }
}
