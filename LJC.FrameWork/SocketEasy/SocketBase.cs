using LJC.FrameWork.EntityBuf;
using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketEasy
{
    public class SocketBase:IDisposable
    {
        protected bool stop = false;
        private Socket udpMCSocket;
        public event Action<Exception> Error;

        /// <summary>
        /// 广播
        /// </summary>
        public event Action<Message> OnBroadCast;
        /// <summary>
        /// 组播
        /// </summary>
        public event Action<Message> OnMultiCast;

        /// <summary>
        /// 用来收发广播组播
        /// </summary>
        private Socket udpBCSocket;
        /// <summary>
        /// 用来发组播
        /// </summary>
        private UdpClient udpMCClient;

        private bool _enbaleBCast = false;
        /// <summary>
        /// 是否接收广播
        /// </summary>
        public bool EnableBroadCast
        {
            get
            {
                return _enbaleBCast;
            }
            set
            {
                _enbaleBCast = value;
                if (_enbaleBCast)
                {
                    if (udpBCSocket == null)
                    {
                        udpBCSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        udpBCSocket.ExclusiveAddressUse = false;
                        udpBCSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        udpBCSocket.EnableBroadcast = true;
                        udpBCSocket.Bind(new IPEndPoint(IPAddress.Any, SocketApplicationComm.BCAST_PORT));
                        udpBCSocket.ReceiveBufferSize = 32000;
                        udpBCSocket.SendBufferSize = 32000;

                        new Action(ReceivingBroadCast).BeginInvoke(null, null);
                    }
                }
            }
        }

        /// <summary>
        /// 是否接收组播
        /// </summary>
        private bool _enableMCast = false;
        public bool EnableMultiCast
        {
            get
            {
                return _enableMCast;
            }
            set
            {
                _enableMCast = value;
                if (_enableMCast)
                {
                    if (udpMCClient == null)
                    {
                        udpMCSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        udpMCSocket.ExclusiveAddressUse = false;
                        udpMCSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        //udpMCSocket.EnableBroadcast = true;
                        udpMCSocket.MulticastLoopback = true;
                        udpMCSocket.Ttl = 10;
                        udpMCSocket.Bind(new IPEndPoint(IPAddress.Any, SocketApplicationComm.MCAST_PORT));

                        MulticastOption optionValue = new MulticastOption(SocketApplicationComm.MCAST_ADDR);
                        udpMCSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
                        //udpMCSocket.JoinMulticastGroup(SocketApplicationComm.MCAST_ADDR);

                        //udpMCClient = new UdpClient(SocketApplicationComm.MCAST_PORT);
                        //udpMCClient.JoinMulticastGroup(SocketApplicationComm.MCAST_ADDR);

                        new Action(ReceivingMultiCast).BeginInvoke(null, null);
                    }
                }
            }
        }

        public void BroadCast(Message message)
        {
            SocketApplicationComm.Broadcast(this.udpBCSocket, message);
        }

        public void MultiCast(Message message)
        {
            SocketApplicationComm.MulitBroadcast(this.udpMCSocket, message);
        }

        private void ReceivingBroadCast()
        {
            EndPoint endPoint = new IPEndPoint(SocketApplicationComm.BROADCAST_ADDR, SocketApplicationComm.BCAST_PORT);
            while (!stop)
            {
                try
                {

                    byte[] buffer = new byte[SocketApplicationComm.Udp_MTU];
                    int count = this.udpBCSocket.ReceiveFrom(buffer, ref endPoint);

                    if (count == 0)
                        break;

                    //byte[] buffer = new UdpClient().Receive(ref endPoint);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessBoradCastMessage), buffer);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
        }

        private void ReceivingMultiCast()
        {
            EndPoint multicast = new IPEndPoint(SocketApplicationComm.MCAST_ADDR, SocketApplicationComm.MCAST_PORT + 1);

            while (!stop)
            {
                try
                {

                    byte[] buffer = new byte[SocketApplicationComm.Udp_MTU];
                    this.udpMCSocket.ReceiveFrom(buffer, ref multicast);
                    if (buffer.Count() == 0)
                        break;

                    //byte[] buffer = this.udpMCClient.Receive(ref multicast);


                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMultiCastMessage), buffer);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
        }

        private void ProcessMultiCastMessage(object buffer)
        {
            try
            {
                byte[] data = (byte[])buffer;
                Message message = EntityBufCore.DeSerialize<Message>(data,SocketApplicationComm.IsMessageCompress);

                if (OnMultiCast != null)
                {
                    OnMultiCast(message);
                }
            }
            catch (Exception e)
            {

            }
        }

        private void ProcessBoradCastMessage(object buffer)
        {
            try
            {
                byte[] data = (byte[])buffer;
                Message message = EntityBufCore.DeSerialize<Message>(data,SocketApplicationComm.IsMessageCompress);

                if (OnBroadCast != null)
                {
                    OnBroadCast(message);
                }
            }
            catch (Exception e)
            {

            }
        }

        protected virtual void OnError(Exception e)
        {
            if (stop)
                return;

            if (Error != null)
            {
                Error(e);
            }
        }

        /// <summary>
        /// 承类dispose方法
        /// </summary>
        protected virtual void DoDispose()
        {

        }

        public void Dispose()
        {
            stop = true;

            if (udpBCSocket != null)
            {
                udpBCSocket.Close();
            }

            if (udpMCClient != null)
            {
                udpMCClient.DropMulticastGroup(SocketApplicationComm.MCAST_ADDR);
                udpMCClient.Close();
            }

            DoDispose();
        }
    }
}
