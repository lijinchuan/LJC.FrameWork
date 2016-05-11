using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;

namespace LJC.FrameWork.Comm
{
    public static class SerializerHelper
    {
        /// <summary>
        /// 序列化保存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="o"></param>
        public static void BinarySave(string path, object o)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream ms = new FileStream(path, FileMode.Create))
            {
                bf.Serialize(ms, o);
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object BinaryGet(string path)
        {
            object o = null;

            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    o = bf.Deserialize(fs);
                }
            }
            catch(Exception e)
            {

            }

            return o;
        }

        public static object Clone(object source)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, source);
                return bf.Deserialize(ms);
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeserializerXML<T>(string xml)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));

                StringReader sr = new StringReader(xml);
                return (T)ser.Deserialize(sr);
            }
            catch
            {

            }

            return default(T);
        }

        /// <summary>
        /// 从文档序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public static T DeSerializerFile<T>(string savePath, bool catchErr = true)
        {
            try
            {
                if (!File.Exists(savePath))
                    return default(T);
                using (StreamReader sr = new StreamReader(savePath, Encoding.UTF8))
                {
                    string xml = sr.ReadToEnd();
                    sr.Close();
                    return DeserializerXML<T>(xml);
                }
            }
            catch
            {
                if (catchErr)
                    throw;
                else
                    return default(T);
            }


        }

        /// <summary>
        /// 序列化保存成文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerializer"></param>
        /// <param name="savePath"></param>
        public static string SerializerToXML<T>(T objectToSerializer, string savePath = null, bool catchErr = false)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));

                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);

                ser.Serialize(sw, objectToSerializer);

                if (!string.IsNullOrWhiteSpace(savePath))
                {
                    using (StreamWriter fs = new StreamWriter(savePath, false, Encoding.UTF8))
                    {
                        fs.Write(sb.ToString());
                        fs.Close();
                    }
                }

                return sb.ToString();
            }
            catch
            {
                if (catchErr)
                    throw;
                else
                    return string.Empty;
            }

            //return string.Empty;
        }
    }
}
