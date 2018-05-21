using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public class IOUtil
    {
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

        public static void CopyFile(string source, string dest,FileMode destmode, long begin, long end)
        {
            if (begin < 0 || end < begin)
            {
                return;
            }
            byte[] buffer = new byte[1024 * 1024];
            int offset=0;
            using (System.IO.FileStream fs = new FileStream(dest,destmode))
            {
                using (System.IO.FileStream fs2 = new FileStream(source, FileMode.Open))
                {
                    if (end > fs2.Length)
                    {
                        end = fs2.Length;
                    }
                    fs2.Position = begin;
                    while (true)
                    {
                        var copylen = Math.Min(end - begin - offset, buffer.Length);
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
        }
    }
}
