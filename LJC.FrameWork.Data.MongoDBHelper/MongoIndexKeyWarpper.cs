using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.MongoDBHelper
{
    public class MongoIndexKeyWarpper
    {
        internal IndexKeysBuilder MongoIndexKeys = null;

        private MongoIndexKeyWarpper()
        {

        }

        public static MongoIndexKeyWarpper NewWarpper()
        {
            return new MongoIndexKeyWarpper();
        }
    }
}
