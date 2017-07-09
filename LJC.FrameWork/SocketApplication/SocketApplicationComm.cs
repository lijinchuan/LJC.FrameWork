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
using System.Collections;
using System.Net.NetworkInformation;

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

                    var crc32 = LJC.FrameWork.Comm.HashEncrypt.GetCRC32(data, 8);
                    LogManager.LogHelper.Instance.Debug("校验值:" + crc32);
                    var crc32bytes = BitConverter.GetBytes(crc32);
                    for (int i = 4; i < 8; i++)
                    {
                        data[i] = crc32bytes[i - 4];
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
                        for (int i = 0; i < 4; i++)
                        {
                            _sendBufferManger.Buffer[i + offset] = dataLen[i];
                        }

                        var crc32 = LJC.FrameWork.Comm.HashEncrypt.GetCRC32(_sendBufferManger.Buffer, offset + 8, (int)size - 8);
                        LogManager.LogHelper.Instance.Debug("校验值:" + crc32);
                        var crc32bytes = BitConverter.GetBytes(crc32);
                        for (int i = 4; i < 8; i++)
                        {
                            _sendBufferManger.Buffer[i + offset] = crc32bytes[i - 4];
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

        /// <summary>
        /// 获取操作系统已用的端口号
        /// </summary>
        /// <returns></returns>
        public static List<int> GetTcpPortUsed()
        {
            //获取本地计算机的网络连接和通信统计数据的信息
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            //返回本地计算机上的所有Tcp监听程序
            IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();
            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            List<int> allPorts = new List<int>();
            foreach (IPEndPoint ep in ipsTCP)
            {
                allPorts.Add(ep.Port);
            }
            foreach (TcpConnectionInformation conn in tcpConnInfoArray)
            {
                allPorts.Add(conn.LocalEndPoint.Port);
            }
            return allPorts;
        }

        /// <summary>
        /// 获取操作系统已用的端口号
        /// </summary>
        /// <returns></returns>
        public static List<int> GetUdpPortUsed()
        {
            //获取本地计算机的网络连接和通信统计数据的信息
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            //返回本地计算机上的所有UDP监听程序
            IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();

            List<int> allPorts = new List<int>();
            foreach (IPEndPoint ep in ipsUDP)
            {
                allPorts.Add(ep.Port);
            }
            return allPorts;
        }

        public static int GetIdelTcpPort(int testport=0)
        {
            var portsused = GetTcpPortUsed();
            if (testport != 0)
            {
                if (!portsused.Contains(testport))
                {
                    return testport;
                }
            }
            for (int i = 1025; i < 65536; i++)
            {
                if (!portsused.Contains(i))
                {
                    return i;
                }
            }

            return 0;
        }

        public static int GetIdelUdpPort(int testport = 0)
        {
            var portsused = GetUdpPortUsed();
            if (testport != 0)
            {
                if (!portsused.Contains(testport))
                {
                    return testport;
                }
            }
            for (int i = 1025; i < 65536; i++)
            {
                if (!portsused.Contains(i))
                {
                    return i;
                }
            }

            return 0;
        }

        public static void EnumInterface()
        {
            System.Net.NetworkInformation.NetworkInterface[] interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            foreach (System.Net.NetworkInformation.NetworkInterface ni in interfaces)
            {
                Console.WriteLine("网卡名：{0}", ni.Name);
            }
        }
    }
}
