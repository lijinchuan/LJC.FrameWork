using LJC.FrameWork.Comm;
using LJC.FrameWork.Comm.Coroutine;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.Data.EntityDataBase
{
    public class EntityTableEngine
    {
        Dictionary<string, EntityTableMeta> metadic = new Dictionary<string, EntityTableMeta>();
        ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<long,EntityTableIndexItem>>> keyindexdic = new ConcurrentDictionary<string,ConcurrentDictionary<string,Dictionary<long,EntityTableIndexItem>>>();
        Dictionary<string, object> keylocker = new Dictionary<string, object>();
        /// <summary>
        /// 索引缓存
        /// </summary>
        ConcurrentDictionary<string, EntityTableIndexItemBag> indexdic = new ConcurrentDictionary<string, EntityTableIndexItemBag>();

        string dirbase = System.AppDomain.CurrentDomain.BaseDirectory+"\\localdb\\";

        Dictionary<string, ObjTextWriter> writerdic = new Dictionary<string, ObjTextWriter>();

        public static EntityTableEngine LocalEngine = new EntityTableEngine(null);

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

          
            public LockerDestroy(Dictionary<string,object> lockerdic,string lockkey)
            {
                this._lockerdic = lockerdic;
                this._lockerkey = lockkey;
            }

            public void Exceute()
            {
                if(DateTime.Now.Subtract(_timeadd).TotalSeconds>60)
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
            private ConcurrentDictionary<string, EntityTableIndexItemBag> _dic;
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


            public IndexDestroy(ConcurrentDictionary<string, EntityTableIndexItemBag> dic, string key)
            {
                this._dic = dic;
                this._key = key;
            }

            public void Exceute()
            {
                EntityTableIndexItemBag val=null;
                _dic.TryGetValue(_key, out val);
                if (val != null)
                {
                    lock (val)
                    {
                        if (DateTime.Now.Subtract(val.LastUsed).TotalSeconds > 30)
                        {
                            EntityTableIndexItemBag val0;
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
            private Dictionary<string,ObjTextWriter> _dic;
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


            public WriterDestroy(Dictionary<string, ObjTextWriter> dic, string key,int locksecs=1)
            {
                this._dic = dic;
                this._key = key;
                this._locksecs = locksecs;
            }

            public void Exceute()
            {
                var o = _dic[_key];
                if (o != null)
                {
                    lock (o)
                    {
                        if (DateTime.Now.Subtract((DateTime)_dic[_key].Tag).TotalSeconds > _locksecs)
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

        public EntityTableEngine(string dir)
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

                            keyindexdic.TryAdd(tablename,new ConcurrentDictionary<string,Dictionary<long,EntityTableIndexItem>>());
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

        private object GetKeyLocker(string table,string key)
        {
            string totalkey=string.Format("{0}:{1}",table,key);
            object locker=null;
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

                CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new LockerDestroy(keylocker, totalkey));
            }

            return locker;
        }

        private void LoadKey(string tablename,EntityTableMeta meta)
        {
            string indexfile = GetKeyFile(tablename);
            var indexmergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
            if (indexmergeinfo == null)
            {
                indexmergeinfo = new IndexMergeInfo();
                indexmergeinfo.IndexName = meta.KeyName;
                meta.IndexMergeInfos.Add(indexmergeinfo);
            }
            using (ObjTextReader idx = ObjTextReader.CreateReader(indexfile))
            {
                if (indexmergeinfo.IndexMergePos > 0)
                {
                    idx.SetPostion(indexmergeinfo.IndexMergePos);
                }
                var idc = keyindexdic[tablename];
                Dictionary<long, EntityTableIndexItem> al = null;
                foreach(var newindex in idx.ReadObjectsWating<EntityTableIndexItem>(1))
                {
                    if (!idc.TryGetValue(newindex.Key, out al))
                    {
                        lock (idc)
                        {
                            if (!idc.TryGetValue(newindex.Key, out al))
                            {
                                al = new Dictionary<long, EntityTableIndexItem>();
                                idc.TryAdd(newindex.Key, al);
                            }
                        }
                    }

                    if (newindex.Del)
                    {
                        al.Remove(newindex.Offset);
                    }
                    else
                    {
                        lock (al)
                        {
                            al.Add(newindex.Offset,newindex);
                        }
                    }
                }
            }
        }

        private EntityTableIndexItemBag LoadIndex(string tablename, string indexname, EntityTableMeta meta)
        {
            string key = string.Format("{0}##{1}", tablename, indexname);
            EntityTableIndexItemBag temp = null;
            if(indexdic.TryGetValue(key,out temp))
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
                    temp = new EntityTableIndexItemBag();
                }

                using (ObjTextReader idxreader = ObjTextReader.CreateReader(indexfile))
                {
                    if (temp.LastOffset > 0)
                    {
                        idxreader.SetPostion(temp.LastOffset);
                    }

                    Dictionary<long, EntityTableIndexItem> al = null;
                    foreach (var newindex in idxreader.ReadObjectsWating<EntityTableIndexItem>(1))
                    {
                        temp.LastOffset = idxreader.ReadedPostion();
                        if (!temp.Dics.TryGetValue(newindex.Key, out al))
                        {
                            al = new Dictionary<long, EntityTableIndexItem>();
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

                if (temp.LastUsed==DateTime.MinValue)
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
            EntityTableMeta meta=null;
            if(metadic.TryGetValue(tablename,out meta))
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

            if(!metadic.ContainsKey(tablename))
            {
                lock (metadic)
                {
                    if (!metadic.ContainsKey(tablename))
                    {
                        metadic.Add(tablename, meta);
                    }
                }
            }

            if (!keyindexdic.ContainsKey(tablename))
            {
                lock (keyindexdic)
                {
                    if (!keyindexdic.ContainsKey(tablename))
                    {
                        keyindexdic.TryAdd(tablename,new ConcurrentDictionary<string,Dictionary<long,EntityTableIndexItem>>());
                    }
                }
            }

            LoadKey(tablename,meta);

            return meta;
        }

        private ObjTextWriter GetWriter(string filename,int locksecs=1)
        {
            ObjTextWriter writer=null;
            if(writerdic.TryGetValue(filename,out writer))
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
                    }
                }

                writer = ObjTextWriter.CreateWriter(filename, ObjTextReaderWriterEncodeType.entitybuf);
                writer.Tag = DateTime.Now;
                writerdic.Add(filename, writer);

                LJC.FrameWork.Comm.Coroutine.CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new WriterDestroy(writerdic, filename,locksecs));
            }
            return writer;
        }

        private bool Insert2<T>(string tablename, IEnumerable<T> items, EntityTableMeta meta) where T : new()
        {
            string tablefile = GetTableFile(tablename);

            var locker = GetKeyLocker(tablename, string.Empty);

            lock (locker)
            {
                ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf);
                string keyindexfile = GetKeyFile(tablename);
                ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf);
                Dictionary<string, ObjTextWriter> idxwriterdic = new Dictionary<string, ObjTextWriter>();

                try
                {
                    foreach (var item in items)
                    {
                        var keyvalue = item.Eval(meta.KeyProperty);
                        //var keyvalue = meta.KeyProperty.GetValueMethed(item);
                        if (keyvalue == null)
                        {
                            throw new Exception("key值不能为空");
                        }

                        var keystr = keyvalue.ToString();

                        var keylocker = GetKeyLocker(tablename, keystr);

                        Dictionary<long, EntityTableIndexItem> arr = null;
                        lock (keylocker)
                        {
                            if (keyindexdic[tablename].TryGetValue(keystr, out arr))
                            {
                                throw new Exception(string.Format("key:{0}不可重复", keystr));
                            }
                        }

                        var tableitem = new EntityTableItem<T>(item);
                        tableitem.Flag = (byte)EntityTableItemFlag.Ok;

                        //using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                        //var otw = GetWriter(tablefile);
                        //lock(otw)
                        {

                            var offset = otw.AppendObject(tableitem);

                            Dictionary<long, EntityTableIndexItem> al = null;
                            var idc = keyindexdic[tablename];
                            if (!idc.TryGetValue(keystr, out al))
                            {
                                lock (idc)
                                {
                                    if (!idc.TryGetValue(keystr, out al))
                                    {
                                        al = new Dictionary<long, EntityTableIndexItem>();
                                        idc.TryAdd(keystr, al);
                                    }
                                }
                            }

                            var newkey = new EntityTableIndexItem
                            {
                                Key = keystr,
                                Offset = offset.Item1,
                                len = (int)(offset.Item2 - offset.Item1)
                            };
                            lock (al)
                            {
                                al.Add(newkey.Offset, newkey);
                            }

                            //string keyindexfile = GetKeyFile(tablename);
                            //using (ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                            //ObjTextWriter keywriter = GetWriter(keyindexfile);
                            //lock(keywriter)
                            {
                                keywriter.AppendObject(newkey);
                            }

                            foreach (var idx in meta.Indexs)
                            {
                                string indexfile = GetIndexFile(tablename, idx);
                                var indexvalue = item.Eval(meta.IndexProperties[idx]);
                                var newindex = new EntityTableIndexItem
                                {
                                    Key = indexvalue == null ? string.Empty : indexvalue.ToString(),
                                    Offset = offset.Item1,
                                    len = (int)(offset.Item2 - offset.Item1)
                                };
                                //using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                                //ObjTextWriter idxwriter = GetWriter(indexfile);
                                //lock(idxwriter)
                                {
                                    ObjTextWriter idxwriter = null;
                                    if (!idxwriterdic.TryGetValue(indexfile, out idxwriter))
                                    {
                                        idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf);
                                        idxwriterdic.Add(indexfile, idxwriter);
                                    }

                                    idxwriter.AppendObject(newindex);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    otw.Dispose();
                    keywriter.Dispose();
                    foreach (var kv in idxwriterdic)
                    {
                        kv.Value.Dispose();
                    }
                }

            }

            return true;
        }

        public bool Insert<T>(string tablename, T item) where T : new()
        {
            if (item == null)
            {
                return false;
            }

            //item.Eval()
            EntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != item.GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }

            return Insert2(tablename, new T[] { item }, meta);
        }

        public bool Insert<T>(string tablename, IEnumerable<T> items) where T : new()
        {
            if (items == null || items.Count() == 0)
            {
                return false;
            }

            //item.Eval()
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
            Dictionary<long, EntityTableIndexItem> arr = null;

            var locker = GetKeyLocker(tablename, string.Empty);
            lock (locker)
            {
                if (keyindexdic[tablename].TryRemove(key, out arr))
                {
                    string keyindexfile = GetKeyFile(tablename);
                    using (ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                    //ObjTextWriter keywriter = GetWriter(keyindexfile);
                    //lock(keywriter)
                    {
                        foreach (var item in arr)
                        {
                            var indexitem = (EntityTableIndexItem)item.Value;
                            indexitem.Del = true;
                            keywriter.AppendObject(indexitem);

                            foreach (var idx in meta.Indexs)
                            {
                                string indexfile = GetIndexFile(tablename, idx);
                                using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                                //ObjTextWriter idxwriter = GetWriter(indexfile);
                                //lock(idxwriter)
                                {
                                    idxwriter.AppendObject(indexitem);
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public bool Upsert<T>(string tablename, T item) where T : new()
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
            Dictionary<long, EntityTableIndexItem> arr = null;
            if (keyindexdic[tablename].TryGetValue(key, out arr) && arr.Count > 0)
            {
                return Update2(tablename, key, item, meta);
            }
            else
            {
                return Insert2(tablename, new[] { item }, meta);
            }
        }

        private bool Update2<T>(string tablename, string key, T item, EntityTableMeta meta) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            Dictionary<long, EntityTableIndexItem> arr = null;
            Tuple<long, long> offset = null;
            int indexpos = 0;

            var locker = GetKeyLocker(tablename, string.Empty);
            lock (locker)
            {
                if (!keyindexdic[tablename].TryGetValue(key, out arr))
                {
                    throw new Exception(string.Format("更新失败，key为{0}的记录数为0", key));
                }
                string keyindexfile = GetKeyFile(tablename);
                EntityTableIndexItem indexitem = (EntityTableIndexItem)arr.Last().Value;
                using (ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                //ObjTextWriter keywriter = GetWriter(keyindexfile);
                {
                    indexitem.Del = true;
                    keywriter.AppendObject(indexitem);
                }

                var keyvalue = item.Eval(meta.KeyProperty);
                //var keyvalue=meta.KeyProperty.GetValueMethed(item);
                if (keyvalue == null)
                {
                    throw new Exception("key值不能为空");
                }
                var tableitem = new EntityTableItem<T>(item);
                tableitem.Flag = (byte)EntityTableItemFlag.Ok;
                using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                //ObjTextWriter otw = GetWriter(tablefile);
                {
                    offset = otw.PreAppendObject(tableitem, (s1, s2) =>
                    {
                        if (s1.Length <= indexitem.len)
                        {
                            Console.WriteLine("修改->覆盖");
                            return otw.Override(indexitem.Offset, s1);
                        }
                        return null;
                    });
                }


                Dictionary<long, EntityTableIndexItem> al = null;
                var keyidc = keyindexdic[tablename];
                if (!keyidc.TryGetValue(keyvalue.ToString(), out al))
                {
                    lock (keyidc)
                    {
                        if (!keyidc.TryGetValue(keyvalue.ToString(), out al))
                        {
                            al = new Dictionary<long, EntityTableIndexItem>();
                            keyidc.TryAdd(keyvalue.ToString(), al);
                        }
                    }
                }

                EntityTableIndexItem newkey = new EntityTableIndexItem
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

                arr[indexpos] = newkey;

                using (ObjTextWriter keyidxwriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                //ObjTextWriter keyidxwriter = GetWriter(keyindexfile);
                //lock(keyidxwriter)
                {
                    keyidxwriter.AppendObject(newkey);
                }

                foreach (var idx in meta.Indexs)
                {
                    string indexfile = GetIndexFile(tablename, idx);
                    var indexvalue = item.Eval(meta.IndexProperties[idx]);
                    var newindex = new EntityTableIndexItem
                    {
                        Key = indexvalue == null ? string.Empty : indexvalue.ToString(),
                        Offset = offset.Item1,
                        len = (int)(offset.Item2 - offset.Item1),
                        Del = false
                    };
                    using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                    //ObjTextWriter idxwriter = GetWriter(indexfile);
                    //lock(idxwriter)
                    {
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
            try
            {
                var meta = this.GetMetaData(tablename);
                Dictionary<long, EntityTableIndexItem> val = null;
                return keyindexdic[tablename].TryGetValue(key, out val) && val.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<T> ListAll<T>(string tablename) where T:new()
        {
            var meta = this.GetMetaData(tablename);
            var keys = keyindexdic[tablename].Keys;
            foreach (var key in keys)
            {
                foreach (var kk in Find<T>(tablename, key))
                {
                    yield return kk;
                }
            }
        }

        public IEnumerable<T> List<T>(string tablename,int pi,int ps) where T : new()
        {
            var meta = this.GetMetaData(tablename);
            var keys = keyindexdic[tablename].Keys;
            keys = keys.Skip((pi - 1) * ps).Take(ps).ToList();
            foreach (var key in keys)
            {
                foreach (var kk in Find<T>(tablename, key))
                {
                    yield return kk;
                }
            }
        }

        public int Count(string tablename)
        {
            var meta = this.GetMetaData(tablename);
            var keys = keyindexdic[tablename].Keys;
            return keys.Count;
        }

        public IEnumerable<T> Find<T>(string tablename, string key) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            EntityTableMeta meta = GetMetaData(tablename);
            Dictionary<long, EntityTableIndexItem> arr = null;
            EntityTableIndexItem indexitem = null;
            if (keyindexdic[tablename].TryGetValue(key, out arr))
            {
                //先找到offset
                using (ObjTextReader otw = ObjTextReader.CreateReader(tablefile))
                {
                    foreach (var o in arr)
                    {
                        indexitem = (EntityTableIndexItem)o.Value;
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

        public IEnumerable<T> Find<T>(string tablename, string index,string value) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            EntityTableMeta meta = GetMetaData(tablename);
            Dictionary<long, EntityTableIndexItem> arr = null;
            EntityTableIndexItem indexitem = null;

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
                        indexitem = (EntityTableIndexItem)o.Value;
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
