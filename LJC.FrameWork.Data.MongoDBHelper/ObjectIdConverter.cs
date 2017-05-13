using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    public class ObjectIdConverter : Newtonsoft.Json.JsonConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            MongoDB.Bson.ObjectId result;
            MongoDB.Bson.ObjectId.TryParse(reader.Value as string, out result);
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(MongoDB.Bson.ObjectId).IsAssignableFrom(objectType);
        }
    }
}
