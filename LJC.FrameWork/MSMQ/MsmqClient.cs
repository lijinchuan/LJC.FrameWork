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

        private static Regex Rg_CheckMsmqPath_Remoting = new Regex(@"^FormatName:Direct\=((TCP:\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3})|(OS:[A-z0-9\-]{1,256}))\\\\private\$\\\\[A-z_]{1}[A-z0-9_]{0,256}(?:\s{0}|(\,Direct\=((TCP:\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3})|(OS:[A-z0-9\-]{1,256}))\\\\private\$\\\\[A-z_]{1}[A-z0-9_]{0,256}){1,})$");
        private static Regex Rg_CheckMsmqPath_Local = new Regex(@"^\.\\private\$\\[A-z_]{1}[A-z0-9_]{0,256}$");

        private static IMessageFormatter XMLMessageFormatter = new XmlMessageFormatter(new Type[] { typeof(string) });
        private int _lastActivityMills = 0;

        /// <summary>
        /// 是否可以重复使用
        /// </summary>
        private bool _canbeReused = true;

        private Lazy<MessageQueue> _mq = null;

        private MessageQueue GetMessageQueue()
        {
            AssertMqpath(mqpath);

            var mq = new MessageQueue(mqpath, false, _canbeReused, QueueAccessMode.SendAndReceive);
            mq.Formatter = XMLMessageFormatter;
            return mq;
        }

        private void Init()
        {
            _mq = new Lazy<MessageQueue>(() => GetMessageQueue());
        }

        private static void AssertMqpath(string path)
        {
            if (!(Rg_CheckMsmqPath_Remoting.IsMatch(path) | Rg_CheckMsmqPath_Local.IsMatch(path)))
            {
                throw new Exception("格式错误:" + path);
            }
        }

        public MsmqClient(string path,bool fromconfig=false,bool reused=true)
        {
            if(fromconfig)
            {
                path = System.Configuration.ConfigurationManager.AppSettings[path];
            }

            AssertMqpath(path);
            mqpath = path;
            _canbeReused = reused;

            Init();
        }

        public MsmqClient(string hostname, string queuename, bool reused = true)
        {
            mqpath = string.Format(MsmqPathFormatHostname, hostname, queuename);
            _canbeReused = reused;

            Init();
        }

        public MsmqClient(IPEndPoint endpoint, string queuename, bool reused = true)
        {
            mqpath = string.Format(MsmqPathFormatIP, endpoint.Address.ToString(), queuename);
            _canbeReused = reused;

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

            if (_canbeReused)
            {
                try
                {
                    _mq.Value.Send(msg);
                }
                catch (MessageQueueException)
                {
                    _mq.Value.Dispose();
                    _mq = new Lazy<MessageQueue>(() => new MessageQueue(mqpath, false, _canbeReused, QueueAccessMode.SendAndReceive));

                    _mq.Value.Send(msg);
                }
            }
            else
            {
                using (var mq = GetMessageQueue())
                {
                    _mq.Value.Send(msg);
                }
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
            MessageQueue mq = null;
            TimeSpan timeoutspan = new TimeSpan(0, 0, sec_timeout);

            if (_canbeReused)
            {
                mq = _mq.Value;
            }
            else
            {
                mq = GetMessageQueue();
            }
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

                    if (_canbeReused)
                    {
                        _mq.Value.Dispose();
                        _mq = new Lazy<MessageQueue>(() => GetMessageQueue());
                        mq = _mq.Value;

                        try
                        {
                            msg = mq.Receive(timeoutspan);
                        }
                        catch (MessageQueueException ex)
                        {
                            if(IsTimeOutEx(ex))
                            {
                                break;
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        throw e;
                    }
                }

                yield return msg;
            }

            _lastActivityMills = Environment.TickCount;
        }

        public void Dispose()
        {
            if(_mq.IsValueCreated)
            {
                _mq.Value.Dispose();
            }
        }
    }
}
