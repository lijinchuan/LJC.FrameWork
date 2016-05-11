using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using LJC.FrameWork.Data.QuickDataBase;

namespace LJC.FrameWork.Data
{
    /// <summary>
    /// 内存数据库操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachDataContextMoudel<T> where T:new()
    {
        private List<ReportAttr> rpAttrList;
        private ReportAttr keyColumn;
        private DataTable cachTable;
        private static DataSet cachDataSet;
        private int maxKeyID;

        private ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();

        static CachDataContextMoudel()
        {
            cachDataSet = new DataSet("cachDataTableSet");
        }

        private DataTable CreateCachTable()
        {
            DataTable tb = new DataTable(typeof(T).FullName);

            rpAttrList.ForEach(p =>
            {
                DataColumn column = new DataColumn(p.Column);
                column.DataType = p.Property.PropertyType;
                tb.Columns.Add(column);
            });

            return tb;
        }

        private T ConvertTo(DataRow row)
        {
            T t = new T();

            foreach (ReportAttr att in rpAttrList)
            {
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
                throw new Exception("缺少表主键，无法创建缓存数据表对象。");

            readWriteLock.EnterWriteLock();
            try
            {
                for (int i = 0; i < cachDataSet.Tables.Count; i++)
                {
                    if (cachDataSet.Tables[i].TableName.Equals(typeof(T).FullName))
                    {
                        cachTable = cachDataSet.Tables[i];
                    }
                }

                if (cachTable == null)
                {
                    cachTable = CreateCachTable();
                    cachDataSet.Tables.Add(cachTable);
                }

                if (cachTable.Rows.Count > 0)
                {
                    maxKeyID = (int)cachTable.Rows[cachTable.Rows.Count - 1][keyColumn.Column];
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
                    else
                        newRow[x.Column] = x.Property.GetValue(item, null);
                }

                cachTable.Rows.Add(newRow);
                result = true;
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }

            return result;
        }

        private DataRow GetRow(int rowKey)
        {
            foreach (DataRow row in cachTable.Rows)
            {
                if ((int)row[keyColumn.Column] == rowKey)
                    return row;
            }
            return null;
        }

        public T Find(Predicate<T> express)
        {
            foreach (DataRow row in cachTable.Rows)
            {
                T t = ConvertTo(row);
                if (express.Invoke(t))
                {
                    return t;
                }
            }

            return default(T);
        }

        public IEnumerable<T> Where(Predicate<T> express)
        {
            List<T> result = new List<T>();

            foreach (DataRow row in cachTable.Rows)
            {
                T t = ConvertTo(row);
                if (express.Invoke(t))
                {
                    result.Add(t);
                }
            }

            return result;
        }

        public List<T> ExecuteList()
        {
            List<T> result = new List<T>();
            foreach (DataRow row in cachTable.Rows)
            {
                T t = ConvertTo(row);
                result.Add(t);
            }
            return result;
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
                if (row != null)
                {
                    foreach (ReportAttr att in rpAttrList)
                    {
                        if (!att.isKey)
                        {
                            row[att.Column] = att.Property.GetValue(item, null);
                        }
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

                foreach (DataRow row in cachTable.Rows)
                {
                    T t = ConvertTo(row);
                    if (express.Invoke(t))
                    {
                        cachTable.Rows.Remove(row);
                        result = true;
                    }
                }
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
                    cachTable.Rows.Remove(row);
                    result = true;
                }
            }
            finally
            {
                readWriteLock.ExitWriteLock();
                
            }

            return result;
        }
    }
}
