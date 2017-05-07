using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    public class MongoIndexKeysWarpper<T>:MongoIndexKeysWarpper
    {
        public MongoIndexKeysWarpper<T> Ascending(params Expression<Func<T, object>>[] names)
        {
            base.Ascending(names.Select(p => MongoDBUtil.GetMongoElementField(p.Body)).ToArray());

            return this;
        }

        public MongoIndexKeysWarpper<T> Descending(params Expression<Func<T, object>>[] names)
        {
            base.Descending(names.Select(p => MongoDBUtil.GetMongoElementField(p.Body)).ToArray());

            return this;
        }

        public MongoIndexKeysWarpper<T> Hashed(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.Hashed(field);

            return this;
        }

        public MongoIndexKeysWarpper<T> GeoSpatial(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.GeoSpatial(field);
            return this;
        }

        public MongoIndexKeysWarpper<T> GeoSpatialHaystack(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.GeoSpatialHaystack(field);
            return this;
        }

        public MongoIndexKeysWarpper<T> GeoSpatialHaystack(Expression<Func<T, object>> name, string additionalName)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.GeoSpatialHaystack(field,additionalName);
            return this;
        }

        public MongoIndexKeysWarpper<T> GeoSpatialSpherical(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);
            base.GeoSpatialSpherical(field);
            return this;
        }

        public MongoIndexKeysWarpper<T> Text(params Expression<Func<T, object>>[] names)
        {
            base.Text(names.Select(p => MongoDBUtil.GetMongoElementField(p.Body)).ToArray());
            return this;
        }

        public MongoIndexKeysWarpper<T> TextAll()
        {
            base.TextAll();
            return this;
        }
    }
}
