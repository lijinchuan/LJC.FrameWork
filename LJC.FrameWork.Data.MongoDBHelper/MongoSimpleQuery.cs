using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MD = MongoDB.Driver;
using MB = MongoDB.Bson;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoSimpleQuery
    {
        /// <summary>
        /// 1-and 0-or
        /// </summary>
        internal int _uniontype = 1;

        public string Key
        {
            get;
            set;
        }

        public MongoQueryCodition Condition
        {
            get;
            set;
        }

        public object Val
        {
            get;
            set;
        }

        protected MongoSimpleQuery()
        {

        }

        public MongoSimpleQuery(string key, MongoQueryCodition conditon, object val)
        {
            this.Key = key;
            this.Condition = conditon;
            this.Val = val;
        }

        internal static IMongoQuery DetectQuery(string key, MongoQueryCodition condtion, object val)
        {
            var submongoquery = Query.Null;
            switch (condtion)
            {
                case MongoQueryCodition.EQ:
                    {
                        var bsonval = MB.BsonValue.Create(val);
                        submongoquery = Query.EQ(key, bsonval);
                        break;
                    }
                case MongoQueryCodition.NE:
                    {
                        var bsonval = MB.BsonValue.Create(val);
                        submongoquery = Query.NE(key, bsonval);
                        break;
                    }
                case MongoQueryCodition.In:
                    {
                        var bsonval = MB.BsonArray.Create(val);
                        submongoquery = Query.In(key, bsonval);
                        break;
                    }
                case MongoQueryCodition.NotIn:
                    {
                        var bsonval = MB.BsonArray.Create(val);
                        submongoquery = Query.NotIn(key, bsonval);
                        break;
                    }
                case MongoQueryCodition.GT:
                    {
                        var bsonval = MB.BsonValue.Create(val);
                        submongoquery = Query.GT(key, bsonval);
                        break;
                    }
                case MongoQueryCodition.GTE:
                    {
                        var bsonval = MB.BsonValue.Create(val);
                        submongoquery = Query.GTE(key, bsonval);
                        break;
                    }
                case MongoQueryCodition.LT:
                    {
                        var bsonval = MB.BsonValue.Create(val);
                        submongoquery = Query.LT(key, bsonval);
                        break;
                    }
                case MongoQueryCodition.LTE:
                    {
                        var bsonval = MB.BsonValue.Create(val);
                        submongoquery = Query.LTE(key, bsonval);
                        break;
                    }
                case MongoQueryCodition.All:
                    {
                        var bsonval = MB.BsonArray.Create(val);
                        submongoquery = Query.All(key, bsonval);
                        break;
                    }
                case MongoQueryCodition.Exists:
                    {
                        var bsonval = MB.BsonBoolean.Create(val);
                        if (bsonval.AsBoolean)
                        {
                            submongoquery = Query.Exists(key);
                        }
                        else
                        {
                            submongoquery = Query.NotExists(key);
                        }
                        break;
                    }
                case MongoQueryCodition.Size:
                    {
                        var bsonval = MB.BsonInt32.Create(val);
                        submongoquery = Query.Size(key, bsonval.AsInt32);
                        break;
                    }
                case MongoQueryCodition.SizeGreaterThan:
                    {
                        var bsonval = MB.BsonInt32.Create(val);
                        submongoquery = Query.SizeGreaterThan(key, bsonval.AsInt32);
                        break;
                    }
                case MongoQueryCodition.SizeGreaterThanOrEqual:
                    {
                        var bsonval = MB.BsonInt32.Create(val);
                        submongoquery = Query.SizeGreaterThanOrEqual(key, bsonval.AsInt32);
                        break;
                    }
                case MongoQueryCodition.SizeLessThan:
                    {
                        var bsonval = MB.BsonInt32.Create(val);
                        submongoquery = Query.SizeLessThan(key, bsonval.AsInt32);
                        break;
                    }
                case MongoQueryCodition.SizeLessThanOrEqual:
                    {
                        var bsonval = MB.BsonInt32.Create(val);
                        submongoquery = Query.SizeLessThanOrEqual(key, bsonval.AsInt32);
                        break;
                    }
            }

            return submongoquery;
        }

        public virtual IMongoQuery BuildQuery(IMongoQuery querybuilder)
        {
            if (!string.IsNullOrWhiteSpace(this.Key))
            {
                var newquery = DetectQuery(this.Key, this.Condition, this.Val);

                if (querybuilder == Query.Null)
                {
                    querybuilder = newquery;
                }
                else
                {
                    if (this._uniontype == 0)
                    {
                        querybuilder = Query.Or(querybuilder, newquery);
                    }
                    else
                    {
                        querybuilder = Query.And(querybuilder, newquery);
                    }
                }

            }

            return querybuilder;
        }
    }
}
