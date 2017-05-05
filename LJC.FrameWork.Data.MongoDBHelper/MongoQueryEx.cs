using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoQueryEx
    {
        internal IMongoQuery MongoQuery = Query.Null;

        private MongoQueryEx()
        {

        }

        public static MongoQueryEx NewQuery()
        {
            return new MongoQueryEx();
        }

        public MongoQueryEx EQ(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            MongoQuery = Query.And(MongoQuery, Query.EQ(name, bsonval));
            return this;
        }

        public MongoQueryEx NE(string key, object val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.NE, val));
            return this;
        }

        public MongoComplexQuery All(string key, object val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.All, val));
            return this;
        }

        public MongoComplexQuery Exists(string key, bool val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.Exists, val));
            return this;
        }

        public MongoComplexQuery GT(string key, object val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.GT, val));
            return this;
        }

        public MongoComplexQuery GTE(string key, object val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.GTE, val));
            return this;
        }

        public MongoComplexQuery In(string key, object[] val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.In, val));
            return this;
        }

        public MongoComplexQuery LT(string key, object val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.LT, val));
            return this;
        }

        public MongoComplexQuery LTE(string key, object val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.LTE, val));
            return this;
        }

        public MongoComplexQuery NotIn(string key, object[] val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.NotIn, val));
            return this;
        }

        public MongoComplexQuery Size(string key, long val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.Size, val));
            return this;
        }

        public MongoComplexQuery SizeGreaterThan(string key, long val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.SizeGreaterThan, val));
            return this;
        }

        public MongoComplexQuery SizeGreaterThanOrEqual(string key, long val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.SizeGreaterThanOrEqual, val));
            return this;
        }

        public MongoComplexQuery SizeLessThan(string key, long val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.SizeLessThan, val));
            return this;
        }

        public MongoComplexQuery SizeLessThanOrEqual(string key, long val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.SizeLessThanOrEqual, val));
            return this;
        }

        public MongoComplexQuery And(MongoComplexQuery subComplexQuery)
        {
            subComplexQuery._uniontype = 1;
            _queue.Enqueue(subComplexQuery);
            return this;
        }

        public MongoComplexQuery Or(MongoComplexQuery subComplexQuery)
        {
            subComplexQuery._uniontype = 0;
            _queue.Enqueue(subComplexQuery);
            return this;
        }
    }
}
