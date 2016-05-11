using LJC.FrameWork.Data.QuickDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    [ReportAttr(TableName = "UserPrestigeBand_Compare")]
    public class UserPrestigeBand_CompareEntity
    {
        //表名
        public const string TbName = "PrestigeV2TestDB.UserPrestigeBand_Compare";

        private string _uid;
        //支持ProtoBuf[ProtoMember(2)]
        [ReportAttr(Column = "Uid", isKey = true)]
        //支持display属性[Display(Name = "")]
        public string 用户ID
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
        private double _level;
        //支持ProtoBuf[ProtoMember(3)]
        [ReportAttr(Column = "Level")]
        //支持display属性[Display(Name = "")]
        public double 等级
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
        private double _stars;
        //支持ProtoBuf[ProtoMember(4)]
        [ReportAttr(Column = "Stars")]
        //支持display属性[Display(Name = "")]
        public double 星级
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
        private int _score;
        //支持ProtoBuf[ProtoMember(5)]
        [ReportAttr(Column = "Score")]
        //支持display属性[Display(Name = "")]
        public int 影响力分值
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
            }
        }


        private string _quan;
        //支持ProtoBuf[ProtoMember(8)]
        [ReportAttr(Column = "Quan")]
        //支持display属性[Display(Name = "")]
        public string 能力圈
        {
            get
            {
                return _quan;
            }
            set
            {
                _quan = value;
            }
        }

    }
}
