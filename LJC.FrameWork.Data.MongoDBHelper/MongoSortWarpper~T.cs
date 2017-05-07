using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    public class MongoSortWarpper<T>:MongoSortWarpper
    {
        public MongoSortWarpper<T> Asc(params Expression<Func<T,object>>[] keys)
        {
            base.Asc(keys.Select(p => MongoDBUtil.GetMongoElementField(p.Body)).ToArray());
            return this;
        }

        public MongoSortWarpper<T> Desc(params Expression<Func<T, object>>[] keys)
        {
            base.Desc(keys.Select(p => MongoDBUtil.GetMongoElementField(p.Body)).ToArray());
            return this;
        }
    }
}
