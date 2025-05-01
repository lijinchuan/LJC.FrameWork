using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace LJC.FrameWork.Comm
{
    [Serializable]
    public class CacheItem<T>
    {
        private T _item;

        /// <summary>
        /// 值
        /// </summary>
        public T Item
        {
            get
            {

                if(_item == null && _data != null)
                {
                    var json = Encoding.UTF8.GetString(GZip.Decompress(_data));
                    var entity = JsonHelper.JsonToEntity<T>(json);

                    return entity;
                }

                return _item;
            }
            set
            {
                var count = 0;
                if (value is IList)
                {
                    count = (value as IList).Count;
                }
                else if (value is Array)
                {
                    count = (value as Array).Length;
                }

                if (count > 100)
                {
                    var jsonBytes = Encoding.UTF8.GetBytes(JsonHelper.ToJson(value));
                    jsonBytes = GZip.Compress(jsonBytes);
                    _data = jsonBytes;
                }
                else
                {
                    _item = value;
                }
            }
        }

        private byte[] _data
        {
            get;
            set;
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime Expired
        {
            get;
            set;
        }

        public Func<T> RefrashFunc
        {
            get;
            set;
        }

        private int _cachMinis = 10;
        public int CachMinis
        {
            get
            {
                return _cachMinis;
            }
            set
            {
                if (value < 1)
                    value = 1;
                _cachMinis = value;
                Expired = DateTime.Now.AddMinutes(value);
            }
        }
    }
}
