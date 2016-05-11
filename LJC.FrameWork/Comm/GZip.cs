using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    /// <summary>
    /// 压缩数据
    /// </summary>
    public class GZip
    {
        /// <summary>
        /// 将字节数组进行压缩后返回压缩的字节数组
        /// </summary>
        /// <param name="data">需要压缩的数组</param>
        /// <returns>压缩后的数组</returns>
        public static byte[] Compress(byte[] data)
        {
            MemoryStream stream = new MemoryStream();
            GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress);
            gZipStream.Write(data, 0, data.Length);
            gZipStream.Close();
            return stream.ToArray();

            //MemoryStream stream = new MemoryStream();
            //ICSharpCode.SharpZipLib.GZip.GZipOutputStream gZipStream = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(stream);
            //gZipStream.Write(data, 0, data.Length);
            //gZipStream.Flush();
            //gZipStream.Finish();
            //return stream.ToArray();

        }

        /// <summary>
        /// 解压字符数组
        /// </summary>
        /// <param name="data">压缩的数组</param>
        /// <returns>解压后的数组</returns>
        public static byte[] Decompress(byte[] data)
        {
            MemoryStream stream = new MemoryStream();

            GZipStream gZipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
            byte[] bytes = new byte[4096];
            int n;
            while ((n = gZipStream.Read(bytes, 0, bytes.Length)) != 0)
            {
                stream.Write(bytes, 0, n);
            }
            gZipStream.Close();
            return stream.ToArray();

            //ICSharpCode.SharpZipLib.GZip.GZipInputStream gZipStream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(new MemoryStream(data));
            //byte[] buf=new byte[4096];
            //int count = 4096;

            //while (true)
            //{
            //    int numRead = gZipStream.Read(buf, 0, count);
            //    if (numRead <= 0)
            //    {
            //        break;
            //    }
            //    stream.Write(buf, 0, numRead);
            //}

            //return stream.ToArray();
        }
    }
}
