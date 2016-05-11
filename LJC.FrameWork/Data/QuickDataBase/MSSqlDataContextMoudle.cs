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
                string key = string.Format("MS_{0}_{1}_AddSql", database, tabName);
                return LookUpDataContextMoudleCach(key, () =>
                    {
                        //add
                        List<string> columns = rpAttrList.Where(r => !r.isKey && !string.IsNullOrWhiteSpace(r.Column)).Select(r => r.Column).ToList();
                        return string.Format(@"insert into {0}({1}) values({2});
                                       select @@identity;", tabName, "[" + string.Join("],[", columns) + "]", "@" + string.Join(",@", columns));
                    });
            }
        }

        //更新
        protected override string UpdateSql
        {
            get
            {
                string key = string.Format("MS_{0}_{1}_UpdateSql", database, tabName);
                return LookUpDataContextMoudleCach(key, () =>
                   {
                       List<string> columns = rpAttrList.Where(r => !r.isKey && !string.IsNullOrWhiteSpace(r.Column)).Select(c => "[" + c.Column + "]=@" + c.Column).ToList();
                       return string.Format("update {0} set {1} where {2}", tabName, string.Join(",", columns), "[" + keyName + "]=@" + keyName);
                   });
            }
        }

        //删除
        protected override string DelSql
        {
            get
            {
                string key = string.Format("MS_{0}_{1}_DelSql", database, tabName);
                return LookUpDataContextMoudleCach(key, () =>
                   {
                       return string.Format("delete from {0} where {1}", tabName, keyName + "=@" + keyName);
                   });
            }
        }

        //查询
        protected override string SelSql
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_selSql))
                {
                    string key = string.Format("MS_{0}_{1}_SelSql", database, tabName);
                    _selSql = LookUpDataContextMoudleCach(key, () =>
                        {
                            object[] os = typeof(T).GetCustomAttributes(typeof(ReportAttr), true);
                            tabName = ((ReportAttr)os[0]).TableName;

                            return string.Format(" {0} from {1}(nolock) where 1=1",
                                string.Join(",", rpAttrList.Select(r => "[" + r.Column + "]").ToList()), tabName);
                        });
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

        public override bool Update(params string[] colArray)
        {
            if (colArray == null || colArray.Length == 0)
                return Update();

            var colArrayToList=colArray.ToList();
            List<string> columns = rpAttrList.Where(r => !r.isKey && !string.IsNullOrWhiteSpace(r.Column)&&colArrayToList.Exists(f=>f.Equals(r.Column,StringComparison.OrdinalIgnoreCase)))
                .Select(c => "[" + c.Column + "]=@" + c.Column).ToList();
            if (columns.Count != colArray.Length)
                throw new Exception("参数错误：要更新的列不存在");

            string sqlPartUpdate = string.Format("update {0} set {1} where {2}", tabName, string.Join(",", columns), "[" + keyName + "]=@" + keyName);

            return ExecuteNonQuery(sqlPartUpdate)>0;
        }

        public override bool NotUpdate(params string[] colArray)
        {
            if (colArray == null || colArray.Length == 0)
                return Update();

            var colArrayToList = colArray.ToList();
            List<string> columns = rpAttrList.Where(r => !r.isKey && !string.IsNullOrWhiteSpace(r.Column) && !colArrayToList.Exists(f => f.Equals(r.Column, StringComparison.OrdinalIgnoreCase)))
                .Select(c => "[" + c.Column + "]=@" + c.Column).ToList();

            string sqlPartUpdate = string.Format("update {0} set {1} where {2}", tabName, string.Join(",", columns), "[" + keyName + "]=@" + keyName);

            return ExecuteNonQuery(sqlPartUpdate) > 0;
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
