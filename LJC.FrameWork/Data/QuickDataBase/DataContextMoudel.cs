using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using LJC.FrameWork.Comm;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Threading;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public class DataContextMoudle<T> where T : new()
    {
        private static Dictionary<string, string> dataContextMoudleCach = new Dictionary<string, string>();
        private static ReaderWriterLockSlim dataContextMoudleCachLock = new ReaderWriterLockSlim();
        private static Regex sqlParamRegex = new Regex(@"(?<!@)(@\w+)\s?", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 实例
        /// </summary>
        protected T Instance;
        internal List<ReportAttr> rpAttrList;
        protected string _selSql;
        internal string database;

        internal string tabName, keyName;
        internal List<Mess_Three<string, string, object>> selPara;
        internal List<DBOrderby> orderByList;
        internal int topNum = 1000;

        //添加
        protected virtual string AddSql
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //更新
        protected virtual string UpdateSql
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //删除
        protected virtual string DelSql
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //查询
        protected virtual string SelSql
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal object GetKeyValue()
        {
            if (string.IsNullOrWhiteSpace(keyName))
                throw new Exception("没有定义主键！");

            return rpAttrList.Find(r => r.isKey).Property.GetValue(Instance, null);
        }

        /// <summary>
        /// 查找缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fillByValue"></param>
        /// <returns></returns>
        protected string LookUpDataContextMoudleCach(string key,Func<string> fillByValue)
        {
            try
            {
                dataContextMoudleCachLock.EnterUpgradeableReadLock();
                string val;
                if (dataContextMoudleCach.TryGetValue(key, out val))
                {
                    return val;
                }
                if (fillByValue != null)
                {
                    try
                    {
                        dataContextMoudleCachLock.EnterWriteLock();
                        val = fillByValue();
                        if (!string.IsNullOrEmpty(val))
                        {
                            dataContextMoudleCach.Add(key, val);
                        }
                    }
                    finally
                    {
                        dataContextMoudleCachLock.ExitWriteLock();
                    }

                    return val;
                }
            }
            finally
            {
                dataContextMoudleCachLock.ExitUpgradeableReadLock();
            }
            return string.Empty;
        }

        protected void Init()
        {
            rpAttrList = CommFun.GetQuickDataBaseAttr<T>();


            if (rpAttrList.Count <= 0)
                throw new Exception("无法操作空表！");

            tabName = rpAttrList[0].TableName;

            selPara = new List<Mess_Three<string, string, object>>();
            orderByList = new List<DBOrderby>();

            if (!rpAttrList.Exists(r => r.isKey))
                throw new Exception("表需要一个定义一个主键！");

            if(rpAttrList.Where(r=>r.isKey).Count()>1)
            {
                throw new Exception("key太多。");
            }

            keyName = rpAttrList.Find(r => r.isKey).Column;
        }

        protected DataContextMoudle()
        {
            Init();
        }

        protected DataContextMoudle(T instance)
        {
            Init();

            CommFun.Assert(instance != null);
            Instance = instance;
        }


        internal DbType GetDbType(Type tp)
        {
            if (tp == typeof(int))
            {
                return DbType.Int32;
            }
            else if (tp == typeof(Int16))
            {
                return DbType.Int16;
            }
            else if (tp == typeof(Int32))
            {
                return DbType.Int32;
            }
            else if (tp == typeof(Int64))
            {
                return DbType.Int64;
            }
            else if (tp == typeof(long))
            {
                return DbType.Int64;
            }
            else if(tp==typeof(float))
            {
                return DbType.Double;
            }
            else if(tp==typeof(double))
            {
                return DbType.Double;
            }
            else if (tp == typeof(decimal))
            {
                return DbType.Decimal;
            }
            else if (tp == typeof(DateTime))
            {
                return DbType.DateTime;
            }
            else if (tp == typeof(bool))
            {
                return DbType.Boolean;
            }
            else if (tp == typeof(byte))
            {
                return DbType.Byte;
            }
            else if (tp == typeof(string))
            {
                return DbType.String;
            }
            else
            {
                return DbType.Object;
            }

            //return DbType.Object;
        }

        /// <summary>
        /// 为sql语句准备参数
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        private void PrepareSQLParameters(Database db, DbCommand cmd,string sql,params DbParameter[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var param in parameters)
                {
                    if (param.Direction == ParameterDirection.Input)
                    {
                        db.AddInParameter(cmd, param.ParameterName, param.DbType, param.Value);
                    }
                    else if (param.Direction == ParameterDirection.Output)
                    {
                        db.AddOutParameter(cmd, param.ParameterName, param.DbType, param.Size);
                    }
                    else
                    {
                        throw new Exception("parameters is valid.");
                    }
                }
            }

            if (Instance == null)
                return;

            //
            var matchs= sqlParamRegex.Matches(sql);
            List<string> sqlParams = new List<string>();
            foreach (Match m in matchs)
            {
                if (m.Success)
                    sqlParams.Add(m.Groups[1].Value);
            }
            sqlParams = sqlParams.Distinct().ToList();

            if (matchs.Count > 0)
            {
                var rpAttrKeyList = rpAttrList.Where(p => p.isKey).ToList();
                foreach (var x in sqlParams)
                {
                    var r = rpAttrList.Find(p => p.Column.Equals(x.Substring(1), StringComparison.OrdinalIgnoreCase));
                    //草，oledb要参数对齐

                    object val = r.Property.GetValue(Instance, null);

                    if (r.isKey)
                    {
                        if (r.IsEncry)
                        {
                            throw new Exception("主键不能设为加密！");
                        }
                    }
                    else
                    {
                        if (r.IsEncry && val != null)
                        {
                            if (!(val is string))
                                throw new Exception("只有字符串才能加密。");

                            if (!string.IsNullOrEmpty(val.ToString()))
                                val = new EncryHelper().Encrypto(val.ToString());
                        }
                    }

                    var dbtype = GetDbType(r.Property.PropertyType);
                    if (dbtype == DbType.DateTime)
                    {
                        var tval = (DateTime)val;
                        if (tval == default(DateTime))
                        {
                            val = DBNull.Value;
                        }
                    }
                    db.AddInParameter(cmd, x, dbtype, val);
                }
            }
        }

        /// <summary>
        /// 从游标获取目标实体
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private T ParseDataReader(IDataReader dr,ref List<string> cloumnsList)
        {
            T t = new T();

            if (cloumnsList == null)
            {
                cloumnsList = new List<string>();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    cloumnsList.Add(dr.GetName(i));
                }
            }

            cloumnsList.ForEach(
                c =>
                {
                    object val = dr[c];
                    
                    if (val == DBNull.Value)
                    {
                        return;
                    }

                    var r = rpAttrList.Find(p => p.Column.Equals(c, StringComparison.OrdinalIgnoreCase));
                    if (r == null)
                        return;

                    if (r.IsEncry)
                    {
                        if (!string.IsNullOrEmpty(val.ToString()))
                            val = new EncryHelper().Decrypto(val.ToString());
                    }

                    if (r.Property.PropertyType.IsEnum /*&& r.Property.PropertyType.IsEnumDefined(val)*/)
                    {
                        t.SetValue(r.PropertyEx, Enum.Parse(r.Property.PropertyType, val.ToString()));
                    }
                    else
                    {
                        t.SetValue(r.PropertyEx, val);
                    }

                }

                );

            return t;
        }

        private T ParseDataRow(DataRow row)
        {
            T t = new T();

            foreach (DataColumn col in row.Table.Columns)
            {
                object val = row[col];
               
                if (val == DBNull.Value)
                {
                    continue;
                }

                var r = rpAttrList.Find(p => p.Column.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (r == null)
                    continue;
                if (r.IsEncry)
                {
                    if (!string.IsNullOrEmpty(val.ToString()))
                        val = new EncryHelper().Decrypto(val.ToString());
                }

                if (r.Property.PropertyType.IsEnum /*&& r.Property.PropertyType.IsEnumDefined(val)*/)
                {
                    t.SetValue(r.PropertyEx, Enum.Parse(r.Property.PropertyType, val.ToString()));
                }
                else
                {
                    t.SetValue(r.PropertyEx, val);
                }

            }

            return t;
        }

        public int ExecuteNonQuery(string sql, params DbParameter[] parameters)
        {
            Database DB = DatabaseFactory.CreateDatabase(database);

            DbCommand cmd = DB.GetSqlStringCommand(sql);

            PrepareSQLParameters(DB, cmd, sql, parameters);

            return DB.ExecuteNonQuery(cmd);
        }

        private bool CachResult(out string cachKey)
        {
            cachKey = string.Empty;

            T t = Instance==null?new T():Instance;

            bool cach =t is IDBCachAble
               && selPara.Count == 0;

            if (cach)
            {
                cachKey = ((IDBCachAble)t).GetCollectCachKey();

            }

            return cach;
        }

        public object ExecuteScalar(string sql, params DbParameter[] parameters)
        {
            Database DB = DatabaseFactory.CreateDatabase(database);
            var cmd = DB.GetSqlStringCommand(sql);

            this.PrepareSQLParameters(DB,cmd,sql,parameters);

            return DB.ExecuteScalar(cmd);
        }

        public T ExecuteEntity()
        {
            return ExecuteList().FirstOrDefault();
        }

        #region 执行语句
        internal DataTable ExecuteSQLTable(string sql,params DbParameter[] parameters)
        {
            Database DB = DatabaseFactory.CreateDatabase(database);
            var cmd = DB.GetSqlStringCommand(sql);

            PrepareSQLParameters(DB, cmd, sql, parameters);

            var ds = DB.ExecuteDataSet(cmd);
            if (ds.Tables.Count > 0)
            {
                var table0 = ds.Tables[0];
                return table0;
            }

            return null;
        }

        public List<T> ExecuteSQL(string sql,params DbParameter[] parameters)
        {
            List<T> result = new List<T>();
            Database DB = DatabaseFactory.CreateDatabase(database);
            var cmd = DB.GetSqlStringCommand(sql);

            PrepareSQLParameters(DB, cmd, sql, parameters);

            var ds = DB.ExecuteDataSet(cmd);
            if (ds.Tables.Count > 0)
            {
                var table0 = ds.Tables[0];
                result = table0.AsEnumerable().Select(p => ParseDataRow(p)).ToList();
            }

            return result;
        }

        public List<T> ExecuteProc(string proc,DbParameter[] parameters,ref Dictionary<string,object> outPutValues)
        {
            List<T> result = new List<T>();
            Database DB = DatabaseFactory.CreateDatabase(database);
            
            var cmd = DB.GetStoredProcCommand(proc);

            if (parameters != null && parameters.Length > 0)
            {
                foreach (var param in parameters)
                {
                    
                    if (param.Direction == ParameterDirection.Input)
                    {
                        //DB.AddInParameter(cmd, param.ParameterName, param.DbType, param.Value);
                        (cmd as System.Data.SqlClient.SqlCommand).Parameters.Add(param);
                    }
                    else if (param.Direction == ParameterDirection.Output)
                    {
                        DB.AddOutParameter(cmd, param.ParameterName, param.DbType, param.Size);
                    }
                    else
                    {
                        throw new Exception("parameters is valid.");
                    }
                }
            }

            var ds=DB.ExecuteDataSet(cmd);
            if (ds.Tables.Count > 0)
            {
                var table0=ds.Tables[0];
                result = table0.AsEnumerable().Select(p => ParseDataRow(p)).ToList();
            }

            #region 抛弃连接对象
            //using (IDataReader dr = DB.ExecuteReader(cmd))
            //{
            //    List<string> cols = null;
            //    while (dr.Read())
            //    {
            //        T t = ParseDataReader(dr,ref cols);
            //        result.Add(t);
            //    }              
            //}
            #endregion

            if (outPutValues != null && outPutValues.Count > 0)
            {
                for (int i = 0; i < outPutValues.Count; i++)
                {
                    var pair = outPutValues.ElementAt(i);
                    object val = DB.GetParameterValue(cmd, pair.Key);
                    outPutValues[pair.Key] = val;
                }
            }

            return result;
        }

        #endregion

        public List<T> ExecuteList()
        {
            string cachKey;
            bool cach = CachResult(out cachKey);
            if (cach)
            {
                object o = MemCach.GetCach(cachKey);
                if (o != null)
                {
                    return (List<T>)o;
                }
            }

            List<T> result = new List<T>();

            Database DB = DatabaseFactory.CreateDatabase(database);

            StringBuilder sb = new StringBuilder();
            selPara.ForEach(s =>
            {
                string[] sus = s.First.Split('$');
                if (s.Second.Trim().Equals("in"))
                {
                    sb.AppendFormat(" And {0}{1}({2})", sus[0], s.Second, s.Thrid);
                }
                else
                {
                    
                    sb.AppendFormat(" And {0}{1}@{0}{2}", sus[0], s.Second, (sus.Length > 1 ? sus[1] : ""));
                }
            });

            if (orderByList != null && orderByList.Count > 0)
            {
                sb.AppendLine(" order by ");
                foreach (var orderby in orderByList)
                {
                    sb.AppendFormat("{0} {1},",orderby.OrderbyColumnName,orderby.OrderbyDirection.ToString());
                }
                sb.Remove(sb.Length-1, 1);
            }
            DbCommand cmd = DB.GetSqlStringCommand(string.Format(SelSql, sb.ToString()));

            selPara.ForEach(s =>
            {
                if (s.Second.Trim().Equals("in"))
                    return;

                DbParameter para = cmd.CreateParameter();
                para.DbType = GetDbType(s.Thrid.GetType());
                string[] sus = s.First.Split('$');
                para.ParameterName = "@" + sus[0] + (sus.Length > 1 ? sus[1] : "");
                para.Value = s.Thrid;

                cmd.Parameters.Add(para);
            });

            var ds = DB.ExecuteDataSet(cmd);
            if (ds.Tables.Count > 0)
            {
                var table0 = ds.Tables[0];
                result = table0.AsEnumerable().Select(p => ParseDataRow(p)).ToList();
            }

            ////这里可以优化下，采用断线对象
            //using (IDataReader dr = DB.ExecuteReader(cmd))
            //{
            //    List<string> cols = null;
            //    while (dr.Read())
            //    {
            //        T t = ParseDataReader(dr,ref cols);
            //        result.Add(t);
            //    }
            //}

            if (cach)
            {
                MemCach.AddCach(cachKey, result, 1000);
            }

            return result;
        }

        public Int64 Add()
        {
            var result = Int64.Parse(ExecuteScalar(AddSql).ToString());

            if (result>0)
            {
                TryClearCach();
            }

            return result;
        }

        private void TryClearCach()
        {
            string cachKey;
            if (CachResult(out cachKey))
            {
                MemCach.RemoveCachItem(cachKey);
            }
        }

        public bool Del()
        {
            bool result = ExecuteNonQuery(DelSql) > 0;

            if (result)
            {
                TryClearCach();
            }

            return result;
        }

        public bool Update()
        {
            bool result = ExecuteNonQuery(UpdateSql) > 0;

            if (result)
            {
                TryClearCach();
            }

            return result;
        }

        public virtual bool Update(params string[] colArray)
        {
            throw new NotImplementedException("未实现");
        }

        public virtual bool NotUpdate(params string[] colArray)
        {
            throw new NotImplementedException("未实现");
        }

        public bool Update(params Expression<Func<T, object>>[] colExpressaArray)
        {
            return Update(colExpressaArray.Select(p => this.GetColNameFromExpression(p.Body.ToString())).ToArray());
        }

        public bool NotUpdate(params Expression<Func<T, object>>[] colExpressaArray)
        {
            return NotUpdate(colExpressaArray.Select(p => this.GetColNameFromExpression(p.Body.ToString())).ToArray());
        }

        public virtual object Max(string column)
        {
            string maxSql = string.Format("select max({0}) from {1}(nolock)", column, tabName);
            return ExecuteScalar(maxSql);
        }

        public virtual object Min(string column)
        {
            string minSql = string.Format("select min({0}) from {1}(nolock)", column, tabName);
            return ExecuteScalar(minSql);
        }
    }
}
