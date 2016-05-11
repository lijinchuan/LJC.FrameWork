using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace LJC.FrameWork.Comm
{
    public static class WebUtility
    {
        public static string UrlEncode(string content)
        {
            return System.Web.HttpUtility.UrlEncode(content);
        }

        public static string UrlDecode(string content)
        {
            return System.Web.HttpUtility.UrlDecode(content);
        }

        public static string HtmlEncode(string content)
        {
            return System.Web.HttpUtility.HtmlEncode(content);
        }

        public static string HtmlDecode(string content)
        {
            return System.Web.HttpUtility.HtmlDecode(content);
        }

        public static string UrlEncode(string content,Encoding encode)
        {

            return System.Web.HttpUtility.UrlEncode(encode.GetString(Encoding.Default.GetBytes(content)));
        }

        /// <summary>
        /// 合并网址
        /// </summary>
        /// <param name="url1">基址</param>
        /// <param name="url2"></param>
        /// <returns></returns>
        public static string CombURL(string url1, string url2)
        {
            string oldUrl1 = url1.Trim();
            string oldUrl2 = url2.Trim();
            url1 = url1.Trim().ToLower();
            url2 = url2.Trim().ToLower();
            url1 = url1.Substring(0, (url1.IndexOf("?") == -1) ? url1.Length : url1.IndexOf("?"));
            url2 = url2.Replace("..\\", "../");

            if (!(url1.EndsWith(".aspx") || url1.EndsWith(".asp") || url1.EndsWith(".html") || url1.EndsWith(".htm") || url1.EndsWith(".jsp") || url1.EndsWith(".php") || url1.EndsWith(".shtml")))
                url1 = url1.Trim('/', '\\') + "/";

            url1 = url1.Trim();
            url2 = url2.Trim();

            if (url2.StartsWith("http://") || url2.StartsWith("https://") || url2.StartsWith(@"http:\\") || url2.StartsWith(@"https:\\"))
                return oldUrl2;

            if ((url1.EndsWith("/")) && !(url2.StartsWith("../") || url2.StartsWith("..\\") || url2.StartsWith("/")))
                return oldUrl1 + oldUrl2;

            string[] subUrl2 = oldUrl2.Split(new string[] { "../", "..\\" }, StringSplitOptions.None);

            int len = subUrl2.Length - 1;

            string url1_head = oldUrl1.Substring(0, url1.IndexOf(':') + 3);

            oldUrl1 = oldUrl1.Substring(oldUrl1.IndexOf(':') + 3);

            string[] subUrl1 = oldUrl1.Split('/', '\\');

            if (subUrl1.Length - 1 <= len)
                throw (new Exception("父目录长度过小"));

            string url = url1_head;

            if (url2.StartsWith("/"))
                return url + subUrl1[0] + oldUrl2;

            for (int i = 0; i < subUrl1.Length - len - 1; i++)
                url += subUrl1[i] + "/";

            return url + oldUrl2.Trim('/').Replace("../", "");

        }
    }
}
