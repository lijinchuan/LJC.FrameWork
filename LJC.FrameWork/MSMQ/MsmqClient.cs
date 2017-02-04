using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.MSMQ
{
    /// <summary>
    /// msmq客户端类，当前只适用私有队列和非事务队列，采用TCP通信
    /// 使用方法：
    /// 1、配置<![CDATA[ 
    ///  <add key="msmq_path" value="127.0.0.1/queuename,127.0.0.1/queuename2" />
    ///  或者
    ///  <add key="msmq_path" value="./queuename2" />
    ///  或者使用标准的msmq路径写法
    /// ]]>
    /// 2、构造
    /// var client= MsmqClient("127.0.0.1","queuename")
    /// </summary>
    public sealed class MsmqClient:IDisposable
    {
        private string _mqpath = null;
        private bool _isPathValid = false;


        public const string MsmqPathFormatIP = "FormatName:Direct=TCP:{0}\\private$\\{1}";
        public const string MsmqPathFormatHostname = "FormatName:Direct=OS:{0}\\private$\\{1}";

        private static Regex Rg_CheckMsmqPath_Remoting = new Regex(@"^FormatName:Direct\=((((TCP:\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3})|(OS:[A-z0-9\-]{1,256}))\\\\private\$\\\\[A-z_]{1}[A-z0-9_]{0,256}(?:\s{0})|(http\:\/\/\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3}\/msmq\/private\$\/[A-z_]{1}[A-z0-9_]{0,256}))|(\,(Direct\=((TCP:\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3})|(OS:[A-z0-9\-]{1,256}))\\\\private\$\\\\[A-z_]{1}[A-z0-9_]{0,256})|(http\:\/\/\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3}\/msmq\/Private\$\/[A-z_]{1}[A-z0-9_]{0,256})){1,})$");
        private static Regex Rg_CheckMsmqPath_Local = new Regex(@"^\.\\private\$\\[A-z_]{1}[A-z0-9_]{0,256}$");

        private static Regex Rg_IpConfig = new Regex(@"^\.(?:\:\d{1,5})?/[A-z_]{1}[A-z0-9_]{0,256}$|\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3}(?:\:\d{1,5})?/[A-z_]{1}[A-z0-9_]{0,256}(?:$|(\,(?:\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3}(?:\:\d{1,5})?/[A-z_]{1}[A-z0-9_]{0,256})){1,}$)");
        private static Regex Rg_IpConfigPair = new Regex(@"(\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3}|\.)(:\d{1,5})?/([A-z_]{1}[A-z0-9_]{0,256})");

        private static IMessageFormatter XMLMessageFormatter = new XmlMessageFormatter(new Type[] { typeof(string) });
        private int _lastActivityMills = 0;

        private Lazy<MessageQueue> _mq = null;

        private MessageQueue GetMessageQueue()
        {
            bool needparse = false;
            AssertMqpath(_mqpath,ref needparse);

            var mq = new MessageQueue(_mqpath, false, true, QueueAccessMode.SendAndReceive);
            mq.Formatter = XMLMessageFormatter;
            return mq;
        }

        private void Init()
        {
            _mq = new Lazy<MessageQueue>(() => GetMessageQueue());
        }

        private void AssertMqpath(string path,ref bool needparse)
        {
            if(_isPathValid)
            {
                return;
            }

            if (!(Rg_CheckMsmqPath_Remoting.IsMatch(path) || Rg_CheckMsmqPath_Local.IsMatch(path)))
            {
                if (!Rg_IpConfig.IsMatch(path))
                {
                    needparse = true;
                }
                else
                {
                    throw new Exception("格式错误:" + path); 
                }
            }

            _isPathValid = true;
        }

        public MsmqClient(string path,bool fromconfig=false)
        {
            if(fromconfig)
            {
                path = System.Configuration.ConfigurationManager.AppSettings[path];
            }

            bool needparse = false;
            AssertMqpath(path,ref needparse);
            if (needparse)
            {
                StringBuilder sb = new StringBuilder();
                foreach(Match m in Rg_IpConfigPair.Matches(path))
                {
                    if (m.Groups[1].Value.Equals("."))
                    {
                        sb.AppendFormat(@"\.\\private\$\\{0}", m.Groups[3].Value);
                    }
                    else
                    {
                        if(sb.Length==0)
                        {
                            sb.Append("FormatName:");
                        }
                        sb.AppendFormat("Direct=TCP:{0}\\private$\\{1},", m.Groups[1].Value, m.Groups[3].Value);
                    }
                }
                if (sb[sb.Length - 1].Equals(','))
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                _mqpath = sb.ToString();
            }
            else
            {
                _mqpath = path;
            }
            Init();
        }

        public MsmqClient(string hostname, string queuename)
        {
            _mqpath = string.Format(MsmqPathFormatHostname, hostname, queuename);
            Init();
        }

        public void CreateIfNotExis()
        {
            if (!MessageQueue.Exists(_mqpath))
            {
                var mq = MessageQueue.Create(_mqpath, false);
                _mq.Value.SetPermissions("ANONYMOUS LOGON", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
                _mq.Value.SetPermissions("Everyone", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
            }
        }

        public void SendQueue(string content, bool recoverable = true)
        {
            Message msg = new Message(content);
            msg.Recoverable = recoverable;

            try
            {
                _mq.Value.Send(msg);
            }
            catch (MessageQueueException)
            {
                _mq.Value.Dispose();
                _mq = new Lazy<MessageQueue>(() => GetMessageQueue());

                _mq.Value.Send(msg);
            }

            _lastActivityMills = Environment.TickCount;
        }

        private static bool IsTimeOutEx(Exception ex)
        {
            if (ex.Message.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) != -1 || ex.Message.IndexOf("超时") != -1)
            {
                return true;
            }

            return false;
        }

        public IEnumerable<Message> ReadQueue(int sec_timeout=5)
        {
            MessageQueue mq = _mq.Value;
            TimeSpan timeoutspan = new TimeSpan(0, 0, sec_timeout);

            Message msg = null;

            while (true)
            {
                try
                {
                    msg = mq.Receive(timeoutspan);
                }
                catch (MessageQueueException e)
                {
                    if(IsTimeOutEx(e))
                    {
                        break;
                    }

                    _mq.Value.Dispose();
                    _mq = new Lazy<MessageQueue>(() => GetMessageQueue());
                    mq = _mq.Value;

                    try
                    {
                        msg = mq.Receive(timeoutspan);
                    }
                    catch (MessageQueueException ex)
                    {
                        if (IsTimeOutEx(ex))
                        {
                            break;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }

                yield return msg;
            }

            _lastActivityMills = Environment.TickCount;
        }

        private bool _isDisposed = false;
        public void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(_mq.IsValueCreated)
                {
                    _mq.Value.Dispose();
                }
            }
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
