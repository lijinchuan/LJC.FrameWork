using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.MSMQ
{
    public sealed class MsmqClient:IDisposable
    {
        private string mqpath = null;
        public const string MsmqPathFormatIP = "FormatName:Direct=TCP:{0}\\private$\\{1}";
        public const string MsmqPathFormatHostname = "FormatName:Direct=OS:{0}\\private$\\{1}";

        //private static Regex Rg_CheckMsmqPath_Remoting = new Regex(@"^FormatName:Direct\=((TCP:\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3})|(OS:[A-z0-9\-]{1,256}))\\\\private\$\\\\[A-z_]{1}[A-z0-9_]{0,256}(?:\s{0}|(\,Direct\=((TCP:\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3})|(OS:[A-z0-9\-]{1,256}))\\\\private\$\\\\[A-z_]{1}[A-z0-9_]{0,256}){1,})$");
        private static Regex Rg_CheckMsmqPath_Remoting = new Regex(@"^FormatName:Direct\=((((TCP:\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3})|(OS:[A-z0-9\-]{1,256}))\\\\private\$\\\\[A-z_]{1}[A-z0-9_]{0,256}(?:\s{0})|(http\:\/\/\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3}\/msmq\/private\$\/[A-z_]{1}[A-z0-9_]{0,256}))|(\,(Direct\=((TCP:\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3})|(OS:[A-z0-9\-]{1,256}))\\\\private\$\\\\[A-z_]{1}[A-z0-9_]{0,256})|(http\:\/\/\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3}\/msmq\/Private\$\/[A-z_]{1}[A-z0-9_]{0,256})){1,})$");
        private static Regex Rg_CheckMsmqPath_Local = new Regex(@"^\.\\private\$\\[A-z_]{1}[A-z0-9_]{0,256}$");

        //private static Regex Rg_CheckMsmqPath_Http = new Regex(@"^http\:\/\/\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3}\/msmq\/Private\$\/[A-z_]{1}[A-z0-9_]{0,256}");

        private static IMessageFormatter XMLMessageFormatter = new XmlMessageFormatter(new Type[] { typeof(string) });
        private int _lastActivityMills = 0;

        private Lazy<MessageQueue> _mq = null;

        private MessageQueue GetMessageQueue()
        {
            AssertMqpath(mqpath);

            var mq = new MessageQueue(mqpath, false, true, QueueAccessMode.SendAndReceive);
            mq.Formatter = XMLMessageFormatter;
            return mq;
        }

        private void Init()
        {
            _mq = new Lazy<MessageQueue>(() => GetMessageQueue());
        }

        private static void AssertMqpath(string path)
        {
            if (!(Rg_CheckMsmqPath_Remoting.IsMatch(path) || Rg_CheckMsmqPath_Local.IsMatch(path)))
            {
                throw new Exception("格式错误:" + path);
            }
        }

        public MsmqClient(string path,bool fromconfig=false)
        {
            if(fromconfig)
            {
                path = System.Configuration.ConfigurationManager.AppSettings[path];
            }

            AssertMqpath(path);
            mqpath = path;
            Init();
        }

        public MsmqClient(string hostname, string queuename)
        {
            mqpath = string.Format(MsmqPathFormatHostname, hostname, queuename);

            Init();
        }

        public MsmqClient(IPEndPoint endpoint, string queuename)
        {
            mqpath = string.Format(MsmqPathFormatIP, endpoint.Address.ToString(), queuename);
            Init();
        }

        public void CreateIfNotExis()
        {
            if (!MessageQueue.Exists(mqpath))
            {
                var mq = MessageQueue.Create(mqpath, false);
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
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
