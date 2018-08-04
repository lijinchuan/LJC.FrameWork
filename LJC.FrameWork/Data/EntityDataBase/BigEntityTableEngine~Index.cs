using LJC.FrameWork.Collections;
using LJC.FrameWork.Comm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LJC.FrameWork.Data.EntityDataBase
{
    partial class BigEntityTableEngine
    {
        private void LoadIndex(string tablename, string index, BigEntityTableMeta meta)
        {
            string indexfile = GetIndexFile(tablename, index);
            var indexmergeinfo = meta.IndexMergeInfos.Find(p => p.IndexName.Equals(index));
            if (indexmergeinfo == null)
            {
                indexmergeinfo = new IndexMergeInfo();
                indexmergeinfo.IndexName = index;
                meta.IndexMergeInfos.Add(indexmergeinfo);
            }
            var indexkey = tablename + ":" + index;
            //计算加载因子
            indexmergeinfo.LoadFactor = (int)Math.Max(4, new FileInfo(indexfile).Length / MAX_KEYBUFFER);

            int i = 0;
            BigEntityTableIndexItem lastreadindex = null;
            List<BigEntityTableIndexItem> list = new List<BigEntityTableIndexItem>();
            long currentpos = 0;
            byte[] buffer = new byte[1024 * 1024 * 10];
            using (ObjTextReader idx = ObjTextReader.CreateReader(indexfile))
            {
                Console.WriteLine("loadindex:" + index);
                foreach (var newindex in idx.ReadObjectsWating<BigEntityTableIndexItem>(1, p => currentpos = p, buffer))
                {
                    newindex.KeyOffset = currentpos;
                    if (newindex.KeyOffset >= indexmergeinfo.IndexMergePos)
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

                if (list.Count > 0 && list.Last().KeyOffset != lastreadindex.KeyOffset)
                {
                    list.Add(lastreadindex);
                }
            }
            indexmergeinfo.TotalCount = i;

            BigEntityTableIndexItem[] oldindexitems = null;
            keyindexdisklist.TryRemove(indexkey, out oldindexitems);
            keyindexdisklist.TryAdd(indexkey, list.ToArray());

            using (ObjTextReader idr = ObjTextReader.CreateReader(indexfile))
            {
                Console.WriteLine("loadindex2:" + index);

                if (indexmergeinfo.IndexMergePos > 0)
                {
                    idr.SetPostion(indexmergeinfo.IndexMergePos);
                }
                //Dictionary<string, BigEntityTableIndexItem> indexdic = new Dictionary<string, BigEntityTableIndexItem>();
                //keyindexlistdic[tablename];
                SortArrayList<BigEntityTableIndexItem> keymemlist = new SortArrayList<BigEntityTableIndexItem>();

                foreach (var newindex in idr.ReadObjectsWating<BigEntityTableIndexItem>(1, p => currentpos = p, buffer))
                {
                    newindex.KeyOffset = currentpos;
                    if (!newindex.Del)
                    {
                        //indexdic.Add(newindex.Key, newindex);
                        keymemlist.Add(newindex);
                        i++;
                    }
                }

                var tablelocker = GetKeyLocker(tablename, string.Empty);
                tablelocker.EnterWriteLock();
                try
                {
                    foreach (var newindex in idr.ReadObjectsWating<BigEntityTableIndexItem>(1, p => currentpos = p))
                    {
                        newindex.KeyOffset = currentpos;
                        if (!newindex.Del)
                        {
                            //indexdic.Add(newindex.Key, newindex);
                            keymemlist.Add(newindex);
                            i++;
                        }
                    }
                    //keyindexlistdic[tablename] = indexdic;
                    keyindexmemlist[indexkey] = keymemlist;
                }
                finally
                {
                    tablelocker.ExitWriteLock();
                }
            }
        }

        #region 合并其它索引
        public void MergeIndex(string tablename, string indexname)
        {
            var meta = GetMetaData(tablename);
            MergeIndex2(tablename, indexname, meta);
        }

        public void MergeIndex2(string tablename, string indexname, BigEntityTableMeta meta)
        {
            ProcessTraceUtil.StartTrace();

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
            string newindexfile = string.Empty;
            try
            {
                ProcessTraceUtil.TraceMem("开始整理，索引名称:" + tablename + "." + indexname, "m");

                long lasmargepos = 0;
                long newIndexMergePos = mergeinfo.IndexMergePos;
                string indexfile = indexname.Equals(meta.KeyName) ? GetKeyFile(tablename) : GetIndexFile(tablename, indexname);
                var tablelocker = GetKeyLocker(tablename, string.Empty);
                string indexkey = tablename + ":" + indexname;

                //新的磁盘索引
                List<BigEntityTableIndexItem> newdiskindexlist = new List<BigEntityTableIndexItem>();
                var loadFactor = (int)Math.Max(4, new FileInfo(indexfile).Length / MAX_KEYBUFFER);

                using (var reader = ObjTextReader.CreateReader(indexfile))
                {
                    long readstartpostion = reader.ReadedPostion();
                    tablelocker.EnterWriteLock();
                    try
                    {
                        keyindexmemtemplist[indexkey] = keyindexmemlist[indexkey];
                        keyindexmemlist[indexkey] = new SortArrayList<BigEntityTableIndexItem>();
                    }
                    finally
                    {
                        tablelocker.ExitWriteLock();
                    }
                    var listtemp = keyindexmemtemplist[indexkey].GetList().ToList();
                    listtemp = listtemp.Select(p => new BigEntityTableIndexItem
                    {
                        Del = p.Del,
                        Key = p.Key,
                        KeyOffset = p.KeyOffset,
                        len = p.len,
                        Offset = p.Offset,
                        KeyType = p.KeyType
                    }).ToList();
                    int readcount = listtemp.Count;

                    ProcessTraceUtil.TraceMem("从内存中读取新增数据，共" + readcount + "条", "m");

                    if (readcount == 0)
                    {
                        return;
                    }

                    lasmargepos = listtemp.Max(p => p.KeyOffset);
                    reader.SetPostion(lasmargepos);
                    reader.ReadObject<BigEntityTableIndexItem>();
                    lasmargepos = reader.ReadedPostion();

                    //优化确定哪些部分是不需要一个个读入的
                    long copystart = 0, copyend = 0;
                    var indexarray = keyindexdisklist[indexkey];
                    using (var sortarray = new Collections.SorteArray<BigEntityTableIndexItem>(indexarray))
                    {
                        int mid = -1;
                        int pos = sortarray.Find(listtemp.First(), ref mid);
                        ProcessTraceUtil.Trace("查找新数据在老数中插入的开始位置:mid=" + mid + ",pos=" + pos);
                        if (pos == -1 && mid != -1)
                        {
                            //小于最小的
                            copystart = indexarray[mid].KeyOffset;
                            ProcessTraceUtil.Trace("老数据可以直接copy的部分:0->" + copystart);
                        }
                        else if (pos != -1)
                        {
                            copystart = indexarray[pos].KeyOffset;
                            ProcessTraceUtil.Trace("老数据可以直接copy的部分:0->" + copystart);
                        }

                        //优化确定后面读到哪

                        mid = -1;
                        pos = sortarray.Find(listtemp.Last(), ref mid);
                        ProcessTraceUtil.Trace("查找新数据在老数据中插入的结束位置:mid=" + mid + ",pos=" + pos);
                        if (pos == -1 && mid != -1 && mid < indexarray.Length - 1)
                        {
                            //小于最小的
                            copyend = indexarray[mid + 1].KeyOffset;
                            ProcessTraceUtil.Trace("老数据可以直接copy的部分:" + copyend + "->" + mergeinfo.IndexMergePos);
                        }
                        else if (pos != -1)
                        {
                            copyend = indexarray[pos].KeyOffset;
                            ProcessTraceUtil.Trace("老数据可以直接copy的部分:" + copyend + "->" + mergeinfo.IndexMergePos);
                        }
                    }

                    newindexfile = (indexname.Equals(meta.KeyName) ? GetKeyFile(tablename) : GetIndexFile(tablename, indexname)) + ".temp";
                    if (File.Exists(newindexfile))
                    {
                        File.Delete(newindexfile);
                    }
                    //快速copy
                    if (copystart > 0)
                    {
                        ProcessTraceUtil.TraceMem("直接copy前面不在排序范围的数据:0->" + copystart, "m");
                        IOUtil.CopyFile(indexfile, newindexfile, FileMode.Create, 0, copystart - 1);
                        readstartpostion = copystart;
                        ProcessTraceUtil.TraceMem("copy数据完成", "m");

                        newdiskindexlist.AddRange(indexarray.Where(p => p.KeyOffset < copystart));
                    }

                    bool isall = false;
                    ProcessTraceUtil.TraceMem("开始读取在排序范围内的数据", "m");
                    while (true)
                    {
                        ProcessTraceUtil.Trace("读取老数据,开始位置:" + readstartpostion);
                        reader.SetPostion(readstartpostion);
                        var listordered = new List<BigEntityTableIndexItem>();
                        var loadcount = 0;
                        long keyoffset = 0;
                        foreach (var item in reader.ReadObjectsWating<BigEntityTableIndexItem>(1, p => keyoffset = p))
                        {
                            if (item.KeyOffset != keyoffset)
                            {
                                //ProcessTraceUtil.Trace("数据位置不同，记录的:" + item.KeyOffset + "，实际:" + keyoffset + "，校正");
                                item.KeyOffset = keyoffset;
                            }

                            if (item.KeyOffset >= mergeinfo.IndexMergePos)
                            {
                                break;
                            }
                            if (copyend > 0 && item.KeyOffset >= copyend)
                            {
                                break;
                            }
                            listordered.Add(item);
                            if (++loadcount >= MERGE_TRIGGER_NEW_COUNT)
                            {
                                break;
                            }
                        }

                        readstartpostion = reader.ReadedPostion();
                        bool isonlyorderedlist = false;

                        if (listordered.Count == 0)
                        {
                            ProcessTraceUtil.TraceMem("老数据没有了，全部是新数据:" + listtemp.Count, "m");

                            listordered = MergeAndSort2(listordered, listtemp).ToList();
                            isall = true;
                        }
                        else if (listtemp.Count == 0 && listordered.Count > 10000)
                        {
                            ProcessTraceUtil.TraceMem("新数据没有了，全部是老数据:" + listordered.Count, "m");
                            //copy
                            IOUtil.CopyFile(indexfile, newindexfile, FileMode.Open, listordered.First().KeyOffset, listordered.Last().KeyOffset - 1, false);
                            var listorderedlast = listordered.Last();
                            long copyoffset = 0;
                            using (var nw = ObjTextWriter.CreateWriter(newindexfile, ObjTextReaderWriterEncodeType.entitybuf2))
                            {
                                var item = new BigEntityTableIndexItem { Del = listorderedlast.Del, Key = listorderedlast.Key, len = listorderedlast.len, Offset = listorderedlast.Offset, KeyType = listorderedlast.KeyType };
                                item.KeyOffset = nw.GetWritePosition();

                                copyoffset = nw.GetWritePosition() - listorderedlast.KeyOffset;

                                nw.AppendObject(item);
                                newIndexMergePos = nw.GetWritePosition();
                            }
                            isonlyorderedlist = true;
                            //更新索引
                            foreach (var item in listordered)
                            {
                                item.KeyOffset += copyoffset;
                            }

                            ProcessTraceUtil.TraceMem("直接copy数据完成", "m");
                        }
                        else
                        {
                            ProcessTraceUtil.TraceMem("老数据条数为:" + listordered.Count + "，新数据条数为:" + listtemp.Count, "m");

                            var listordermax = listordered.Last();

                            int idx = 0;
                            foreach (var item in listtemp)
                            {
                                if (item.CompareTo(listordermax) > 0)
                                {
                                    break;
                                }
                                else
                                {
                                    idx++;
                                }
                            }
                            List<BigEntityTableIndexItem> smalllist = listtemp.Take(idx).ToList();
                            listtemp = listtemp.Skip(idx).ToList();
                            listordered = MergeAndSort2(listordered, smalllist).ToList();
                            ProcessTraceUtil.TraceMem("排序完成:" + listordered.Count + "条", "m");
                        }

                        if (listordered.Count > 0)
                        {
                            if (!isonlyorderedlist)
                            {
                                ProcessTraceUtil.TraceMem("把排好的数据写入到新索引文件:" + listordered.Count + "条", "m");
                                using (var nw = ObjTextWriter.CreateWriter(newindexfile, ObjTextReaderWriterEncodeType.entitybuf2))
                                {
                                    foreach (var item in listordered)
                                    {
                                        item.KeyOffset = nw.GetWritePosition();
                                        nw.AppendObject(item);
                                    }
                                    newIndexMergePos = nw.GetWritePosition();
                                }
                                ProcessTraceUtil.TraceMem("写入到新索引文件完成", "m");
                            }

                            if (listordered.Count <= 2 || loadFactor == 1)
                            {
                                newdiskindexlist.AddRange(listordered);
                            }
                            else
                            {
                                newdiskindexlist.Add(listordered.First());
                                int idx = 0;
                                foreach (var item in listordered)
                                {
                                    if ((++idx) % loadFactor == 0)
                                    {
                                        newdiskindexlist.Add(item);
                                    }
                                }
                                //newdiskindexlist.AddRange(listordered.Where(p => (++idx) % loadFactor == 0));
                                if (idx % loadFactor != 0)
                                {
                                    newdiskindexlist.Add(listordered.Last());
                                }
                            }
                        }

                        ProcessTraceUtil.TraceMem("写入到新索引文件后整理索引完成", "m");

                        if (isall)
                        {
                            if (copyend > 0 && copyend < mergeinfo.IndexMergePos)
                            {
                                ProcessTraceUtil.TraceMem("copy已排序的大于新增最大数部分" + copyend + "->" + mergeinfo.IndexMergePos, "m");
                                var offset = 0L;
                                using (var nw = ObjTextWriter.CreateWriter(newindexfile, ObjTextReaderWriterEncodeType.entitybuf2))
                                {
                                    offset = nw.GetWritePosition() - copyend;
                                }
                                //copy
                                long newindexpos = IOUtil.CopyFile(indexfile, newindexfile, FileMode.Open, copyend, mergeinfo.IndexMergePos - 1, false);

                                foreach (var p in indexarray)
                                {
                                    if (p.KeyOffset >= copyend && p.KeyOffset < mergeinfo.IndexMergePos)
                                    {
                                        newdiskindexlist.Add(new BigEntityTableIndexItem
                                        {
                                            Del = p.Del,
                                            Key = p.Key,
                                            KeyOffset = p.KeyOffset + offset,
                                            len = p.len,
                                            Offset = p.Offset,
                                            KeyType = p.KeyType
                                        });
                                    }
                                }

                                ProcessTraceUtil.TraceMem("copy数据完成->" + offset, "m");
                            }
                            break;
                        }
                        else
                        {
                            if (listtemp.Count > 0)
                            {
                                long newcopyend = 0;
                                using (var sortarray = new Collections.SorteArray<BigEntityTableIndexItem>(indexarray))
                                {
                                    int mid = -1;
                                    int pos = sortarray.Find(listtemp.First(), ref mid);
                                    ProcessTraceUtil.Trace("查找已经排序的小于新增最小数据部分:mid=" + mid + ",pos=" + pos);
                                    if (pos == -1 && mid != -1)
                                    {
                                        //小于最小的
                                        newcopyend = indexarray[mid].KeyOffset;
                                    }
                                    else if (pos != -1)
                                    {
                                        newcopyend = indexarray[pos].KeyOffset;
                                    }
                                }
                                if (newcopyend > readstartpostion)
                                {
                                    ProcessTraceUtil.Trace("中间copy");
                                    var offset = 0L;
                                    using (var nw = ObjTextWriter.CreateWriter(newindexfile, ObjTextReaderWriterEncodeType.entitybuf2))
                                    {
                                        offset = nw.GetWritePosition() - readstartpostion;
                                    }
                                    IOUtil.CopyFile(indexfile, newindexfile, FileMode.Open, readstartpostion, newcopyend - 1, false);

                                    foreach (var p in indexarray)
                                    {
                                        if (p.KeyOffset >= readstartpostion && p.KeyOffset < newcopyend)
                                        {
                                            newdiskindexlist.Add(new BigEntityTableIndexItem
                                            {
                                                Del = p.Del,
                                                Key = p.Key,
                                                KeyOffset = p.KeyOffset + offset,
                                                len = p.len,
                                                Offset = p.Offset,
                                                KeyType = p.KeyType
                                            });
                                        }
                                    }

                                    readstartpostion = newcopyend;
                                    ProcessTraceUtil.Trace("中间copy完成");
                                }
                            }
                            else
                            {
                                ProcessTraceUtil.Trace("中间copy2");
                                var offset = 0L;
                                using (var nw = ObjTextWriter.CreateWriter(newindexfile, ObjTextReaderWriterEncodeType.entitybuf2))
                                {
                                    offset = nw.GetWritePosition() - readstartpostion;
                                }
                                IOUtil.CopyFile(indexfile, newindexfile, FileMode.Open, readstartpostion, copyend - 1, false);

                                foreach (var p in indexarray)
                                {
                                    if (p.KeyOffset >= readstartpostion && p.KeyOffset < copyend)
                                    {
                                        newdiskindexlist.Add(new BigEntityTableIndexItem
                                        {
                                            Del = p.Del,
                                            Key = p.Key,
                                            KeyOffset = p.KeyOffset + offset,
                                            len = p.len,
                                            Offset = p.Offset,
                                            KeyType = p.KeyType
                                        });
                                    }
                                }

                                readstartpostion = copyend;
                                ProcessTraceUtil.Trace("中间copy2完成");
                            }
                        }
                    }
                }

                //后面copy
                string tablefile = GetTableFile(tablename);

                var idxreader = ObjTextReader.CreateReader(indexfile);
                var newwriter = ObjTextWriter.CreateWriter(newindexfile, ObjTextReaderWriterEncodeType.entitybuf2);
                try
                {
                    long nextcopypos = 0;
                    ProcessTraceUtil.TraceMem("读取后面的数据->" + lasmargepos, "m");
                    idxreader.SetPostion(lasmargepos);
                    bool hasitem = false;
                    bool isfirst = true;
                    long keyoffset = 0;
                    foreach (var item in idxreader.ReadObjectsWating<BigEntityTableIndexItem>(1, p => keyoffset = p))
                    {
                        hasitem = true;
                        item.KeyOffset = keyoffset;
                        if (item.KeyOffset > newwriter.GetWritePosition())
                        {
                            var spacelen = item.KeyOffset - newwriter.GetWritePosition();
                            ProcessTraceUtil.Trace("没有对齐，尝试对齐，spacelen->" + spacelen);
                            if (spacelen % 3 == 0)
                            {
                                newwriter.FillSpace(spacelen / 3);
                            }
                            else
                            {
                                ProcessTraceUtil.Trace("对齐失败");
                                var ex = new Exception("无法对齐");
                                ex.Data.Add("老索引文件当前位置", item.KeyOffset);
                                ex.Data.Add("新索引文件写入位置", newwriter.GetWritePosition());
                                throw ex;
                            }
                        }

                        if (item.KeyOffset == newwriter.GetWritePosition())
                        {
                            nextcopypos = newwriter.GetWritePosition();
                            if (isfirst)
                            {
                                newIndexMergePos = nextcopypos;
                            }
                            ProcessTraceUtil.Trace("新老索引文件已经对齐:" + item.KeyOffset);
                            break;
                        }

                        isfirst = false;
                        item.KeyOffset = newwriter.GetWritePosition();
                        newwriter.AppendObject(item);
                    }

                    if (nextcopypos > 0)
                    {
                        newwriter.Dispose();
                        ProcessTraceUtil.Trace("copy后面的数据->" + nextcopypos);
                        nextcopypos = IOUtil.CopyFile(indexfile, newindexfile, FileMode.Open, nextcopypos, -512);
                    }
                    else if (!hasitem)
                    {
                        var idxpos = idxreader.ReadedPostion();
                        if (idxpos == newwriter.GetWritePosition())
                        {
                            nextcopypos = idxpos;
                            newwriter.Dispose();
                        }
                        else
                        {
                            ProcessTraceUtil.Trace(idxpos + " vs " + newwriter.GetWritePosition());
                        }
                    }
                    ProcessTraceUtil.TraceMem("读取后面的数据完成", "m");
                    if (nextcopypos == 0)
                    {
                        throw new Exception("更新索引出错");
                    }

                    try
                    {
                        BigEntityTableIndexItem[] oldindexarray = null;
                        BigEntityTableIndexItem[] newdiskindexarray = newdiskindexlist.ToArray();

                        tablelocker.EnterWriteLock();
                        ProcessTraceUtil.TraceMem("读取后面的数据->" + lasmargepos, "m");
                        if (nextcopypos <= 0)
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
                        }
                        else
                        {
                            nextcopypos = IOUtil.CopyFile(indexfile, newindexfile, FileMode.Open, nextcopypos, long.MaxValue);
                            ProcessTraceUtil.TraceMem("继续copy后面的数据->" + nextcopypos, "m");
                            idxreader.Dispose();
                        }
                        ProcessTraceUtil.TraceMem("读取后面的数据完成", "m");

                        //更新索引
                        mergeinfo.LoadFactor = loadFactor;
                        keyindexdisklist.TryRemove(indexkey, out oldindexarray);
                        keyindexdisklist.TryAdd(indexkey, newdiskindexarray);
                        keyindexmemtemplist[indexkey] = new SortArrayList<BigEntityTableIndexItem>();

                        int trycount = 0;
                        while (true)
                        {
                            try
                            {
                                File.Delete(indexfile);
                                ProcessTraceUtil.Trace("删除旧文件完成");
                                break;
                            }
                            catch (System.IO.IOException ex)
                            {
                                Thread.Sleep(1);
                                trycount++;
                                if (trycount > 1000)
                                {
                                    throw ex;
                                }
                            }
                        }

                        File.Move(newindexfile, indexfile);

                        ProcessTraceUtil.TraceMem("删除旧文件，重命名新文件完成", "m");

                        idxreader = null;
                    }
                    finally
                    {
                        tablelocker.ExitWriteLock();
                    }


                    string metafile = GetMetaFile(tablename);
                    mergeinfo.IndexMergePos = newIndexMergePos;
                    LJC.FrameWork.Comm.SerializerHelper.SerializerToXML(meta, metafile, true);

                    ProcessTraceUtil.Trace("更新元文件，更新索引完成");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("整理索引后面部分出错" + ex.ToString());

                    LogManager.LogHelper.Instance.Error("整理索引后面部分出错", ex);
                }
                finally
                {
                    if (idxreader != null)
                    {
                        idxreader.Dispose();
                    }

                    if (newwriter != null && !newwriter.Isdispose)
                    {
                        newwriter.Dispose();
                    }

                    if (File.Exists(newindexfile))
                    {
                        File.Delete(newindexfile);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                LogManager.LogHelper.Instance.Error("整理索引出错", ex);
            }
            finally
            {
                //GC.Collect();
                //ProcessTraceUtil.TraceMem("回收内存","m");
                mergeinfo.IsMergin = false;
                var info = ProcessTraceUtil.PrintTrace();
                Console.WriteLine(info);
                LogManager.LogHelper.Instance.Debug("整理索引过程:" + info);
            }
        }
        #endregion
    }
}
