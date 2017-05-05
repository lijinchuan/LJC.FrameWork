using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoSortWarpper<T>:MongoSortWarpper
    {
        public MongoSortWarpper Asc(params Expression<Func<T,object>>[] keys)
        {
            base.Asc(keys.Select(p => MongoDBUtil.GetMongoElementField(p)).ToArray());
            return this;
        }

        public MongoSortWarpper Desc(params Expression<Func<T, object>>[] keys)
        {
            base.Desc(keys.Select(p => MongoDBUtil.GetMongoElementField(p)).ToArray());
            return this;
        }
    }
}
