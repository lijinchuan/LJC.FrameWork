using Ljc.Com.NewsService.Entity;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test2
{
    public class NewsEntityEx : NewsEntity
    {
        public ObjectId _id
        {
            get;
            set;
        }
    }
}
