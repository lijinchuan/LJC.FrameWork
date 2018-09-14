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
        private DateTime _lastCfgChageTime = DateTime.MinValue;
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

        private bool _onerrorresumenext = false;
        public bool OnErrorResumeNext
        {
            get
            {
                return _onerrorresumenext;
            }
            set
            {
                _onerrorresumenext = value;
            }
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

        public LocalFileQueue(string queuename, string queuefilepath,bool canwrite=true,bool canread=true)
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

            if (canwrite)
            {
                //由于文件写是独占式的，当程序快速重启时，文件可能未及时释放
                int trytimes = 0;
                while (true)
                {
                    try
                    {
                        //_queueWriter = ObjTextWriter.CreateWriter(queuefilepath, ObjTextReaderWriterEncodeType.jsonbuf);
                        _queueWriter = ObjTextWriter.CreateWriter(queuefilepath, ObjTextReaderWriterEncodeType.entitybuf);
                        _queueWriter.Flush();
                        break;
                    }
                    catch (Exception ex)
                    {
                        trytimes++;
                        if (trytimes >= 3)
                        {
                            throw ex;
                        }
                        Thread.Sleep(1000 * trytimes);
                    }
                }
            }

            if (canread)
            {
                while (true)
                {
                    if (File.Exists(queuefilepath))
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
                _queueReader = ObjTextReader.CreateReader(queuefilepath);

                FileInfo finfo = new FileInfo(queuefilepath);
                QueueCfgFile = finfo.Directory.FullName + "\\" + queuename + ".cfg";
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
                    _logger.LastChageTime = DateTime.Now;
                    _logger.QueueFile = queuefilepath;
                    SaveConfig();
                }
            }

            _backtimer = new Timer(new TimerCallback(TimerAction), null, 0, 0);
        }

        private void TimerAction(object o)
        {
            var tc = DateTime.Now;
            try
            {
                _backtimer.Change(Timeout.Infinite, Timeout.Infinite);
                if (_queueWriter != null && !this._isdispose)
                {
                    _queueWriter.Flush();
                }

                if (!IsRuning && _queueReader != null)
                {
                    new Action(ProcessQueue).BeginInvoke(null, null);
                }

                SaveConfig();
            }
            finally
            {
                _backtimer.Change((int)Math.Max(1000 - (DateTime.Now.Subtract(tc)).TotalMilliseconds, 0), 0);
            }
        }

        public void Enqueue(T obj)
        {
            if (obj.Equals(default(T)))
            {
                throw new Exception("null值不能写入队列");
            }
            if (_queueWriter == null)
            {
                throw new NotSupportedException("不支持写入");
            }
            _queueWriter.AppendObject(obj);
        }

        private void ProcessBadQueue(T last)
        {
            if (_queueReader == null)
            {
                return;
            }
            var oldpostion = _queueReader.ReadedPostion();

            while (true)
            {
                if (_queueReader.PostionNextSplitChar())
                {
                    var newpostion = _queueReader.ReadedPostion();

                    if (_queueReader.ReadObject<T>() != default(T))
                    {
                        _queueReader.SetPostion(newpostion);
                        OnProcessError(last, new Exception(string.Format("队列损环，将尝试读取下一个队列。损坏位置{0}，恢复位置{1}，损坏长度：{2}kb。", oldpostion, newpostion,(newpostion-oldpostion)/1000)));

                        break;
                    }
                }
                else
                {
                    throw new Exception("队列已损坏，无法恢复。");
                }
            }
        }

        private void ProcessQueue()
        {
            if (OnProcessQueue == null)
            {
                return;
            }
            lock (this)
            {
                if (IsRuning)
                {
                    return;
                }
                IsRuning = true;
            }

            T last = null;
            int errortimes = 0;
            try
            {
                //ProcessBadQueue(last);
                foreach (var t in _queueReader.ReadObjectsWating<T>())
                {
                    last = t;
                    if (OnProcessQueue(t))
                    {
                        errortimes = 0;
                        _logger.LastPos = _queueReader.ReadedPostion();
                        _logger.LastChageTime = DateTime.Now;

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
            catch (Newtonsoft.Json.JsonReaderException)
            {
                ProcessBadQueue(last);
            }
            catch (Newtonsoft.Json.JsonSerializationException)
            {
                ProcessBadQueue(last);
            }
            catch (Exception ex)
            {
                if (OnErrorResumeNext)
                {
                    _queueReader.SetPostion(_logger.LastPos);
                }
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
            if (isdispose)
            {
                _backtimer.Dispose();
                if (_queueWriter != null)
                {
                    _queueWriter.Flush();
                    _queueWriter.Dispose();
                }

                if (_queueReader != null)
                {
                    _queueReader.Dispose();
                }

                SaveConfig();

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
