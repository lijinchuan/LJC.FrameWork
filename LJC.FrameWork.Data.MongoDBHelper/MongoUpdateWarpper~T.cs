using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoUpdateWarpper<T>:MongoUpdateWarpper
    {
        public MongoUpdateWarpper<T> Set(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Set(field, val);

            return this;
        }

        public MongoUpdateWarpper<T> AddToSet(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.AddToSet(field, val);

            return this;
        }

        public MongoUpdateWarpper<T> AddToSetEach(Expression<Func<T,object>> name, object[] val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.AddToSetEach(field, val);

            return this;
        }

        public MongoUpdateWarpper<T> BitwiseAnd(Expression<Func<T,object>> name, long val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.BitwiseAnd(field, val);

            return this;
        }

        public MongoUpdateWarpper<T> BitwiseOr(Expression<Func<T,object>> name, long val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.BitwiseOr(field, val);

            return this;
        }

        public MongoUpdateWarpper<T> BitwiseXor(Expression<Func<T,object>> name, long val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.BitwiseXor(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> Max(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Max(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> Min(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Min(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> Mul(Expression<Func<T,object>> name, double val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Mul(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> Mul(Expression<Func<T,object>> name, long val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Mul(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> SetOnInsert(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.SetOnInsert(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> PopFirst(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.PopFirst(field);
            return this;
        }

        public MongoUpdateWarpper<T> PopLast(Expression<Func<T,object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.PopLast(field);

            return this;
        }

        public MongoUpdateWarpper<T> UnSet(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.UnSet(field);

            return this;
        }

        public MongoUpdateWarpper<T> Incr(Expression<Func<T,object>> name, long val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Incr(field, val);

            return this;
        }

        public MongoUpdateWarpper<T> Incr(Expression<Func<T,object>> name, double val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Incr(field, val);

            return this;
        }

        public MongoUpdateWarpper<T> Pull(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Pull(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> PullAll(Expression<Func<T,object>> name, object[] val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.PullAll(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> Push(Expression<Func<T,object>> name, object val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Push(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> PushAll(Expression<Func<T,object>> name, object[] val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.PushAll(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> PushEach(Expression<Func<T,object>> name, object[] val)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.PushEach(field, val);
            return this;
        }

        public MongoUpdateWarpper<T> Rename(Expression<Func<T, object>> name, string newname)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Rename(field, newname);
            return this;
        }

        public MongoUpdateWarpper<T> CurrentDate(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.CurrentDate(field);
            return this;
        }

        public MongoUpdateWarpper<T> Combine(MongoUpdateWarpper<T> other)
        {
            base.Combine(other);
            return this;
        }
    }
}
