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

        public static string GetSeqNum()
        {
            if (seqNum >= long.MaxValue)
            {
                seqNum = 0;
            }
            long seqNumMiro = Interlocked.Increment(ref seqNum);
            return string.Format("{0}{1}", DateTime.Now.ToString("yyMMddHHmmss-"), seqNumMiro);
        }

        public static byte[] GetSendMessageBytes(Message message)
        {
            byte[] data = EntityBuf.EntityBufCore.Serialize(message, SocketApplicationComm.IsMessageCompress);
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

        /// <summary>
        /// 消息是否压缩
        /// </summary>
        public const bool IsMessageCompress = false;

        public static bool SendMessge(this Socket s, Message message)
        {
            try
            {
                if (s == null || !s.Connected)
                {
                    return false;
                }

                var it = s.Send(GetSendMessageBytes(message), SocketFlags.None);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void Broadcast(this Socket udpSocket,Message message)
        {
            //var bytes=GetSendMessageBytes(message);
            var bytes = EntityBuf.EntityBufCore.Serialize(message,SocketApplicationComm.IsMessageCompress);
            if (bytes.Length >= Udp_MTU)
            {
                throw new Exception("UPD包过大，最大限制为" + Udp_MTU + "字节");
            }
            udpSocket.SendTo(bytes,new IPEndPoint(BROADCAST_ADDR,BCAST_PORT));
        }

        public static void MulitBroadcast(this Socket udpSocket, Message message)
        {
            var bytes = EntityBuf.EntityBufCore.Serialize(message,SocketApplicationComm.IsMessageCompress);
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
