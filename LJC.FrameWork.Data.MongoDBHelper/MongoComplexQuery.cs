using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoComplexQuery : MongoSimpleQuery
    {
        private Queue<MongoSimpleQuery> _queue = new Queue<MongoSimpleQuery>();


        protected MongoComplexQuery()
        {

        }

        public static MongoComplexQuery NewQuery()
        {
            return new MongoComplexQuery();
        }

        public MongoComplexQuery EQ(string key, object val)
        {
            _queue.Enqueue(new MongoSimpleQuery(key, MongoQueryCodition.EQ, val));
            return this;
        }

        public MongoComplexQuery NE(string key, object val)
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

        public override IMongoQuery BuildQuery(IMongoQuery querybuilder)
        {
            if (this._queue.Count == 0)
            {
                return querybuilder;
            }

            IMongoQuery newquery = Query.Null;

            MongoSimpleQuery simplequery = null;
            while (_queue.Count > 0)
            {
                simplequery = _queue.Dequeue();
                if (querybuilder == Query.Null)
                {
                    querybuilder = simplequery.BuildQuery(newquery);
                }
                else
                {
                    if (simplequery._uniontype == 0)
                    {
                        querybuilder = Query.Or(querybuilder, simplequery.BuildQuery(newquery));
                    }
                    else
                    {
                        querybuilder = Query.And(querybuilder, simplequery.BuildQuery(newquery));
                    }
                }
            }

            return querybuilder;

        }
    }
}
