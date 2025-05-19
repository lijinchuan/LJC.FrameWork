using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using LJC.FrameWork.Data.QuickDataBase;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Text.RegularExpressions;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.Data
{
    /// <summary>
    /// 内存数据库操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachDataContextMoudel<T> where T:new()
    {
        private List<ReportAttr> rpAttrList;
        //自增主键或者是唯一主键
        private ReportAttr keyColumn;
        private DataTable cachTable;
        //private static DataSet cachDataSet;
        private int maxKeyID;
        private Dictionary<string, int> setFlushTBColumnsOrderDic = null;
        private bool needReColumnWhenFlushTB = true;
        private object innerCodeLock = new object();

        const string StateColumnName = "state";
        const string TablenameColumnName = "tbname";

        private ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();

        public int Count()
        {
            if (cachTable == null)
                return 0;

            return cachTable.Rows.Count;
        }

        static CachDataContextMoudel()
        {
            //cachDataSet = new DataSet("cachDataTableSet");
        }

        private bool _canTrancateTable = false;
        /// <summary>
        /// 是否允许trancateTable，默认不许
        /// </summary>
        public bool CanTrancateTable
        {
            get
            {
                return _canTrancateTable;
            }
            set
            {
                _canTrancateTable = value;
            }
        }

        private DataTable CreateCachTable()
        {
            DataTable tb = new DataTable(typeof(T).FullName);

            rpAttrList.ForEach(p =>
            {
                DataColumn column = new DataColumn(p.Column);
                if (p.Property.PropertyType.IsEnum)
                {
                    column.DataType = typeof(int);
                }
                else
                {
                    column.DataType = p.Property.PropertyType;
                }
                tb.Columns.Add(column);
            });

            tb.Columns.Add(new DataColumn
            {
                ColumnName=StateColumnName,
                DataType=typeof(int)
            });

            //if (TableAttr.IsSplitTable)
            //{
            //    tb.Columns.Add(new DataColumn
            //    {
            //        ColumnName = TablenameColumnName,
            //        DataType = typeof(string)
            //    });
            //}

            return tb;
        }

        private T ConvertTo(DataRow row)
        {
            T t = new T();

            foreach (ReportAttr att in rpAttrList)
            {
                if (row[att.Column] is DBNull)
                    continue;
                att.Property.SetValue(t, row[att.Column],null);
            }

            return t;
        }

        public T FirstOrDefault()
        {
            if (cachTable.Rows.Count == 0)
                return default(T);

            return ConvertTo(cachTable.Rows[0]);
        }

        public void Clear()
        {
            readWriteLock.EnterWriteLock();

            try
            {
                cachTable.Clear();
            }
            finally
            {
                readWriteLock.ExitWriteLock(); 
            }
        }

        public CachDataContextMoudel()
        {
            rpAttrList = Comm.CommFun.GetQuickDataBaseAttr<T>();
            if (rpAttrList.Count == 0)
                throw new Exception("无法生成缓存数据表对象！");

            keyColumn = rpAttrList.Find(p => p.isKey);
           
            if (keyColumn == null)
                throw new Exception("缺少表自增主键或者唯一主键，无法创建缓存数据表对象。");

            readWriteLock.EnterWriteLock();
            try
            {
                //for (int i = 0; i < cachDataSet.Tables.Count; i++)
                //{
                //    if (cachDataSet.Tables[i].TableName.Equals(typeof(T).FullName))
                //    {
                //        cachTable = cachDataSet.Tables[i];
                //    }
                //}

                if (cachTable == null)
                {
                    cachTable = CreateCachTable();
                    //cachDataSet.Tables.Add(cachTable);
                }

                if (cachTable.Rows.Count > 0)
                {
                    maxKeyID = int.Parse(cachTable.Rows[cachTable.Rows.Count - 1][keyColumn.Column].ToString());
                }
                else
                {
                    maxKeyID = 0;
                }
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }
        }

        public bool AddUpate(T item)
        {
            bool result = false;
            readWriteLock.EnterWriteLock();

            try
            {
                DataRow newRow = cachTable.NewRow();
                foreach (var x in rpAttrList)
                {
                    newRow[x.Column] = x.Property.GetValue(item, null);
                }
                newRow[StateColumnName] = (int)DataTableRowState.Update;
                cachTable.Rows.Add(newRow);
                result = true;
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }

            return result;
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                return;

            items.ToList().ForEach(p =>
            {
                Add(p);
            });
        }

        public bool Add(T item)
        {
            bool result = false;
            readWriteLock.EnterWriteLock();

            try
            {
                DataRow newRow = cachTable.NewRow();
                foreach (var x in rpAttrList)
                {
                    if (x.isKey)
                    {
                        newRow[keyColumn.Column] = ++maxKeyID;
                        x.Property.SetValue(item, maxKeyID, null);
                    }
                    else if (x.Property.PropertyType.IsEnum)
                    {
                        newRow[x.Column] = (int)x.Property.GetValue(item, null);
                    }
                    else
                    {
                        newRow[x.Column] = x.Property.GetValue(item, null);
                    }
                }
                newRow[StateColumnName] = (int)DataTableRowState.Add;
                //if (TableAttr.IsSplitTable)
                //{
                //    newRow[TablenameColumnName] = typeof(T).GetProperty("SplitTbName").GetValue(item, null);
                //}
                //else
                //{
                //    newRow[TablenameColumnName] = this.TabName;
                //}
                cachTable.Rows.Add(newRow);
                result = true;
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }

            return result;
        }

        private static DataTableRowState  GetRowState(DataRow row)
        {
            return (DataTableRowState)row[StateColumnName];
        }

        private static void SetRowState(DataRow row, DataTableRowState state)
        {
            row[StateColumnName] = state;
        }

        private DataRow GetRow(int rowKey)
        {
            foreach (DataRow row in cachTable.Rows)
            {
                if ((int)row[keyColumn.Column] == rowKey)
                {
                    return row;
                }
            }
            return null;
        }

        public T Find(Predicate<T> express)
        {
            try
            {
                readWriteLock.EnterReadLock();
                foreach (DataRow row in cachTable.Rows)
                {
                    T t = ConvertTo(row);
                    if (express.Invoke(t))
                    {
                        if (GetRowState(row) == DataTableRowState.Del)
                            return default(T);
                        return t;
                    }
                }

                return default(T);
            }
            finally
            {
                readWriteLock.ExitReadLock();
            }
        }

        public IEnumerable<T> Last(Func<T,bool> func)
        {
            try
            {
                readWriteLock.EnterReadLock();
                if (cachTable.Rows.Count == 0)
                {
                    yield break;
                }
                var top = 1;
                var count = cachTable.Rows.Count;
                while (top <= count)
                {
                    var item = ConvertTo(cachTable.Rows[count - top]);
                    if (func(item))
                    {
                        yield return item;
                    }
                    else
                    {
                        yield break;
                    }
                    top++;
                }
            }
            finally
            {
                readWriteLock.ExitReadLock();
            }
        }

        public IEnumerable<T> Where(Predicate<T> express)
        {
            List<T> result = new List<T>();

            try
            {
                readWriteLock.EnterReadLock();
                foreach (DataRow row in cachTable.Rows)
                {
                    if (GetRowState(row) == DataTableRowState.Del)
                    {
                        continue;
                    }
                    T t = ConvertTo(row);
                    if (express.Invoke(t))
                    {
                        result.Add(t);
                    }
                }

                return result;
            }
            finally
            {
                readWriteLock.ExitReadLock();
            }
        }

        public List<T> ExecuteList()
        {
            try
            {
                readWriteLock.EnterReadLock();
                List<T> result = new List<T>();
                foreach (DataRow row in cachTable.Rows)
                {
                    if (GetRowState(row) == DataTableRowState.Del)
                    {
                        continue;
                    }
                    T t = ConvertTo(row);
                    result.Add(t);
                }
                return result;
            }
            finally
            {
                readWriteLock.ExitReadLock();
            }
        }

        public bool Update(T item)
        {
            bool result = false;

            try
            {
                readWriteLock.EnterWriteLock();

                int id = 0;
                if (!int.TryParse(keyColumn.Property.GetValue(item, null).ToString(), out id))
                {
                    throw new Exception("主键值为空，更新数据！");
                }

                DataRow row = GetRow(id);
                if (row != null && GetRowState(row) != DataTableRowState.Del)
                {
                    foreach (ReportAttr att in rpAttrList)
                    {
                        if (!att.isKey)
                        {
                            row[att.Column] = att.Property.GetValue(item, null);
                        }
                    }
                    if (GetRowState(row) == DataTableRowState.Unchanged)
                    {
                        row[StateColumnName] = (int)DataTableRowState.Update;
                    }
                    result = true;
                }
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }
            
            return result;
        }

        public bool Del(Predicate<T> express)
        {
            bool result = false;

            try
            {
                readWriteLock.EnterWriteLock();

                List<DataRow> delrows = new List<DataRow>();

                foreach (DataRow row in cachTable.Rows)
                {
                    T t = ConvertTo(row);
                    if (express.Invoke(t))
                    {
                        if (GetRowState(row) == DataTableRowState.Add)
                        {
                            //cachTable.Rows.Remove(row);
                            delrows.Add(row);
                        }
                        else
                        {
                            row[StateColumnName] = (int)DataTableRowState.Del;
                        }
                        result = true;
                    }
                }

                delrows.ForEach(p => cachTable.Rows.Remove(p));
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }

            return result;
        }

        public bool Del(int rowKey)
        {
            bool result = false;

            readWriteLock.EnterWriteLock();

            try
            {
                DataRow row = GetRow(rowKey);
                if (row != null)
                {
                    if (GetRowState(row) == DataTableRowState.Add)
                    {
                        cachTable.Rows.Remove(row);
                    }
                    else
                    {
                        row[StateColumnName] = (int)DataTableRowState.Del;
                    }
                    result = true;
                }
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }

            return result;
        }

        private ReportAttr _tableAttr = null;
        internal ReportAttr TableAttr
        {
            get
            {
                if (_tableAttr == null)
                {
                    object[] os = typeof(T).GetCustomAttributes(typeof(ReportAttr), true);
                    _tableAttr = (ReportAttr)os[0];
                }

                return _tableAttr;
            }
        }

        private string _tabname = null;
        public string TabName
        {
            get
            {
                if (_tabname != null)
                    return _tabname;

                _tabname = TableAttr.TableName;

                return _tabname;
            }
            private set
            {
                _tabname = value;
            }
        }

        private bool SqlServerFlush(DataContextMoudle<T> dataContext)
        {
            Dictionary<string, object> retdic = new Dictionary<string, object>();

            lock (innerCodeLock)
            {
                if (needReColumnWhenFlushTB && setFlushTBColumnsOrderDic == null)
                {
                    needReColumnWhenFlushTB = false;

                    setFlushTBColumnsOrderDic = new Dictionary<string, int>();
                    string sql = string.Format("declare @t Mulitinsertmidtable${0};select top 0 * from @t;", TabName);

                    DataTable dttable = dataContext.ExecuteSQLTable(sql, null);
                    for (int i = 0; i < dttable.Columns.Count; i++)
                    {
                        setFlushTBColumnsOrderDic.Add(dttable.Columns[i].ColumnName.ToLower(), i);
                        if (!needReColumnWhenFlushTB && !string.Equals(cachTable.Columns[i].ColumnName, dttable.Columns[i].ColumnName, StringComparison.OrdinalIgnoreCase))
                        {
                            needReColumnWhenFlushTB = true;
                        }
                    }
                }
            }

            var param = new System.Data.SqlClient.SqlParameter("@dt", System.Data.SqlDbType.Structured, -1);
            if (needReColumnWhenFlushTB)
            {
                param.Value = this.cachTable.DefaultView.ToTable(false, setFlushTBColumnsOrderDic.Keys.ToArray());
            }
            else
            {
                param.Value = this.cachTable;
            }
            dataContext.ExecuteProc(string.Format("{0}_mulitinsert", TabName)
                ,
                new System.Data.Common.DbParameter[]
                {
                    param
                }
                , ref retdic);

            cachTable.Clear();

            return true;

        }

       #region
        private void Append(StringBuilder sb,object o)
        {
            sb.AppendFormat("{0},", o);
        }

        private void AppendBool(StringBuilder sb, object o)
        {
            sb.AppendFormat("{0},", o.Equals(true) ? 1 : 0);
        }

        private void AppendStr(StringBuilder sb, object o)
        {
            if (o == null)
            {
                sb.Append("'',");
            }
            else
            {
                sb.Append(string.Format("'{0}',", MySql.Data.MySqlClient.MySqlHelper.EscapeString(o.ToString())));
            }
        }

        private void AppendTime(StringBuilder sb, object o)
        {
            sb.AppendFormat("'{0}',", ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private bool MysqlServerFlush(DataContextMoudle<T> dataContext, string insertDuplicate = null)
        {
            try
            {
                readWriteLock.EnterWriteLock();
                List<Action<StringBuilder, object>> acts = new List<Action<StringBuilder, object>>();
                List<int> columnIndex = new List<int>();
                List<string> sqlColumns = new List<string>();
                int idx = -1;
                foreach (DataColumn column in this.cachTable.Columns)
                {
                    idx++;
                    var r = rpAttrList.Find(p => p.Column.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase));
                    if (r == null || r.isKey)
                    {
                        continue;
                    }
                    sqlColumns.Add(r.Column);
                    columnIndex.Add(idx);
                    if (column.DataType.Name.Equals("int32", StringComparison.OrdinalIgnoreCase)
                                || column.DataType.Name.Equals("int64", StringComparison.OrdinalIgnoreCase)
                                || column.DataType.Name.Equals("long", StringComparison.OrdinalIgnoreCase)
                                || column.DataType.Name.Equals("bit", StringComparison.OrdinalIgnoreCase)
                                || column.DataType.Name.IndexOf("float", StringComparison.OrdinalIgnoreCase) > -1
                                || column.DataType.Name.IndexOf("real", StringComparison.OrdinalIgnoreCase) > -1
                                || column.DataType.Name.IndexOf("money", StringComparison.OrdinalIgnoreCase) > -1
                                || column.DataType.Name.IndexOf("timestamp", StringComparison.OrdinalIgnoreCase) > -1
                                || column.DataType.Name.IndexOf("money", StringComparison.OrdinalIgnoreCase) > -1
                            )
                    {
                        acts.Add(Append);
                    }
                    else if (column.DataType.Name.Equals("boolean", StringComparison.OrdinalIgnoreCase)
                               || column.DataType.Name.Equals("bool", StringComparison.OrdinalIgnoreCase))
                    {
                        acts.Add(AppendBool);
                    }
                    else if (column.DataType.Name.Equals("datetime", StringComparison.OrdinalIgnoreCase))
                    {
                        acts.Add(AppendTime);
                    }
                    else
                    {
                        acts.Add(AppendStr);
                    }
                }

                int page = 0;
                int maxcount = 5000;

                while (true)
                {
                    var y = TableAttr.IsSplitTable ? (from x in cachTable.AsEnumerable().Skip(page * maxcount).Take(maxcount)
                                                      group x by x[TablenameColumnName] into m
                                                      select m) : (
                            from x in cachTable.AsEnumerable().Skip(page * maxcount).Take(maxcount)
                            group x by TableAttr.TableName into m
                            select m
                            );

                    if (y.Count() == 0)
                    {
                        break;
                    }

                    page++;

                    foreach (var rowgp in y)
                    {
                        StringBuilder sbaddsql = new StringBuilder();
                        //sbaddsql.AppendFormat("SET SQL_SAFE_UPDATES = 0;Insert into {0} ({1}) values", string.Concat("`", rowgp.Key, "`"), string.Join(",", sqlColumns.Select(p => string.Concat("`", p, "`"))));
                        sbaddsql.AppendFormat("SET SQL_SAFE_UPDATES = 0;Insert {0}into {1} ({2}) values", "IGNORE".Equals(insertDuplicate, StringComparison.OrdinalIgnoreCase) ? "IGNORE " : string.Empty, string.Concat("`", rowgp.Key, "`"), string.Join(",", sqlColumns.Select(p => string.Concat("`", p, "`"))));
                        var sbaddsqlolen = sbaddsql.Length;

                        StringBuilder sbinsertsql = new StringBuilder();
                        sbinsertsql.AppendFormat("SET SQL_SAFE_UPDATES = 0;update {0} a inner join (", rowgp.Key);
                        bool isInstFirstRow = true;
                        var sbinsertsqlolen = sbinsertsql.Length;

                        foreach (var row in rowgp)
                        {
                            if (GetRowState(row) == DataTableRowState.Add)
                            {

                                sbaddsql.Append("(");
                                for (int i = 0; i < acts.Count; i++)
                                {
                                    if (row[columnIndex[i]] == DBNull.Value)
                                    {
                                        sbaddsql.Append("NULL,");
                                    }
                                    else
                                    {
                                        acts[i](sbaddsql, row[columnIndex[i]]);
                                    }
                                }
                                sbaddsql.Remove(sbaddsql.Length - 1, 1);
                                sbaddsql.Append("),");
                                SetRowState(row, DataTableRowState.Del);
                            }
                            else if (GetRowState(row) == DataTableRowState.Update)
                            {
                                if (isInstFirstRow)
                                {
                                    sbinsertsql.Append("select ");
                                    sbinsertsql.AppendFormat("{0} as {1},", row[keyColumn.Column], keyColumn.Column);
                                }
                                else
                                {
                                    sbinsertsql.Append(" union all select ");
                                    sbinsertsql.AppendFormat("{0},", row[keyColumn.Column]);
                                }

                                for (int i = 0; i < acts.Count; i++)
                                {
                                    if (row[columnIndex[i]] == DBNull.Value)
                                    {
                                        sbinsertsql.AppendFormat("NULL ");
                                        if (isInstFirstRow)
                                        {
                                            sbinsertsql.Append(" as ");
                                            sbinsertsql.Append(sqlColumns[i]);
                                        }
                                        sbinsertsql.Append(",");
                                    }
                                    else
                                    {
                                        acts[i](sbinsertsql, row[columnIndex[i]]);
                                        if (isInstFirstRow)
                                        {
                                            sbinsertsql.Remove(sbinsertsql.Length - 1, 1);
                                            sbinsertsql.Append(" as ");
                                            sbinsertsql.Append(sqlColumns[i]);
                                            sbinsertsql.Append(",");
                                        }
                                    }
                                }

                                sbinsertsql.Remove(sbinsertsql.Length - 1, 1);
                                SetRowState(row, DataTableRowState.Del);

                                if (isInstFirstRow)
                                    isInstFirstRow = false;
                            }
                        }

                        try
                        {
                            if (sbaddsql.Length > sbaddsqlolen)
                            {
                                sbaddsql.Remove(sbaddsql.Length - 1, 1);

                                if (!string.IsNullOrWhiteSpace(insertDuplicate) && !"IGNORE".Equals(insertDuplicate, StringComparison.OrdinalIgnoreCase))
                                {
                                    sbaddsql.AppendFormat(" ON DUPLICATE KEY {0}", insertDuplicate);
                                }

                                dataContext.ExecuteNonQuery(sbaddsql.ToString());
                            }

                            if (sbinsertsql.Length > sbinsertsqlolen)
                            {
                                sbinsertsql.Append(") b ");
                                sbinsertsql.AppendFormat(" on a.{0}=b.{0} set ", keyColumn.Column);

                                foreach (var col in sqlColumns)
                                {
                                    sbinsertsql.AppendFormat("a.{0}=b.{0},", col);
                                }

                                sbinsertsql.Remove(sbinsertsql.Length - 1, 1);
                                sbinsertsql.Append(" where 1=1;");

                                dataContext.ExecuteNonQuery(sbinsertsql.ToString());
                            }

                        }
                        catch (Exception ex)
                        {
                            if (Regex.IsMatch(ex.Message, string.Format(@"table '[^']*\.?{0}' doesn't exist", rowgp.Key), RegexOptions.IgnoreCase))
                            {
                                dataContext.ExecuteNonQuery(string.Format("CREATE TABLE IF NOT EXISTS `{0}` select * from {1} limit 0,0;alter table {0} change {2} {2} bigint not null auto_increment primary key;", rowgp.Key, this.TabName, keyColumn.Column));
                                dataContext.ExecuteNonQuery(sbaddsql.ToString());

                            }
                            else
                            {
                                if (ex.Data != null)
                                {
                                    ex.Data.Add("addsql", sbaddsql.ToString());
                                    ex.Data.Add("insertsql", sbinsertsql);
                                }
                                throw ex;
                            }
                        }
                    }
                }

                cachTable.Clear();

                return true;
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }
        }
        #endregion

        public bool FlushToTable(string database = "DefaultDB")
        {
            return FlushToTable(database, null);
        }

        /// <summary>
        /// 临时数据刷进数据库，仅支持sqlserver
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool FlushToTable(string database = "DefaultDB", string insertDuplicate = null)
        {
            if (Count() == 0)
            {
                return true;
            }

            if (string.IsNullOrEmpty(TabName))
                throw new Exception("未定义表名");

            DataContextMoudle<T> dataContext = DataContextMoudelFactory<T>.GetDataContext(database);

            if (dataContext is MSSqlDataContextMoudle<T>)
            {
                return SqlServerFlush(dataContext);
            }
            else if (dataContext is MySqlDataContextMoudle<T>)
            {
                return MysqlServerFlush(dataContext, insertDuplicate);
            }

            throw new NotImplementedException();
        }

        public void TrancateTable(string database = "DefaultDB")
        {
            if (!CanTrancateTable)
                throw new Exception("不允许执行trancateTable操作。");

            DatabaseHelper.ExecuteNonQuery(database, string.Format("truncate table {0}", this.TabName));
            CanTrancateTable = false;
        }

        public void WriteToXml(string xmlpath)
        {
            try
            {
                readWriteLock.EnterReadLock();
                SerializerHelper.SerializerToXML<List<T>>(this.ExecuteList(), xmlpath, true);
            }
            finally
            {
                readWriteLock.ExitReadLock();
            }
        }
    }
}
