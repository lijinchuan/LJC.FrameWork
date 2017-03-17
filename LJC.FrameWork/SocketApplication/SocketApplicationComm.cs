using LJC.FrameWork.Comm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;

namespace LJC.FrameWork.SocketApplication
{
    public static class SocketApplicationComm
    {
        public static IPAddress BROADCAST_ADDR = IPAddress.Parse("255.255.255.255");
        /// <summary>
        /// 广播端口
        /// </summary>
        public static int BCAST_PORT = 55544;

        /// <summary>
        /// 组播地址224.0.0.0-239.255.255.255
        /// </summary>
        public static IPAddress MCAST_ADDR = IPAddress.Parse("239.239.239.239");
        /// <summary>
        /// 组播端口
        /// </summary>
        public static int MCAST_PORT = 55545;
        /// <summary>
        /// UDP包大小
        /// </summary>
        public static readonly int Udp_MTU = 1400 * 8;

        private static long seqNum;
        private static object sendMessageLock = new object();

        private static LJC.FrameWork.Comm.BufferPollManager _sendBufferManger = new BufferPollManager(100, 1024 * 100);

        private static string _seqperfix = Guid.NewGuid().ToString().Replace("-", "");

        public static string GetSeqNum()
        {
            if (seqNum >= long.MaxValue)
            {
                seqNum = 0;
            }
            long seqNumMiro = Interlocked.Increment(ref seqNum);
            return string.Format("{0}_{1}", _seqperfix, seqNumMiro);
        }

        [Obsolete]
        public static byte[] GetSendMessageBytes(Message message)
        {
            byte[] data = EntityBuf.EntityBufCore.Serialize(message);
            byte[] dataLen = BitConverter.GetBytes(data.Length);

            if (data.Length == 0 || data.Length >= Int32.MaxValue)
            {
                throw new Exception("发送长度过长或过小");
            }

            using (MemoryStream ms = new System.IO.MemoryStream())
            {
                ms.Write(dataLen, 0, dataLen.Length);
                ms.Write(data, 0, data.Length);
                return ms.ToArray();
            }
        }

        public static bool SendMessge(this Socket s, Message message)
        {
            try
            {
                if (s == null || !s.Connected)
                {
                    return false;
                }

                byte[] data = null;
                int bufferindex=-1;
                long size=0;
                EntityBuf.EntityBufCore.Serialize(message, _sendBufferManger, ref bufferindex,ref size, ref data);
                if (bufferindex == -1)
                {
                    byte[] dataLen = BitConverter.GetBytes(data.Length-4);

                    for (int i = 0; i < 4;i++ )
                    {
                        data[i] = dataLen[i];
                    }

                    lock (s)
                    {
                        var sendcount = s.Send(data, SocketFlags.None);

                        if (SocketApplicationEnvironment.TraceSocketDataBag && !string.IsNullOrWhiteSpace(message.MessageHeader.TransactionID))
                        {
                            LogManager.LogHelper.Instance.Debug(s.Handle + "发送数据:" + message.MessageHeader.TransactionID + "长度:" + data.Length + ", " + Convert.ToBase64String(data));
                        }

                        return sendcount > 0;
                    }
                }
                else
                {
                    try
                    {
                        //LogManager.LogHelper.Instance.Error("发送数据bufferindex:" + bufferindex + ",size:" + size);

                        byte[] dataLen = BitConverter.GetBytes((int)size - 4);
                        int offset=_sendBufferManger.GetOffset(bufferindex);
                        for (int i = 0;i<4;i++)
                        {
                            _sendBufferManger.Buffer[i + offset] = dataLen[i];
                        }

                        int sendcount = 0;
                        lock (s)
                        {
                            SocketError senderror=SocketError.Success;

                            sendcount = s.Send(_sendBufferManger.Buffer, offset, (int)size, SocketFlags.None, out senderror);

                            if (SocketApplicationEnvironment.TraceSocketDataBag && !string.IsNullOrWhiteSpace(message.MessageHeader.TransactionID))
                            {
                                var sendbytes = _sendBufferManger.Buffer.Skip(offset).Take((int)size).ToArray();
                                LogManager.LogHelper.Instance.Debug(s.Handle + "发送数据:" + message.MessageHeader.TransactionID + "长度:" + size + ", " + Convert.ToBase64String(sendbytes));
                            }

                            if(senderror!=SocketError.Success)
                            {
                                throw new Exception(senderror.ToString());
                            }
                        }
                        return sendcount > 0;
                    }
                    finally
                    {
                        _sendBufferManger.RealseBuffer(bufferindex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogHelper.Instance.Error("发送消息失败:" + message.MessageHeader.TransactionID, ex);
                return false;
            }
        }

        public static void Broadcast(this Socket udpSocket,Message message)
        {
            //var bytes=GetSendMessageBytes(message);
            var bytes = EntityBuf.EntityBufCore.Serialize(message);
            if (bytes.Length >= Udp_MTU)
            {
                throw new Exception("UPD包过大，最大限制为" + Udp_MTU + "字节");
            }
            udpSocket.SendTo(bytes,new IPEndPoint(BROADCAST_ADDR,BCAST_PORT));
        }

        public static void MulitBroadcast(this Socket udpSocket, Message message)
        {
            var bytes = EntityBuf.EntityBufCore.Serialize(message);
            if (bytes.Length >= Udp_MTU)
            {
                throw new Exception("UPD包过大，最大限制为"+Udp_MTU+"字节");
            }
            udpSocket.SendTo(bytes, new IPEndPoint(MCAST_ADDR, MCAST_PORT));
        }

        internal static void Debug(string info)
        {
            bool boo;

            if (bool.TryParse(ConfigHelper.AppConfig("Debug"), out boo) && boo)
            {
                Console.WriteLine(info);
            }
        }
    }
}
