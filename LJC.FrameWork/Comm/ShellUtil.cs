using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public static class ShellUtil
    {
        public class ShellExeResult
        {
            public string OutPut
            {
                get;
                set;
            }

            public string Error
            {
                get;
                set;
            }

            public int ExitCode
            {
                get;
                set;
            }

        }

        public static ShellExeResult CallShell(string shellfile, string args)
        {
           Process proc = null;
           ShellExeResult result = new ShellExeResult();
            try
            {
                using (proc = new Process())
                {
                    var si = new ProcessStartInfo();
                    si.RedirectStandardError = true;
                    si.RedirectStandardInput = true;
                    si.RedirectStandardOutput = true;
                    si.FileName = shellfile;
                    si.WorkingDirectory=new FileInfo(shellfile).Directory.FullName;
                    proc.StartInfo = si;

                    if (!string.IsNullOrWhiteSpace(args))
                    {
                        proc.StartInfo.Arguments = args;//this is argument
                    }

                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    
                    proc.Start();
                    proc.WaitForExit(60*1000);
                    
                    result.ExitCode= proc.ExitCode;
                    result.OutPut= proc.StandardOutput.ReadToEnd();
                    result.Error= proc.StandardError.ReadToEnd();
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());

                result.Error = ex.ToString();
            }

            return result;
        }
    }
}
