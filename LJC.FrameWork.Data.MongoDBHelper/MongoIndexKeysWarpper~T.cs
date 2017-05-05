using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoIndexKeysWarpper<T>:MongoIndexKeysWarpper
    {
        public MongoIndexKeysWarpper Ascending(params Expression<Func<T, object>>[] names)
        {
            base.Ascending(names.Select(p => MongoDBUtil.GetMongoElementField(p)).ToArray());

            return this;
        }

        public MongoIndexKeysWarpper Descending(params Expression<Func<T, object>>[] names)
        {
            base.Descending(names.Select(p => MongoDBUtil.GetMongoElementField(p)).ToArray());

            return this;
        }

        public MongoIndexKeysWarpper Hashed(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.Hashed(field);

            return this;
        }

        public MongoIndexKeysWarpper GeoSpatial(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.GeoSpatial(field);
            return this;
        }

        public MongoIndexKeysWarpper GeoSpatialHaystack(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.GeoSpatialHaystack(field);
            return this;
        }

        public MongoIndexKeysWarpper GeoSpatialHaystack(Expression<Func<T, object>> name, string additionalName)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.GeoSpatialHaystack(field,additionalName);
            return this;
        }

        public MongoIndexKeysWarpper GeoSpatialSpherical(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name);
            base.GeoSpatialSpherical(field);
            return this;
        }

        public MongoIndexKeysWarpper Text(params Expression<Func<T, object>>[] names)
        {
            base.Text(names.Select(p => MongoDBUtil.GetMongoElementField(p)).ToArray());
            return this;
        }

        public MongoIndexKeysWarpper TextAll()
        {
            base.TextAll();
            return this;
        }
    }
}
