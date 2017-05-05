using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MD = MongoDB.Driver;
using MB = MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Linq.Expressions;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public static class MongoDBHelper
    {
        #region 连接
        /// <summary>
        /// 锁对象
        /// </summary>
        private static object lockObj = new object();

        /// <summary>
        /// 并发锁
        /// </summary>
        private static Dictionary<string, object> initLockManage = new Dictionary<string, object>();

        /// <summary>
        /// MongoDB连接实体
        /// </summary>
        private static Dictionary<string, MongoServer> mongClientPoolDict = new Dictionary<string, MongoServer>();


        public static MongoServer CreateInstanceUseConfig(string configname)
        {
            var connstr = System.Configuration.ConfigurationManager.AppSettings[configname];
            if (string.IsNullOrWhiteSpace(connstr))
            {
                throw new Exception(string.Format("mongo配置不存在或者为空值：{0}", configname));
            }

            return CreateInstance(connstr);
        }

        /// <summary>
        /// 创建实体对象
        /// </summary>
        /// <param name="strCon"></param>
        /// <returns></returns>
        public static MongoServer CreateInstance(string path)
        {
            MongoServer result = null;

            if (!mongClientPoolDict.ContainsKey(path))
            {
                if (!initLockManage.ContainsKey(path))
                {
                    lock (lockObj)
                    {
                        if (!initLockManage.ContainsKey(path))
                        {
                            initLockManage.Add(path, new object());
                        }
                    }
                }
                lock (initLockManage[path])
                {
                    if (!mongClientPoolDict.ContainsKey(path))
                    {
                        result = new MongoClient(path).GetServer();
                        mongClientPoolDict.Add(path, result);
                    }
                    else
                    {
                        result = mongClientPoolDict[path];
                    }
                }
            }
            else
            {
                result = mongClientPoolDict[path];
            }

            return result;
        }
        #endregion

        #region
        public static void Drop(string connectionName, string database, string collection)
        {
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
            mongocollection.Drop();
        }

        public static bool Exists(string connectionName, string database, string collection)
        {
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
            return mongocollection.Exists();
        }

        #endregion

        #region 一般增删改查
        public static bool Insert<T>(string connectionName, string database, string collection, T entity)
        {
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);

            mongocollection.Insert(entity);

            return true;
        }

        public static bool InsertBatch<T>(string connectionName, string database, string collection, List<T> entities)
        {
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);

            mongocollection.InsertBatch<T>(entities);

            return true;
        }

        private static List<T> GetList<T>(string connectionName, string database, string collection, Expression<Func<T, bool>> query)
        {
            return new List<T>();
        }

        private static IMongoQuery BuildQuery(List<Tuple<MongoQueryCodition, string, object>> querys)
        {
            if (querys == null || querys.Count == 0)
            {
                return Query.Null;
            }

            IMongoQuery mongoquery = Query.Null;
            List<IMongoQuery> querylist = new List<IMongoQuery>();
            foreach (var item in querys)
            {
                var submongoquery = MongoSimpleQuery.DetectQuery(item.Item2, item.Item1, item.Item3);

                if (submongoquery != Query.Null)
                {
                    querylist.Add(submongoquery);
                }
            }

            return Query.And(querylist);
        }

        private static UpdateBuilder BuildUpdate(List<Tuple<MongoUpdateCodition, string, object>> updates)
        {
            MD.Builders.UpdateBuilder updateBuilder = null;
            foreach (var item in updates)
            {
                switch (item.Item1)
                {
                    case MongoUpdateCodition.Set:
                        {
                            var bsonval = MB.BsonValue.Create(item.Item3);
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.Set(item.Item2, bsonval) : updateBuilder.Set(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.AddToSet:
                        {
                            var bsonval = MB.BsonInt32.Create(item.Item3).AsInt64;
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.AddToSet(item.Item2, bsonval) : updateBuilder.AddToSet(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.AddToSetEach:
                        {
                            var bsonval = MB.BsonArray.Create(item.Item3);
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.AddToSetEach(item.Item2, bsonval) : updateBuilder.AddToSetEach(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.BitwiseAnd:
                        {
                            var bsonval = MB.BsonInt64.Create(item.Item3).AsInt64;
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.BitwiseAnd(item.Item2, bsonval) : updateBuilder.BitwiseAnd(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.BitwiseOr:
                        {
                            var bsonval = MB.BsonInt64.Create(item.Item3).AsInt64;
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.BitwiseOr(item.Item2, bsonval) : updateBuilder.BitwiseOr(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.BitwiseXor:
                        {
                            var bsonval = MB.BsonInt64.Create(item.Item3).AsInt64;
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.BitwiseXor(item.Item2, bsonval) : updateBuilder.BitwiseXor(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.Max:
                        {
                            var bsonval = MB.BsonValue.Create(item.Item3);
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.Max(item.Item2, bsonval) : updateBuilder.Max(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.Min:
                        {
                            var bsonval = MB.BsonValue.Create(item.Item3);
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.Min(item.Item2, bsonval) : updateBuilder.Min(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.Mul:
                        {
                            var bsonval = MB.BsonInt32.Create(item.Item3).AsInt64;
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.Mul(item.Item2, bsonval) : updateBuilder.Mul(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.SetOnInsert:
                        {
                            var bsonval = MB.BsonValue.Create(item.Item3);
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.SetOnInsert(item.Item2, bsonval) : updateBuilder.SetOnInsert(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.PopFirst:
                        {
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.PopFirst(item.Item2) : updateBuilder.PopFirst(item.Item2);
                            break;
                        }
                    case MongoUpdateCodition.PopLast:
                        {
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.PopLast(item.Item2) : updateBuilder.PopLast(item.Item2);
                            break;
                        }
                    case MongoUpdateCodition.UnSet:
                        {
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.Unset(item.Item2) : updateBuilder.Unset(item.Item2);
                            break;
                        }
                    case MongoUpdateCodition.Incr:
                        {
                            var bsonval = MB.BsonInt32.Create(item.Item3).AsInt64;
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.Inc(item.Item2, bsonval) : updateBuilder.Inc(item.Item2, bsonval);
                            break;
                        }
                    case MongoUpdateCodition.Push:
                        {
                            var bsonval = MB.BsonValue.Create(item.Item3);
                            updateBuilder = updateBuilder == null ? MD.Builders.Update.Push(item.Item2, bsonval) : updateBuilder.Push(item.Item2, bsonval);
                            break;
                        }
                }

            }

            return updateBuilder;
        }

        private static IMongoSortBy BuildSort(List<Tuple<string, MongoSort>> sorts)
        {
            if (sorts == null || sorts.Count == 0)
            {
                return SortBy.Null;
            }

            SortByBuilder sortby = new SortByBuilder();
            foreach (var sort in sorts)
            {
                switch (sort.Item2)
                {
                    case MongoSort.ASC:
                        {
                            sortby.Ascending(sort.Item1);
                            break;
                        }
                    case MongoSort.DESC:
                        {
                            sortby.Descending(sort.Item1);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return sortby;
        }

        public static List<T> Find<T>(string connectionName, string database, string collection, List<Tuple<MongoQueryCodition, string, object>> querys, int pageindex, int pagesize, List<Tuple<string, MongoSort>> sorts, out long total)
        {
            var mongoquery = BuildQuery(querys);
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
            var mongosortby = BuildSort(sorts);
            int skip = (pageindex - 1) * pagesize;
            List<T> list = null;
            if (mongosortby != SortBy.Null)
            {
                list = mongocollection.FindAs<T>(mongoquery).SetSkip(skip).SetLimit(pagesize).SetSortOrder(BuildSort(sorts)).ToList();
            }
            else
            {
                list = mongocollection.FindAs<T>(mongoquery).SetSkip(skip).SetLimit(pagesize).ToList();
            }
            if (list.Count < pagesize && list.Count > 0)
            {
                total = skip + list.Count;
            }
            else
            {
                total = mongocollection.Count(mongoquery);
            }
            return list;
        }

        public static T FindAndModify<T>(string connectionName, string database, string collection, List<Tuple<MongoQueryCodition, string, object>> querys, List<Tuple<MongoUpdateCodition, string, object>> updates, List<Tuple<string, MongoSort>> sorts, bool returnNew, bool upsert)
        {
            var mongoquery = BuildQuery(querys);
            var mongosort = BuildSort(sorts);
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
            var retresult = mongocollection.FindAndModify(mongoquery, mongosort, BuildUpdate(updates), returnNew, upsert);

            return retresult.GetModifiedDocumentAs<T>();
        }

        public static T FindAndRemove<T>(string connectionName, string database, string collection, List<Tuple<MongoQueryCodition, string, object>> querys, List<Tuple<string, MongoSort>> sorts)
        {
            var mongoquery = BuildQuery(querys);
            var mongosort = BuildSort(sorts);
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
            var retresult = mongocollection.FindAndRemove(new FindAndRemoveArgs { Query = mongoquery, SortBy = mongosort });

            return retresult.GetModifiedDocumentAs<T>();
        }

        public static long Count(string connectionName, string database, string collection, List<Tuple<MongoQueryCodition, string, object>> querys)
        {
            var mongoquery = BuildQuery(querys);
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);

            var count = mongocollection.Count(mongoquery);

            return count;
        }

        public static List<T> Distinct<T>(string connectionName, string database, string collection, string key, List<Tuple<MongoQueryCodition, string, object>> querys)
        {
            var mongoquery = BuildQuery(querys);
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);

            return mongocollection.Distinct<T>(key, mongoquery).ToList();
        }

        public static List<T> FindAll<T>(string connectionName, string database, string collection, List<Tuple<string, MongoSort>> sorts)
        {
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);

            var list = mongocollection.FindAllAs<T>().SetSortOrder(BuildSort(sorts)).ToList();

            return list;
        }

        public static T FindOne<T>(string connectionName, string database, string collection, List<Tuple<MongoQueryCodition, string, object>> querys)
        {
            var mongoquery = BuildQuery(querys);
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);

            var one = mongocollection.FindOneAs<T>(mongoquery);
            return one;
        }

        public static bool Update<T>(string connectionName, string database, string collection, List<Tuple<MongoQueryCodition, string, object>> querys, List<Tuple<MongoUpdateCodition, string, object>> updates)
        {
            IMongoQuery mongoquery = BuildQuery(querys);

            MD.Builders.UpdateBuilder updateBuilder = BuildUpdate(updates);

            if (updateBuilder != null)
            {
                var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);

                mongocollection.Update(mongoquery, updateBuilder);
                return true;
            }

            return false;
        }

        public static bool Delete(string connectionName, string database, string collection, List<Tuple<MongoQueryCodition, string, object>> querys)
        {
            IMongoQuery mongoquery = BuildQuery(querys);

            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
            mongocollection.Remove(mongoquery);
            return true;
        }
        #endregion

        #region 复杂的
        public static List<T> Find2<T>(string connectionName, string database, string collection, MongoComplexQuery querys, int pageindex, int pagesize, List<Tuple<string, MongoSort>> sorts, out long total)
        {
            var mongoquery = Query.Null;
            if (querys != null)
            {
                mongoquery = querys.BuildQuery(mongoquery);
            }
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
            var mongosortby = BuildSort(sorts);
            int skip = (pageindex - 1) * pagesize;
            List<T> list = null;
            if (mongosortby != SortBy.Null)
            {
                list = mongocollection.FindAs<T>(mongoquery).SetSkip(skip).SetLimit(pagesize).SetSortOrder(BuildSort(sorts)).ToList();
            }
            else
            {
                list = mongocollection.FindAs<T>(mongoquery).SetSkip(skip).SetLimit(pagesize).ToList();
            }
            if (list.Count < pagesize && list.Count > 0)
            {
                total = skip + list.Count;
            }
            else
            {
                total = mongocollection.Count(mongoquery);
            }
            return list;
        }

        #endregion

        #region 索引
        public static void EnsureIndex(string connectionName, string database, string collection, params string[] keynames)
        {
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
            mongocollection.CreateIndex(keynames);
        }

        public static void EnsureIndex(string connectionName, string database, string collection, Dictionary<string, MongoSort> sortkeynames, bool unique = false)
        {
            IndexKeysBuilder indexkeys = new IndexKeysBuilder();
            foreach (var kv in sortkeynames)
            {
                if (kv.Value == MongoSort.DESC)
                {
                    indexkeys.Descending(kv.Key);
                }
                else
                {
                    indexkeys.Ascending(kv.Key);
                }
            }

            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
            if (unique)
            {
                mongocollection.CreateIndex(indexkeys, IndexOptions.SetUnique(true));
            }
            else
            {
                mongocollection.CreateIndex(indexkeys);
            }
        }

        public static void DropIndex(string connectionName, string database, string collection, params string[] keynames)
        {
            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);

            mongocollection.DropIndex(keynames);

        }

        public static void DropIndex(string connectionName, string database, string collection, Dictionary<string, MongoSort> sortkeynames)
        {
            IndexKeysBuilder indexkeys = new IndexKeysBuilder();
            foreach (var kv in sortkeynames)
            {
                if (kv.Value == MongoSort.DESC)
                {
                    indexkeys.Descending(kv.Key);
                }
                else
                {
                    indexkeys.Ascending(kv.Key);
                }
            }

            var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);

            mongocollection.DropIndex(indexkeys);

        }
        #endregion


        public static void Test()
        {

        }
    }
}
