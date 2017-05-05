using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoUpdateBuilderEx
    {
        internal UpdateBuilder MongoUpdateBuilder = new UpdateBuilder();

        private MongoUpdateBuilderEx()
        {

        }

        public static MongoUpdateBuilderEx NewBuilder()
        {
            return new MongoUpdateBuilderEx();
        }

        public MongoUpdateBuilderEx Set(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Set(name, bsonval);

            return this;
        }

        public MongoUpdateBuilderEx AddToSet(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.AddToSet(name, bsonval);

            return this;
        }

        public MongoUpdateBuilderEx AddToSetEach(string name, object[] val)
        {
            var bsonval = BsonArray.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.AddToSetEach(name, bsonval);

            return this;
        }

        public MongoUpdateBuilderEx BitwiseAnd(string name, long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.BitwiseAnd(name, val);

            return this;
        }

        public MongoUpdateBuilderEx BitwiseOr(string name, long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.BitwiseOr(name, val);

            return this;
        }

        public MongoUpdateBuilderEx BitwiseXor(string name, long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.BitwiseXor(name, val);
            return this;
        }

        public MongoUpdateBuilderEx Max(string name, object val)
        {
            var bsonval=BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Max(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx Min(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Min(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx Mul(string name, double val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Mul(name, val);
            return this;
        }

        public MongoUpdateBuilderEx Mul(string name, long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Mul(name, val);
            return this;
        }

        public MongoUpdateBuilderEx SetOnInsert(string name, object val)
        {
            var bsonval=BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.SetOnInsert(name, bsonval);

            return this;
        }

        public MongoUpdateBuilderEx PopFirst(string name)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.PopFirst(name);
            return this;
        }

        public MongoUpdateBuilderEx PopLast(string name)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.PopLast(name);

            return this;
        }

        public MongoUpdateBuilderEx UnSet(string name)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Unset(name);

            return this;
        }

        public MongoUpdateBuilderEx Incr(string name,long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Inc(name, val);

            return this;
        }

        public MongoUpdateBuilderEx Incr(string name, double val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Inc(name, val);

            return this;
        }

        public MongoUpdateBuilderEx Pull(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Pull(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx PullAll(string name,object[] val)
        {
            var bsonval=BsonArray.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.PullAll(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx Push(string name,object val)
        {
            var bsonval=BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Push(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx PushAll(string name, object[] val)
        {
            var bsonval=BsonArray.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.PushAll(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx PushEach(string name, object[] val)
        {
            var bsonval=BsonArray.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.PushEach(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx Rename(string oldname,string newname)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Rename(oldname, newname);
            return this;
        }

        public MongoUpdateBuilderEx CurrentDate(string name)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.CurrentDate(name);
            return this;
        }

        public MongoUpdateBuilderEx Combine(MongoUpdateBuilderEx other)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Combine(other.MongoUpdateBuilder);
            return this;
        }
    }
}
