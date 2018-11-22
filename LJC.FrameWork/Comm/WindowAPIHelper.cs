using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LJC.FrameWork.Comm
{
    public class WindowAPIHelper
    {
        private WindowAPIHelper()
        {

        }

        private static WindowAPIHelper _instance;

        public static WindowAPIHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WindowAPIHelper();
                }

                return _instance;
            }
        }

        #region 查找字窗口，根据窗口标题

        /// <summary>
        /// 改进
        /// </summary>
        /// <param name="fWnd"></param>
        /// <param name="findChildWindTit"></param>
        /// <returns></returns>
        public IntPtr FindChildWindByTitle(IntPtr fWnd, string findChildWindTit)
        {
            IntPtr ret = IntPtr.Zero;
            WindowsAPI.EnumChildWindows(fWnd, new EnumWindProc((hwnd, lptr) =>
                {
                    StringBuilder sb = new StringBuilder(1024);
                    if (WindowsAPI.GetWindowText(hwnd, sb, sb.Capacity) > 0)
                    {
                        if (sb.ToString().Equals(findChildWindTit))
                        {
                            ret = hwnd;
                            return false;
                        }
                    }
                    if (FindChildWindByTitle(hwnd, findChildWindTit) == IntPtr.Zero)
                        return true;
                    else
                        return false;
                }), IntPtr.Zero);

            return IntPtr.Zero;
        }

        #endregion


        #region 查找下一个窗口，根据窗口标题
      
        #endregion

        #region 根据窗口大小

        /// <summary>
        /// 重新改写,根据控件大小取所有的窗体
        /// </summary>
        /// <param name="processName">进程名，如一个程序运行程序名称为 app.exe,则进程名为app</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="lpClassName">筛选类名</param>
        /// <returns></returns>
        public List<IntPtr> FindWindowBySizeAll(string processName, int width, int height,string lpClassName=null)
        {
            var ret = new List<IntPtr>();
            EnumAllWindow(processName, new EnumWindProc((hWnd, lptr) =>
                {
                    if (lpClassName != null)
                    {
                        StringBuilder sbClassName=new StringBuilder(1024);
                        if (WindowsAPI.GetClassName(hWnd, sbClassName, 1024)&&!lpClassName.Trim().Equals(sbClassName.ToString()))
                        {
                            return true;
                        }
                    }

                    Rect rect = new Rect();
                    bool b = WindowsAPI.GetWindowRect(hWnd, out rect);

                    if (rect.Hight == height && rect.Width == width)
                    {
                        ret.Add(hWnd);
                    }

                    return true;
                }), null);
            return ret;
        }

        /// <summary>
        /// 重新改写,根据控件大小取所有的窗体
        /// </summary>
        /// <param name="processName">进程名，如一个程序运行程序名称为 app.exe,则进程名为app</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="lpClassName">筛选类名</param>
        /// <returns></returns>
        public List<IntPtr> FindWindowByClassName(string processName, string lpClassName)
        {
            var ret = new List<IntPtr>();
            if (string.IsNullOrWhiteSpace(lpClassName))
            {
                return ret;
            }
            EnumAllWindow(processName, new EnumWindProc((hWnd, lptr) =>
            {
                StringBuilder sbClassName = new StringBuilder(1024);
                if (WindowsAPI.GetClassName(hWnd, sbClassName, 1024) && lpClassName.Trim().Equals(sbClassName.ToString()))
                {
                    ret.Add(hWnd);
                }
       
                return true;
            }), null);
            return ret;
        }

        #endregion

        #region 根据窗口标题查找窗口

        /// <summary>
        /// 重新改写
        /// </summary>
        /// <param name="findWindTit">要查找的窗口标题</param>
        /// <returns></returns>
        public IntPtr FindWindowByWindTitle(string findWindTit)
        {
            IntPtr ret = IntPtr.Zero;
            
            WindowsAPI.EnumWindows(new EnumWindProc((hWnd, lPtr) =>
                {
                    StringBuilder sb = new StringBuilder(1024);
                    if (WindowsAPI.GetWindowText(hWnd, sb, sb.Capacity) > 0)
                    {
                        if (sb.ToString().Equals(findWindTit))
                        {
                            ret = hWnd;
                            return false;
                        }
                    }

                    return true;
                }), IntPtr.Zero);

            return ret;
        }
        #endregion


        #region

        public Process GetProcess(string processName)
        {
            Process[] p = Process.GetProcessesByName(processName);

            foreach (Process pp in p)
            {
                if (pp.MainWindowHandle != IntPtr.Zero)
                    return pp;
            }

            return null;
        }

        public IntPtr EnumAllWindow(string processName, EnumWindProc proc, object obj)
        {
            Process process = GetProcess(processName);
            if (process == null)
                return IntPtr.Zero;

            WindowsAPI.ShowWindow(process.MainWindowHandle, 1);
            IntPtr enumAllWindowIsSuccess = IntPtr.Zero;
            EnumWindProc enumAllWindProc = proc;

            foreach (ProcessThread th in process.Threads)
            {
                WindowsAPI.EnumThreadWindows(th.Id, new EnumWindProc((hwnd, lptr) =>
                    {
                        if (!enumAllWindProc(hwnd, lptr))
                            return false;

                        IntPtr intptr = WindowsAPI.EnumChildWindows(hwnd, enumAllWindProc, lptr);
                        return intptr == IntPtr.Zero;
                    }
                    ), IntPtr.Zero);
                if (enumAllWindowIsSuccess != IntPtr.Zero)
                    return enumAllWindowIsSuccess;
            }

            return IntPtr.Zero;
        }

        #endregion

        #region
        /// <summary>
        /// 改进过，多种方法赋值
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool SetText(IntPtr hWnd, string text)
        {
            var ret = false;
            ret = WindowsAPI.SendMessage(hWnd, WindowsAPI.WM_SETTEXT, IntPtr.Zero, new StringBuilder(text));
            if (!ret)
            {
                //ret = WindowsAPI.SwitchToThisWindow(hWnd, false)!=0;
                //ret = ret || WindowsAPI.SetFocus(hWnd);
                var chs=text.ToCharArray();
                foreach (var ch in chs)
                {
                    WindowsAPI.SendMessage(hWnd, WindowsAPI.WM_KEYDOWN, 0, 0);
                    ret=WindowsAPI.SendMessage(hWnd, WindowsAPI.WM_CHAR, ch, 0)!=0;
                    WindowsAPI.SendMessage(hWnd, WindowsAPI.WM_KEYUP, 0, 0);
                }
                ret = true;
            }
            return ret;
        }

        #endregion

        public IntPtr FindTaskBar(string title)
        {
            IntPtr ret = IntPtr.Zero;
            WindowsAPI.EnumWindows(new EnumWindProc((hwnd,lptr)=>
            {
                try
                {
                    StringBuilder szClass = new StringBuilder();

                    if (!WindowsAPI.GetWindow(hwnd, WindowsAPI.GW_OWNER) /*&& WindowAPI.IsWindowVisible(hwnd)*/) // 滤掉不在任务栏显示的窗口
                    {
                        WindowsAPI.GetClassName(hwnd, szClass, 256);
                        if (!szClass.Equals("Shell_TrayWnd")// 滤掉任务栏本身
                            && !szClass.Equals("Progman") // 滤掉桌面
                            )
                        {
                            if (WindowsAPI.GetWindowLong(hwnd, WindowsAPI.GWL_EXSTYLE & WindowsAPI.WS_EX_TOOLWINDOW))
                            {
                                return true;
                            }
                            //这就是你想要的窗口了。

                            StringBuilder szTitle = new StringBuilder();
                            WindowsAPI.GetWindowText(hwnd, szTitle, 256);

                            if (szTitle.Equals(title))
                            {
                                ret = hwnd;
                                return false;
                            }
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {

                }

               return true;   
            }), IntPtr.Zero);
            return ret;
        }

        //private static bool EnumWindProcForGetChildWindowByClass(IntPtr hWnd, IntPtr lParam)
        //{
        //    var result=(EnumWindProcForGetChildWindowByClassResult)Marshal.PtrToStructure(lParam, typeof(EnumWindProcForGetChildWindowByClassResult));
        //    StringBuilder sb=new StringBuilder(256);
        //    WindowsAPI.GetClassName(hWnd, sb, sb.Capacity);

        //    if (string.Equals(sb.ToString(), result.SbClassName))
        //    {
        //        return false;
        //    }

        //    return EnumWindProcForGetChildWindowByClass(hWnd, lParam);
        //}

        //public void Test(IntPtr parent,string className)
        //{
        //    EnumWindProcForGetChildWindowByClassResult result = new EnumWindProcForGetChildWindowByClassResult(className);
        //    IntPtr lparam=Marshal.AllocHGlobal(Marshal.SizeOf(result));
        //    Marshal.StructureToPtr(result, lparam, true);
        //    WindowsAPI.EnumChildWindows(parent, EnumWindProcForGetChildWindowByClass,lparam);
        //}
    }

    //public class EnumWindProcForGetChildWindowByClassResult
    //{
    //    public EnumWindProcForGetChildWindowByClassResult(string className)
    //    {
    //        SbClassName = className;
    //    }

    //    public string SbClassName
    //    {
    //        get;
    //        set;
    //    }
    //}
}
