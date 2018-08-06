using LJC.FrameWork.Data.QuickDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    [DataBaseMapperAttr(TableName = "GubaBandResult")]
    public class GubaBandResultEntity
    {
        //表名
        public const string TbName = "GubaData.GubaBandResult";

        private long _iD;
        [DataBaseMapperAttr(Column = "ID", isKey = true)]
        public long ID
        {
            get
            {
                return _iD;
            }
            set
            {
                _iD = value;
            }
        }

        private string _gubaCode;
        [DataBaseMapperAttr(Column = "GubaCode")]
        public string GubaCode
        {
            get
            {
                return _gubaCode;
            }
            set
            {
                _gubaCode = value;
            }
        }

        private string _uid;
        [DataBaseMapperAttr(Column = "Uid")]
        public string Uid
        {
            get
            {
                return _uid;
            }
            set
            {
                _uid = value;
            }
        }

        private int _recount;
        [DataBaseMapperAttr(Column = "Recount")]
        public int Recount
        {
            get
            {
                return _recount;
            }
            set
            {
                _recount = value;
            }
        }

        private int _zfCount;
        [DataBaseMapperAttr(Column = "ZfCount")]
        public int ZfCount
        {
            get
            {
                return _zfCount;
            }
            set
            {
                _zfCount = value;
            }
        }

        private int _postCount;
        [DataBaseMapperAttr(Column = "PostCount")]
        public int PostCount
        {
            get
            {
                return _postCount;
            }
            set
            {
                _postCount = value;
            }
        }

        private int _lenPostCount;
        [DataBaseMapperAttr(Column = "LenPostCount")]
        public int LenPostCount
        {
            get
            {
                return _lenPostCount;
            }
            set
            {
                _lenPostCount = value;
            }
        }

        private int _likeCount;
        [DataBaseMapperAttr(Column = "LikeCount")]
        public int LikeCount
        {
            get
            {
                return _likeCount;
            }
            set
            {
                _likeCount = value;
            }
        }

        private int _dateType;
        [DataBaseMapperAttr(Column = "DateType")]
        public int DateType
        {
            get
            {
                return _dateType;
            }
            set
            {
                _dateType = value;
            }
        }
    }
}

