using LJC.FrameWork.Data.QuickDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    [DataBaseMapperAttr(TableName = "PrestigeInfo_Compare")]
    public class PrestigeInfo_CompareEntity
    {
        //表名
        public const string TbName = "PrestigeV2TestDB.PrestigeInfo_Compare";

        private int _iD;
        //支持ProtoBuf[ProtoMember(1)]
        [DataBaseMapperAttr(Column = "ID", isKey = true)]
        //支持display属性[Display(Name = "")]
        public int ID
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
        private double _stars;
        //支持ProtoBuf[ProtoMember(2)]
        [DataBaseMapperAttr(Column = "Stars")]
        //支持display属性[Display(Name = "")]
        public double Stars
        {
            get
            {
                return _stars;
            }
            set
            {
                _stars = value;
            }
        }
        private int _level;
        //支持ProtoBuf[ProtoMember(3)]
        [DataBaseMapperAttr(Column = "Level")]
        //支持display属性[Display(Name = "")]
        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }
        private double _needScores;
        //支持ProtoBuf[ProtoMember(4)]
        [DataBaseMapperAttr(Column = "NeedScores")]
        //支持display属性[Display(Name = "")]
        public double NeedScores
        {
            get
            {
                return _needScores;
            }
            set
            {
                _needScores = value;
            }
        }
        private int _persons;
        //支持ProtoBuf[ProtoMember(5)]
        [DataBaseMapperAttr(Column = "Persons")]
        //支持display属性[Display(Name = "")]
        public int Persons
        {
            get
            {
                return _persons;
            }
            set
            {
                _persons = value;
            }
        }
        private double _personsRate;
        //支持ProtoBuf[ProtoMember(6)]
        [DataBaseMapperAttr(Column = "PersonsRate")]
        //支持display属性[Display(Name = "")]
        public double PersonsRate
        {
            get
            {
                return _personsRate;
            }
            set
            {
                _personsRate = value;
            }
        }
        private int _recent7DaysPosts;
        //支持ProtoBuf[ProtoMember(7)]
        [DataBaseMapperAttr(Column = "Recent7DaysPosts")]
        //支持display属性[Display(Name = "")]
        public int Recent7DaysPosts
        {
            get
            {
                return _recent7DaysPosts;
            }
            set
            {
                _recent7DaysPosts = value;
            }
        }
        private double _recent7DaysPostsRate;
        //支持ProtoBuf[ProtoMember(8)]
        [DataBaseMapperAttr(Column = "Recent7DaysPostsRate")]
        //支持display属性[Display(Name = "")]
        public double Recent7DaysPostsRate
        {
            get
            {
                return _recent7DaysPostsRate;
            }
            set
            {
                _recent7DaysPostsRate = value;
            }
        }
        private bool _isOld;
        //支持ProtoBuf[ProtoMember(9)]
        [DataBaseMapperAttr(Column = "isOld")]
        //支持display属性[Display(Name = "是否旧版")]
        public bool IsOld
        {
            get
            {
                return _isOld;
            }
            set
            {
                _isOld = value;
            }
        }
    }
}
