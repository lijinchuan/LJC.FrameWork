using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.Data.QuickDataBase;

namespace LJC.FrameWork.Test
{
    public enum CmdType
    {
        /// <summary>
        /// 买入
        /// </summary>
        buy,
        /// <summary>
        /// 卖出
        /// </summary>
        sell
    }

    [ReportAttr(TableName = "tb_StockCmd")]
    public class StockCmd
    {
        /// <summary>
        /// 命令ID
        /// </summary>
        [ReportAttr(Column = "ID", ColumnName = "ID", isKey = true, Width = 80)]
        public int ID
        {
            get;
            set;
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        [ReportAttr(Column = "CreateTime", ColumnName = "创建时间", Sort = true, Width = 150)]
        public DateTime CreateTime
        {
            get;
            set;
        }
        /// <summary>
        /// 提交时间
        /// </summary>
        [ReportAttr(Column = "SubmitTime", ColumnName = "成交时间", Width = 150)]
        public DateTime SubmitTime
        {
            get;
            set;
        }
        /// <summary>
        /// 股票代码
        /// </summary>
        [ReportAttr(Column = "StockCode", ColumnName = "股票代码", Sort = true)]
        public string StockCode
        {
            get;
            set;
        }
        /// <summary>
        /// 股票名称
        /// </summary>
        [ReportAttr(Column = "StockName", ColumnName = "股票名称")]
        public string StockName
        {
            get;
            set;
        }
        /// <summary>
        /// 命令类型
        /// </summary>
        [ReportAttr(Column = "CmdType", ColumnName = "操作指令")]
        public CmdType CmdType
        {
            get;
            set;
        }

        /// <summary>
        /// 委托价格
        /// </summary>
        [ReportAttr(Column = "price", ColumnName = "委托价格")]
        public decimal Price
        {
            get;
            set;
        }

        /// <summary>
        /// 委托数量
        /// </summary>
        [ReportAttr(Column = "Quantity", ColumnName = "委托数量")]
        public int Quantity
        {
            get;
            set;
        }

        /// <summary>
        /// 最多数量
        /// </summary>
        [ReportAttr(Column = "CanQuantity", ColumnName = "最多数量")]
        public int CanQuantity
        {
            get;
            set;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        [ReportAttr(Column = "IsSuccess", ColumnName = "是否成功")]
        public bool IsSuccess
        {
            get;
            set;
        }

        [ReportAttr(Column = "CmdReason", ColumnName = "操作原因", Width = 200)]
        public string CmdReason
        {
            get;
            set;
        }

        [ReportAttr(Column = "EffDate", ColumnName = "有效日期")]
        public DateTime EffDate
        {
            get;
            set;
        }

        /// <summary>
        /// 成本计算
        /// </summary>
        [ReportAttr(Column = "Cost", ColumnName = "成本")]
        public decimal Cost
        {
            get;
            set;
        }

        [ReportAttr(ColumnName = "成本价格", Column = "CostPrice")]
        public decimal CostPrice
        {
            get;
            set;
        }


        /// <summary>
        /// 证券余额
        /// </summary>
        [ReportAttr(ColumnName = "证券余额", Column = "StockBlance")]
        public int StockBlance
        {
            get;
            set;
        }

        /// <summary>
        /// 是否已经得到邮件通知
        /// </summary>
        [ReportAttr(Column = "IsEmailCommit")]
        public bool IsEmailCommit
        {
            get;
            set;
        }

        /// <summary>
        /// 是否已经发送邮件
        /// </summary>
        [ReportAttr(Column = "IsSendEmail")]
        public bool IsSendEmail
        {
            get;
            set;
        }

        /// <summary>
        /// 是否取消
        /// </summary>
        [ReportAttr(Column = "IsCancel")]
        public bool IsCancel
        {
            get;
            set;
        }
    }
}
