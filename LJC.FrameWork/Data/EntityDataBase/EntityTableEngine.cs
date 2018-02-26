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
        ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>>> indexdic = new ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>>>();

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
            private DateTime _timeadd = DateTime.Now;
            private ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>>> _dic;
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


            public IndexDestroy(ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>>> dic, string key)
            {
                this._dic = dic;
                this._key = key;
            }

            public void Exceute()
            {
                if (DateTime.Now.Subtract(_timeadd).TotalSeconds > 30)
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
                ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>> val;
                _dic.TryRemove(_key,out val);
            }
        }

        class WriterDestroy : ICoroutineUnit
        {
            private Dictionary<string,ObjTextWriter> _dic;
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


            public WriterDestroy(Dictionary<string, ObjTextWriter> dic, string key)
            {
                this._dic = dic;
                this._key = key;
            }

            public void Exceute()
            {
                if (DateTime.Now.Subtract((DateTime)_dic[_key].Tag).TotalSeconds > 5)
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

                var o = _dic[_key];
                if (o != null)
                {
                    lock (o)
                    {
                        lock (_dic)
                        {
                            _dic.Remove(_key);
                            o.Dispose();
                        }
                    }
                }
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
                    //using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                    ObjTextWriter otw = GetWriter(tablefile);
                    lock(otw)
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
                        //using (ObjTextWriter keyidxwriter = ObjTextWriter.CreateWriter(keyfile, ObjTextReaderWriterEncodeType.entitybuf))
                        ObjTextWriter keyidxwriter = GetWriter(keyfile);
                        {
                        }

                        if (indexs != null)
                        {
                            foreach (var idx in indexs)
                            {                              
                                var indexfile = GetIndexFile(tablename, idx);
                                //using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                                ObjTextWriter idxwriter = GetWriter(indexfile);
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

        private void LoadKey(string tablename)
        {
            string indexfile = GetKeyFile(tablename);
            using (ObjTextReader idx = ObjTextReader.CreateReader(indexfile))
            {
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

        private ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>> LoadIndex(string tablename, string indexname, EntityTableMeta meta)
        {
            string key = string.Format("{0}##{1}", tablename, indexname);
            ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>> temp=null;
            if(indexdic.TryGetValue(key,out temp))
            {
                return temp;
            }

            var locker = GetKeyLocker(tablename, "index_" + indexname);

            lock (locker)
            {
                if (indexdic.TryGetValue(key, out temp))
                {
                    return temp;
                }

                temp = new ConcurrentDictionary<string, Dictionary<long, EntityTableIndexItem>>();
                string indexfile = GetIndexFile(tablename, indexname);

                using (ObjTextReader idxreader = ObjTextReader.CreateReader(indexfile))
                {
                    Dictionary<long, EntityTableIndexItem> al = null;
                    foreach (var newindex in idxreader.ReadObjectsWating<EntityTableIndexItem>(1))
                    {
                        if (!temp.TryGetValue(newindex.Key, out al))
                        {
                            if (!temp.TryGetValue(newindex.Key, out al))
                            {
                                al = new Dictionary<long, EntityTableIndexItem>();
                                temp.TryAdd(newindex.Key, al);
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
                                al.Add(newindex.Offset, newindex);
                            }
                        }
                    }
                }

                indexdic.TryAdd(key, temp);

                LJC.FrameWork.Comm.Coroutine.CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new IndexDestroy(indexdic, key));
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

            LoadKey(tablename);

            return meta;
        }

        private ObjTextWriter GetWriter(string filename)
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

                LJC.FrameWork.Comm.Coroutine.CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new WriterDestroy(writerdic, filename));
            }
            return writer;
        }

        private bool Insert2<T>(string tablename, T item, EntityTableMeta meta) where T : new()
        {
            var keyvalue = item.Eval(meta.KeyProperty);
            //var keyvalue = meta.KeyProperty.GetValueMethed(item);
            if (keyvalue == null)
            {
                throw new Exception("key值不能为空");
            }

            var keystr = keyvalue.ToString();

            if (!meta.KeyDuplicate)
            {
                var keylocker = GetKeyLocker(tablename, keystr);

                Dictionary<long, EntityTableIndexItem> arr = null;
                lock (keylocker)
                {
                    if (keyindexdic[tablename].TryGetValue(keystr, out arr))
                    {
                        throw new Exception(string.Format("key:{0}不可重复", keystr));
                    }
                }
            }

            string tablefile = GetTableFile(tablename);
            var tableitem = new EntityTableItem<T>(item);
            tableitem.Flag = EntityTableItemFlag.Ok;
            var locker = GetKeyLocker(tablename, string.Empty);
            lock (locker)
            {
                //using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                var otw = GetWriter(tablefile);
                lock(otw)
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

                    string keyindexfile = GetKeyFile(tablename);
                    //using (ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                    ObjTextWriter keywriter = GetWriter(keyindexfile);
                    lock(keywriter)
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
                        ObjTextWriter idxwriter = GetWriter(indexfile);
                        lock(idxwriter)
                        {
                            idxwriter.AppendObject(newindex);
                        }
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

            return Insert2(tablename, item, meta);
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
                    //using (ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                    ObjTextWriter keywriter = GetWriter(keyindexfile);
                    lock(keywriter)
                    {
                        foreach (var item in arr)
                        {
                            var indexitem = (EntityTableIndexItem)item.Value;
                            indexitem.Del = true;
                            keywriter.AppendObject(indexitem);

                            foreach (var idx in meta.Indexs)
                            {
                                string indexfile = GetIndexFile(tablename, idx);
                                //using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                                ObjTextWriter idxwriter = GetWriter(indexfile);
                                lock(idxwriter)
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
            if(keyindexdic[tablename].TryGetValue(key,out arr))
            {
                return Update2(tablename, key, item, meta);
            }
            else
            {
                return Insert2(tablename, item, meta);
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

                indexpos = arr.Count - 1;
                string keyindexfile = GetKeyFile(tablename);
                EntityTableIndexItem indexitem = (EntityTableIndexItem)arr[indexpos];
                //using (ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                ObjTextWriter keywriter = GetWriter(keyindexfile);
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
                tableitem.Flag = EntityTableItemFlag.Ok;
                //using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                ObjTextWriter otw = GetWriter(tablefile);
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

                //using (ObjTextWriter idx = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf))
                ObjTextWriter keyidxwriter = GetWriter(keyindexfile);
                lock(keyidxwriter)
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
                    //using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                    ObjTextWriter idxwriter = GetWriter(indexfile);
                    lock(idxwriter)
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

                return keyindexdic[tablename].ContainsKey(key);
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
            if (indexobj.TryGetValue(value, out arr))
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
