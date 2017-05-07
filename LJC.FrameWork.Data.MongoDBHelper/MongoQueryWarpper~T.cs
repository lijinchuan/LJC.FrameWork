using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    public class MongoQueryWarpper<T>:MongoQueryWarpper where T:new()
    {
        public MongoQueryWarpper<T> EQ(Expression<Func<T,object>> name,object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);

            base.EQ(field, val);

            return this;
        }

        public MongoQueryWarpper<T> NE(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.NE(field, val);
            return this;
        }

        public MongoQueryWarpper<T> All(Expression<Func<T,object>> name, object[] val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.All(field, val);
            return this;
        }

        public MongoQueryWarpper<T> Exists(Expression<Func<T,object>> name, bool val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.Exists(field, val);
            return this;
        }

        public MongoQueryWarpper<T> GT(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.GT(field, val);
            return this;
        }

        public MongoQueryWarpper<T> GTE(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);

            base.GTE(field, val);
            return this;
        }

        public MongoQueryWarpper<T> In(Expression<Func<T,object>> name, object[] val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.In(field, val);
            return this;
        }

        public MongoQueryWarpper<T> LT(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.LT(field, val);
            return this;
        }

        public MongoQueryWarpper<T> LTE(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.LTE(field, val);
            return this;
        }

        public MongoQueryWarpper<T> NotIn(Expression<Func<T,object>> name, object[] val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.NotIn(field, val);
            return this;
        }

        public MongoQueryWarpper<T> Size(Expression<Func<T,object>> name, int val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.Size(field, val);
            return this;
        }

        public MongoQueryWarpper<T> SizeGreaterThan(Expression<Func<T,object>> name, int val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.SizeGreaterThan(field, val);
            return this;
        }

        public MongoQueryWarpper<T> SizeGreaterThanOrEqual(Expression<Func<T,object>> name, int val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.SizeGreaterThanOrEqual(field, val);
            return this;
        }

        public MongoQueryWarpper<T> SizeLessThan(Expression<Func<T,object>> name, int val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.SizeLessThan(field, val);
            return this;
        }

        public MongoQueryWarpper<T> SizeLessThanOrEqual(Expression<Func<T,object>> name, int val)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.SizeLessThanOrEqual(field, val);
            return this;
        }

        public MongoQueryWarpper<T> And(MongoQueryWarpper<T> query)
        {
            base.And(query);
            return this;
        }

        public MongoQueryWarpper<T> Or(MongoQueryWarpper<T> query)
        {
            base.Or(query);
            return this;
        }
    }
}
