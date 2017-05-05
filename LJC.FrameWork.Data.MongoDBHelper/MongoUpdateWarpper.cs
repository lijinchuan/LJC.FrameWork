using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoUpdateWarpper
    {
        internal UpdateBuilder MongoUpdateBuilder = new UpdateBuilder();

        public MongoUpdateWarpper()
        {

        }

        public MongoUpdateWarpper Set(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Set(name, bsonval);

            return this;
        }

        public MongoUpdateWarpper AddToSet(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.AddToSet(name, bsonval);

            return this;
        }

        public MongoUpdateWarpper AddToSetEach(string name, object[] val)
        {
            var bsonval = BsonArray.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.AddToSetEach(name, bsonval);

            return this;
        }

        public MongoUpdateWarpper BitwiseAnd(string name, long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.BitwiseAnd(name, val);

            return this;
        }

        public MongoUpdateWarpper BitwiseOr(string name, long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.BitwiseOr(name, val);

            return this;
        }

        public MongoUpdateWarpper BitwiseXor(string name, long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.BitwiseXor(name, val);
            return this;
        }

        public MongoUpdateWarpper Max(string name, object val)
        {
            var bsonval=BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Max(name, bsonval);
            return this;
        }

        public MongoUpdateWarpper Min(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Min(name, bsonval);
            return this;
        }

        public MongoUpdateWarpper Mul(string name, double val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Mul(name, val);
            return this;
        }

        public MongoUpdateWarpper Mul(string name, long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Mul(name, val);
            return this;
        }

        public MongoUpdateWarpper SetOnInsert(string name, object val)
        {
            var bsonval=BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.SetOnInsert(name, bsonval);

            return this;
        }

        public MongoUpdateWarpper PopFirst(string name)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.PopFirst(name);
            return this;
        }

        public MongoUpdateWarpper PopLast(string name)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.PopLast(name);

            return this;
        }

        public MongoUpdateWarpper UnSet(string name)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Unset(name);

            return this;
        }

        public MongoUpdateWarpper Incr(string name,long val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Inc(name, val);

            return this;
        }

        public MongoUpdateWarpper Incr(string name, double val)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Inc(name, val);

            return this;
        }

        public MongoUpdateWarpper Pull(string name, object val)
        {
            var bsonval = BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Pull(name, bsonval);
            return this;
        }

        public MongoUpdateWarpper PullAll(string name,object[] val)
        {
            var bsonval=BsonArray.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.PullAll(name, bsonval);
            return this;
        }

        public MongoUpdateWarpper Push(string name,object val)
        {
            var bsonval=BsonValue.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.Push(name, bsonval);
            return this;
        }

        public MongoUpdateWarpper PushAll(string name, object[] val)
        {
            var bsonval=BsonArray.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.PushAll(name, bsonval);
            return this;
        }

        public MongoUpdateWarpper PushEach(string name, object[] val)
        {
            var bsonval=BsonArray.Create(val);
            MongoUpdateBuilder = MongoUpdateBuilder.PushEach(name, bsonval);
            return this;
        }

        public MongoUpdateWarpper Rename(string oldname,string newname)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Rename(oldname, newname);
            return this;
        }

        public MongoUpdateWarpper CurrentDate(string name)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.CurrentDate(name);
            return this;
        }

        public MongoUpdateWarpper Combine(MongoUpdateWarpper other)
        {
            MongoUpdateBuilder = MongoUpdateBuilder.Combine(other.MongoUpdateBuilder);
            return this;
        }
    }
}
