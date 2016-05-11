using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public sealed class MySqlDataContextMoudle<T> : DataContextMoudle<T> where T : new()
    {


        //添加
        protected override string AddSql
        {
            get
            {
                List<string> columns = rpAttrList.Where(r => !r.isKey && !string.IsNullOrWhiteSpace(r.Column)).Select(r => r.Column).ToList();
                return string.Format(@"insert into `{0}`({1}) values({2});
                               SELECT LAST_INSERT_ID();", tabName, "`" + string.Join("`,`", columns) + "`", "@" + string.Join(",@", columns));

            }
        }

        //更新
        protected override string UpdateSql
        {
            get
            {
                List<string> columns = rpAttrList.Where(r => !r.isKey && !string.IsNullOrWhiteSpace(r.Column)).Select(c => "`" + c.Column + "`=@" + c.Column).ToList();
                return string.Format("update `{0}` set {1} where {2}", tabName, string.Join(",", columns), "`" + keyName + "`=@" + keyName);
            }
        }

        //删除
        protected override string DelSql
        {
            get
            {
                return string.Format("delete from `{0}` where {1}", tabName, keyName + "=@" + keyName);
            }
        }

        //查询
        protected override string SelSql
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_selSql))
                {
                    object[] os = typeof(T).GetCustomAttributes(typeof(ReportAttr), true);
                    tabName = ((ReportAttr)os[0]).TableName;

                    _selSql = string.Format(" {0} from {1} where 1=1",
                        string.Join(",", rpAttrList.Select(r => "`" + r.Column + "`").ToList()), tabName);

                    _selSql += " {0} ";

                }
                if (topNum != 0)
                    return string.Concat("select ", _selSql, " limit 0,", topNum);
                return string.Concat("select ", _selSql);
            }
        }

        public override object Max(string column)
        {
            string maxSql = string.Format("select max(`{0}`) from `{1}`", column, tabName);
            return ExecuteScalar(maxSql);
        }

        public override object Min(string column)
        {
            string minSql = string.Format("select min(`{0}`) from `{1}`", column, tabName);
            return ExecuteScalar(minSql);
        }

        public MySqlDataContextMoudle()
            : base()
        {

        }

        public MySqlDataContextMoudle(T instance)
            : base(instance)
        {

        }
    }
}
