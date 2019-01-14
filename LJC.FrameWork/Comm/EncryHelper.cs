using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace LJC.FrameWork.Comm
{
    [Obsolete("使用AseEncryHelper代替")]
    /// <summary>   
    /// 对称加密算法类   
    /// </summary>   
    public class EncryHelper
    {

        private SymmetricAlgorithm mobjCryptoService;
        //private static readonly string Key = "CaTct(%&hj7x89H$yuBI0456FtmaT5&fvHUFCy76*h%(HilJ$lhj!y6&(*jkP87j1p";
        private static readonly string Key = "01234567891102345689955abced*&^33###@!!!(&%%$";

        /// <summary>   
        /// 对称加密类的构造函数   
        /// </summary>   
        public EncryHelper()
        {
            mobjCryptoService = new RijndaelManaged();
        }

        public static readonly string Empty = new EncryHelper().Encrypto(string.Empty);

        /// <summary>   
        /// 获得密钥   
        /// </summary>   
        /// <returns>密钥</returns>   
        private byte[] GetLegalKey()
        {
            string sTemp = Key;
            mobjCryptoService.GenerateKey();
            byte[] bytTemp = mobjCryptoService.Key;
            int KeyLength = bytTemp.Length;
            if (sTemp.Length > KeyLength)
                sTemp = sTemp.Substring(0, KeyLength);
            else if (sTemp.Length < KeyLength)
                sTemp = sTemp.PadRight(KeyLength, ' ');
            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }
        /// <summary>   
        /// 获得初始向量IV   
        /// </summary>   
        /// <returns>初试向量IV</returns>   
        private byte[] GetLegalIV()
        {
            //string sTemp = "E4ghj*Ghg7!rNIfb&95GUY86GfghUb#er57HBh(u%g6HJ($jhWk7&!hg4ui%$hjk";
            string sTemp = "E4ghj*Ghg7!rNIfb&95GUY86GfghUb#er57HBh(u%g6HJ";
            mobjCryptoService.GenerateIV();
            byte[] bytTemp = mobjCryptoService.IV;
            int IVLength = bytTemp.Length;
            if (sTemp.Length > IVLength)
                sTemp = sTemp.Substring(0, IVLength);
            else if (sTemp.Length < IVLength)
                sTemp = sTemp.PadRight(IVLength, ' ');
            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }
        /// <summary>   
        /// 加密方法   
        /// </summary>   
        /// <param name="Source">待加密的串</param>   
        /// <returns>经过加密的串</returns>   
        public string Encrypto<T>(T Source)
        {
            byte[] bytIn = UTF8Encoding.UTF8.GetBytes(Source.ToString());
            MemoryStream ms = new MemoryStream();
            ICryptoTransform encrypto = null;
            CryptoStream cs = null;
            byte[] bytOut = null;
            try
            {
                mobjCryptoService.Key = GetLegalKey();
                mobjCryptoService.IV = GetLegalIV();
                encrypto = mobjCryptoService.CreateEncryptor();
                cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
                cs.Write(bytIn, 0, bytIn.Length);
                cs.FlushFinalBlock();
                bytOut = ms.ToArray();
            }
            catch
            {
                throw new Exception("加密失败");
            }
            finally
            {
                if (encrypto != null) encrypto.Dispose();
                if (ms != null) ms.Close();
                if (cs != null) cs.Close();
            }
            return Convert.ToBase64String(bytOut);
        }
        /// <summary>   
        /// 解密方法   
        /// </summary>   
        /// <param name="Source">待解密的串</param>   
        /// <returns>经过解密的串</returns>   
        public string Decrypto<T>(T Source)
        {
            string rtnStr = string.Empty;
            MemoryStream ms = null;
            ICryptoTransform encrypto = null;
            CryptoStream cs = null;
            StreamReader sr = null;
            try
            {
                byte[] bytIn = Convert.FromBase64String(Source.ToString().Replace(" ", "+"));
                ms = new MemoryStream(bytIn, 0, bytIn.Length);
                mobjCryptoService.Key = GetLegalKey();
                mobjCryptoService.IV = GetLegalIV();
                encrypto = mobjCryptoService.CreateDecryptor();
                cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
                sr = new StreamReader(cs);
                rtnStr = sr.ReadToEnd();
            }
            catch
            {
                throw new Exception("解密失败！");
            }
            finally
            {
                if (encrypto != null) encrypto.Dispose();
                if (ms != null) ms.Close();
                if (cs != null) cs.Close();
                if (sr != null) cs.Close();
            }
            return rtnStr;
        }
    }
}
