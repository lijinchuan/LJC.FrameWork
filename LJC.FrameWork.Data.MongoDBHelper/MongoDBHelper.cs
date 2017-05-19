using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MD = MongoDB.Driver;
using MB = MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Linq.Expressions;

namespace LJC.FrameWork.Data.Mongo
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

        #region 表对象缓存
        private static Dictionary<string, MongoCollectionWarpper> CollectionDic = new Dictionary<string, MongoCollectionWarpper>();

        private static MongoCollectionWarpper GetCollecionInternal(string connectionName, string database, string collection)
        {
            string collectionkey = string.Format("{1}:{2}@{0}", connectionName, database, collection);

            MongoCollectionWarpper collectionWarpper = null;
            if (!CollectionDic.TryGetValue(collectionkey, out collectionWarpper))
            {
                lock (CollectionDic)
                {
                    if (!CollectionDic.TryGetValue(collectionkey, out collectionWarpper))
                    {
                        var mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
                        if (mongocollection != null)
                        {
                            collectionWarpper = new MongoCollectionWarpper(mongocollection);
                        }
                        CollectionDic.Add(collectionkey, collectionWarpper);
                    }
                }
            }

            return collectionWarpper;
        }

        private static MongoCollection GetCollecion(string connectionName, string database, string collection)
        {

            MongoCollectionWarpper collectionWarpper = GetCollecionInternal(connectionName, database, collection);


            return collectionWarpper == null ? null : collectionWarpper.MongoDBCollection;
        }

        private static MongoCollection GetCollecion<T>(string connectionName, string database, string collection)
        {

            MongoCollectionWarpper collectionWarpper = GetCollecionInternal(connectionName, database, collection);

            if (collectionWarpper != null && !collectionWarpper.IsCreateIndex && typeof(T) == typeof(MongoDocumentObject))
            {
                lock (collectionWarpper)
                {
                    if (!collectionWarpper.IsCreateIndex)
                    {
                        var instance = (MongoDocumentObject)System.Activator.CreateInstance(typeof(T));
                        bool unique = false, background = false;
                        var indexkeys = instance.CreateIndex(ref unique, ref background);

                        if (indexkeys != null && indexkeys.Length > 0 && !collectionWarpper.MongoDBCollection.IndexExists(indexkeys))
                        {
                            if (unique)
                            {
                                collectionWarpper.MongoDBCollection.CreateIndex(new MongoIndexKeysWarpper(indexkeys).MongoIndexKeys, IndexOptions.SetUnique(true).SetBackground(background));
                            }
                            else
                            {
                                collectionWarpper.MongoDBCollection.CreateIndex(new MongoIndexKeysWarpper(indexkeys).MongoIndexKeys);
                            }
                        }

                        collectionWarpper.IsCreateIndex = true;
                    }
                }
            }

            return collectionWarpper == null ? null : collectionWarpper.MongoDBCollection;
        }

        #endregion

        #region
        public static void Drop(string connectionName, string database, string collection)
        {
            var mongocollection = GetCollecion(connectionName,database,collection);
            mongocollection.Drop();
        }

        public static bool Exists(string connectionName, string database, string collection)
        {
            var mongocollection = GetCollecion(connectionName, database, collection);
            return mongocollection.Exists();
        }

        #endregion

        #region 增删改查
        public static bool Insert<T>(string connectionName, string database, string collection, T entity)
        {
            var mongocollection = GetCollecion<T>(connectionName, database, collection);

            mongocollection.Insert(entity);

            return true;
        }

        public static bool InsertBatch<T>(string connectionName, string database, string collection, List<T> entities)
        {
            var mongocollection = GetCollecion<T>(connectionName, database, collection);

            mongocollection.InsertBatch<T>(entities);

            return true;
        }

        public static List<T> Find<T>(string connectionName, string database, string collection, MongoQueryWarpper querys, int pageindex, int pagesize, MongoSortWarpper sorts, out long total)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongocollection = GetCollecion<T>(connectionName, database, collection);
            var mongosortby = (sorts == null || sorts.MongoSortBy == SortBy.Null) ? SortBy.Null : sorts.MongoSortBy;
            int skip = (pageindex - 1) * pagesize;
            List<T> list = null;
            if (mongosortby != SortBy.Null)
            {
                list = mongocollection.FindAs<T>(mongoquery).SetSkip(skip).SetLimit(pagesize).SetSortOrder(mongosortby).ToList();
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

        public static List<T> Find<T>(string connectionName, string database, string collection, MongoQueryWarpper<T> querys, int pageindex, int pagesize, MongoSortWarpper<T> sorts, out long total) where T : new()
        {
            return Find<T>(connectionName, database, collection, (MongoQueryWarpper)querys, pageindex, pagesize, (MongoSortWarpper)sorts, out total);
        }

        public static T FindOneByIdAs<T>(string connectionName, string database, string collection, string id)
        {
            var mongocollection = GetCollecion<T>(connectionName, database, collection);
            return mongocollection.FindOneByIdAs<T>(id);
        }

        public static T FindAndModify<T>(string connectionName, string database, string collection, MongoQueryWarpper querys, MongoUpdateWarpper updates, MongoSortWarpper sorts, bool returnNew, bool upsert)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongosort = (sorts == null || sorts.MongoSortBy == SortBy.Null) ? SortBy.Null : sorts.MongoSortBy;
            var mongocollection = GetCollecion<T>(connectionName, database, collection);
            var retresult = mongocollection.FindAndModify(mongoquery, mongosort, updates.MongoUpdateBuilder, returnNew, upsert);

            return retresult.GetModifiedDocumentAs<T>();
        }

        public static T FindAndModify<T>(string connectionName, string database, string collection, MongoQueryWarpper<T> querys, MongoUpdateWarpper<T> updates, MongoSortWarpper<T> sorts, bool returnNew, bool upsert) where T : new()
        {
            return FindAndModify<T>(connectionName, database, collection, (MongoQueryWarpper)querys, (MongoUpdateWarpper)updates, (MongoSortWarpper)sorts, returnNew, upsert);
        }

        public static T FindAndRemove<T>(string connectionName, string database, string collection, MongoQueryWarpper querys, MongoSortWarpper sorts)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongosort = (sorts == null || sorts.MongoSortBy == SortBy.Null) ? SortBy.Null : sorts.MongoSortBy;
            var mongocollection = GetCollecion<T>(connectionName, database, collection);
            var retresult = mongocollection.FindAndRemove(new FindAndRemoveArgs { Query = mongoquery, SortBy = mongosort });

            return retresult.GetModifiedDocumentAs<T>();
        }

        public static T FindAndRemove<T>(string connectionName, string database, string collection, MongoQueryWarpper<T> querys, MongoSortWarpper<T> sorts) where T : new()
        {
            return FindAndRemove<T>(connectionName, database, collection, (MongoQueryWarpper)querys, (MongoSortWarpper)sorts);
        }

        public static long Count(string connectionName, string database, string collection, MongoQueryWarpper querys)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongocollection = GetCollecion(connectionName, database, collection);

            var count = mongocollection.Count(mongoquery);

            return count;
        }

        public static long Count<T>(string connectionName, string database, string collection, MongoQueryWarpper<T> querys) where T:new()
        {
            return Count(collection, database, collection, querys);
        }

        public static List<T> Distinct<T>(string connectionName, string database, string collection, string key, MongoQueryWarpper querys)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongocollection = GetCollecion<T>(connectionName, database, collection);

            return mongocollection.Distinct<T>(key, mongoquery).ToList();
        }

        public static List<T> Distinct<T>(string connectionName, string database, string collection, string key, MongoQueryWarpper<T> querys) where T : new()
        {
            return Distinct<T>(connectionName, database, collection, key, (MongoQueryWarpper)querys);
        }

        public static List<T> FindAll<T>(string connectionName, string database, string collection, MongoSortWarpper sorts)
        {
            var mongocollection = GetCollecion<T>(connectionName, database, collection);

            if (sorts == null || sorts.MongoSortBy == SortBy.Null)
            {
                var list = mongocollection.FindAllAs<T>().ToList();

                return list;
            }
            else
            {
                var list = mongocollection.FindAllAs<T>().SetSortOrder(sorts.MongoSortBy).ToList();

                return list;
            }
        }

        public static List<T> FindAll<T>(string connectionName, string database, string collection, MongoSortWarpper<T> sorts) where T : new()
        {
            return FindAll<T>(connectionName, database, collection, (MongoSortWarpper)sorts);
        }

        public static T FindOne<T>(string connectionName, string database, string collection, MongoQueryWarpper querys)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongocollection = GetCollecion<T>(connectionName, database, collection);

            var one = mongocollection.FindOneAs<T>(mongoquery);
            return one;
        }

        public static T FindOne<T>(string connectionName, string database, string collection, MongoQueryWarpper<T> querys) where T : new()
        {
            return FindOne<T>(connectionName, database, collection, (MongoQueryWarpper)querys);
        }

        public static bool Update<T>(string connectionName, string database, string collection, MongoQueryWarpper querys, MongoUpdateWarpper updates)
        {
            if (updates == null || updates.IsEmpty)
            {
                return false;
            }

            IMongoQuery mongoquery = querys == null ? Query.Null : querys.MongoQuery;

            MD.Builders.UpdateBuilder updateBuilder = updates.MongoUpdateBuilder;
            if (updateBuilder != null)
            {
                var mongocollection = GetCollecion<T>(connectionName, database, collection);

                mongocollection.Update(mongoquery, updateBuilder);
                return true;
            }

            return false;
        }

        public static bool Update<T>(string connectionName, string database, string collection, MongoQueryWarpper<T> querys, MongoUpdateWarpper<T> updates) where T:new()
        {
            return Update<T>(connectionName, database, collection, (MongoQueryWarpper)querys, (MongoUpdateWarpper)updates);
        }

        public static bool Remove(string connectionName, string database, string collection, MongoQueryWarpper querys)
        {
            if (querys == null)
            {
                return false;
            }
            IMongoQuery mongoquery = querys.MongoQuery;

            var mongocollection = GetCollecion(connectionName, database, collection);
            mongocollection.Remove(mongoquery);
            return true;
        }

        public static bool Remove<T>(string connectionName, string database, string collection, MongoQueryWarpper<T> querys) where T : new()
        {
            return Remove(connectionName, database, collection, (MongoQueryWarpper)querys);
        }

        public static bool RemoveAll(string connectionName, string database, string collection)
        {
            var mongocollection = GetCollecion(connectionName, database, collection);
            mongocollection.RemoveAll();
            return true;
        }
        #endregion

        #region 索引
        public static void EnsureIndex(string connectionName, string database, string collection, params string[] keynames)
        {
            var mongocollection = GetCollecion(connectionName, database, collection);
            mongocollection.CreateIndex(keynames);
        }

        public static void EnsureIndex<T>(string connectionName, string database, string collection, params Expression<Func<T>>[] keynames) where T : new()
        {
            EnsureIndex(connectionName, database, collection, keynames.Select(p => MongoDBUtil.GetMongoElementField(p.Body)).ToArray());
        }

        public static void EnsureIndex(string connectionName, string database, string collection, MongoIndexKeysWarpper indexkeys, bool unique = false)
        {
            var mongocollection = GetCollecion(connectionName, database, collection);
            if (unique)
            {
                mongocollection.CreateIndex(indexkeys.MongoIndexKeys, IndexOptions.SetUnique(true));
            }
            else
            {
                mongocollection.CreateIndex(indexkeys.MongoIndexKeys);
            }
        }

        public static void EnsureIndex<T>(string connectionName, string database, string collection, MongoIndexKeysWarpper<T> indexkeys, bool unique = false) where T:new()
        {
            EnsureIndex(connectionName, database, collection, indexkeys, unique);
        }

        public static void DropIndex(string connectionName, string database, string collection, params string[] keynames)
        {
            var mongocollection = GetCollecion(connectionName, database, collection);

            mongocollection.DropIndex(keynames);

        }

        public static void DropIndex<T>(string connectionName, string database, string collection, params Expression<Func<T>>[] keynames) where T:new()
        {
            DropIndex(connectionName,database,collection,keynames.Select(p => MongoDBUtil.GetMongoElementField(p.Body)).ToArray());

        }

        public static void DropIndex(string connectionName, string database, string collection, MongoIndexKeysWarpper indexkeys)
        {
            var mongocollection = GetCollecion(connectionName, database, collection);

            mongocollection.DropIndex(indexkeys.MongoIndexKeys);

        }

        public static void DropIndex<T>(string connectionName, string database, string collection, MongoIndexKeysWarpper<T> indexkeys) where T:new()
        {
            DropIndex(connectionName, database, collection, indexkeys);

        }
        #endregion
    }
}
