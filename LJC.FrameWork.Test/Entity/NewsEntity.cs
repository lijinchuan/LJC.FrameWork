using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.Data.QuickDataBase;

namespace LJC.FrameWork.Test.Entity
{
    [ReportAttr(TableName="news")]
    public class NewsEntity
    {
        //表名
        public const string TbName = "CjzfDB.news";

        private DateTime _cdate;
        //支持ProtoBuf[ProtoMember(1)]
        [ReportAttr(Column="cdate")]
        //支持display属性[Display(Name = "")]
        public DateTime Cdate
        {
            get
            {
                return _cdate;
            }
            set
            {
                _cdate = value;
            }
        }
        private DateTime _mdate;
        //支持ProtoBuf[ProtoMember(2)]
        [ReportAttr(Column="mdate")]
        //支持display属性[Display(Name = "")]
        public DateTime Mdate
        {
            get
            {
                return _mdate;
            }
            set
            {
                _mdate = value;
            }
        }
        private string _title;
        //支持ProtoBuf[ProtoMember(3)]
        [ReportAttr(Column="title")]
        //支持display属性[Display(Name = "")]
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
        private string _content;
        //支持ProtoBuf[ProtoMember(4)]
        [ReportAttr(Column="content")]
        //支持display属性[Display(Name = "")]
        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
            }
        }
        private string _class;
        //支持ProtoBuf[ProtoMember(5)]
        [ReportAttr(Column="class")]
        //支持display属性[Display(Name = "")]
        public string Class
        {
            get
            {
                return _class;
            }
            set
            {
                _class = value;
            }
        }
        private string _source;
        //支持ProtoBuf[ProtoMember(6)]
        [ReportAttr(Column="source")]
        //支持display属性[Display(Name = "")]
        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }
        private string _formurl;
        //支持ProtoBuf[ProtoMember(7)]
        [ReportAttr(Column="formurl")]
        //支持display属性[Display(Name = "")]
        public string Formurl
        {
            get
            {
                return _formurl;
            }
            set
            {
                _formurl = value;
            }
        }
        private string _keywords;
        //支持ProtoBuf[ProtoMember(8)]
        [ReportAttr(Column="keywords")]
        //支持display属性[Display(Name = "")]
        public string Keywords
        {
            get
            {
                return _keywords;
            }
            set
            {
                _keywords = value;
            }
        }
        private DateTime _newsDate;
        //支持ProtoBuf[ProtoMember(9)]
        [ReportAttr(Column="newsDate")]
        //支持display属性[Display(Name = "")]
        public DateTime NewsDate
        {
            get
            {
                return _newsDate;
            }
            set
            {
                _newsDate = value;
            }
        }
        private long _isImp;
        //支持ProtoBuf[ProtoMember(10)]
        [ReportAttr(Column="isImp")]
        //支持display属性[Display(Name = "")]
        public long IsImp
        {
            get
            {
                return _isImp;
            }
            set
            {
                _isImp = value;
            }
        }
        private bool _isvalid;
        //支持ProtoBuf[ProtoMember(11)]
        [ReportAttr(Column="isvalid")]
        //支持display属性[Display(Name = "")]
        public bool Isvalid
        {
            get
            {
                return _isvalid;
            }
            set
            {
                _isvalid = value;
            }
        }
        private string _conkeywords;
        //支持ProtoBuf[ProtoMember(12)]
        [ReportAttr(Column="conkeywords")]
        //支持display属性[Display(Name = "")]
        public string Conkeywords
        {
            get
            {
                return _conkeywords;
            }
            set
            {
                _conkeywords = value;
            }
        }
        private int _id;
        //支持ProtoBuf[ProtoMember(13)]
        [ReportAttr(Column="id",isKey=true)]
        //支持display属性[Display(Name = "")]
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        private bool _isList;
        //支持ProtoBuf[ProtoMember(14)]
        [ReportAttr(Column="isList")]
        //支持display属性[Display(Name = "")]
        public bool IsList
        {
            get
            {
                return _isList;
            }
            set
            {
                _isList = value;
            }
        }
        private bool _isRead;
        //支持ProtoBuf[ProtoMember(15)]
        [ReportAttr(Column="isRead")]
        //支持display属性[Display(Name = "")]
        public bool IsRead
        {
            get
            {
                return _isRead;
            }
            set
            {
                _isRead = value;
            }
        }
        private bool _isReqest;
        //支持ProtoBuf[ProtoMember(16)]
        [ReportAttr(Column="isReqest")]
        //支持display属性[Display(Name = "")]
        public bool IsReqest
        {
            get
            {
                return _isReqest;
            }
            set
            {
                _isReqest = value;
            }
        }
        private string _newsWriter;
        //支持ProtoBuf[ProtoMember(17)]
        [ReportAttr(Column="newsWriter")]
        //支持display属性[Display(Name = "")]
        public string NewsWriter
        {
            get
            {
                return _newsWriter;
            }
            set
            {
                _newsWriter = value;
            }
        }
        private string _path;
        //支持ProtoBuf[ProtoMember(18)]
        [ReportAttr(Column="path")]
        //支持display属性[Display(Name = "")]
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
            }
        }
        private int _power;
        //支持ProtoBuf[ProtoMember(19)]
        [ReportAttr(Column="power")]
        //支持display属性[Display(Name = "")]
        public int Power
        {
            get
            {
                return _power;
            }
            set
            {
                _power = value;
            }
        }
        private int _clicktime;
        //支持ProtoBuf[ProtoMember(20)]
        [ReportAttr(Column="clicktime")]
        //支持display属性[Display(Name = "")]
        public int Clicktime
        {
            get
            {
                return _clicktime;
            }
            set
            {
                _clicktime = value;
            }
        }
        private bool _isHtmlMaked;
        //支持ProtoBuf[ProtoMember(21)]
        [ReportAttr(Column="IsHtmlMaked")]
        //支持display属性[Display(Name = "")]
        public bool IsHtmlMaked
        {
            get
            {
                return _isHtmlMaked;
            }
            set
            {
                _isHtmlMaked = value;
            }
        }
    }
}

