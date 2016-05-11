using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class GCHelper
    {
        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetCurrentProcess();

        /// <summary>
        /// 释放当前程序内存
        /// </summary>
        /// <returns></returns>
        public static bool Collect()
        {
            IntPtr ptr = GetCurrentProcess();
            if (ptr != IntPtr.Zero)
            {
                return SetProcessWorkingSetSize(ptr, -1, -1);
            }
            return false;
        }
    }
}
