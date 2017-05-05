using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Linq.Expressions;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoQueryWarpper
    {
        internal IMongoQuery MongoQuery = Query.Null;

        public MongoQueryWarpper()
        {

        }

        public MongoQueryWarpper EQ(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.EQ(name, bsonval);
                return this;
            }
            
            MongoQuery = Query.And(MongoQuery, Query.EQ(name, bsonval));
            return this;
        }

        public MongoQueryWarpper NE(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.NE(name, bsonval);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.NE(name, bsonval));
            return this;
        }

        public MongoQueryWarpper All(string name, object[] val)
        {
            var bsonval=new BsonArray(val);
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.All(name, bsonval);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.All(name, bsonval));
            return this;
        }

        public MongoQueryWarpper Exists(string name, bool val)
        {
            if (MongoQuery == Query.Null)
            {
                MongoQuery = val ? Query.Exists(name) : Query.NotExists(name);
                return this;
            }

            if (val)
            {
                MongoQuery = Query.And(MongoQuery, Query.Exists(name));
            }
            else
            {
                MongoQuery = Query.And(MongoQuery, Query.NotExists(name));
            }
            return this;
        }

        public MongoQueryWarpper GT(string name, object val)
        {
            var bsonval=BsonValue.Create(val);
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.GT(name, bsonval);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.GT(name, bsonval));
            return this;
        }

        public MongoQueryWarpper GTE(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.GTE(name, bsonval);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.GTE(name, bsonval));
            return this;
        }

        public MongoQueryWarpper In(string name, object[] val)
        {
            var bsonval = new BsonArray(val);
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.In(name, bsonval);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.In(name, bsonval));
            return this;
        }

        public MongoQueryWarpper LT(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.LT(name, bsonval);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.LT(name, bsonval));
            return this;
        }

        public MongoQueryWarpper LTE(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.LTE(name, bsonval);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.LTE(name, bsonval));
            return this;
        }

        public MongoQueryWarpper NotIn(string name, object[] val)
        {
            var bsonval = new BsonArray(val);
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.NotIn(name, bsonval);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.NotIn(name, bsonval));
            return this;
        }

        public MongoQueryWarpper Size(string name, int val)
        {
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.Size(name, val);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.Size(name, val));
            return this;
        }

        public MongoQueryWarpper SizeGreaterThan(string name, int val)
        {
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.SizeGreaterThan(name, val);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.SizeGreaterThan(name, val));
            return this;
        }

        public MongoQueryWarpper SizeGreaterThanOrEqual(string name, int val)
        {
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.SizeGreaterThanOrEqual(name, val);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.SizeGreaterThanOrEqual(name, val));
            return this;
        }

        public MongoQueryWarpper SizeLessThan(string name, int val)
        {
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.SizeLessThan(name, val);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.SizeLessThan(name, val));
            return this;
        }

        public MongoQueryWarpper SizeLessThanOrEqual(string name, int val)
        {
            if (MongoQuery == Query.Null)
            {
                MongoQuery = Query.SizeLessThanOrEqual(name, val);
                return this;
            }
            MongoQuery = Query.And(MongoQuery, Query.SizeLessThanOrEqual(name, val));
            return this;
        }

        public MongoQueryWarpper And(MongoQueryWarpper query)
        {
            if (MongoQuery == Query.Null)
            {
                MongoQuery = query.MongoQuery;
                return this;
            }
            MongoQuery = Query.And(MongoQuery, query.MongoQuery);
            return this;
        }

        public MongoQueryWarpper Or(MongoQueryWarpper query)
        {
            if (MongoQuery == Query.Null)
            {
                MongoQuery = query.MongoQuery;
                return this;
            }
            MongoQuery = Query.Or(MongoQuery, query.MongoQuery);
            return this;
        }
    }
}
