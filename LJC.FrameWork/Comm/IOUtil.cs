using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class IOUtil
    {
        protected static byte[] endSpanChar = new byte[] { (byte)239, (byte)187, (byte)191 };

        public static void MakeDirs(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            var root = System.IO.Path.GetPathRoot(path);
            if (!Directory.Exists(root))
            {
                throw new System.IO.FileNotFoundException("根目录不存在:" + root);
            }

            var subpath = path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            if (subpath.Length == 1)
            {
                return;
            }
            string deeppath=root;
            for (int i = 1; i < subpath.Length;i++ )
            {
                deeppath += Path.DirectorySeparatorChar + subpath[i];
                if (!Directory.Exists(deeppath))
                {
                    Directory.CreateDirectory(deeppath);
                }
            }
        }

        static bool CheckHasEndSpan(Stream s)
        {

            var oldpos = s.Position;
            if (s.Position >= 4)
            {
                var byts3 = new byte[3];
                s.Position = s.Position - 3;
                s.Read(byts3, 0, 3);

                s.Position = oldpos;
                return byts3[0] == endSpanChar[0] && byts3[1] == endSpanChar[1] && byts3[2] == endSpanChar[2];
            }
            return false;

        }

        public static long CopyFile(string source, string dest, FileMode destmode, long begin, long end, bool samepos = true)
        {
            long pos = -1;
            if (begin < 0 || (end > 0 && end < begin))
            {
                return pos;
            }
            byte[] buffer = new byte[1024 * 1024];
            long offset = 0;
            using (System.IO.FileStream fs = new FileStream(dest, destmode))
            {
                if (samepos)
                {
                    fs.Position = begin;
                }
                else// if (destmode == FileMode.Append)
                {
                    fs.Position = fs.Length;
                    while (true)
                    {
                        if (CheckHasEndSpan(fs))
                        {
                            fs.Position -= 3;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                using (System.IO.FileStream fs2 = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (end >= fs2.Length)
                    {
                        end = fs2.Length - 1;
                        pos = end;
                    }
                    else if (end < 0)
                    {
                        end = fs2.Length + end;
                        pos = end;
                        if (pos <= begin)
                        {
                            pos = begin;
                        }
                    }
                    fs2.Position = begin;
                    while (true)
                    {
                        var copylen = Math.Min(end - begin - offset + 1, buffer.Length);
                        if (copylen <= 0)
                        {
                            break;
                        }
                        var readlen = fs2.Read(buffer, 0, (int)copylen);
                        if (readlen > 0)
                        {
                            fs.Write(buffer, 0, readlen);
                        }
                        offset += readlen;
                    }
                }
            }

            pos = begin + offset;
            return pos;
        }
    }
}
