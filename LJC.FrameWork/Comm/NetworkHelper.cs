using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public static class NetworkHelper
    {
        /// <summary>
        /// 获取当前活动的IP地址
        /// </summary>
        /// <returns></returns>
        public static IPAddress[] GetActiveIpV4s(bool containsLoopBack=false)
        {
            var ipAddresses = NetworkInterface.GetAllNetworkInterfaces()
                                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                                .Where(addr => containsLoopBack || !IPAddress.IsLoopback(addr.Address))
                                .Select(addr => addr.Address)
                                .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork)
                                .ToArray();

            return ipAddresses;
        }
    }
}
