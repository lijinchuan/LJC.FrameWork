using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using LJC.FrameWork.Comm;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public class DataContextMoudle<T> where T : new()
    {
        /// <summary>
        /// 实例
        /// </summary>
        protected T Instance;
        protected List<ReportAttr> rpAttrList;
        protected string _selSql;
        internal string database;

        internal string tabName, keyName;
        internal List<Mess_Three<string, string, object>> selPara;
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

        protected void Init()
        {
            rpAttrList = CommFun.GetQuickDataBaseAttr<T>();

            if (rpAttrList.Count <= 0)
                throw new Exception("无法操作空表！");

            //object[] os = typeof(T).GetCustomAttributes(typeof(ReportAttr), true);
            //if (os.Length > 0)
            //{
            //    tabName = ((ReportAttr)os[0]).TableName;
            //}
            //else
            //{
            //    tabName = typeof(T).Name;
            //}
            tabName = rpAttrList[0].TableName;

            //_selSql = string.Format(" {0} from {1}(nolock) where 1=1",
            //    string.Join(",", rpAttrList.Select(r => "["+r.Column+"]").ToList()),tabName);

            selPara = new List<Mess_Three<string, string, object>>();

            if (!rpAttrList.Exists(r => r.isKey))
                throw new Exception("表需要一个定义一个主键！");

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

        public int ExecuteNonQuery(string sql)
        {
            Database DB = DatabaseFactory.CreateDatabase(database);

            DbCommand cmd = DB.GetSqlStringCommand(sql);

            //草，oledb要参数对齐
            rpAttrList.Where(p => !p.isKey).ToList().ForEach(p =>
            {
                if (sql.IndexOf("@" + p.Column) > -1)
                {
                    object val = p.Property.GetValue(Instance, null);

                    if (p.IsEncry && val != null)
                    {
                        if (!(val is string))
                            throw new Exception("只有字符串才能加密。");

                        if (!string.IsNullOrEmpty(val.ToString()))
                            val = new EncryHelper().Encrypto(val.ToString());
                    }

                    DB.AddInParameter(cmd, "@" + p.Column, GetDbType(p.Property.PropertyType), val);
                }
            });

            rpAttrList.Where(p => p.isKey).ToList().ForEach(p =>
            {
                if (sql.IndexOf("@" + p.Column) > -1)
                {
                    if (p.IsEncry)
                    {
                        throw new Exception("主键不能设为加密！");
                    }

                    object val = p.Property.GetValue(Instance, null);
                    DB.AddInParameter(cmd, "@" + p.Column, GetDbType(p.Property.PropertyType), val);
                }
            });

            return DB.ExecuteNonQuery(cmd);
        }

        private bool CachResult(out string cachKey)
        {
            cachKey = string.Empty;

            //bool cach = this.Instance != null && this.Instance is IDBCachAble
            //   && selPara.Count == 0;
            T t = Instance==null?new T():Instance;

            bool cach =t is IDBCachAble
               && selPara.Count == 0;

            if (cach)
            {
                cachKey = ((IDBCachAble)t).GetCollectCachKey();

            }

            return cach;
        }

        public object ExecuteScalar(string sql)
        {
            Database DB = DatabaseFactory.CreateDatabase(database);
            var cmd = DB.GetSqlStringCommand(sql);

            //草，oledb要参数对齐
            rpAttrList.Where(p => !p.isKey).ToList().ForEach(p =>
            {
                if (sql.IndexOf(string.Concat("@" , p.Column)) > -1)
                {
                    object val = p.Property.GetValue(Instance, null);

                    if (p.IsEncry && val != null)
                    {
                        if (!(val is string))
                            throw new Exception("只有字符串才能加密。");

                        if (!string.IsNullOrEmpty(val.ToString()))
                            val = new EncryHelper().Encrypto(val.ToString());
                    }

                    DB.AddInParameter(cmd, string.Concat("@", p.Column), GetDbType(p.Property.PropertyType), val);
                }
            });

            rpAttrList.Where(p => p.isKey).ToList().ForEach(p =>
            {
                if (sql.IndexOf("@" + p.Column) > -1)
                {
                    if (p.IsEncry)
                    {
                        throw new Exception("主键不能设为加密！");
                    }

                    object val = p.Property.GetValue(Instance, null);
                    DB.AddInParameter(cmd, "@" + p.Column, GetDbType(p.Property.PropertyType), val);
                }
            });

            return DB.ExecuteScalar(cmd);
        }

        public T ExecuteEntity()
        {
            return ExecuteList().FirstOrDefault();
        }

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
                sb.AppendFormat(" And {0}{1}@{0}{2}", sus[0], s.Second, (sus.Length > 1 ? sus[1] : ""));
            });

            DbCommand cmd = DB.GetSqlStringCommand(string.Format(SelSql, sb.ToString()));

            selPara.ForEach(s =>
            {
                DbParameter para = cmd.CreateParameter();
                para.DbType = GetDbType(s.Thrid.GetType());
                string[] sus = s.First.Split('$');
                para.ParameterName = "@" + sus[0] + (sus.Length > 1 ? sus[1] : "");
                para.Value = s.Thrid;

                cmd.Parameters.Add(para);
            });

            using (IDataReader dr = DB.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    T t = new T();

                    rpAttrList.ForEach(
                        r =>
                        {
                            object val = dr[r.Column];
                            try
                            {
                                if (r.IsEncry)
                                {
                                    if (!string.IsNullOrEmpty(val.ToString()))
                                        val = new EncryHelper().Decrypto(val.ToString());
                                }

                                if (r.Property.PropertyType.IsEnum && r.Property.PropertyType.IsEnumDefined(val))
                                {
                                    r.Property.SetValue(t, Enum.Parse(r.Property.PropertyType, val.ToString()), null);
                                }
                                else
                                {
                                    r.Property.SetValue(t, val, null);
                                }
                            }
                            catch
                            {
                                //throw;
                            }

                        }

                        );
                    result.Add(t);
                }
            }

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
