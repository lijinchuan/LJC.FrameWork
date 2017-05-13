using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    [BsonIgnoreExtraElements]
    public class IMongoDocumentObject
    {
        [Newtonsoft.Json.JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId _id
        {
            get;
            set;
        }
    }
}
