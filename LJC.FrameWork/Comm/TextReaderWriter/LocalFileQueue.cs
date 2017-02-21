using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Comm.TextReaderWriter
{
    public class LocalFileQueue<T> : IDisposable where T : class
    {
        private ObjTextWriter _queueWriter = null;
        private ObjTextReader _queueReader = null;
        private LocalFileQueueCfg _logger = null;
        private Timer _backtimer = null;
        private long _lastCfgChageTime = 0;
        private object _lock = new object();
        private bool IsRuning = false;
        /// <summary>
        /// 注册队列投递处理方法，返回true表示处理成功，将会继续投递下一条，返回false或者异常，会继承投递之前未成功的那一条数据
        /// </summary>
        public event Func<T, bool> OnProcessQueue;
        /// <summary>
        /// 出错时会回调通知给应用程序
        /// </summary>
        public event Action<T, Exception> OnProcessError;
        /// <summary>
        /// 处理成功回调方法
        /// </summary>
        public event Action<T> OnProcessQueueSuccessed;

        private string QueueFilePath
        {
            get;
            set;
        }

        private string QueueCfgFile
        {
            get;
            set;
        }

        private string QueueName
        {
            get;
            set;
        }

        private void SaveConfig()
        {
            if (_logger != null)
            {
                var lasttime = _logger.LastChageTime;
                if (_lastCfgChageTime < lasttime)
                {
                    LJC.FrameWork.Comm.SerializerHelper.SerializerToXML(_logger, QueueCfgFile, true);
                    _lastCfgChageTime = lasttime;
                }
            }
        }

        public LocalFileQueue(string queuename, string queuefilepath)
        {
            if (string.IsNullOrWhiteSpace(queuename))
            {
                throw new ArgumentNullException("queuename");
            }
            QueueName = queuename;

            if (string.IsNullOrWhiteSpace(queuefilepath))
            {
                throw new ArgumentNullException("queuefilepath");
            }
            QueueFilePath = queuefilepath;

            if (!File.Exists(queuefilepath))
            {
                FileInfo file = new FileInfo(queuefilepath);
                if (!file.Directory.Exists)
                {
                    try
                    {
                        file.Directory.Create();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("创建文件夹失败:" + file.Directory.FullName, ex);
                    }
                }
            }

            //由于文件写是独占式的，当程序快速重启时，文件可能未及时释放
            int trytimes = 0;
            while (true)
            {
                try
                {
                    _queueWriter = ObjTextWriter.CreateWriter(queuefilepath, ObjTextReaderWriterEncodeType.jsonbuf);
                    break;
                }
                catch (Exception ex)
                {
                    trytimes++;
                    if (trytimes >= 3)
                    {
                        throw ex;
                    }
                }

                Thread.Sleep(1000 * trytimes);
            }

            _queueReader = ObjTextReader.CreateReader(queuefilepath);

            FileInfo finfo = new FileInfo(queuefilepath);
            QueueCfgFile = finfo.Directory.FullName +"\\"+ queuename + ".cfg";
            if (File.Exists(QueueCfgFile))
            {
                _logger = LJC.FrameWork.Comm.SerializerHelper.DeSerializerFile<LocalFileQueueCfg>(QueueCfgFile, true);
                if (_logger.LastPos > 0)
                {
                    _queueReader.SetPostion(_logger.LastPos);
                }
            }
            else
            {
                _logger = new LocalFileQueueCfg();
                _logger.LastChageTime = Environment.TickCount;
                _logger.QueueFile = queuefilepath;
                SaveConfig();
            }

            _backtimer = new Timer(new TimerCallback(TimerAction), null, 0, 0);
        }

        private void TimerAction(object o)
        {
            var tc = Environment.TickCount;
            try
            {
                _backtimer.Change(Timeout.Infinite, Timeout.Infinite);
                if (_queueWriter != null)
                {
                    _queueWriter.Flush();
                }

                if (!IsRuning)
                {
                    new Action(ProcessQueue).BeginInvoke(null, null);
                }

                SaveConfig();
            }
            finally
            {
                _backtimer.Change(1000 - (Environment.TickCount - tc), 0);
            }
        }

        public void Enqueue(T obj)
        {
            if (obj.Equals(default(T)))
            {
                throw new Exception("null值不能写入队列");
            }
            _queueWriter.AppendObject(obj);
        }

        private void ProcessQueue()
        {
            if (OnProcessQueue == null)
            {
                return;
            }

            if (IsRuning)
            {
                return;
            }
            IsRuning = true;
            T last = null;
            int errortimes = 0;
            try
            {
                foreach (var t in _queueReader.ReadObjectWating<T>())
                {
                    last = t;
                    if (OnProcessQueue(t))
                    {
                        errortimes = 0;
                        _logger.LastPos = _queueReader.ReadedPostion();
                        _logger.LastChageTime = Environment.TickCount;

                        if (OnProcessQueueSuccessed != null)
                        {
                            OnProcessQueueSuccessed(t);
                        }
                    }
                    else
                    {
                        _queueReader.SetPostion(_logger.LastPos);
                        errortimes++;
                    }

                    if (errortimes > 0)
                    {
                        Thread.Sleep(1000 * errortimes);
                    }

                    if (errortimes > 5)
                    {
                        throw new Exception("尝试次数过多");
                    }
                }
            }
            catch (Exception ex)
            {
                _queueReader.SetPostion(_logger.LastPos);
                if (OnProcessError != null)
                {
                    OnProcessError(last, ex);
                }
            }
            finally
            {
                IsRuning = false;
            }
        }

        protected void Dispose(bool isdispose)
        {
            if (_isdispose)
            {
                if (_queueWriter != null)
                {
                    _queueWriter.Flush();
                    _queueWriter.Dispose();
                    _queueWriter = null;
                }

                if (_queueReader != null)
                {
                    _queueReader.Dispose();
                    _queueReader = null;
                }

                SaveConfig();

                _backtimer.Dispose();

                _isdispose = true;
            }
        }

        private bool _isdispose = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LocalFileQueue()
        {
            Dispose(false);
        }
    }
}
