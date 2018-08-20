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
    public partial class BigEntityTableEngine
    {
        #region 基本数据
        Dictionary<string, BigEntityTableMeta> metadic = new Dictionary<string, BigEntityTableMeta>();
        //磁盘索引
        ConcurrentDictionary<string, BigEntityTableIndexItem[]> keyindexdisklist = new ConcurrentDictionary<string, BigEntityTableIndexItem[]>();

        //内存索引
        ConcurrentDictionary<string, SortArrayList<BigEntityTableIndexItem>> keyindexmemlist = new ConcurrentDictionary<string, SortArrayList<BigEntityTableIndexItem>>();
        ConcurrentDictionary<string, SortArrayList<BigEntityTableIndexItem>> keyindexmemtemplist = new ConcurrentDictionary<string, SortArrayList<BigEntityTableIndexItem>>();

        Dictionary<string, ReaderWriterLockSlim> keylocker = new Dictionary<string, ReaderWriterLockSlim>();

        string dirbase = System.AppDomain.CurrentDomain.BaseDirectory + "\\localdb\\";
        #endregion

        public static BigEntityTableEngine LocalEngine = new BigEntityTableEngine(null);

        private const int MERGE_TRIGGER_NEW_COUNT = 1000000;
        /// <summary>
        /// 最大单个key占用内存
        /// </summary>
        private const long MAX_KEYBUFFER = 100 * 1000 * 1000;

        private System.Threading.Timer _margetimer = null;

        private static bool HasDBError = false;

        static BigEntityTableEngine()
        {

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

        public void CreateTable(string tablename, string keyname, Type ttype, IndexInfo[] indexs = null)
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
                            BigEntityTableMeta meta = new BigEntityTableMeta();
                            meta.KeyName = keyname;
                            meta.IndexInfos = indexs ?? new IndexInfo[] { };
                            meta.CTime = DateTime.Now;
                            meta.TType = ttype;
                            var pp = ttype.GetProperty(keyname);
                            if (pp == null)
                            {
                                throw new Exception("找不到主键:" + keyname);
                            }
                            meta.KeyProperty = new PropertyInfoEx(pp);
                            var keyentitytype = EntityBuf2.EntityBufCore2.GetTypeBufType(pp.PropertyType).Item1.EntityType;
                            meta.EntityTypeDic.Add(keyname, keyentitytype);                         
                            meta.KeyIndexInfo=new IndexInfo
                            {
                                IndexName = keyname,
                                Indexs = new IndexItem[]{
                                    new IndexItem
                                {
                                    Field=keyname,
                                    Direction=1,
                                    FieldType=keyentitytype
                                }
                            }
                            };

                            if (indexs != null && indexs.Length > 0)
                            {
                                var dupkeys = indexs.GroupBy(p => p.IndexName).FirstOrDefault(p => p.Count() > 1);
                                if (dupkeys!=null)
                                {
                                    throw new Exception("索引名称不能重复:" + dupkeys.Key);
                                }
                                foreach (var idx in indexs)
                                {
                                    foreach (var id in idx.Indexs)
                                    {
                                        var idxpp = ttype.GetProperty(id.Field);
                                        if (idxpp == null)
                                        {
                                            throw new Exception("对象不存在字段:" + id.Field);
                                        }

                                        if (!meta.IndexProperties.ContainsKey(id.Field))
                                        {
                                            meta.IndexProperties.Add(id.Field, new PropertyInfoEx(idxpp));
                                        }
                                        if (!meta.EntityTypeDic.ContainsKey(id.Field))
                                        {
                                            meta.EntityTypeDic.Add(id.Field, EntityBuf2.EntityBufCore2.GetTypeBufType(idxpp.PropertyType).Item1.EntityType);
                                        }
                                    }
                                }
                            }

                            LJC.FrameWork.Comm.SerializerHelper.SerializerToXML<BigEntityTableMeta>(meta, metafile, catchErr: true);
                            metadic.Add(tablename, meta);

                            keyindexdisklist.TryAdd(tablename, new BigEntityTableIndexItem[0]);
                            keyindexmemlist.TryAdd(tablename, new SortArrayList<BigEntityTableIndexItem>());
                            keyindexmemtemplist.TryAdd(tablename, new SortArrayList<BigEntityTableIndexItem>());
                        }
                        else
                        {
                            var meta = LJC.FrameWork.Comm.SerializerHelper.DeSerializerFile<BigEntityTableMeta>(metafile, true);
                            if (!meta.KeyName.Equals(keyname) || !meta.TypeString.Equals(ttype))
                            {
                                delfile = false;

                                throw new Exception("meta文件内容检查不一致");
                            }

                            var pp = ttype.GetProperty(keyname);
                            meta.KeyProperty = new PropertyInfoEx(pp);
                            meta.EntityTypeDic.Add(keyname, EntityBuf2.EntityBufCore2.GetTypeBufType(pp.PropertyType).Item1.EntityType);
                            metadic.Add(tablename, meta);

                            if (indexs != null && indexs.Length > 0)
                            {
                                indexs = indexs.Distinct().ToArray();
                                foreach (var idx in indexs)
                                {
                                    foreach (var id in idx.Indexs)
                                    {
                                        var idxpp = ttype.GetProperty(id.Field);

                                        if (!meta.IndexProperties.ContainsKey(id.Field))
                                        {
                                            meta.IndexProperties.Add(id.Field, new PropertyInfoEx(idxpp));
                                        }
                                        if (!meta.EntityTypeDic.ContainsKey(id.Field))
                                        {
                                            meta.EntityTypeDic.Add(id.Field, EntityBuf2.EntityBufCore2.GetTypeBufType(idxpp.PropertyType).Item1.EntityType);
                                        }
                                    }
                                }
                            }
                        }

                        //索引
                        string keyfile = GetKeyFile(tablename);
                        using (ObjTextWriter keyidxwriter = ObjTextWriter.CreateWriter(keyfile, ObjTextReaderWriterEncodeType.entitybuf2))
                        //ObjTextWriter keyidxwriter = GetWriter(keyfile);
                        {
                        }

                        if (indexs != null)
                        {
                            foreach (var idx in indexs)
                            {
                                var idxkey = tablename + ":" + idx.IndexName;
                                keyindexdisklist.TryAdd(idxkey, new BigEntityTableIndexItem[0]);
                                keyindexmemlist.TryAdd(idxkey, new SortArrayList<BigEntityTableIndexItem>());
                                keyindexmemtemplist.TryAdd(idxkey, new SortArrayList<BigEntityTableIndexItem>());

                                var indexfile = GetIndexFile(tablename, idx.IndexName);
                                using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf2))
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

        public BigEntityTableEngine(string dir)
        {
            if (!string.IsNullOrWhiteSpace(dir))
            {
                this.dirbase = dir;
            }

            IOUtil.MakeDirs(this.dirbase);

            _margetimer = new Timer(new TimerCallback((o) =>
            {
                _margetimer.Change(Timeout.Infinite, Timeout.Infinite);
                try
                {
                    foreach (var tablename in metadic.Keys)
                    {
                        BigEntityTableMeta meta = metadic[tablename];
                        if (meta.NewAddCount >= MERGE_TRIGGER_NEW_COUNT ||
                        (this.keyindexmemlist.ContainsKey(tablename) && keyindexmemlist[tablename].Length() >= MERGE_TRIGGER_NEW_COUNT))
                        {
                            //meta.NewAddCount = 0;
                            MergeKey(tablename, meta.KeyName);
                        }

                        if (meta.IndexInfos != null && meta.IndexInfos.Length > 0)
                        {
                            foreach (var index in meta.IndexInfos)
                            {
                                var indexkey = tablename + ":" + index.IndexName;
                                if (this.keyindexmemlist.ContainsKey(indexkey) && keyindexmemlist[indexkey].Length() >= MERGE_TRIGGER_NEW_COUNT)
                                {
                                    MergeIndex(tablename, index.IndexName);
                                }

                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    _margetimer.Change(1000, 1000);
                }
            }), null, 1000, 1000);
        }

        /// <summary>
        /// 取元数据
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        private BigEntityTableMeta GetMetaData(string tablename)
        {
            BigEntityTableMeta meta = null;
            if (metadic.TryGetValue(tablename, out meta))
            {
                return meta;
            }

            var metalocker = GetKeyLocker(tablename, "GetMetaData");
            try
            {
                metalocker.EnterWriteLock();
                if (metadic.TryGetValue(tablename, out meta))
                {
                    return meta;
                }

                string metafile = GetMetaFile(tablename);
                if (!File.Exists(metafile))
                {
                    throw new Exception("找不到元文件:" + tablename);
                }

                meta = LJC.FrameWork.Comm.SerializerHelper.DeSerializerFile<BigEntityTableMeta>(metafile, true);
                if (meta.IndexInfos == null)
                {
                    meta.IndexInfos = new IndexInfo[] { };
                }
                meta.KeyProperty = new PropertyInfoEx(meta.TType.GetProperty(meta.KeyName));
                var pp = meta.TType.GetProperty(meta.KeyName);
                meta.EntityTypeDic.Add(meta.KeyName, EntityBuf2.EntityBufCore2.GetTypeBufType(pp.PropertyType).Item1.EntityType);
                metadic.Add(tablename, meta);

                if (meta.IndexInfos != null)
                {
                    foreach (var idx in meta.IndexInfos)
                    {
                        foreach (var id in idx.Indexs)
                        {
                            var idxpp = meta.TType.GetProperty(id.Field);
                            if (idxpp == null)
                            {
                                throw new Exception("对象不存在字段:" + id.Field);
                            }

                            if (!meta.IndexProperties.ContainsKey(id.Field))
                            {
                                meta.IndexProperties.Add(id.Field, new PropertyInfoEx(idxpp));
                            }
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

                if (!keyindexmemlist.ContainsKey(tablename))
                {
                    lock (keyindexmemlist)
                    {
                        keyindexmemlist.TryAdd(tablename, new SortArrayList<BigEntityTableIndexItem>());
                    }
                }

                if (!keyindexmemtemplist.ContainsKey(tablename))
                {
                    lock (keyindexmemtemplist)
                    {
                        keyindexmemtemplist.TryAdd(tablename, new SortArrayList<BigEntityTableIndexItem>());
                    }
                }

                LoadKey(tablename, meta);

                if (meta.IndexInfos != null && meta.IndexInfos.Length > 0)
                {
                    foreach (var index in meta.IndexInfos)
                    {
                        var indexkey = tablename + ":" + index.IndexName;
                        if (!keyindexmemlist.ContainsKey(indexkey))
                        {
                            lock (keyindexmemlist)
                            {
                                keyindexmemlist.TryAdd(indexkey, new SortArrayList<BigEntityTableIndexItem>());
                            }
                        }
                        if (!keyindexmemtemplist.ContainsKey(indexkey))
                        {
                            lock (keyindexmemtemplist)
                            {
                                keyindexmemtemplist.TryAdd(indexkey, new SortArrayList<BigEntityTableIndexItem>());
                            }
                        }
                        LoadIndex(tablename, index.IndexName, meta);
                    }
                }

                return meta;
            }
            finally
            {
                metalocker.ExitWriteLock();
            }
        }

        private ReaderWriterLockSlim GetKeyLocker(string table, string key)
        {
            string totalkey = string.Format("{0}:{1}", table, key);
            ReaderWriterLockSlim locker = null;
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

                locker = new ReaderWriterLockSlim();
                keylocker.Add(totalkey, locker);

                //CoroutineEngine.DefaultCoroutineEngine.Dispatcher(new LockerDestroy(keylocker, totalkey));
            }

            return locker;
        }

        #region 合并主键索引

        private IEnumerable<BigEntityTableIndexItem> MergeAndSort2(List<BigEntityTableIndexItem> sortedlist, List<BigEntityTableIndexItem> sortedlist2)
        {
            if (sortedlist.Count() == 0)
            {
                foreach (var item in sortedlist2)
                {
                    yield return item;
                }
            }
            else if (sortedlist2.Count() == 0)
            {
                foreach (var item in sortedlist)
                {
                    yield return item;
                }
            }
            else
            {
                var it1 = sortedlist.GetEnumerator();
                it1.MoveNext();
                var item1 = it1.Current;
                var it2 = sortedlist2.GetEnumerator();
                it2.MoveNext();
                var item2 = it2.Current;
                int compareval = 0;
                while (item1 != null && item2 != null)
                {
                    compareval = item1.CompareTo(item2);
                    if (compareval > 0)
                    {
                        yield return item2;
                        item2 = it2.MoveNext() ? it2.Current : null;
                    }
                    else
                    {
                        yield return item1;
                        item1 = it1.MoveNext() ? it1.Current : null;
                    }
                }

                if (item1 != null)
                {
                    yield return item1;
                    while (it1.MoveNext())
                    {
                        yield return it1.Current;
                    }
                }

                if (item2 != null)
                {
                    yield return item2;
                    while (it2.MoveNext())
                    {
                        yield return it2.Current;
                    }
                }
            }
        }


        #endregion

        #region 增删改查

        private bool Insert2<T>(string tablename, IEnumerable<T> items, BigEntityTableMeta meta) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            var tablelocker = GetKeyLocker(tablename, string.Empty);
            string keyindexfile = GetKeyFile(tablename);

            try
            {
                tablelocker.EnterWriteLock();
                var otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf);
                ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf2);
                var keyreader = ObjTextReader.CreateReader(keyindexfile);
                Dictionary<string, ObjTextWriter> idxwriterdic = new Dictionary<string, ObjTextWriter>();
                var tableitem = new EntityTableItem<T>();
                tableitem.Flag = EntityTableItemFlag.Ok;

                try
                {
                    var keylist = keyindexmemlist[tablename];
                    var findkey = new BigEntityTableIndexItem();
                    foreach (var item in items)
                    {
                        var keyvalue = meta.KeyProperty.GetValueMethed(item);
                        if (keyvalue == null)
                        {
                            throw new Exception("key值不能为空");
                        }

                        findkey.Key =new object[] { keyvalue };
                        findkey.Index = meta.KeyIndexInfo;
                        if (KeyExsitsInner(tablename, findkey, keyreader))
                        {
                            throw new Exception("不能重复写入:" + keyvalue);
                        }


                        tableitem.Data = item;
                        var offset = otw.AppendObject(tableitem);

                        var newkey = new BigEntityTableIndexItem
                        {
                            Key = new object[] { keyvalue },
                            Offset = offset.Item1,
                            len = (int)(offset.Item2 - offset.Item1),
                            Index = meta.KeyIndexInfo
                        };

                        for(int j=0;j<meta.IndexInfos.Length;j++)
                        {
                            var idx = meta.IndexInfos[j];
                            string indexfile = GetIndexFile(tablename, idx.IndexName);
                            object[] indexvalues = new object[idx.Indexs.Length];
                            for (int i = 0; i < indexvalues.Length; i++)
                            {
                                var indexvalue = meta.IndexProperties[idx.Indexs[i].Field].GetValueMethed(item);
                                indexvalues[i] = indexvalue;
                            }
                            var newindex = new BigEntityTableIndexItem
                            {
                                Key = indexvalues,
                                Offset = newkey.Offset,
                                len = newkey.len,
                                Index=idx
                            };

                            ObjTextWriter idxwriter = null;
                            if (!idxwriterdic.TryGetValue(indexfile, out idxwriter))
                            {
                                idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf2);
                                idxwriterdic.Add(indexfile, idxwriter);
                            }

                            keyindexmemlist[tablename + ":" + idx.IndexName].Add(newindex);

                            newindex.KeyOffset = idxwriter.GetWritePosition();
                            idxwriter.AppendObject(newindex);
                        }

                        newkey.KeyOffset = keywriter.GetWritePosition();
                        keywriter.AppendObject(newkey);
                        keylist.Add(newkey);
                    }

                    meta.NewAddCount += items.Count();
                }
                finally
                {
                    using (keyreader) { };
                    using (otw) { };
                    using (keywriter) { };

                    foreach (var kv in idxwriterdic)
                    {
                        using (kv.Value) { }
                    }
                }
            }
            finally
            {
                tablelocker.ExitWriteLock();
            }

            return true;
        }

        public bool Insert<T>(string tablename, T item) where T : new()
        {
            if (HasDBError)
            {
                return false;
            }

            //item.Eval()
            BigEntityTableMeta meta = GetMetaData(tablename);

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

            if (HasDBError)
            {
                return false;
            }

            BigEntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != items.First().GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }

            return Insert2(tablename, items, meta);
        }

        public bool Delete<T>(string tablename, object key) where T : new()
        {
            BigEntityTableMeta meta = GetMetaData(tablename);

            var delkey = FindKey(tablename, key);
            if (delkey == null)
            {
                throw new Exception(string.Format("主键查找失败:{0}.{1}", tablename, key));
            }

            var delitem = Find<T>(tablename, key);
            if (delitem == null)
            {
                throw new Exception(string.Format("数据查找失败:{0}.{1}", tablename, key));
            }

            var tablelocker = GetKeyLocker(tablename, string.Empty);
            string keyindexfile = GetKeyFile(tablename);
            string tablefile = GetTableFile(tablename);
            try
            {
                Tuple<long, long> offset = null;
                tablelocker.EnterWriteLock();

                var tableitem = new EntityTableItem<T>(delitem);
                tableitem.Flag = EntityTableItemFlag.Del;
                using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                {
                    var posend = otw.GetWritePosition();
                    offset = otw.PreAppendObject(tableitem, (s1, s2) =>
                    {
                        if (s1.Length <= delkey.len)
                        {
                            var tp = otw.Override(delkey.Offset, s1, s1.Length - 2);
                            otw.SetPosition(posend);
                            return tp;
                        }
                        return null;
                    });
                }
                if (offset.Item1 != delkey.Offset)
                {
                    delkey.Offset = offset.Item1;
                    delkey.len = (int)(offset.Item2 - offset.Item1);
                }

                using (ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf2))
                {
                    var oldoffset = keywriter.GetWritePosition();
                    delkey.Del = true;

                    keywriter.SetPosition(delkey.KeyOffset);
                    keywriter.AppendObject(delkey);

                    keywriter.SetPosition(oldoffset);
                }

                foreach (var idx in meta.IndexInfos)
                {
                    string indexfile = GetIndexFile(tablename, idx.IndexName);
                    var idxvals = new object[idx.Indexs.Length];
                    for (int i = 0; i < idx.Indexs.Length; i++)
                    {
                        var idxval = meta.IndexProperties[idx.Indexs[i].Field].GetValueMethed(delitem);
                        idxvals[i] = idxval;
                    }
                    //var idxfindkey = new BigEntityTableIndexItem { Key = idxval, Offset=delkey.Offset };
                    BigEntityTableIndexItem idxitem = FindIndex(tablename, meta, idx, idxvals, delkey.Offset).FirstOrDefault();

                    if (idxitem == null)
                    {
                        throw new Exception("查找索引失败:" + idx);
                    }

                    bool success = false;
                    using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf2))
                    {
                        var posend = idxwriter.GetWritePosition();
                        idxitem.Del = true;
                        idxitem.Offset = delkey.Offset;
                        idxitem.len = delkey.len;
                        idxwriter.SetPosition(idxitem.KeyOffset);
                        idxwriter.AppendObject(idxitem);
                        success = true;
                        idxwriter.SetPosition(posend);
                    }
                    if (!success)
                    {
                        throw new Exception(string.Format("查找索引失败:{0}.{1}", tablename, key));
                    }
                }
            }
            finally
            {
                tablelocker.ExitWriteLock();
            }

            return true;
        }

        public bool Upsert<T>(string tablename, T item) where T : new()
        {
            BigEntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != item.GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }

            var keyobj = meta.KeyProperty.GetValueMethed(item);
            if (keyobj == null)
            {
                throw new Exception("key不能为空");
            }

            var upitem = FindKey(tablename, keyobj);

            if (upitem != null)
            {
                return Update2(tablename, keyobj, item, meta);
            }
            else
            {
                return Insert2(tablename, new T[] { item }, meta);
            }
        }

        private bool Update2<T>(string tablename, object key, T item, BigEntityTableMeta meta) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            Tuple<long, long> offset = null;

            var tablelocker = GetKeyLocker(tablename, string.Empty);
            try
            {
                BigEntityTableIndexItem oldindexitem = FindKey(tablename, key);
                if (oldindexitem == null)
                {
                    throw new Exception("查找索引失败:" + key);
                }

                var olditem = Find<T>(tablename, key);
                if (olditem == null)
                {
                    throw new Exception("查找数据失败");
                }

                tablelocker.EnterWriteLock();
                string keyindexfile = GetKeyFile(tablename);

                var keyvalue = meta.KeyProperty.GetValueMethed(item);
                if (keyvalue == null)
                {
                    throw new Exception("key值不能为空");
                }
                var tableitem = new EntityTableItem<T>(item);
                tableitem.Flag = EntityTableItemFlag.Ok;
                using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                {
                    var posend = otw.GetWritePosition();
                    offset = otw.PreAppendObject(tableitem, (s1, s2) =>
                    {
                        if (s1.Length <= oldindexitem.len)
                        {
                            var tp = otw.Override(oldindexitem.Offset, s1, s1.Length - 2);
                            otw.SetPosition(posend);
                            return tp;
                        }
                        return null;
                    });
                }

                BigEntityTableIndexItem newkey = new BigEntityTableIndexItem
                {
                    Key =new object[] { keyvalue },
                    Offset = offset.Item1,
                    len = (oldindexitem.Offset == offset.Item1) ? oldindexitem.len : (int)(offset.Item2 - offset.Item1),
                    KeyOffset = oldindexitem.KeyOffset,
                    Del = false,
                    Index=meta.KeyIndexInfo
                };

                //更新索引
                foreach (var idx in meta.IndexInfos)
                {
                    var oldidxval = idx.GetIndexValues(olditem,meta);
                    var newidxval = idx.GetIndexValues(item, meta);
                    bool equal = true;
                    if (oldidxval.Length == newidxval.Length)
                    {
                        for(int i = 0; i < oldidxval.Length; i++)
                        {
                            if (oldidxval[i] != newidxval[i])
                            {
                                equal = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        equal = false;
                    }
                    if (equal && oldindexitem.Offset == offset.Item1)
                    {
                        continue;
                    }
                    var indexfile = GetIndexFile(tablename, idx.IndexName);
                    //这里有问题。索引重排后会变的
                    var idxfindkey = new BigEntityTableIndexItem { Key = oldidxval, Offset = oldindexitem.Offset };
                    var idxkey = tablename + ":" + idx.IndexName;
                    BigEntityTableIndexItem idxitem = FindIndex(tablename, meta, idx, oldidxval, oldindexitem.Offset).FirstOrDefault();

                    if (idxitem == null)
                    {
                        throw new Exception("查找索引失败:" + idx);
                    }

                    using (ObjTextWriter idxwriter = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf2))
                    {
                        var posend = idxwriter.GetWritePosition();

                        if (oldidxval == newidxval)
                        {
                            idxitem.Offset = newkey.Offset;
                            idxitem.len = newkey.len;
                            idxwriter.SetPosition(idxitem.KeyOffset);
                            idxwriter.AppendObject(idxitem);
                            idxwriter.SetPosition(posend);
                        }
                        else
                        {
                            idxitem.Del = true;
                            idxwriter.SetPosition(idxitem.KeyOffset);
                            idxwriter.AppendObject(idxitem);
                            idxwriter.SetPosition(posend);

                            var newidxitem = new BigEntityTableIndexItem
                            {
                                Del = false,
                                Key = newidxval,
                                KeyOffset = posend,
                                len = newkey.len,
                                Offset = newkey.Offset
                            };
                            idxwriter.AppendObject(newidxitem);
                            keyindexmemlist[idxkey].Add(newidxitem);
                        }
                    }
                }

                using (ObjTextWriter keywriter = ObjTextWriter.CreateWriter(keyindexfile, ObjTextReaderWriterEncodeType.entitybuf2))
                {
                    var posend = keywriter.GetWritePosition();
                    newkey.KeyOffset = oldindexitem.KeyOffset;
                    keywriter.SetPosition(newkey.KeyOffset);
                    keywriter.AppendObject(newkey);
                    keywriter.SetPosition(posend);
                }

                //更新oldkey
                oldindexitem.Key = newkey.Key;
                oldindexitem.KeyOffset = newkey.KeyOffset;
                oldindexitem.len = newkey.len;
                oldindexitem.Offset = newkey.Offset;
                oldindexitem.Del = newkey.Del;

                Console.WriteLine("写入成功:" + keyvalue + "->" + offset);
            }
            finally
            {
                tablelocker.ExitWriteLock();
            }
            return true;
        }

        public bool Update<T>(string tablename, T item) where T : new()
        {
            BigEntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != item.GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }

            var keyobj = meta.KeyProperty.GetValueMethed(item);
            if (keyobj == null)
            {
                throw new Exception("key不能为空");
            }

            string key = keyobj.ToString();

            return Update2(tablename, key, item, meta);
        }
        #endregion

        #region 根据的主键操作

        public bool Exists(string tablename, string key)
        {
            return FindKey(tablename, key) != null;
        }

        public IEnumerable<T> List<T>(string tablename, int pi, int ps) where T : new()
        {
            int count = 0;
            var start = (pi - 1) * ps;
            var end = pi * ps;
            var buffer = new byte[1024 * 1024 * 10];
            using (var reader = ObjTextReader.CreateReader(GetTableFile(tablename)))
            {
                foreach (var item in reader.ReadObjectsWating<EntityTableItem<T>>(1, null, buffer))
                {
                    if (item == null)
                    {
                        yield break;
                    }

                    if (item.Flag == EntityTableItemFlag.Del)
                    {
                        continue;
                    }

                    if (count++ >= start)
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

        public long Count(string tablename)
        {
            var meta = this.GetMetaData(tablename);
            var keylocker = GetKeyLocker(tablename, string.Empty);
            keylocker.EnterReadLock();
            try
            {
                var memkeys = this.keyindexmemlist[tablename];
                var mergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
                var lastkeyitem = this.keyindexdisklist[tablename].LastOrDefault();
                return memkeys.Length() + keyindexmemtemplist[tablename].Length() + (lastkeyitem == null ? 0 : lastkeyitem.RangeIndex + 1);
            }
            finally
            {
                keylocker.ExitReadLock();
            }
        }

        private bool KeyExsitsInner(string tablename, BigEntityTableIndexItem key, ObjTextReader keyReader)
        {
            var meta = GetMetaData(tablename);

            BigEntityTableIndexItem findkeyitem = keyindexmemlist[tablename].Find(key);
            if (findkeyitem != null)
            {
                return !findkeyitem.Del;
            }
            findkeyitem = keyindexmemtemplist[tablename].Find(key);
            if (findkeyitem != null)
            {
                return !findkeyitem.Del;
            }

            var indexarr = keyindexdisklist[tablename];
            int mid = -1;
            int pos = new Collections.SorteArray<BigEntityTableIndexItem>(indexarr).Find(key, ref mid);

            if (pos == -1 && (mid == -1 || mid == indexarr.Length - 1))
            {
                return false;
            }
            else if (pos > -1)
            {
                findkeyitem = indexarr[pos];
                return !findkeyitem.Del;
            }

            if (findkeyitem == null)
            {
                var keymergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
                if (pos == -1 && keymergeinfo.LoadFactor == 1)
                {
                    return false;
                }

                var posstart = indexarr[mid].KeyOffset;
                var posend = indexarr[mid + 1].KeyOffset;

                var buffer = new byte[1024];
                keyReader.SetPostion(posstart);

                foreach (var item in keyReader.ReadObjectsWating<BigEntityTableIndexItem>(1, null, buffer))
                {
                    item.SetIndex(meta.KeyIndexInfo);
                    if (keyReader.ReadedPostion() > posend)
                    {
                        return false;
                    }
                    if (item.Key.Equals(key))
                    {
                        return !item.Del;
                    }
                }
            }

            return false;

        }

        public BigEntityTableIndexItem FindKey(string tablename, object key)
        {
            var meta = GetMetaData(tablename);

            var findkey = new BigEntityTableIndexItem { Key = new object[] { key }, Index = meta.KeyIndexInfo };
            var locker = GetKeyLocker(tablename, string.Empty);
            try
            {
                locker.EnterReadLock();
                BigEntityTableIndexItem findkeyitem = keyindexmemlist[tablename].Find(findkey);
                if (findkeyitem != null)
                {
                    if (findkeyitem.Del)
                    {
                        return null;
                    }
                    return findkeyitem;
                }
                findkeyitem = keyindexmemtemplist[tablename].Find(findkey);
                if (findkeyitem != null)
                {
                    if (findkeyitem.Del)
                    {
                        return null;
                    }
                    return findkeyitem;
                }

                var indexarr = keyindexdisklist[tablename];
                int mid = -1;
                int pos = new Collections.SorteArray<BigEntityTableIndexItem>(indexarr).Find(new BigEntityTableIndexItem
                {
                    Key = new object[] { key},
                    Index=meta.KeyIndexInfo
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

                    var buffer = new byte[1024];
                    using (var reader = ObjTextReader.CreateReader(GetKeyFile(tablename)))
                    {
                        reader.SetPostion(posstart);

                        foreach (var item in reader.ReadObjectsWating<BigEntityTableIndexItem>(1, null, buffer))
                        {
                            //var item = reader.ReadObject<BigEntityTableIndexItem>();
                            if (reader.ReadedPostion() > posend)
                            {
                                return null;
                            }
                            if (item.Key.Equals(key))
                            {
                                if (item.Del)
                                {
                                    return null;
                                }
                                return item;
                            }
                        }
                    }
                }

                return findkeyitem;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        /// <summary>
        /// 查找索引，不加锁
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="meta"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private IEnumerable<BigEntityTableIndexItem> FindIndex(string tablename, BigEntityTableMeta meta, IndexInfo index, object[] value, long offset = 0)
        {
            var findkey = new BigEntityTableIndexItem { Key = value, Offset = offset, Index = index };
            var indexkey = tablename + ":" + index.IndexName;
            var indexarr = keyindexdisklist[indexkey];
            int mid = -1;
            int pospev = -1;
            int posnext = -1;
            int pos = new Collections.SorteArray<BigEntityTableIndexItem>(indexarr).Find(findkey, ref mid);

            if (pos == -1 && (mid == -1 || mid == indexarr.Length - 1))
            {
                //应该没有数据
            }
            else if (pos > -1)
            {
                pospev = pos;
                while (pospev >= 0)
                {
                    var compare = indexarr[pospev].CompareTo(findkey);
                    if (compare < 0 || pospev == 0)
                    {
                        break;
                    }

                    pospev--;
                }
                posnext = pos;
                while (posnext <= indexarr.Length - 1)
                {
                    var compare = indexarr[posnext].CompareTo(findkey);
                    if (compare > 0 || posnext == indexarr.Length - 1)
                    {
                        break;
                    }

                    posnext++;
                }
            }
            else
            {
                pospev = mid;
                posnext = mid;
                while (posnext <= indexarr.Length - 1)
                {
                    var compare = indexarr[posnext].CompareTo(findkey);
                    if (compare > 0 || posnext == indexarr.Length - 1)
                    {
                        break;
                    }

                    posnext++;

                }
            }
            ProcessTraceUtil.Trace("start scan disk index");
            if (pospev != -1 && posnext != -1 && pospev <= posnext)
            {
                var posstart = indexarr[pospev].KeyOffset;
                var posend = indexarr[posnext].KeyOffset;

                var buffer = new byte[1024];
                int skipcount = 0;
                using (var reader = ObjTextReader.CreateReader(GetIndexFile(tablename, index.IndexName)))
                {
                    ProcessTraceUtil.Trace("open indexfile");
                    reader.SetPostion(posstart);
                    ProcessTraceUtil.Trace("set pos:" + posstart);
                    foreach (var item in reader.ReadObjectsWating<BigEntityTableIndexItem>(1, null, buffer))
                    {
                        skipcount++;
                        if (skipcount == 1)
                        {
                            ProcessTraceUtil.Trace("find first index");
                        }
                        item.SetIndex(index);
                        if (!item.Del)
                        {
                            if (item.CompareTo(findkey) == 0)
                            {
                                yield return item;
                            }
                        }
                        if (reader.ReadedPostion() > posend)
                        {
                            break;
                        }
                    }
                }
                ProcessTraceUtil.Trace("scan disk index end");
                ProcessTraceUtil.Trace("扫描索引条数:" + skipcount);
            }

            ProcessTraceUtil.Trace("find in temp mem");
            //内存里查找
            foreach (var item in keyindexmemtemplist[indexkey].FindAll(findkey))
            {
                yield return item;
            }

            ProcessTraceUtil.Trace("find in mem");
            foreach (var item in keyindexmemlist[indexkey].FindAll(findkey))
            {
                yield return item;
            }

            ProcessTraceUtil.Trace("find in mem end");
        }

        public T Find<T>(string tablename, object key) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            BigEntityTableMeta meta = GetMetaData(tablename);
            var memlist = keyindexmemlist[tablename];
            var findkey = new BigEntityTableIndexItem() { Key = new object[] { key }, Index = meta.KeyIndexInfo };
            BigEntityTableIndexItem indexitem = memlist.Find(findkey) ??
                keyindexmemtemplist[tablename].Find(findkey);
            if (indexitem != null)
            {
                if (indexitem.Del)
                {
                    return default(T);
                }

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

            var indexarr = keyindexdisklist[tablename];
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
                    using (ObjTextReader otw = ObjTextReader.CreateReader(tablefile))
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
                int mid = -1;
                int pos = new Collections.SorteArray<BigEntityTableIndexItem>(indexarr).Find(findkey, ref mid);

                BigEntityTableIndexItem findkeyitem = null;
                if (pos == -1 && (mid == -1 || mid == indexarr.Length - 1))
                {
                    return default(T);
                }
                else if (pos > -1)
                {
                    findkeyitem = indexarr[pos];
                    if (findkeyitem.Del)
                    {
                        return default(T);
                    }
                }

                if (findkeyitem == null)
                {
                    var keymergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(meta.KeyName));
                    if (pos == -1 && keymergeinfo.LoadFactor == 1)
                    {
                        return default(T);
                    }

                    var posstart = indexarr[mid].KeyOffset;
                    var posend = indexarr[mid + 1].KeyOffset;

                    var buffer = new byte[1024];
                    using (var reader = ObjTextReader.CreateReader(GetKeyFile(tablename)))
                    {
                        reader.SetPostion(posstart);
                        foreach (var item in reader.ReadObjectsWating<BigEntityTableIndexItem>(1, null, buffer))
                        {
                            //var item = reader.ReadObject<BigEntityTableIndexItem>();
                            if (reader.ReadedPostion() > posend)
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

                if (findkeyitem != null && !findkeyitem.Del)
                {
                    using (var reader = ObjTextReader.CreateReader(tablefile))
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

        public IEnumerable<T> FindBatch<T>(string tablename, IEnumerable<object> keys) where T : new()
        {
            string tablefile = GetTableFile(tablename);
            BigEntityTableMeta meta = GetMetaData(tablename);
            BigEntityTableIndexItem indexitem = null;
            var indexarr = keyindexdisklist[tablename];

            var tablelocker = GetKeyLocker(tablename, string.Empty);

            var findkey = new BigEntityTableIndexItem();
            findkey.Index = meta.KeyIndexInfo;
            using (ObjTextReader otr = ObjTextReader.CreateReader(tablefile))
            {
                try
                {
                    tablelocker.EnterReadLock();
                    using (var keyreader = ObjTextReader.CreateReader(GetKeyFile(tablename)))
                    {
                        foreach (var key in keys)
                        {
                            findkey.Key = new object[] { key};
                            indexitem = keyindexmemlist[tablename].Find(findkey) ??
                                keyindexmemtemplist[tablename].Find(findkey);
                            if (indexitem != null)
                            {
                                if (!indexitem.Del)
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
                                else
                                {
                                    yield return default(T);
                                    continue;
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
                                        Key = new object[] { key},
                                        Index=meta.KeyIndexInfo
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
                                        var buffer = new byte[1024];
                                        foreach (var item in keyreader.ReadObjectsWating<BigEntityTableIndexItem>(1, null, buffer))
                                        {
                                            item.SetIndex(meta.KeyIndexInfo);
                                            //var item = keyreader.ReadObject<BigEntityTableIndexItem>();
                                            if (keyreader.ReadedPostion() > posend)
                                            {
                                                break;
                                            }
                                            if (item.CompareTo(findkey) == 0)
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
                finally
                {
                    tablelocker.ExitReadLock();
                }
            }
        }

        private BigEntityTableIndexItem GetDiskNear(string tablename,string keyorindex, object[] value, bool start)
        {
            var meta = GetMetaData(tablename);
            IndexInfo index = null;
            string keyfile = GetKeyFile(tablename);
            string keyname = tablename;
            if (string.IsNullOrWhiteSpace(keyorindex) || keyorindex == tablename)
            {
                index = meta.KeyIndexInfo;
            }
            else
            {
                index = meta.IndexInfos.FirstOrDefault(p => p.IndexName == keyorindex);
                if (index == null)
                {
                    throw new Exception("索引不存在:" + keyorindex);
                }
                keyname = tablename + ":" + index.IndexName;
                keyfile = GetIndexFile(tablename, index.IndexName);
            }
            var findkey = new BigEntityTableIndexItem { Key = value, Index = index };

            BigEntityTableIndexItem findkeyitem = null;
            //BigEntityTableIndexItem findkeyitem = keyindexmemlist[keyname].Find(findkey);
            //if (findkeyitem != null)
            //{
            //    return findkeyitem;
            //}

            BigEntityTableIndexItem[] indexarr = keyindexdisklist[keyname];
            int mid = -1;
            int pos = new Collections.SorteArray<BigEntityTableIndexItem>(indexarr).Find(findkey, ref mid);

            if (pos == -1 && (mid == -1 || mid == indexarr.Length - 1))
            {
                if (mid == -1 && indexarr.Length > 0)
                {
                    return indexarr[0];
                }
                return null;
            }
            else if (pos > -1)
            {
                findkeyitem = indexarr[pos];
                return findkeyitem;
            }


            var keymergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(index.IndexName));

            using (var keyreader = ObjTextReader.CreateReader(keyfile))
            {
                keyreader.SetPostion(indexarr[mid].KeyOffset);
                var endoffset = indexarr[mid + 1].KeyOffset;

                foreach (var item in keyreader.ReadObjectsWating<BigEntityTableIndexItem>(1))
                {
                    if (item.KeyOffset > endoffset)
                    {
                        break;
                    }

                    item.SetIndex(index);

                    if (start && item.CompareTo(findkey) >= 0)
                    {
                        return item;
                    }
                    if (!start && item.CompareTo(findkey) >= 0)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public IEnumerable<T> Scan<T>(string tablename,string keyorindex, object[] keystart, object[] keyend,int pi,int ps) where T : new()
        {
            var meta = GetMetaData(tablename);
            var index = (string.IsNullOrWhiteSpace(keyorindex) || keyorindex == tablename) ? meta.KeyIndexInfo : meta.IndexInfos.First(p => p.IndexName == keyorindex);
            var keyindex = (string.IsNullOrWhiteSpace(keyorindex) || keyorindex == tablename) ? tablename : (tablename + ":" + keyorindex);

            var tablelocker = GetKeyLocker(tablename, string.Empty);
            //List<long> keylist = new List<long>();
            List<BigEntityTableIndexItem> keylist = new List<BigEntityTableIndexItem>();
            List<BigEntityTableIndexItem> keylist2 = new List<BigEntityTableIndexItem>();
            List<BigEntityTableIndexItem> keylist3 = new List<BigEntityTableIndexItem>();
            var start = GetDiskNear(tablename, keyorindex, keystart, true);
            if (start != null)
            {
                var end = GetDiskNear(tablename, keyorindex, keyend, false);
                if (end != null)
                {
                    var compere = start.CompareTo(end);
                    if (compere <= 0)
                    {
                        if (compere == 0)
                        {
                            if (!start.Del)
                            {
                                keylist.Add(start);
                            }
                        }
                        else
                        {
                            var keyfile = (string.IsNullOrWhiteSpace(keyorindex) || keyorindex == tablename) ? GetKeyFile(tablename) : GetIndexFile(tablename, keyorindex);
                            try
                            {
                                tablelocker.EnterReadLock();
                                using (var keyreader = ObjTextReader.CreateReader(keyfile))
                                {
                                    keyreader.SetPostion(start.KeyOffset);
                                    foreach (var k in keyreader.ReadObjectsWating<BigEntityTableIndexItem>(0))
                                    {
                                        if (k.KeyOffset >= end.KeyOffset)
                                        {
                                            break;
                                        }
                                        if (!k.Del)
                                        {
                                            k.SetIndex(index);
                                            keylist.Add(k);
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                tablelocker.ExitReadLock();
                            }
                        }
                    }
                }
            }

            try
            {
                tablelocker.EnterReadLock();
                foreach (var item in keyindexmemlist[keyindex].Scan(new BigEntityTableIndexItem { Index = index, Key = keystart }, new BigEntityTableIndexItem { Index = index, Key = keyend }))
                {
                    keylist2.Add(item);
                }

                foreach (var item in keyindexmemtemplist[keyindex].Scan(new BigEntityTableIndexItem { Index = index, Key = keystart }, new BigEntityTableIndexItem { Index = index, Key = keyend }))
                {
                    keylist3.Add(item);
                }
            }
            finally
            {
                tablelocker.ExitReadLock();
            }

            keylist2 = MergeAndSort2(keylist2, keylist3).ToList();

            if (keylist.Count > 0 || keylist2.Count > 0)
            {
                int skip = (pi - 1) * ps;
                int curr = 0;
                int take = 0;
                using (ObjTextReader otr = ObjTextReader.CreateReader(GetTableFile(tablename)))
                {
                    foreach (var k in MergeAndSort2(keylist, keylist2))
                    {
                        if ((++curr) <= skip)
                        {
                            continue;
                        }
                        otr.SetPostion(k.Offset);
                        yield return otr.ReadObject<EntityTableItem<T>>().Data;
                        if ((++take) == ps)
                        {
                            break;
                        }
                    }
                }
            }
        }


        public IEnumerable<T> Find<T>(string tablename, Func<T, bool> findcondition) where T : new()
        {
            var meta = GetMetaData(tablename);
            var buffer = new byte[1024 * 1024 * 10];
            using (var reader = ObjTextReader.CreateReader(GetTableFile(tablename)))
            {
                var curroffset = 0L;
                foreach (var item in reader.ReadObjectsWating<EntityTableItem<T>>(1, p => curroffset = p, buffer))
                {
                    if (item == null)
                    {
                        yield break;
                    }

                    if (item.Flag == EntityTableItemFlag.Del)
                    {

                        continue;
                    }

                    var findkey = FindKey(tablename, meta.KeyProperty.GetValueMethed(item.Data));
                    if (findkey == null || findkey.Offset != curroffset)
                    {
                        continue;
                    }

                    if (findcondition(item.Data))
                    {
                        yield return item.Data;
                    }
                }
            }
        }
        #endregion

        #region 索引操作
        public IEnumerable<T> Find<T>(string tablename, string indexname, object[] value) where T : new()
        {
            var tablefile = GetTableFile(tablename);
            var meta = GetMetaData(tablename);
            var index = meta.IndexInfos.First(p => p.IndexName == indexname);
            if (index == null)
            {
                throw new Exception("索引不存在:"+indexname);
            }
            var locker = GetKeyLocker(tablename, string.Empty);
            List<BigEntityTableIndexItem> indexlist = null;
            locker.EnterReadLock();
            try
            {
                indexlist = FindIndex(tablename, meta,index , value).ToList();
            }
            finally
            {
                locker.ExitReadLock();
            }
            if (indexlist.Count > 0)
            {
                using (ObjTextReader reader = ObjTextReader.CreateReader(tablefile))
                {
                    ProcessTraceUtil.Trace("open tablefile");
                    foreach (var item in indexlist)
                    {
                        reader.SetPostion(item.Offset);
                        var data = reader.ReadObject<EntityTableItem<T>>();
                        if (data.Flag == EntityTableItemFlag.Ok)
                        {
                            yield return data.Data;
                        }
                    }
                    ProcessTraceUtil.Trace("find end");
                }
            }
        }

        public int Count(string tablename, string indexname, object[] value)
        {
            var meta = GetMetaData(tablename);
            var index = meta.IndexInfos.First(p => p.IndexName == indexname);
            if (index == null)
            {
                throw new Exception("索引不存在:" + indexname);
            }
            var locker = GetKeyLocker(tablename, string.Empty);
            locker.EnterReadLock();
            try
            {
                List<BigEntityTableIndexItem> indexlist = FindIndex(tablename, meta, index, value).ToList();
                return indexlist.Count;
            }
            finally
            {
                locker.ExitReadLock();
            }

        }

        public void ReBuildIndex(string tablename)
        {
            var tablelocker = GetKeyLocker(tablename, string.Empty);
            var tablefile = GetTableFile(tablename);
            tablelocker.EnterWriteLock();
            try
            {
                var files = System.IO.Directory.GetFiles(dirbase);
                foreach (var f in files)
                {
                    var filename = System.IO.Path.GetFileName(f);
                    if (filename.EndsWith(".id") && filename.StartsWith(tablename))
                    {
                        System.IO.File.Delete(f);
                    }
                }
                //开始创建
                string metafile = GetMetaFile(tablename);
                if (!File.Exists(metafile))
                {
                    throw new Exception("找不到元文件:" + tablename);
                }
            }
            finally
            {
                tablelocker.ExitWriteLock();
            }
        }
        #endregion
    }
}
