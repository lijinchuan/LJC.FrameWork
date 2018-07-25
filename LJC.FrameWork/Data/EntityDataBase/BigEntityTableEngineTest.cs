using LJC.FrameWork.Comm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.EntityDataBase
{
    partial class BigEntityTableEngine
    {
        public void AssertFindKeyMem(string tablename)
        {
            var meta = GetMetaData(tablename);
            foreach (var idx in meta.Indexs)
            {
                var idxkey = tablename + ":" + idx;
                var memlist = keyindexmemlist[idxkey];
                var alllist = memlist.GetList().ToList();
                foreach (var key in alllist)
                {
                    var finditem = memlist.FindAll(key).FirstOrDefault();
                    if (finditem == null)
                    {
                        throw new Exception("not find index item " + key.Key + "," + key.Offset);
                    }
                }
            }

        }

        public void AssertFindEqual<T>(string tablename) where T : new()
        {
            var meta = GetMetaData(tablename);
            List<Dictionary<object, int>> list = LJC.FrameWork.Comm.LocalCacheManager<List<Dictionary<object, int>>>.Find("asfasfaefaes", () =>
            {
                List<Dictionary<object, int>> list1 = new System.Collections.Generic.List<Dictionary<object, int>>();

                List<T> listt = new System.Collections.Generic.List<T>();
                foreach (var item in LocalEngine.Find<T>(tablename, (p) => true))
                {
                    listt.Add(item);
                }

                foreach (var idx in meta.Indexs)
                {
                    Dictionary<object, int> keyscount = new Dictionary<object, int>();
                    foreach (var item in listt)
                    {
                        var idxval = meta.IndexProperties[idx].GetValueMethed(item);
                        if (keyscount.ContainsKey(idxval))
                        {
                            keyscount[idxval] = keyscount[idxval] + 1;
                        }
                        else
                        {
                            keyscount.Add(idxval, 1);
                        }
                    }
                    list1.Add(keyscount);
                }

                return list1;
            }, 36000);

            Console.WriteLine("分析完成");

            int i = 0;
            int cnt = 0;
            DateTime now = DateTime.Now;
            //var thekey = "新疆城建";
            foreach (var dic in list)
            {
                foreach (var kv in dic)
                {
                    cnt++;

                    //if (!kv.Key.Equals(thekey))
                    //{
                    //    continue;
                    //}

                    ProcessTraceUtil.StartTrace();

                    var count = FindIndex(tablename, meta, meta.Indexs[i], kv.Key).Count();
                    if (count != kv.Value)
                    {
                        //Console.WriteLine("查找不相等:" + kv.Key + ",查找到的" + count + "!=统计的" + kv.Value);
                        LogManager.LogHelper.Instance.Error("[" + meta.Indexs[i] + "]查找不相等:" + kv.Key + ",查找到的" + count + "!=统计的" + kv.Value);
                    }

                    //Console.WriteLine(ProcessTraceUtil.PrintTrace());
                    if (cnt % 10000 == 0)
                    {
                        Console.WriteLine("10000条用时:" + (DateTime.Now.Subtract(now)).TotalMilliseconds + "ms");
                        now = DateTime.Now;
                    }
                }
                i++;
            }
        }
    }
}
