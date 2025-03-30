using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork
{
    [Flags]
    public enum Option : uint
    {
        // From dbghelp.h:
        Normal = 0x00000000,
        WithDataSegs = 0x00000001,
        WithFullMemory = 0x00000002,
        WithHandleData = 0x00000004,
        FilterMemory = 0x00000008,
        ScanMemory = 0x00000010,
        WithUnloadedModules = 0x00000020,
        WithIndirectlyReferencedMemory = 0x00000040,
        FilterModulePaths = 0x00000080,
        WithProcessThreadData = 0x00000100,
        WithPrivateReadWriteMemory = 0x00000200,
        WithoutOptionalData = 0x00000400,
        WithFullMemoryInfo = 0x00000800,
        WithThreadInfo = 0x00001000,
        WithCodeSegs = 0x00002000,
        WithoutAuxiliaryState = 0x00004000,
        WithFullAuxiliaryState = 0x00008000,
        WithPrivateWriteCopyMemory = 0x00010000,
        IgnoreInaccessibleMemory = 0x00020000,
        ValidTypeFlags = 0x0003ffff,
    }

    enum ExceptionInfo
    {
        None,
        Present
    }

    public class ApplicationUtils
    {
        //typedef struct _MINIDUMP_EXCEPTION_INFORMATION {
        //    DWORD ThreadId;
        //    PEXCEPTION_POINTERS ExceptionPointers;
        //    BOOL ClientPointers;
        //} MINIDUMP_EXCEPTION_INFORMATION, *PMINIDUMP_EXCEPTION_INFORMATION;
        [StructLayout(LayoutKind.Sequential, Pack = 4)]  // Pack=4 is important! So it works also for x64!
        struct MiniDumpExceptionInformation
        {
            public uint ThreadId;
            public IntPtr ExceptionPointers;
            [MarshalAs(UnmanagedType.Bool)]
            public bool ClientPointers;
        }

        //BOOL
        //WINAPI
        //MiniDumpWriteDump(
        //    __in HANDLE hProcess,
        //    __in DWORD ProcessId,
        //    __in HANDLE hFile,
        //    __in MINIDUMP_TYPE DumpType,
        //    __in_opt PMINIDUMP_EXCEPTION_INFORMATION ExceptionParam,
        //    __in_opt PMINIDUMP_USER_STREAM_INFORMATION UserStreamParam,
        //    __in_opt PMINIDUMP_CALLBACK_INFORMATION CallbackParam
        //    );
        // Overload requiring MiniDumpExceptionInformation
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]

        static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, ref MiniDumpExceptionInformation expParam, IntPtr userStreamParam, IntPtr callbackParam);

        // Overload supporting MiniDumpExceptionInformation == NULL
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        static extern uint GetCurrentThreadId();

        static bool Write(SafeHandle fileHandle, Option options, ExceptionInfo exceptionInfo)
        {
            Process currentProcess = Process.GetCurrentProcess();
            IntPtr currentProcessHandle = currentProcess.Handle;
            uint currentProcessId = (uint)currentProcess.Id;
            MiniDumpExceptionInformation exp;
            exp.ThreadId = GetCurrentThreadId();
            exp.ClientPointers = false;
            exp.ExceptionPointers = IntPtr.Zero;
            if (exceptionInfo == ExceptionInfo.Present)
            {
                exp.ExceptionPointers = Marshal.GetExceptionPointers();
            }
            return exp.ExceptionPointers == IntPtr.Zero ? MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle, (uint)options, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) : MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle, (uint)options, ref exp, IntPtr.Zero, IntPtr.Zero);
        }

        static bool Write(SafeHandle fileHandle, Option dumpType)
        {
            return Write(fileHandle, dumpType, ExceptionInfo.None);
        }

        public static bool TryDump(String dmpPath, Option dmpType = Option.Normal)
        {
            string curTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string name = "Dump_" + curTime + ".dmp";
            var path = Path.Combine(dmpPath, name);
            var dir = Path.GetDirectoryName(path);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (var fs = new FileStream(path, FileMode.Create))
            {
                return Write(fs.SafeFileHandle, dmpType);
            }
        }

        public static void CatchCrashDump()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashDumpHandler);
        }

        private static void CrashDumpHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("捕获到未处理的异常: " + e.Message);

            // 指定dump文件的保存路径和文件名
            string dumpPath =Path.Combine($"{AppDomain.CurrentDomain.BaseDirectory}",$"{DateTime.Now:yyyyMMddHHmmss}.dump");
            try
            {
                TryDump(dumpPath, Option.Normal);
            }
            catch (Exception ex)
            {
                Console.WriteLine("生成dump文件失败: " + ex.Message);
            }

            throw e;
        }
    }
}
