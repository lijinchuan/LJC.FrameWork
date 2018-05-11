using LJC.FrameWork.Collections;
using LJC.FrameWork.Comm;
using LJC.FrameWork.Comm.Coroutine;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LJC.FrameWork.Data.EntityDataBase
{
    public class BigEntityTableEngine
    {
        Dictionary<string, EntityTableMeta> metadic = new Dictionary<string, EntityTableMeta>();
        //磁盘索引
        ConcurrentDictionary<string, BigEntityTableIndexItem[]> keyindexarrdic = new ConcurrentDictionary<string, BigEntityTableIndexItem[]>();
        //内存索引
        ConcurrentDictionary<string, Dictionary<string, BigEntityTableIndexItem>> keyindexlistdic = new ConcurrentDictionary<string, Dictionary<string, BigEntityTableIndexItem>>();
        Dictionary<string, object> keylocker = new Dictionary<string, object>();
        /// <summary>
        /// 索引缓存
        /// </summary>
        ConcurrentDictionary<string, BigEntityTableIndexItemBag> indexdic = new ConcurrentDictionary<string, BigEntityTableIndexItemBag>();

        string dirbase = System.AppDomain.CurrentDomain.BaseDirectory + "\\localdb\\";

        Dictionary<string, ObjTextWriter> writerdic = new Dictionary<string, ObjTextWriter>();

        public static BigEntityTableEngine LocalEngine = new BigEntityTableEngine(null);

        private const int MERGE_TRIGGER_NEW_COUNT = 10000;
        /// <summary>
        /// 最大单个key占用内存
        /// </summary>
        private const long MAX_KEYBUFFER = 500 * 1000 * 1000;

        class LockerDestroy : ICoroutineUnit
        {
            private DateTime _timeadd = DateTime.Now;
            private Dictionary<string, object> _lockerdic;
            private string _lockerkey;
            public bool IsSuccess()
            {
                return false;
            }

            private bool _isdone = false;
            public bool IsDone()
            {
                return _isdone;
            }

            public bool IsTimeOut()
            {
                return false;
            }


            public LockerDestroy(Dictionary<string, object> lockerdic, string lockkey)
            {
                this._lockerdic = lockerdic;
                this._lockerkey = lockkey;
            }

            public void Exceute()
            {
                if (DateTime.Now.Subtract(_timeadd).TotalSeconds > 60)
                {
                    _isdone = true;
                }
            }

            private object _result = null;
            public object GetResult()
            {
                return _result;
            }

            public void CallBack(CoroutineCallBackEventArgs args)
            {
                lock (this._lockerdic)
                {
                    this._lockerdic.Remove(this._lockerkey);
                }
            }
        }

        class IndexDestroy : ICoroutineUnit
        {
            private ConcurrentDictionary<string, BigEntityTableIndexItemBag> _dic;
            private string _key;
            public bool IsSuccess()
            {
                return false;
            }

            private bool _isdone = false;
            public bool IsDone()
            {
                return _isdone;
            }

            public bool IsTimeOut()
            {
                return false;
            }


            public IndexDestroy(ConcurrentDictionary<string, BigEntityTableIndexItemBag> dic, string key)
            {
                this._dic = dic;
                this._key = key;
            }

            public void Exceute()
            {
                BigEntityTableIndexItemBag val = null;
                _dic.TryGetValue(_key, out val);
                if (val != null)
                {
                    lock (val)
                    {
                        if (DateTime.Now.Subtract(val.LastUsed).TotalSeconds > 30)
                        {
                            BigEntityTableIndexItemBag val0;
                            _dic.TryRemove(_key, out val0);
                            _isdone = true;
                        }
                    }
                }
            }

            private object _result = null;
            public object GetResult()
            {
                return _result;
            }

            public void CallBack(CoroutineCallBackEventArgs args)
            {

            }
        }

        class WriterDestroy : ICoroutineUnit
        {
            private Dictionary<string, ObjTextWriter> _dic;
            private string _key;
            private int _locksecs = 1;
            public bool IsSuccess()
            {
                return false;
            }

            private bool _isdone = false;
            public bool IsDone()
            {
                return _isdone;
            }

            public bool IsTimeOut()
            {
                return false;
            }


            public WriterDestroy(Dictionary<string, ObjTextWriter> dic, string key, int locksecs = 1)
            {
                this._dic = dic;
                this._key = key;
                this._locksecs = locksecs;
            }

            public void Exceute()
            {
                ObjTextWriter o;
                if (!_dic.TryGetValue(_key, out o))
                {
                    _isdone = true;
                    return;
                }
                if (o != null)
                {
                    lock (o)
                    {
                        if (DateTime.Now.Subtract((DateTime)o.Tag).TotalSeconds > _locksecs)
                        {
                            lock (_dic)
                            {
                                o.Dispose();
                                _dic.Remove(_key);
                            }
                            _isdone = true;
                        }
                    }
                }
                else
                {
                    _isdone = true;
                }
            }

            private object _result = null;
            public object GetResult()
            {
                return _result;
            }

            public void CallBack(CoroutineCallBackEventArgs args)
            {
            }
        }

        public BigEntityTableEngine(string dir)
        {
            if (!string.IsNullOrWhiteSpace(dir))
            {
                this.dirbase = dir;
            }

            IOUtil.MakeDirs(this.dirbase);
        }

        private string GetTableFile(string tablename)
        {
            string tablefile = dirbase + "\\" + tablename + ".etb";
            return tablefile;
        }

        private string GetMetaFile(string tablename)
        {
            string metafile = dirbase + "\\" + tablename + ".meta";
            return metafile;
        }

        private string GetKeyFile(string tablename)
        {
            string keyfile = dirbase + "\\" + tablename + ".id";
            return keyfile;
        }

        private string GetIndexFile(string tablename, string indexname)
        {
            string indexfile = dirbase + "\\" + tablename + "##" + indexname + ".id";
            return indexfile;
        }

        public void CreateTable(string tablename, string keyname, Type ttype, string[] indexs = null)
        {
            string tablefile = GetTableFile(tablename);
            bool delfile = true;
            if (!File.Exists(tablefile))
            {
                try
                {
                    using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                    //ObjTextWriter otw = GetWriter(tablefile);
                    //lock(otw)
                    {
                        string metafile = GetMetaFile(tablename);
                        if (!File.Exists(metafile))
                        {
                            EntityTableMeta meta = new EntityTableMeta();
                            meta.KeyName = keyname;
                            meta.Indexs = indexs ?? new string[] { };
                            meta.CTime = DateTime.Now;
                            meta.TType = ttype;
                            var pp = ttype.GetProperty(keyname);
                            if (pp == null)
                            {
                                throw new Exception("找不到主键:" + keyname);
                            }
                            meta.KeyProperty = new PropertyInfoEx(pp);

                            if (indexs != null && indexs.Length > 0)
                            {
                                if (indexs.Contains(keyname))
                                {
                                    throw new Exception("索引不能包含主键");
                                }
                                indexs = indexs.Distinct().ToArray();
                                foreach (var idx in indexs)
                                {
                                    var idxpp = ttype.GetProperty(idx);
                                    if (idxpp == null)
                                    {
                                        throw new Exception("找不到索引:" + idx);
                                    }

                                    if (!meta.IndexProperties.ContainsKey(idx))
                                    {
                                        meta.IndexProperties.Add(idx, new PropertyInfoEx(idxpp));
                                    }
                                }
                            }

                            LJC.FrameWork.Comm.SerializerHelper.SerializerToXML<EntityTableMeta>(meta, metafile, catchErr: true);
                            metadic.Add(tablename, meta);

                            keyindexarrdic.TryAdd(tablename, new BigEntityTableIndexItem[0]);
                            keyindexlistdic.TryAdd(tablename, new Dictionary<string, BigEntityTableIndexItem>());
                        }
                        else
                        {
                            var meta = LJC.FrameWork.Comm.SerializerHelper.DeSerializerFile<EntityTableMeta>(metafile, true);
                            if (!meta.KeyName.Equals(keyname) || !meta.TypeString.Equals(ttype))
                            {
                                delfile = false;

                                throw new Exception("meta文件内容检查不一致");
                            }
                        }

                        //索引
                        string keyfile = GetKeyFile(tablename);
                        using (ObjTextWriter keyidxwriter = ObjTextWriter.CreateWriter(keyfile, ObjTextReaderWriterEncodeType.entitybuf))
                        //ObjTextWriter keyidxwriter = GetWriter(keyfile);
                        {
                        }

                        if (indexs != null)
                        {
                            foreach (var idx in indexs)
                            {
                                var indexfile = GetIndexFile(tablename, idx);
                                using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                                //ObjTextWriter idxwriter = GetWriter(indexfile);
                                {
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    if (delfile && File.Exists(tablefile))
                    {
                        File.Delete(tablefile);
                    }
                    throw ex;
                }
            }
        }

        private object GetKeyLocker(string table, string key)
        {
            string totalkey = string.Format("{0}:{1}", table, key);
            object locker = null;
            if (keylocker.TryGetValue(totalkey, out locker))
            {
                return locker;
            }

            lock (keylocker)
            {
                if (keylocker.TryGetValue(totalkey, out locker))
                {
                    return locker;
                }

                locker = new object();
                keylocker.Add(totalkey, locker);

                //CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new LockerDestroy(keylocker, totalkey));
            }

            return locker;
        }

        public void MergeIndex(string tablename, string indexname)
        {
            var meta = GetMetaData(tablename);
            MergeIndex(tablename, indexname, meta);
        }

        public void MergeIndex(string tablename, string indexname, EntityTableMeta meta)
        {
            //Console.WriteLine("开始整理索引");

            IndexMergeInfo mergeinfo = null;

            lock (meta)
            {
                mergeinfo = meta.IndexMergeInfos.Find(p => indexname.Equals(p.IndexName));
                if (mergeinfo == null)
                {
                    mergeinfo = new IndexMergeInfo();
                    mergeinfo.IndexName = indexname;
                    meta.IndexMergeInfos.Add(mergeinfo);
                }

                if (mergeinfo.IsMergin)
                {
                    return;
                }
                meta.NewAddCount = 0;
                mergeinfo.IsMergin = true;
            }
            DateTime timestart = DateTime.Now;
            try
            {
                long lasmargepos = 0;
                long newIndexMergePos = 0;
                string newindexfile = string.Empty;
                string indexfile = indexname.Equals(meta.KeyName) ? GetKeyFile(tablename) : GetIndexFile(tablename, indexname);
                using (var reader = ObjTextReader.CreateReader(indexfile))
                {
                    long readstartpostion = reader.ReadedPostion();
                    if (mergeinfo.IndexMergePos > 0)
                    {
                        reader.SetPostion(mergeinfo.IndexMergePos);
                    }

                    var listtemp = new List<BigEntityTableIndexItem>();
                    int readcount = 0;

                    foreach (var obj in reader.ReadObjectsWating<BigEntityTableIndexItem>(1))
                    {
                        listtemp.Add(obj);
                        if (++readcount > 5000000)
                        {
                            break;
                        }
                    }

                    if (readcount == 0)
                    {
                        return;
                    }

                    lasmargepos = reader.ReadedPostion();

                    listtemp = listtemp.OrderBy(p => p).ToList();

                    newindexfile = (indexname.Equals(meta.KeyName) ? GetKeyFile(tablename) : GetIndexFile(tablename, indexname)) + ".temp";
                    bool isall = false;
                    while (true)
                    {
                        Console.WriteLine("readstartpostion->" + readstartpostion);
                        reader.SetPostion(readstartpostion);
                        var listordered = new List<BigEntityTableIndexItem>();
                        var loadcount = 0;
                        foreach (var item in reader.ReadObjectsWating<BigEntityTableIndexItem>(1))
                        {
                            if (item.KeyOffset >= mergeinfo.IndexMergePos)
                            {
                                break;
                            }
                            listordered.Add(item);
                            if (++loadcount >= 1000000)
                            {
                                break;
                            }
                        }

                        readstartpostion = reader.ReadedPostion();

                        if (listordered.Count == 0)
                        {
                            Console.WriteLine("顺序列表为空，无序列表条数:" + listtemp.Count);

                            listordered = listtemp;
                            Console.WriteLine("--->" + listtemp.Count);
                            isall = true;
                        }
                        else
                        {
                            Console.WriteLine("顺序列表不为空:" + listordered.Count + "，无序列表条数:" + listtemp.Count);

                            var subtemplist = listtemp.Where(p => p.CompareTo(listordered.Last()) < 0).ToList();
                            listordered.AddRange(subtemplist);
                            listordered = listordered.OrderBy(p => p).ToList();

                            //存储
                            listtemp = listtemp.Skip(subtemplist.Count).ToList();
                        }

                        int mid = 0;
                        var pos = new LJC.FrameWork.Collections.SorteArray<BigEntityTableIndexItem>(listordered.ToArray()).Find(new BigEntityTableIndexItem { Key = "name7" }, ref mid);
                        if (pos == -1)
                        {
                            Console.WriteLine("查不到");
                        }
                        else
                        {
                            Console.WriteLine("能查到");
                        }

                        using (var nw = ObjTextWriter.CreateWriter(newindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                        {
                            foreach (var item in listordered)
                            {
                                item.KeyOffset = nw.GetWritePosition();
                                nw.AppendObject(item);
                            }
                            newIndexMergePos = nw.GetWritePosition();
                        }

                        if (isall)
                        {
                            break;
                        }
                    }
                }

                mergeinfo.IndexMergePos = newIndexMergePos;

                string tablefile = GetTableFile(tablename);
                var locker = GetKeyLocker(tablename, string.Empty);
                //using (var writer = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                //var writer = GetWriter(indexfile, secs);
                //lock (writer)

                var idxreader = ObjTextReader.CreateReader(indexfile);
                var newwriter = ObjTextWriter.CreateWriter(newindexfile, ObjTextReaderWriterEncodeType.entitybuf);
                try
                {
                    idxreader.SetPostion(lasmargepos);
                    foreach (var item in idxreader.ReadObjectsWating<BigEntityTableIndexItem>(1))
                    {
                        item.KeyOffset = newwriter.GetWritePosition();
                        newwriter.AppendObject(item);
                    }

                    lock (locker)
                    {
                        using (newwriter)
                        {
                            using (idxreader)
                            {
                                foreach (var item in idxreader.ReadObjectsWating<BigEntityTableIndexItem>(1))
                                {
                                    item.KeyOffset = newwriter.GetWritePosition();
                                    newwriter.AppendObject(item);
                                }

                            }
                        }

                        idxreader = null;

                        Console.WriteLine("删除源索引文件");

                        File.Delete(indexfile);

                        
                        Console.WriteLine("更改源文件");
                        File.Move(newindexfile, indexfile);
                    }
                    

                    string metafile = GetMetaFile(tablename);

                    LJC.FrameWork.Comm.SerializerHelper.SerializerToXML(meta, metafile, true);

                    Console.WriteLine("整理索引完成：" + (DateTime.Now.Subtract(timestart).TotalMilliseconds));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("整理出错" + ex.ToString());
                }
                finally
                {
                    if (idxreader != null)
                    {
                        idxreader.Dispose();
                    }
                }

            }
            finally
            {
                mergeinfo.IsMergin = false;
            }
        }

        private void LoadKey(string tablename, EntityTableMeta meta)
        {
            string indexfile = GetKeyFile(tablename);
            var indexmergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
            if (indexmergeinfo == null)
            {
                indexmergeinfo = new IndexMergeInfo();
                indexmergeinfo.IndexName = meta.KeyName;
                meta.IndexMergeInfos.Add(indexmergeinfo);
            }

            //计算加载因子
            indexmergeinfo.LoadFactor = (int)Math.Max(4, new FileInfo(indexfile).Length / MAX_KEYBUFFER);

            int i = 0;
            BigEntityTableIndexItem lastreadindex = null;
            List<BigEntityTableIndexItem> list = new List<BigEntityTableIndexItem>();
            using (ObjTextReader idx = ObjTextReader.CreateReader(indexfile))
            {
                Console.WriteLine("loadkey");

                foreach (var newindex in idx.ReadObjectsWating<BigEntityTableIndexItem>(1))
                {
                    if (newindex.KeyOffset == indexmergeinfo.IndexMergePos)
                    {
                        //list.Add(newindex);
                        if (list.Count > 0)
                        {
                            if (list.Last().KeyOffset != lastreadindex.KeyOffset)
                            {
                                list.Add(lastreadindex);
                            }
                        }
                        break;
                    }
                    if (!newindex.Del)
                    {
                        if (indexmergeinfo.LoadFactor == 1 || i % indexmergeinfo.LoadFactor == 0)
                        {
                            list.Add(newindex);
                        }
                        i++;
                        lastreadindex = newindex;
                    }
                }
            }
            indexmergeinfo.TotalCount = i;

            BigEntityTableIndexItem[] oldindexitems = null;
            keyindexarrdic.TryRemove(tablename, out oldindexitems);
            keyindexarrdic.TryAdd(tablename, list.ToArray());

            using (ObjTextReader idr = ObjTextReader.CreateReader(indexfile))
            {
                Console.WriteLine("loadkey2");

                if (indexmergeinfo.IndexMergePos > 0)
                {
                    idr.SetPostion(indexmergeinfo.IndexMergePos);
                }
                var indexdic = keyindexlistdic[tablename];

                foreach (var newindex in idr.ReadObjectsWating<BigEntityTableIndexItem>(1))
                {
                    if (!newindex.Del)
                    {
                        indexdic.Add(newindex.Key, newindex);
                    }
                }
            }
        }

        private BigEntityTableIndexItemBag LoadIndex(string tablename, string indexname, EntityTableMeta meta)
        {
            string key = string.Format("{0}##{1}", tablename, indexname);
            BigEntityTableIndexItemBag temp = null;
            if (indexdic.TryGetValue(key, out temp))
            {
                temp.LastUsed = DateTime.Now;
            }

            string indexfile = GetIndexFile(tablename, indexname);
            var locker = GetKeyLocker(tablename, "index_" + indexname);
            lock (locker)
            {
                if (indexdic.TryGetValue(key, out temp))
                {
                    temp.LastUsed = DateTime.Now;
                }
                else
                {
                    temp = new BigEntityTableIndexItemBag();
                }

                using (ObjTextReader idxreader = ObjTextReader.CreateReader(indexfile))
                {
                    Console.WriteLine("loadindex");
                    if (temp.LastOffset > 0)
                    {
                        idxreader.SetPostion(temp.LastOffset);
                    }

                    Dictionary<long, BigEntityTableIndexItem> al = null;
                    foreach (var newindex in idxreader.ReadObjectsWating<BigEntityTableIndexItem>(1))
                    {
                        temp.LastOffset = idxreader.ReadedPostion();
                        if (!temp.Dics.TryGetValue(newindex.Key, out al))
                        {
                            al = new Dictionary<long, BigEntityTableIndexItem>();
                            temp.Dics.TryAdd(newindex.Key, al);
                        }

                        if (newindex.Del)
                        {
                            al.Remove(newindex.Offset);
                        }
                        else
                        {
                            al.Add(newindex.Offset, newindex);
                        }
                    }
                }

                if (temp.LastUsed == DateTime.MinValue)
                {
                    temp.LastUsed = DateTime.Now;
                    indexdic.TryAdd(key, temp);
                    LJC.FrameWork.Comm.Coroutine.CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new IndexDestroy(indexdic, key));
                }
                else
                {
                    temp.LastUsed = DateTime.Now;
                }
            }

            return temp;
        }

        private EntityTableMeta GetMetaData(string tablename)
        {
            EntityTableMeta meta = null;
            if (metadic.TryGetValue(tablename, out meta))
            {
                return meta;
            }

            var locker = GetKeyLocker(tablename, "GetMetaData");
            lock (locker)
            {
                if (metadic.TryGetValue(tablename, out meta))
                {
                    return meta;
                }

                string metafile = GetMetaFile(tablename);
                if (!File.Exists(metafile))
                {
                    throw new Exception("找不到元文件:" + tablename);
                }

                meta = LJC.FrameWork.Comm.SerializerHelper.DeSerializerFile<EntityTableMeta>(metafile, true);
                if (meta.Indexs == null)
                {
                    meta.Indexs = new string[] { };
                }
                meta.KeyProperty = new PropertyInfoEx(meta.TType.GetProperty(meta.KeyName));

                if (meta.Indexs != null)
                {
                    foreach (var idx in meta.Indexs)
                    {
                        var idxpp = meta.TType.GetProperty(idx);
                        if (idxpp == null)
                        {
                            throw new Exception("找不到索引:" + idx);
                        }

                        if (!meta.IndexProperties.ContainsKey(idx))
                        {
                            meta.IndexProperties.Add(idx, new PropertyInfoEx(idxpp));
                        }
                    }
                }

                if (!metadic.ContainsKey(tablename))
                {
                    lock (metadic)
                    {
                        if (!metadic.ContainsKey(tablename))
                        {
                            metadic.Add(tablename, meta);
                        }
                    }
                }

                if (!keyindexlistdic.ContainsKey(tablename))
                {
                    lock (keyindexlistdic)
                    {
                        if (!keyindexlistdic.ContainsKey(tablename))
                        {
                            keyindexlistdic.TryAdd(tablename, new Dictionary<string, BigEntityTableIndexItem>());
                        }
                    }
                }

                LoadKey(tablename, meta);

                return meta;
            }
        }

        private ObjTextWriter GetWriter(string filename, int locksecs = 1)
        {
            ObjTextWriter writer = null;
            if (writerdic.TryGetValue(filename, out writer))
            {
                lock (writer)
                {
                    if (!writer.Isdispose)
                    {
                        writer.Tag = DateTime.Now;
                        return writer;
                    }
                }
            }

            lock (writerdic)
            {
                if (writerdic.TryGetValue(filename, out writer))
                {
                    lock (writer)
                    {
                        if (!writer.Isdispose)
                        {
                            writer.Tag = DateTime.Now;
                            return writer;
                        }
                        else
                        {
                            writerdic.Remove(filename);
                        }
                    }
                }

                writer = ObjTextWriter.CreateWriter(filename, ObjTextReaderWriterEncodeType.entitybuf);
                writer.Tag = DateTime.Now;
                writerdic.Add(filename, writer);

                LJC.FrameWork.Comm.Coroutine.CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new WriterDestroy(writerdic, filename, locksecs));
            }
            return writer;
        }

        private bool Insert2<T>(string tablename, IEnumerable<T> items, EntityTableMeta meta) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            var locker = GetKeyLocker(tablename, string.Empty);
            string keyindexfile = GetKeyFile(tablename);

            lock (locker)
            {
                var otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf);
                ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf);
                Dictionary<string, ObjTextWriter> idxwriterdic = new Dictionary<string, ObjTextWriter>();
                try
                {
                    foreach (var item in items)
                    {
                        var keyvalue = item.Eval(meta.KeyProperty);
                        if (keyvalue == null)
                        {
                            throw new Exception("key值不能为空");
                        }

                        var keystr = keyvalue.ToString();

                        var keylocker = GetKeyLocker(tablename, keystr);

                        lock (keylocker)
                        {
                            if (keyindexlistdic[tablename].ContainsKey(keystr))
                            {
                                throw new Exception(string.Format("key:{0}不可重复", keystr));
                            }
                        }


                        var tableitem = new EntityTableItem<T>(item);
                        tableitem.Flag = EntityTableItemFlag.Ok;

                        //var otw = GetWriter(tablefile);
                        //lock(otw)
                        {
                            var offset = otw.AppendObject(tableitem);

                            var keylist = keyindexlistdic[tablename];

                            var newkey = new BigEntityTableIndexItem
                            {
                                Key = keystr,
                                Offset = offset.Item1,
                                len = (int)(offset.Item2 - offset.Item1)
                            };

                            keylist.Add(newkey.Key, newkey);

                            //string keyindexfile = GetKeyFile(tablename);
                            //ObjTextWriter keywriter = GetWriter(keyindexfile);
                            //lock (keywriter)
                            {
                                newkey.KeyOffset = keywriter.GetWritePosition();
                                keywriter.AppendObject(newkey);
                            }

                            foreach (var idx in meta.Indexs)
                            {
                                string indexfile = GetIndexFile(tablename, idx);
                                var indexvalue = item.Eval(meta.IndexProperties[idx]);
                                var newindex = new BigEntityTableIndexItem
                                {
                                    Key = indexvalue == null ? string.Empty : indexvalue.ToString(),
                                    Offset = offset.Item1,
                                    len = (int)(offset.Item2 - offset.Item1)
                                };

                                ObjTextWriter idxwriter = null;
                                if (!idxwriterdic.TryGetValue(indexfile, out idxwriter))
                                {
                                    idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf);
                                    idxwriterdic.Add(indexfile, idxwriter);
                                }

                                //ObjTextWriter idxwriter = GetWriter(indexfile);
                                //lock (idxwriter)
                                {
                                    newindex.KeyOffset = idxwriter.GetWritePosition();
                                    idxwriter.AppendObject(newindex);
                                }
                            }
                        }
                    }

                    meta.NewAddCount += items.Count();
                    if (meta.NewAddCount >= MERGE_TRIGGER_NEW_COUNT)
                    {
                        //meta.NewAddCount = 0;
                        new Action(() => MergeIndex(tablename, meta.KeyName)).BeginInvoke(null, null);
                    }
                }
                finally
                {
                    using (otw) { };
                    using (keywriter) { };

                    foreach (var kv in idxwriterdic)
                    {
                        using (kv.Value) { }
                    }
                }
            }

            return true;
        }

        public bool Insert<T>(string tablename, T item) where T : new()
        {
            //item.Eval()
            EntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != item.GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }

            return Insert2(tablename, new T[] { item }, meta);
        }

        public bool InsertBatch<T>(string tablename, IEnumerable<T> items) where T : new()
        {
            if (items == null || items.Count() == 0)
            {
                return false;
            }

            EntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != items.First().GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }

            return Insert2(tablename, items, meta);
        }

        public bool Delete(string tablename, string key)
        {
            EntityTableMeta meta = GetMetaData(tablename);

            var delkey=FindKey(tablename, key);
            if (delkey == null)
            {
                return false;
            }

            var locker = GetKeyLocker(tablename, string.Empty);
            lock (locker)
            {
                string keyindexfile = GetKeyFile(tablename);
                using (ObjTextWriter keywriter=ObjTextWriter.CreateWriter(keyindexfile,ObjTextReaderWriterEncodeType.entitybuf))
                //ObjTextWriter keywriter = GetWriter(keyindexfile);
                //lock (keywriter)
                {
                    delkey.Del = true;

                    keywriter.SetPosition(delkey.KeyOffset);
                    keywriter.AppendObject(delkey);

                    foreach (var idx in meta.Indexs)
                    {
                        //string indexfile = GetIndexFile(tablename, idx);
                        //ObjTextWriter idxwriter = GetWriter(indexfile);
                        //lock (idxwriter)
                        //{
                        //    delitem.KeyOffset = idxwriter.GetWritePosition();
                        //    idxwriter.AppendObject(delitem);
                        //}
                    }
                }
            }

            return true;
        }

        public bool DeleteMem(string tablename, string key)
        {
            EntityTableMeta meta = GetMetaData(tablename);
            var dic = keyindexlistdic[tablename];

            var locker = GetKeyLocker(tablename, string.Empty);
            lock (locker)
            {
                BigEntityTableIndexItem delitem=null;
                if (!dic.TryGetValue(key, out delitem))
                {
                    return false;
                }

                string keyindexfile = GetKeyFile(tablename);
                using (ObjTextWriter keywriter=ObjTextWriter.CreateWriter(keyindexfile,ObjTextReaderWriterEncodeType.entitybuf))
                //ObjTextWriter keywriter = GetWriter(keyindexfile);
                //lock (keywriter)
                {
                    delitem.Del = true;

                    keywriter.SetPosition(delitem.KeyOffset);
                    keywriter.AppendObject(delitem);

                    foreach (var idx in meta.Indexs)
                    {
                        //string indexfile = GetIndexFile(tablename, idx);
                        //ObjTextWriter idxwriter = GetWriter(indexfile);
                        //lock (idxwriter)
                        //{
                        //    delitem.KeyOffset = idxwriter.GetWritePosition();
                        //    idxwriter.AppendObject(delitem);
                        //}
                    }
                }
            }

            return true;
        }

        public bool UpsertMem<T>(string tablename, T item) where T : new()
        {
            EntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != item.GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }

            var keyobj = item.Eval(meta.KeyProperty);
            if (keyobj == null)
            {
                throw new Exception("key不能为空");
            }

            string key = keyobj.ToString();
            var indexdic = keyindexlistdic[tablename];
            BigEntityTableIndexItem upitem = null;
            if (indexdic.TryGetValue(key, out upitem))
            {
                return Update2(tablename, key, item, meta);
            }
            else
            {
                return Insert2(tablename, new T[] { item }, meta);
            }
        }

        private bool Update2<T>(string tablename, string key, T item, EntityTableMeta meta) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            Dictionary<string,BigEntityTableIndexItem> dic = keyindexlistdic[tablename];
            Tuple<long, long> offset = null;

            var locker = GetKeyLocker(tablename, string.Empty);
            lock (locker)
            {
                BigEntityTableIndexItem indexitem = null;

                if (!dic.TryGetValue(key, out indexitem))
                {
                    throw new Exception(string.Format("更新失败，key为{0}的记录数为0", key));
                }

                string keyindexfile = GetKeyFile(tablename);
                indexitem.Del = true;
                //ObjTextWriter keywriter = GetWriter(keyindexfile);
                using(ObjTextWriter keywriter=ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                //lock (keywriter)
                {
                    keywriter.SetPosition(indexitem.KeyOffset);
                    keywriter.AppendObject(indexitem);
                }

                var keyvalue = item.Eval(meta.KeyProperty);
                if (keyvalue == null)
                {
                    throw new Exception("key值不能为空");
                }
                var tableitem = new EntityTableItem<T>(item);
                tableitem.Flag = EntityTableItemFlag.Ok;
                //ObjTextWriter otw = GetWriter(tablefile);
                using(ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                {
                    offset = otw.PreAppendObject(tableitem, (s1, s2) =>
                    {
                        if (s1.Length <= indexitem.len)
                        {
                            return otw.Override(indexitem.Offset, s1);
                        }
                        return null;
                    });
                }


                var keylist = keyindexlistdic[tablename];

                BigEntityTableIndexItem newkey = new BigEntityTableIndexItem
                {
                    Key = keyvalue.ToString(),
                    Offset = offset.Item1,
                    len = (int)(offset.Item2 - offset.Item1),
                    Del = false
                };

                if (newkey.Offset == indexitem.Offset)
                {
                    newkey.len = indexitem.len;
                }
                
                dic.Add(newkey.Key,newkey);

                using(ObjTextWriter keyidxwriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                //ObjTextWriter keyidxwriter = GetWriter(keyindexfile);
                //lock(keyidxwriter)
                {
                    newkey.KeyOffset = keyidxwriter.GetWritePosition();
                    keyidxwriter.AppendObject(newkey);
                }

                foreach (var idx in meta.Indexs)
                {
                    string indexfile = GetIndexFile(tablename, idx);
                    var indexvalue = item.Eval(meta.IndexProperties[idx]);
                    var newindex = new BigEntityTableIndexItem
                    {
                        Key = indexvalue == null ? string.Empty : indexvalue.ToString(),
                        Offset = offset.Item1,
                        len = (int)(offset.Item2 - offset.Item1),
                        Del = false
                    };
                    using(ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                    //ObjTextWriter idxwriter = GetWriter(indexfile);
                    //lock(idxwriter)
                    {
                        newindex.KeyOffset = idxwriter.GetWritePosition();
                        idxwriter.AppendObject(newindex);
                    }
                }

                Console.WriteLine("写入成功:" + keyvalue + "->" + offset);
            }
            return true;
        }

        public bool Update<T>(string tablename, T item) where T : new()
        {
            EntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != item.GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }

            var keyobj = item.Eval(meta.KeyProperty);
            if (keyobj == null)
            {
                throw new Exception("key不能为空");
            }

            string key = keyobj.ToString();

            return Update2(tablename, key, item,meta);
        }

        public bool Exists(string tablename, string key)
        {
            return FindKey(tablename, key) != null;
        }

        public IEnumerable<T> ListMemAll<T>(string tablename) where T:new()
        {
            var meta = this.GetMetaData(tablename);

            var listkeys = keyindexlistdic[tablename];
            foreach (var key in listkeys)
            {
                yield return FindMem<T>(tablename,key.Key);
            }
        }

        public IEnumerable<T> List<T>(string tablename,int pi,int ps) where T : new()
        {
            int count = 0;
            var start = (pi - 1) * ps;
            var end = pi * ps;
            using (var reader = ObjTextReader.CreateReader(GetTableFile(tablename)))
            {
                foreach(var item in reader.ReadObjectsWating<EntityTableItem<T>>(1))
                {
                    if (item == null)
                    {
                        yield break;
                    }

                    if (count++>=start)
                    {
                        yield return item.Data;
                    }

                    if (count == end)
                    {
                        yield break;
                    }
                }
            }
        }

        public int Count(string tablename)
        {
            var meta = this.GetMetaData(tablename);
            var memkeys = keyindexlistdic[tablename];
            var mergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
            return memkeys.Count + (mergeinfo == null ? 0 : mergeinfo.TotalCount);
        }

        public BigEntityTableIndexItem FindKey(string tablename, string key)
        {
            var meta = GetMetaData(tablename);

            BigEntityTableIndexItem findkeyitem = null;
            if (keyindexlistdic[tablename].TryGetValue(key, out findkeyitem))
            {
                return findkeyitem;
            }

            var indexarr = keyindexarrdic[tablename];
            int mid = -1;
            int pos = new Collections.SorteArray<BigEntityTableIndexItem>(indexarr).Find(new BigEntityTableIndexItem
            {
                Key = key
            }, ref mid);

            if (pos == -1 && (mid == -1 || mid == indexarr.Length - 1))
            {
                return null;
            }
            else if (pos > -1)
            {
                findkeyitem = indexarr[pos];
                if (findkeyitem.Del)
                {
                    return null;
                }
            }

            if (findkeyitem == null)
            {
                var keymergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
                if (pos == -1 && keymergeinfo.LoadFactor == 1)
                {
                    return null;
                }

                var posstart = indexarr[mid].KeyOffset;
                var posend = indexarr[mid + 1].KeyOffset;


                using (var reader = ObjTextReader.CreateReader(GetKeyFile(tablename)))
                {
                    reader.SetPostion(posstart);
                    while (true)
                    {
                        var item = reader.ReadObject<BigEntityTableIndexItem>();
                        if (item == null || reader.ReadedPostion() > posend)
                        {
                            break;
                        }
                        if (item.Key.Equals(key))
                        {
                            findkeyitem = item;
                            break;
                        }
                    }
                }
            }

            return findkeyitem;
        }

        public BigEntityTableIndexItem FindDiskKey(string tablename, string key)
        {   
            var indexarr = keyindexarrdic[tablename];
            int mid = -1;
            int pos = new Collections.SorteArray<BigEntityTableIndexItem>(indexarr).Find(new BigEntityTableIndexItem
            {
                Key = key
            }, ref mid);

            BigEntityTableIndexItem findkeyitem = null;
            if (pos == -1 && (mid == -1 || mid == indexarr.Length - 1))
            {
                return null;
            }
            else if (pos > -1)
            {
                findkeyitem = indexarr[pos];
                if (findkeyitem.Del)
                {
                    return null;
                }
            }

            if (findkeyitem == null)
            {
                var meta = GetMetaData(tablename);
                var keymergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
                if (pos == -1 && keymergeinfo.LoadFactor == 1)
                {
                    return null;
                }

                var posstart = indexarr[mid].KeyOffset;
                var posend = indexarr[mid + 1].KeyOffset;


                using (var reader = ObjTextReader.CreateReader(GetKeyFile(tablename)))
                {
                    reader.SetPostion(posstart);
                    while (true)
                    {
                        var item = reader.ReadObject<BigEntityTableIndexItem>();
                        if (item == null || reader.ReadedPostion() > posend)
                        {
                            break;
                        }
                        if (item.Key.Equals(key))
                        {
                            findkeyitem = item;
                            break;
                        }
                    }
                }
            }

            return findkeyitem;

        }

        public T FindDisk<T>(string tablename,string key) where T:new()
        {
            var indexarr = keyindexarrdic[tablename];
            if (indexarr.Length == 0)
            {
                return default(T);
            }

            if (indexarr.Length == 1)
            {
                var index = indexarr.FirstOrDefault();
                if (index == null || index.Del)
                {
                    return default(T);
                }

                if (index.Key.Equals(key))
                {
                    //先找到offset
                    using (ObjTextReader otw = ObjTextReader.CreateReader(GetTableFile(tablename)))
                    {
                        otw.SetPostion(index.Offset);

                        var readobj = otw.ReadObject<EntityTableItem<T>>();
                        if (readobj == null)
                        {
                            return default(T);
                        }
                        else
                        {
                            return readobj.Data;
                        }
                    }
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                int mid=-1;
                int pos=new Collections.SorteArray<BigEntityTableIndexItem>(indexarr).Find(new BigEntityTableIndexItem{
                    Key=key
                },ref mid);

                BigEntityTableIndexItem findkeyitem = null;
                if (pos == -1 && (mid == -1 || mid == indexarr.Length - 1))
                {
                    return default(T);
                }else if (pos > -1)
                {
                    findkeyitem = indexarr[pos];
                    if (findkeyitem.Del)
                    {
                        return default(T);
                    }
                }

                if (findkeyitem == null)
                {
                    var meta = GetMetaData(tablename);
                    var keymergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
                    if (pos == -1 && keymergeinfo.LoadFactor == 1)
                    {
                        return default(T);
                    }

                    var posstart = indexarr[mid].KeyOffset;
                    var posend = indexarr[mid + 1].KeyOffset;


                    using (var reader = ObjTextReader.CreateReader(GetKeyFile(tablename)))
                    {
                        reader.SetPostion(posstart);        

                        while (true)
                        {
                            var item = reader.ReadObject<BigEntityTableIndexItem>();
                            if (item == null || reader.ReadedPostion() > posend)
                            {
                                break;
                            }
                            if (item.Key.Equals(key))
                            {
                                findkeyitem = item;
                                break;
                            }
                        }
                    }
                }

                if (findkeyitem != null)
                {
                    using (var reader = ObjTextReader.CreateReader(GetTableFile(tablename)))
                    {
                        reader.SetPostion(findkeyitem.Offset);

                        var obj = reader.ReadObject<EntityTableItem<T>>();

                        if (obj == null)
                        {
                            return default(T);
                        }

                        return obj.Data;
                    }
                }

                return default(T);
            }
        }

        public T Find<T>(string tablename, string key) where T : new()
        {
            var t = FindMem<T>(tablename, key);
            if (object.Equals(t,default(T)))
            {
                t = FindDisk<T>(tablename, key);
            }

            return t;
        }

        public IEnumerable<T> FindBatch<T>(string tablename, IEnumerable<string> keys) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            EntityTableMeta meta = GetMetaData(tablename);
            var dic = keyindexlistdic[tablename];
            BigEntityTableIndexItem indexitem = null;
            var indexarr = keyindexarrdic[tablename];

            using (ObjTextReader otr = ObjTextReader.CreateReader(tablefile))
            {
                using (var keyreader = ObjTextReader.CreateReader(GetKeyFile(tablename)))
                {
                    foreach (var key in keys)
                    {
                        if (dic.TryGetValue(key, out indexitem))
                        {
                            if (indexitem != null && !indexitem.Del)
                            {
                                otr.SetPostion(indexitem.Offset);

                                var readobj = otr.ReadObject<EntityTableItem<T>>();
                                if (readobj != null)
                                {
                                    yield return readobj.Data;
                                    continue;
                                }
                                else
                                {
                                    yield return default(T);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (indexarr.Length == 0)
                            {
                                yield return default(T);
                                continue;
                            }

                            if (indexarr.Length == 1)
                            {
                                var index = indexarr.FirstOrDefault();
                                if (index == null || index.Del)
                                {
                                    yield return default(T);
                                    continue;
                                }

                                if (index.Key.Equals(key))
                                {
                                    //先找到offset
                                    otr.SetPostion(index.Offset);

                                    var readobj = otr.ReadObject<EntityTableItem<T>>();
                                    if (readobj == null)
                                    {
                                        yield return default(T);
                                        continue;
                                    }
                                    else
                                    {
                                        yield return readobj.Data;
                                        continue;
                                    }
                                }
                                else
                                {
                                    yield return default(T);
                                    continue;
                                }
                            }
                            else
                            {
                                int mid = -1;
                                int pos = new Collections.SorteArray<BigEntityTableIndexItem>(indexarr).Find(new BigEntityTableIndexItem
                                {
                                    Key = key
                                }, ref mid);

                                BigEntityTableIndexItem findkeyitem = null;
                                if (pos == -1 && (mid == -1 || mid == indexarr.Length - 1))
                                {
                                    yield return default(T);
                                    continue;
                                }
                                else if (pos > -1)
                                {
                                    findkeyitem = indexarr[pos];
                                    if (findkeyitem.Del)
                                    {
                                        yield return default(T);
                                        continue;
                                    }
                                }

                                if (findkeyitem == null)
                                {
                                    var keymergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
                                    if (pos == -1 && keymergeinfo.LoadFactor == 1)
                                    {
                                        yield return default(T);
                                        continue;
                                    }

                                    var posstart = indexarr[mid].KeyOffset;
                                    var posend = indexarr[mid + 1].KeyOffset;

                                    keyreader.SetPostion(posstart);

                                    while (true)
                                    {
                                        var item = keyreader.ReadObject<BigEntityTableIndexItem>();
                                        if (item == null || keyreader.ReadedPostion() > posend)
                                        {
                                            break;
                                        }
                                        if (item.Key.Equals(key))
                                        {
                                            findkeyitem = item;
                                            break;
                                        }
                                    }
                                }

                                if (findkeyitem != null)
                                {
                                    otr.SetPostion(findkeyitem.Offset);

                                    var obj = otr.ReadObject<EntityTableItem<T>>();

                                    if (obj == null)
                                    {
                                        yield return default(T);
                                        continue;
                                    }

                                    yield return obj.Data;
                                    continue;
                                }

                                yield return default(T);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        public T FindMem<T>(string tablename, string key) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            EntityTableMeta meta = GetMetaData(tablename);
            var dic = keyindexlistdic[tablename];
            BigEntityTableIndexItem indexitem=null;
            if (dic.TryGetValue(key, out indexitem))
            {
                if (indexitem != null && !indexitem.Del)
                {
                    //先找到offset
                    using (ObjTextReader otw = ObjTextReader.CreateReader(tablefile))
                    {
                        otw.SetPostion(indexitem.Offset);

                        var readobj = otw.ReadObject<EntityTableItem<T>>();
                        if (readobj == null)
                        {
                            return default(T);
                        }
                        else
                        {
                            return readobj.Data;
                        }
                    }
                }
            }

            return default(T);
        }

        public IEnumerable<T> Find<T>(string tablename, string index,string value) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            EntityTableMeta meta = GetMetaData(tablename);
            Dictionary<long, BigEntityTableIndexItem> arr = null;
            BigEntityTableIndexItem indexitem = null;

            var indexobj=LoadIndex(tablename, index, meta);
            if (value == null)
            {
                value = string.Empty;
            }
            if (indexobj.Dics.TryGetValue(value, out arr))
            {
                //先找到offset
                using (ObjTextReader otw = ObjTextReader.CreateReader(tablefile))
                {
                    foreach (var o in arr)
                    {
                        indexitem = (BigEntityTableIndexItem)o.Value;
                        if (!indexitem.Del)
                        {
                            otw.SetPostion(indexitem.Offset);

                            var readobj = otw.ReadObject<EntityTableItem<T>>();
                            if (readobj == null)
                            {
                                yield return default(T);
                            }
                            else
                            {
                                yield return readobj.Data;
                            }
                        }
                    }
                }
            }
        }

    }
}
