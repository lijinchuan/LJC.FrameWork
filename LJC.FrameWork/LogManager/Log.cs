using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.Data.QuickDataBase;

namespace LJC.FrameWork.LogManager
{
    [Serializable]
    [ReportAttr(TableName = "tb_Log")]
    public class Log
    {
        [ReportAttr(Column = "ID", ColumnName = "编号", isKey = true)]
        public int ID
        {
            get;
            set;
        }

        [ReportAttr(Column = "LogTit", ColumnName = "日志标题")]
        public string LogTit
        {
            get;
            set;
        }

        [ReportAttr(Column = "LogBody", ColumnName = "日志正文")]
        public string LogBody
        {
            get;
            set;
        }

        [ReportAttr(Column = "Category", ColumnName = "日志分类")]
        public LogCategory Category
        {
            get;
            set;
        }

        [ReportAttr(Column = "Level", ColumnName = "日志分级")]
        public LogLevel Level
        {
            get;
            set;
        }

        [ReportAttr(Column = "LogTime", ColumnName = "时间")]
        public DateTime LogTime
        {
            get;
            set;
        }
    }
}
