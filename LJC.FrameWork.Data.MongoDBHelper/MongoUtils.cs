using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MD = MongoDB.Driver;
using MB = MongoDB.Bson;
using MongoDB.Driver;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    internal class MongoUtils
    {
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

        internal static IMongoQuery BuildQuery(List<Tuple<MongoQueryCodition, string, object>> querys)
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
    }
}
