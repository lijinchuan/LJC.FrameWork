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
    }
}
