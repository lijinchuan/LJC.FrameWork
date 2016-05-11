using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public static class DataContextMoudelSelect
    {
        private static Regex varNameRegex = new Regex("\\.([_a-zA-Z][_a-zA-Z0-9]*)");
        private static string GetColNameFromExpression(string expressionBody)
        {
            return varNameRegex.Match(expressionBody).Groups[1].Value;
        }

        private static void Addp(this List<Mess_Three<string, string, object>> list, Mess_Three<string, string, object> addVal)
        {
            if (list == null)
                list = new List<Mess_Three<string, string, object>>();

            int i = 0;
            if ((i = list.Where((m) => m.First.Equals(addVal.First)).Count()) > 0)
            {
                addVal.First += "$" + i;
                list.Add(addVal);
            }
            else
            {
                list.Add(addVal);
            }

        }

        public static DataContextMoudle<T> Top<T>(this DataContextMoudle<T> dataContext, int top) where T : new()
        {
            dataContext.topNum = top;
            return dataContext;
        }


        public static DataContextMoudle<T> WhereEq<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, "=", val));

            return dataContext;
        }

        public static DataContextMoudle<T> WhereEq<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereEq(GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereNotEq<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, "<>", val));

            return dataContext;
        }

        public static DataContextMoudle<T> WhereNotEq<T>(this DataContextMoudle<T> dataContext,Expression<Func<T,object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereNotEq(GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereBiger<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, ">", val));
            return dataContext;
        }

        public static DataContextMoudle<T> WhereBiger<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereBiger(GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereBigerEq<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, ">=", val));
            return dataContext;
        }

        public static DataContextMoudle<T> WhereBigerEq<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereBigerEq(GetColNameFromExpression(colExpress.Body.ToString()),val);
        }

        public static DataContextMoudle<T> WhereSmaller<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, "<", val));
            return dataContext;
        }

        public static DataContextMoudle<T> WhereSmaller<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereSmaller(GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereSmallerEq<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, "<=", val));
            return dataContext;
        }

        public static DataContextMoudle<T> WhereSmallerEq<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereSmallerEq(GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static bool Exists<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            return dataContext.WhereEq(column, val).ExecuteEntity() != null;

            //string sql = string.Empty;
            //if (dataContext is MSSqlDataContextMoudle<T>)
            //{
            //    sql = "select top 1 1 from " + dataContext.tabName + "(nolock) where " + column + "=@" + column;
            //}
            //else if (dataContext is MySqlDataContextMoudle<T>)
            //{
            //    sql = "select 1 from " + dataContext.tabName + " where " + column + "=@" + column + " limit 0,1";
            //}
            //Database db = DatabaseFactory.CreateDatabase(dataContext.database);
            //DbCommand cmd = db.GetSqlStringCommand(sql);

            //db.AddInParameter(cmd, "@" + column, dataContext.GetDbType(val.GetType()), val);

            //return db.ExecuteScalar(cmd) != null;
        }

        public static bool Exists<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.Exists(GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static bool Exists<T>(this DataContextMoudle<T> dataContext) where T : new()
        {
            return dataContext.Exists(dataContext.keyName, dataContext.GetKeyValue());
        }
    }
}
