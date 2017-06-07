using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    public class MongoUpdateFlagsWarpper
    {
        internal UpdateFlags MongoUpdateFlags = UpdateFlags.Multi;

        public MongoUpdateFlagsWarpper SetMulti()
        {
            MongoUpdateFlags = MongoUpdateFlags | UpdateFlags.Multi;
            return this;
        }

        public MongoUpdateFlagsWarpper SetUpsert()
        {
            MongoUpdateFlags = MongoUpdateFlags | UpdateFlags.Upsert;
            return this;
        }
    }
}
