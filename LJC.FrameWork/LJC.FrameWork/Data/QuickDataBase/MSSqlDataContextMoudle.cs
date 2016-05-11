using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public class MSSqlDataContextMoudle<T> : DataContextMoudle<T> where T : new()
    {

        //添加
        protected override string AddSql
        {
            get
            {
                //add
                List<string> columns = rpAttrList.Where(r => !r.isKey && !string.IsNullOrWhiteSpace(r.Column)).Select(r => r.Column).ToList();
                return string.Format(@"insert into {0}({1}) values({2});
                                       select @@identity;", tabName, "[" + string.Join("],[", columns) + "]", "@" + string.Join(",@", columns));

            }
        }

        //更新
        protected override string UpdateSql
        {
            get
            {
                //CommFun.Assert(!string.IsNullOrWhiteSpace(keyName));
                //update
                List<string> columns = rpAttrList.Where(r => !r.isKey && !string.IsNullOrWhiteSpace(r.Column)).Select(c => "[" + c.Column + "]=@" + c.Column).ToList();
                return string.Format("update {0} set {1} where {2}", tabName, string.Join(",", columns), "[" + keyName + "]=@" + keyName);
            }
        }

        //删除
        protected override string DelSql
        {
            get
            {
                //CommFun.Assert(!string.IsNullOrWhiteSpace(keyName));
                return string.Format("delete from {0} where {1}", tabName, keyName + "=@" + keyName);
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

                    _selSql = string.Format(" {0} from {1}(nolock) where 1=1",
                        string.Join(",", rpAttrList.Select(r => "[" + r.Column + "]").ToList()), tabName);
                }
                if (topNum != 0)
                    return string.Concat("select top ", topNum, _selSql, " {0}");
                return string.Concat("select ", _selSql, " {0}");
            }
        }

        public override object Max(string column)
        {
            string maxSql = string.Format("select max({0}) from {1}(nolock)", column, tabName);
            return ExecuteScalar(maxSql);
        }

        public override object Min(string column)
        {
            string minSql = string.Format("select min({0}) from {1}(nolock)", column, tabName);
            return ExecuteScalar(minSql);
        }

        public MSSqlDataContextMoudle()
            : base()
        {

        }

        public MSSqlDataContextMoudle(T instance)
            : base(instance)
        {

        }
    }
}
