using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    internal class MongoCollectionWarpper
    {
        internal MongoCollection MongoDBCollection = null;

        public MongoCollectionWarpper(MongoCollection collection)
        {
            this.MongoDBCollection = collection;
        }

        private bool _isCreateIndex = false;
        public bool IsCreateIndex
        {
            get
            {
                return _isCreateIndex;
            }
            set
            {
                _isCreateIndex = value;
            }
        }
    }
}
