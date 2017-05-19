using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.Mongo
{
    public class MongoGridFSWarpper
    {
        internal MongoGridFS MongoGFS = null;

        internal MongoGridFSWarpper(MongoGridFS gfs)
        {
            this.MongoGFS = gfs;
        }

        public string Upload(string filename, byte[] buffer)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer))
            {
                var uploadresult = MongoGFS.Upload(ms, filename);

                return uploadresult.Id.ToString();
            }
        }

        public byte[] GetGFS(string file)
        {
            var info = MongoGFS.FindOne(file);

            using (var stream = info.OpenRead())
            {
                byte[] buffer = new byte[info.Length];
                stream.Read(buffer, 0, buffer.Length);

                return buffer;
            }
        }
    }
}
