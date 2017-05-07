using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    public class MongoSortWarpper
    {
        internal SortByBuilder MongoSortBy = null;

        public MongoSortWarpper()
        {

        }

        public MongoSortWarpper Asc(params string[] keys)
        {
            if (MongoSortBy == null)
            {
                MongoSortBy = SortBy.Ascending(keys);
            }
            else
            {
                MongoSortBy = MongoSortBy.Ascending(keys);
            }
            return this;
        }

        public MongoSortWarpper Desc(params string[] keys)
        {
            if (MongoSortBy == null)
            {
                MongoSortBy = SortBy.Descending(keys);
            }
            else
            {
                MongoSortBy = MongoSortBy.Descending(keys);
            }
            return this;
        }
    }
}
