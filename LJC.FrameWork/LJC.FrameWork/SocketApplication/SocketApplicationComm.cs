using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.SocketApplication
{
    public static class SocketApplicationComm
    {
        private static int seqNum;

        public static string GetSeqNum()
        {
            string str = string.Format("{0}{1}", DateTime.Now.ToString("yyMMddHHmmss-"), seqNum);
            Interlocked.Increment(ref seqNum);
            return str;
        }

        public static void SendMessge(this Socket s, Message message)
        {
            byte[] data = EntityBuf.EntityBufCore.Serialize(message);
            s.Send(BitConverter.GetBytes(data.Length));
            Console.WriteLine("发送消息" + message.ToString());
            s.Send(data);
        }

        internal static void Debug(string info)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["Debug"]))
            {
                Console.WriteLine(info);
            }
        }
    }
}
