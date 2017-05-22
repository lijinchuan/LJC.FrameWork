using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    [BsonIgnoreExtraElements]
    public class MongoDocumentObject
    {
        [Newtonsoft.Json.JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId _id
        {
            get;
            set;
        }

        /// <summary>
        /// 创建索引时需要返回的列，重写此方法，mongodbhelper将自动在合适的时候创建索引
        /// </summary>
        /// <param name="unique">是否是唯一索引</param>
        /// <param name="background">是否在后台创建</param>
        /// <returns>返回列组，如果为空，则不创建</returns>
        public virtual IEnumerable<Tuple<string[],bool,bool>> CreateIndex()
        {
            yield break;
        }
    }
}
