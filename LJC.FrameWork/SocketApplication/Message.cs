using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SocketApplication
{
    public class MessageHeader
    {
        public int MessageType
        {
            get;
            set;
        }

        /// <summary>
        /// 流水号
        /// </summary>
        public string TransactionID
        {
            get;
            set;
        }

        public DateTime MessageTime
        {
            get;
            set;
        }
    }
    /// <summary>
    /// 消息
    /// </summary>
    public class Message
    {
        internal Message(MessageType msgType)
        {
            _messageHeader.MessageType = (int)msgType;
        }

        public Message(int msgType)
        {
            _messageHeader.MessageType = msgType;
        }

        public Message()
        {

        }

        private MessageHeader _messageHeader = new MessageHeader();
        /// <summary>
        /// 消息头
        /// </summary>
        public MessageHeader MessageHeader
        {
            get
            {
                return _messageHeader;
            }
            set
            {
                _messageHeader = value;
            }
        }
        /// <summary>
        /// 内容
        /// </summary>
        private object _messageBody;
        public void SetMessageBody(object body)
        {
            _messageBody = body;
        }

        private byte[] _messageBuffer;
        public byte[] MessageBuffer
        {
            get
            {
                if (_messageBuffer == null)
                {
                    if (_messageBody == null)
                    {
                        _messageBuffer = new byte[0];
                    }
                    else
                    {
                        _messageBuffer = EntityBuf.EntityBufCore.Serialize(_messageBody);
                    }
                }
                return _messageBuffer;
            }
            set
            {
                _messageBuffer = value;
            }
        }

        public T GetMessageBody<T>()
        {
            try
            {
                return EntityBuf.EntityBufCore.DeSerialize<T>(_messageBuffer);
            }
            catch (Exception ex)
            {
                var e = new SocketApplicationException("消息解析失败", ex);
                e.Data.Add("this.MessageHeader.TransactionID", this.MessageHeader.TransactionID);
                e.Data.Add("this.MessageHeader.MessageType", this.MessageHeader.MessageType);
                throw e;
            }
        }

        internal bool IsMessage(MessageType msgType)
        {
            return MessageHeader.MessageType.Equals((int)msgType);
        }

        public bool IsMessage(int msgType)
        {
            return MessageHeader.MessageType.Equals(msgType);
        }
    }
}
