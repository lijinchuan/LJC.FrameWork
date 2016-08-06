using LJC.FrameWork.Data.QuickDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    [ReportAttr(TableName = "UserActive")]
    public class UserActive
    {
        [ReportAttr(Column = "ID", isKey = true)]
        public int ID
        {
            get;
            set;
        }

        [ReportAttr]
        public string Uid
        {
            get;
            set;
        }

        [ReportAttr]
        /// <summary>
        /// 1-关注
        /// 2-取消关注
        /// 3-点赞
        /// 4-取消点赞
        /// 5-收藏
        /// 6-取消收藏
        /// 7-转发
        /// 9-发帖子
        /// 10-删除帖子
        /// 11-回复帖子
        /// 12-删除回复
        /// </summary>
        public int ActiveType
        {
            get;
            set;
        }

        [ReportAttr]
        /// <summary>
        /// 股吧代码
        /// </summary>
        public string GubaCode
        {
            get;
            set;
        }

        [ReportAttr]
        /// <summary>
        /// 互动的用户对象
        /// </summary>
        public string ToUid
        {
            get;
            set;
        }

        [ReportAttr]
        /// <summary>
        /// 互动的帖子ID
        /// </summary>
        public long ToPostId
        {
            get;
            set;
        }

        [ReportAttr]
        /// <summary>
        /// 互动时间
        /// </summary>
        public DateTime ActiveTime
        {
            get;
            set;
        }

        [ReportAttr]

        public DateTime ActiveDate
        {
            get
            {
                return ActiveTime.Date;
            }

        }
    }
}
