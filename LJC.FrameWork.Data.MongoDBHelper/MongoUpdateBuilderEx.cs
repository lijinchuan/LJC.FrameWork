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
        internal UpdateBuilder _mongoUpdateBuilder = new UpdateBuilder();

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
            _mongoUpdateBuilder = _mongoUpdateBuilder.Set(name, bsonval);

            return this;
        }

        public MongoUpdateBuilderEx AddToSet(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.AddToSet(name, bsonval);

            return this;
        }

        public MongoUpdateBuilderEx AddToSetEach(string name, object[] val)
        {
            var bsonval = BsonArray.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.AddToSetEach(name, bsonval);

            return this;
        }

        public MongoUpdateBuilderEx BitwiseAnd(string name, long val)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.BitwiseAnd(name, val);

            return this;
        }

        public MongoUpdateBuilderEx BitwiseOr(string name, long val)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.BitwiseOr(name, val);

            return this;
        }

        public MongoUpdateBuilderEx BitwiseXor(string name, long val)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.BitwiseXor(name, val);
            return this;
        }

        public MongoUpdateBuilderEx Max(string name, object val)
        {
            var bsonval=BsonValue.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.Max(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx Min(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.Min(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx Mul(string name, double val)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.Mul(name, val);
            return this;
        }

        public MongoUpdateBuilderEx Mul(string name, long val)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.Mul(name, val);
            return this;
        }

        public MongoUpdateBuilderEx SetOnInsert(string name, object val)
        {
            var bsonval=BsonValue.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.SetOnInsert(name, bsonval);

            return this;
        }

        public MongoUpdateBuilderEx PopFirst(string name)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.PopFirst(name);
            return this;
        }

        public MongoUpdateBuilderEx PopLast(string name)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.PopLast(name);

            return this;
        }

        public MongoUpdateBuilderEx UnSet(string name)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.Unset(name);

            return this;
        }

        public MongoUpdateBuilderEx Incr(string name,long val)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.Inc(name, val);

            return this;
        }

        public MongoUpdateBuilderEx Incr(string name, double val)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.Inc(name, val);

            return this;
        }

        public MongoUpdateBuilderEx Pull(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.Pull(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx PullAll(string name,object[] val)
        {
            var bsonval=BsonArray.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.PullAll(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx Push(string name,object val)
        {
            var bsonval=BsonValue.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.Push(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx PushAll(string name, object[] val)
        {
            var bsonval=BsonArray.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.PushAll(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx PushEach(string name, object[] val)
        {
            var bsonval=BsonArray.Create(val);
            _mongoUpdateBuilder = _mongoUpdateBuilder.PushEach(name, bsonval);
            return this;
        }

        public MongoUpdateBuilderEx Rename(string oldname,string newname)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.Rename(oldname, newname);
            return this;
        }

        public MongoUpdateBuilderEx CurrentDate(string name)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.CurrentDate(name);
            return this;
        }

        public MongoUpdateBuilderEx Combine(MongoUpdateBuilderEx other)
        {
            _mongoUpdateBuilder = _mongoUpdateBuilder.Combine(other._mongoUpdateBuilder);
            return this;
        }
    }
}
