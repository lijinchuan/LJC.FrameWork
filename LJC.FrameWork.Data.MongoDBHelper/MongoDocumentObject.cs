using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    [BsonIgnoreExtraElements]
    public class MongoDocumentObject
    {
        public ObjectId _id
        {
            get;
            set;
        }
    }
}
