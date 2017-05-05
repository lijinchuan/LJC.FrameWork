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

        #region 表对象缓存
        private static Dictionary<string, MongoCollection> CollectionDic = new Dictionary<string, MongoCollection>();

        private static MongoCollection GetCollecion(string connectionName, string database, string collection)
        {
            string collectionkey = string.Format("{1}:{2}@{0}", connectionName, database, collection);
            MongoCollection mongocollection = null;

            if (!CollectionDic.TryGetValue(collectionkey, out mongocollection))
            {
                lock (CollectionDic)
                {
                    if (!CollectionDic.TryGetValue(collectionkey, out mongocollection))
                    {
                        mongocollection = CreateInstanceUseConfig(connectionName).GetDatabase(database).GetCollection(collection);
                        CollectionDic.Add(collectionkey, mongocollection);
                    }
                }
            }

            return mongocollection;
        }

        private static MongoCollection<T> GetCollecion<T>(string connectionName, string database, string collection)
        {
            MongoCollection mongocollection = GetCollecion(connectionName, database, collection);
            return (MongoCollection<T>)mongocollection;
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

        private static List<T> GetList<T>(string connectionName, string database, string collection, Expression<Func<T, bool>> query)
        {
            return new List<T>();
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

        public static T FindAndModify<T>(string connectionName, string database, string collection, MongoQueryWarpper querys, MongoUpdateWarpper updates, MongoSortWarpper sorts, bool returnNew, bool upsert)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongosort = (sorts == null || sorts.MongoSortBy == SortBy.Null) ? SortBy.Null : sorts.MongoSortBy;
            var mongocollection = GetCollecion<T>(connectionName, database, collection);
            var retresult = mongocollection.FindAndModify(mongoquery, mongosort, updates.MongoUpdateBuilder, returnNew, upsert);

            return retresult.GetModifiedDocumentAs<T>();
        }

        public static T FindAndRemove<T>(string connectionName, string database, string collection, MongoQueryWarpper querys, MongoSortWarpper sorts)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongosort = (sorts == null || sorts.MongoSortBy == SortBy.Null) ? SortBy.Null : sorts.MongoSortBy;
            var mongocollection = GetCollecion<T>(connectionName, database, collection);
            var retresult = mongocollection.FindAndRemove(new FindAndRemoveArgs { Query = mongoquery, SortBy = mongosort });

            return retresult.GetModifiedDocumentAs<T>();
        }

        public static long Count(string connectionName, string database, string collection, MongoQueryWarpper querys)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongocollection = GetCollecion(connectionName, database, collection);

            var count = mongocollection.Count(mongoquery);

            return count;
        }

        public static List<T> Distinct<T>(string connectionName, string database, string collection, string key, MongoQueryWarpper querys)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongocollection = GetCollecion<T>(connectionName, database, collection);

            return mongocollection.Distinct<T>(key, mongoquery).ToList();
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

        public static T FindOne<T>(string connectionName, string database, string collection, MongoQueryWarpper querys)
        {
            var mongoquery = querys == null ? Query.Null : querys.MongoQuery;
            var mongocollection = GetCollecion<T>(connectionName, database, collection);

            var one = mongocollection.FindOneAs<T>(mongoquery);
            return one;
        }

        public static bool Update<T>(string connectionName, string database, string collection, MongoQueryWarpper querys, MongoUpdateWarpper updates)
        {
            if (updates == null)
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

        public static bool Delete(string connectionName, string database, string collection, MongoQueryWarpper querys)
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
        #endregion

        #region 索引
        public static void EnsureIndex(string connectionName, string database, string collection, params string[] keynames)
        {
            var mongocollection = GetCollecion(connectionName, database, collection);
            mongocollection.CreateIndex(keynames);
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

        public static void DropIndex(string connectionName, string database, string collection, params string[] keynames)
        {
            var mongocollection = GetCollecion(connectionName, database, collection);

            mongocollection.DropIndex(keynames);

        }

        public static void DropIndex(string connectionName, string database, string collection, MongoIndexKeysWarpper indexkeys)
        {
            var mongocollection = GetCollecion(connectionName, database, collection);

            mongocollection.DropIndex(indexkeys.MongoIndexKeys);

        }
        #endregion
    }
}
