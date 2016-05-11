using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace LJC.FrameWork.Comm
{
    /// <summary>
    /// 提供MD5,SHA1,SHA256,SHA512加密方法
    /// </summary>
    public class HashEncrypt
    {
                //private string strIN;
        private bool isReturnNum;
        private bool isCaseSensitive;

        /// <summary>
        /// 类初始化，此类提供MD5，SHA1，SHA256，SHA512等四种算法，加密字串的长度依次增大。
        /// </summary>
        /// <param name="IsCaseSensitive">是否区分大小写</param>
        /// <param name="IsReturnNum">是否返回为加密后字符的Byte代码</param>
        public HashEncrypt(bool IsCaseSensitive, bool IsReturnNum)
        {
            this.isReturnNum = IsReturnNum;
            this.isCaseSensitive = IsCaseSensitive;
        }

        private string getstrIN(string strIN)
        {
            //string strIN = strIN;
            if (strIN.Length == 0)
            {
                strIN = "~NULL~";
            }
            if (isCaseSensitive == false)
            {
                strIN = strIN.ToUpper();
            }
            return strIN;
        }

        public string MD5Encrypt(string strIN)
        {
            //string strIN = getstrIN(strIN);
            byte[] tmpByte;
            MD5 md5 = new MD5CryptoServiceProvider();
            tmpByte = md5.ComputeHash(GetKeyByteArray(getstrIN(strIN)));
            md5.Clear();

            return GetStringValue(tmpByte);

        }

        public string SHA1Encrypt(string strIN)
        {
            //string strIN = getstrIN(strIN);
            byte[] tmpByte;
            SHA1 sha1 = new SHA1CryptoServiceProvider();

            tmpByte = sha1.ComputeHash(GetKeyByteArray(strIN));
            sha1.Clear();

            return GetStringValue(tmpByte);

        }

        public string SHA256Encrypt(string strIN)
        {
            //string strIN = getstrIN(strIN);
            byte[] tmpByte;
            SHA256 sha256 = new SHA256Managed();

            tmpByte = sha256.ComputeHash(GetKeyByteArray(strIN));
            sha256.Clear();

            return GetStringValue(tmpByte);

        }

        public string SHA512Encrypt(string strIN)
        {
            //string strIN = getstrIN(strIN);
            byte[] tmpByte;
            SHA512 sha512 = new SHA512Managed();

            tmpByte = sha512.ComputeHash(GetKeyByteArray(strIN));
            sha512.Clear();

            return GetStringValue(tmpByte);

        }

        /// <summary>
        /// SHA256函数，用于javascript脚本中
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果(返回长度为44字节的字符串)</returns>
        public static string SHA256_JS(string str)
        {
            byte[] SHA256Data = Encoding.ASCII.GetBytes(str);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] Result = Sha256.ComputeHash(SHA256Data);
            //转换为hex
            Sha256.Clear();
            return BitConverter.ToString(Result).Replace("-", "").ToLower();
            //return Convert.ToBase64String(Result); //返回长度为44字节的字符串
        }

        /// <summary>
        /// SHA256函数，用于javascript脚本中
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果(返回长度为44字节的字符串)</returns>
        public static string MD5_JS(string str)
        {
            byte[] MD5Data = Encoding.UTF8.GetBytes(str);
            byte[] tmpByte;
            MD5 md5 = new MD5CryptoServiceProvider();
            tmpByte = md5.ComputeHash(MD5Data);
            md5.Clear();
            return BitConverter.ToString(tmpByte).Replace("-", "").ToLower();
        }
  

        private string GetStringValue(byte[] Byte)
        {
            string tmpString = "";
            if (this.isReturnNum == false)
            {
                ASCIIEncoding Asc = new ASCIIEncoding();
                tmpString = Asc.GetString(Byte);
            }
            else
            {
                int iCounter;
                for (iCounter = 0; iCounter < Byte.Length; iCounter++)
                {
                    tmpString = tmpString + Byte[iCounter].ToString();
                }
            }
            return tmpString;
        }

        private byte[] GetKeyByteArray(string strKey)
        {

            UTF8Encoding utf8 = new UTF8Encoding();

            int tmpStrLen = strKey.Length;
            byte[] tmpByte = new byte[tmpStrLen - 1];

            tmpByte = utf8.GetBytes(strKey);

            return tmpByte;

        }
    }
}
