using LJC.FrameWork.Data.QuickDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    [DataBaseMapperAttr(TableName = "NewsKeys")]
    public class NewsKeysEntity
    {
        //表名
        public const string TbName = "CjzfDB.NewsKeys";

        private long _newsKeysID;
        [DataBaseMapperAttr(Column = "NewsKeysID",isKey =true)]
        public long NewsKeysID
        {
            get
            {
                return _newsKeysID;
            }
            set
            {
                _newsKeysID = value;
            }
        }

        private long _newsID;
        [DataBaseMapperAttr(Column = "NewsID")]
        public long NewsID
        {
            get
            {
                return _newsID;
            }
            set
            {
                _newsID = value;
            }
        }

        private string _keys;
        [DataBaseMapperAttr(Column = "Keys")]
        public string Keys
        {
            get
            {
                return _keys;
            }
            set
            {
                _keys = value;
            }
        }

        private DateTime _cDate;
        [DataBaseMapperAttr(Column = "CDate")]
        public DateTime CDate
        {
            get
            {
                return _cDate;
            }
            set
            {
                _cDate = value;
            }
        }

        private DateTime _mDate;
        [DataBaseMapperAttr(Column = "MDate")]
        public DateTime MDate
        {
            get
            {
                return _mDate;
            }
            set
            {
                _mDate = value;
            }
        }

        private string _newsClass;
        [DataBaseMapperAttr(Column = "NewsClass")]
        public string NewsClass
        {
            get
            {
                return _newsClass;
            }
            set
            {
                _newsClass = value;
            }
        }

        private string _currentWord;
        [DataBaseMapperAttr(Column = "CurrentWord")]
        public string CurrentWord
        {
            get
            {
                return _currentWord;
            }
            set
            {
                _currentWord = value;
            }
        }

        private int _appTimes;
        [DataBaseMapperAttr(Column = "AppTimes")]
        public int AppTimes
        {
            get
            {
                return _appTimes;
            }
            set
            {
                _appTimes = value;
            }
        }

        private int _postionStart;
        [DataBaseMapperAttr(Column = "PostionStart")]
        public int PostionStart
        {
            get
            {
                return _postionStart;
            }
            set
            {
                _postionStart = value;
            }
        }

        private int _postionEnd;
        [DataBaseMapperAttr(Column = "PostionEnd")]
        public int PostionEnd
        {
            get
            {
                return _postionEnd;
            }
            set
            {
                _postionEnd = value;
            }
        }

        private string _title;
        [DataBaseMapperAttr(Column = "Title")]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }
    }
}

