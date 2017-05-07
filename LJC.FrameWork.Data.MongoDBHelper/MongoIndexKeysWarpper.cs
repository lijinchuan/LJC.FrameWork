using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    public class MongoIndexKeysWarpper
    {
        internal IndexKeysBuilder MongoIndexKeys = null;

        public MongoIndexKeysWarpper()
        {

        }

        public MongoIndexKeysWarpper Ascending(params string[] names)
        {
            if (MongoIndexKeys == null)
            {
                MongoIndexKeys = IndexKeys.Ascending(names);
            }
            else
            {
                MongoIndexKeys = MongoIndexKeys.Ascending(names);
            }

            return this;
        }

        public MongoIndexKeysWarpper Descending(params string[] names)
        {
            if (MongoIndexKeys == null)
            {
                MongoIndexKeys = IndexKeys.Descending(names);
            }
            else
            {
                MongoIndexKeys = MongoIndexKeys.Descending(names);
            }

            return this;
        }

        public MongoIndexKeysWarpper Hashed(string name)
        {
            if (MongoIndexKeys == null)
            {
                MongoIndexKeys = IndexKeys.Hashed(name);
            }
            else
            {
                MongoIndexKeys = MongoIndexKeys.Hashed(name);
            }

            return this;
        }

        public MongoIndexKeysWarpper GeoSpatial(string name)
        {
            if (MongoIndexKeys == null) 
            {
                MongoIndexKeys = IndexKeys.GeoSpatial(name);
            }
            else
            {
                MongoIndexKeys = MongoIndexKeys.GeoSpatial(name);
            }
            return this;
        }

        public MongoIndexKeysWarpper GeoSpatialHaystack(string name)
        {
            if (MongoIndexKeys == null)
            {
                MongoIndexKeys = IndexKeys.GeoSpatialHaystack(name);
            }
            else
            {
                MongoIndexKeys = MongoIndexKeys.GeoSpatialHaystack(name);
            }
            return this;
        }

        public MongoIndexKeysWarpper GeoSpatialHaystack(string name, string additionalName)
        {
            if (MongoIndexKeys == null)
            {
                MongoIndexKeys = IndexKeys.GeoSpatialHaystack(name, additionalName);
            }
            else
            {
                MongoIndexKeys = MongoIndexKeys.GeoSpatialHaystack(name, additionalName);
            }
            return this;
        }

        public MongoIndexKeysWarpper GeoSpatialSpherical(string name)
        {
            if (MongoIndexKeys == null)
            {
                MongoIndexKeys = IndexKeys.GeoSpatialSpherical(name);
            }
            else
            {
                MongoIndexKeys = MongoIndexKeys.GeoSpatialSpherical(name);
            }
            return this;
        }

        public MongoIndexKeysWarpper Text(params string[] names)
        {
            if (MongoIndexKeys == null)
            {
                MongoIndexKeys = IndexKeys.Text(names);
            }
            else
            {
                MongoIndexKeys = MongoIndexKeys.Text(names);
            }
            return this;
        }

        public MongoIndexKeysWarpper TextAll()
        {
            if (MongoIndexKeys == null)
            {
                MongoIndexKeys = IndexKeys.TextAll();
            }
            else
            {
                MongoIndexKeys = MongoIndexKeys.TextAll();
            }
            return this;
        }
    }
}
