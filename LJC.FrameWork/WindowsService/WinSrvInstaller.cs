using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LJC.FrameWork.WindowsService
{
    public static class WinSrvInstaller
    {
        [DllImport("advapi32.dll")]
        internal static extern IntPtr OpenSCManager(string lpMachineName, string lpSCDB, int scParameter);

        [DllImport("advapi32.dll")]
        internal static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName, int dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpPathName, string lpLoadOrderGroup, int lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);

        [DllImport("advapi32.dll")]
        internal static extern void CloseServiceHandle(IntPtr SCHANDLE);

        [DllImport("advapi32.dll")]
        internal static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);

        [DllImport("advapi32.dll")]
        internal static extern int DeleteService(IntPtr SVHANDLE);

        /// <summary>
        /// 安装Windows服务
        /// </summary>
        /// <param name="servicePath">服务安装的完整路径</param>
        /// <param name="serviceName">服务的名称</param>
        /// <param name="serviceDisplayName">服务在控制面板中的显示内容</param>
        /// <returns>如果安装成功，返回true,否则返回false</returns>
        public static bool InstallService(string servicePath, string serviceName, string serviceDisplayName)
        {
            int SC_MANAGER_CREATE_SERVICE = 0x0002;
            int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
            int SERVICE_ERROR_NORMAL = 0x00000001;
            int STANDARD_RIGHTS_REQUIRED = 0xF0000;
            int SERVICE_QUERY_CONFIG = 0x0001;
            int SERVICE_CHANGE_CONFIG = 0x0002;
            int SERVICE_QUERY_STATUS = 0x0004;
            int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
            int SERVICE_START = 0x0010;
            int SERVICE_STOP = 0x0020;
            int SERVICE_PAUSE_CONTINUE = 0x0040;
            int SERVICE_INTERROGATE = 0x0080;
            int SERVICE_USER_DEFINED_CONTROL = 0x0100;
            int SERVICE_ALL_ACCESS = (
                STANDARD_RIGHTS_REQUIRED |
                SERVICE_QUERY_CONFIG |
                SERVICE_CHANGE_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_ENUMERATE_DEPENDENTS |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_INTERROGATE |
                SERVICE_USER_DEFINED_CONTROL);
            int SERVICE_AUTO_START = 0x00000002;

            try
            {
                IntPtr handle = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
                bool result = false;
                if (handle.ToInt32() != 0)
                {
                    IntPtr serviceHandle = CreateService(handle, serviceName, serviceDisplayName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, SERVICE_AUTO_START, SERVICE_ERROR_NORMAL, servicePath, null, 0, null, null, null);
                    result = (serviceHandle.ToInt32() != 0);
                    CloseServiceHandle(handle);
                }
                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 卸载Windows服务
        /// </summary>
        /// <param name="serviceName">服务的名称</param>
        public static bool UnInstallService(string serviceName)
        {
            int GENERIC_WRITE = 0x40000000;

            try
            {
                IntPtr handle = OpenSCManager(null, null, GENERIC_WRITE);
                bool result = false;
                if (handle.ToInt32() != 0)
                {
                    int DELETE = 0x10000;
                    IntPtr serviceHandle = OpenService(handle, serviceName, DELETE);
                    if (serviceHandle.ToInt32() != 0)
                    {
                        result = (DeleteService(serviceHandle) != 0);
                        CloseServiceHandle(handle);
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 启动Windows服务
        /// </summary>
        /// <param name="serviceName">服务的名称</param>
        public static bool StartService(string serviceName)
        {
            int GENERIC_WRITE = 0x40000000;
            int STANDARD_RIGHTS_REQUIRED = 0xF0000;
            int SERVICE_QUERY_CONFIG = 0x0001;
            int SERVICE_CHANGE_CONFIG = 0x0002;
            int SERVICE_QUERY_STATUS = 0x0004;
            int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
            int SERVICE_START = 0x0010;
            int SERVICE_STOP = 0x0020;
            int SERVICE_PAUSE_CONTINUE = 0x0040;
            int SERVICE_INTERROGATE = 0x0080;
            int SERVICE_USER_DEFINED_CONTROL = 0x0100;
            int SERVICE_ALL_ACCESS = (
                STANDARD_RIGHTS_REQUIRED |
                SERVICE_QUERY_CONFIG |
                SERVICE_CHANGE_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_ENUMERATE_DEPENDENTS |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_INTERROGATE |
                SERVICE_USER_DEFINED_CONTROL);

            try
            {
                IntPtr handle = OpenSCManager(null, null, GENERIC_WRITE);
                bool result = false;
                if (handle.ToInt32() != 0)
                {
                    IntPtr serviceHandle = OpenService(handle, serviceName, SERVICE_ALL_ACCESS);
                    if (serviceHandle.ToInt32() != 0)
                    {
                        result = (StartService(serviceHandle, 0, null) != 0);
                        CloseServiceHandle(handle);
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
        }
    }
}
