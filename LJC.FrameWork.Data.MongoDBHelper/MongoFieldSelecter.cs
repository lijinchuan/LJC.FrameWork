using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    public class MongoFieldSelecter<T>
    {
        private List<string> _fields = new List<string>();

        public MongoFieldSelecter<T> Select(Expression<Func<T, object>> name)
        {
            var field = MongoDBUtil.GetMongoElementField(name.Body);

            if (!_fields.Contains(field))
            {
                _fields.Add(field);
            }

            return this;
        }

        internal string[] GetFields()
        {
            return _fields.ToArray();
        }

    }
}
