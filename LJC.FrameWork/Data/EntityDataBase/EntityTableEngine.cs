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
        ConcurrentDictionary<string, ConcurrentDictionary<string, ArrayList>> indexdic = new ConcurrentDictionary<string, ConcurrentDictionary<string, ArrayList>>();
        Dictionary<string, object> keylocker = new Dictionary<string, object>();

        string dirbase = System.AppDomain.CurrentDomain.BaseDirectory;

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

        public EntityTableEngine(string dir)
        {
            if (!string.IsNullOrWhiteSpace(dir))
            {
                this.dirbase = dir;
            }
        }

        public void CreateTable(string tablename,string keyname,Type ttype)
        {
            string tablefile =dirbase+"\\"+ tablename;
            bool delfile = true;
            if (!File.Exists(tablefile))
            {
                try
                {
                    using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                    {
                        string metafile = dirbase + "\\" + tablename + ".meta";
                        if (!File.Exists(metafile))
                        {
                            EntityTableMeta meta = new EntityTableMeta();
                            meta.KeyName = keyname;
                            meta.CTime = DateTime.Now;
                            meta.TType = ttype;
                            var pp = ttype.GetProperty(keyname);
                            if (pp == null)
                            {
                                throw new Exception("找不到主键:" + keyname);
                            }
                            meta.KeyProperty = new PropertyInfoEx(pp);
                            LJC.FrameWork.Comm.SerializerHelper.SerializerToXML<EntityTableMeta>(meta, metafile, catchErr: true);
                            metadic.Add(tablename, meta);

                            indexdic.TryAdd(tablename, new ConcurrentDictionary<string, ArrayList>());
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
                        string indexfile = dirbase + "\\" + tablename + ".id";
                        using (ObjTextWriter idx = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                        {
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

        private void LoadIndex(string tablename)
        {
            string indexfile = dirbase + "\\" + tablename + ".id";
            using (ObjTextReader idx = ObjTextReader.CreateReader(indexfile))
            {
                var idc = indexdic[tablename];
                ArrayList al = null;
                foreach(var newindex in idx.ReadObjectsWating<EntityTableIndexItem>(1))
                {
                    if (!idc.TryGetValue(newindex.Key, out al))
                    {
                        lock (idc)
                        {
                            if (!idc.TryGetValue(newindex.Key, out al))
                            {
                                al = new ArrayList();
                                idc.TryAdd(newindex.Key, al);
                            }
                        }
                    }

                    if (newindex.Del)
                    {
                        object dela = null;
                        foreach (var a in al)
                        {
                            if (((EntityTableIndexItem)a).Offset == newindex.Offset)
                            {
                                dela = a;
                                break;
                            }
                        }
                        if (dela != null)
                        {
                            al.Remove(dela);
                        }
                    }
                    else
                    {
                        lock (al)
                        {
                            al.Add(newindex);
                        }
                    }
                }
            }
        }

        private EntityTableMeta GetMetaData(string tablename)
        {
            EntityTableMeta meta=null;
            if(metadic.TryGetValue(tablename,out meta))
            {
                return meta;
            }

            string metafile = dirbase + "\\" + tablename + ".meta";
            if (!File.Exists(metafile))
            {
                throw new Exception("找不到元文件:" + tablename);
            }

            meta = LJC.FrameWork.Comm.SerializerHelper.DeSerializerFile<EntityTableMeta>(metafile, true);
            meta.KeyProperty = new PropertyInfoEx(meta.TType.GetProperty(meta.KeyName));
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

            if (!indexdic.ContainsKey(tablename))
            {
                lock (indexdic)
                {
                    if (!indexdic.ContainsKey(tablename))
                    {
                        indexdic.TryAdd(tablename, new ConcurrentDictionary<string, ArrayList>());
                    }
                }
            }

            LoadIndex(tablename);

            return meta;
        }

        private bool Insert2<T>(string tablename, T item, EntityTableMeta meta) where T : new()
        {
            var keyvalue = item.Eval(meta.KeyProperty);
            //var keyvalue = meta.KeyProperty.GetValueMethed(item);
            if (keyvalue == null)
            {
                throw new Exception("key值不能为空");
            }
            string tablefile = dirbase + "\\" + tablename;
            var tableitem = new EntityTableItem<T>(item);
            tableitem.Flag = EntityTableItemFlag.Ok;
            var locker = GetKeyLocker(tablename, keyvalue.ToString());
            lock (locker)
            {
                using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
                {
                    var offset = otw.AppendObject(tableitem);

                    ArrayList al = null;
                    var idc = indexdic[tablename];
                    if (!idc.TryGetValue(keyvalue.ToString(), out al))
                    {
                        lock (idc)
                        {
                            if (!idc.TryGetValue(keyvalue.ToString(), out al))
                            {
                                al = new ArrayList();
                                idc.TryAdd(keyvalue.ToString(), al);
                            }
                        }
                    }

                    var newindex = new EntityTableIndexItem
                    {
                        Key = keyvalue.ToString(),
                        Offset = offset.Item1,
                        len = (int)(offset.Item2 - offset.Item1)
                    };
                    lock (al)
                    {
                        al.Add(newindex);
                    }

                    string indexfile = dirbase + "\\" + tablename + ".id";
                    using (ObjTextWriter idx = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                    {
                        idx.AppendObject(newindex);
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
            ArrayList arr = null;

            var locker = GetKeyLocker(tablename, key);
            lock (locker)
            {
                if (indexdic[tablename].TryRemove(key, out arr))
                {
                    string indexfile = dirbase + "\\" + tablename + ".id";
                    using (ObjTextWriter idx = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                    {
                        foreach (var item in arr)
                        {
                            var indexitem = (EntityTableIndexItem)item;
                            indexitem.Del = true;
                            idx.AppendObject(indexitem);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public bool Upsert<T>(string tablename, string key, T item) where T : new()
        {
            EntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != item.GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }
            
            ArrayList arr=null;
            if(indexdic[tablename].TryGetValue(key,out arr)&&arr.Count>0)
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
            string tablefile = dirbase + "\\" + tablename;
            ArrayList arr = null;
            Tuple<long, long> offset = null;
            int indexpos = 0;

            var locker = GetKeyLocker(tablename, key);
            lock (locker)
            {
                if (!indexdic[tablename].TryGetValue(key, out arr) || arr.Count == 0)
                {
                    throw new Exception(string.Format("更新失败，key为{0}的记录数为0", key));
                }

                indexpos = arr.Count - 1;
                string indexfile = dirbase + "\\" + tablename + ".id";
                EntityTableIndexItem indexitem = (EntityTableIndexItem)arr[indexpos];
                using (ObjTextWriter idx = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                {
                    indexitem.Del = true;
                    idx.AppendObject(indexitem);
                }

                var keyvalue = item.Eval(meta.KeyProperty);
                //var keyvalue=meta.KeyProperty.GetValueMethed(item);
                if (keyvalue == null)
                {
                    throw new Exception("key值不能为空");
                }
                var tableitem = new EntityTableItem<T>(item);
                tableitem.Flag = EntityTableItemFlag.Ok;
                using (ObjTextWriter otw = ObjTextWriter.CreateWriter(tablefile, ObjTextReaderWriterEncodeType.entitybuf))
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


                ArrayList al = null;
                var idc = indexdic[tablename];
                if (!idc.TryGetValue(keyvalue.ToString(), out al))
                {
                    lock (idc)
                    {
                        if (!idc.TryGetValue(keyvalue.ToString(), out al))
                        {
                            al = new ArrayList();
                            idc.TryAdd(keyvalue.ToString(), al);
                        }
                    }
                }

                EntityTableIndexItem newindex = new EntityTableIndexItem
                {
                    Key = keyvalue.ToString(),
                    Offset = offset.Item1,
                    len = (int)(offset.Item2 - offset.Item1),
                    Del = false
                };

                if (newindex.Offset == indexitem.Offset)
                {
                    newindex.len = indexitem.len;
                }

                arr[indexpos] = newindex;

                using (ObjTextWriter idx = ObjTextWriter.CreateWriter(indexfile, ObjTextReaderWriterEncodeType.entitybuf))
                {
                    idx.AppendObject(newindex);
                }

                Console.WriteLine("写入成功:" + keyvalue + "->" + offset);
            }
            return true;
        }

        public bool Update<T>(string tablename, string key, T item) where T : new()
        {
            EntityTableMeta meta = GetMetaData(tablename);

            if (meta.TType != item.GetType())
            {
                throw new NotSupportedException("不是期待数据类型:" + meta.TypeString);
            }

            return Update2(tablename, key, item,meta);
        }

        public bool Exists(string tablename, string key)
        {
            try
            {
                var meta = this.GetMetaData(tablename);

                return indexdic[tablename].ContainsKey(key);
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<T> Find<T>(string tablename, string key) where T : new()
        {
            string tablefile = dirbase + "\\" + tablename;
            EntityTableMeta meta = GetMetaData(tablename);
            ArrayList arr = null;
            EntityTableIndexItem indexitem = null;
            if (indexdic[tablename].TryGetValue(key, out arr))
            {
                //先找到offset
                using (ObjTextReader otw = ObjTextReader.CreateReader(tablefile))
                {
                    foreach (var o in arr)
                    {
                        indexitem = (EntityTableIndexItem)o;
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
