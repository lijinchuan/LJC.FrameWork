using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using LJC.FrameWork.Comm;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public static class DataContextMoudelSelect
    {
        /// <summary>
        /// 变量名正则
        /// </summary>
        private static readonly Regex varNameReg = new Regex(@"\.([_a-zA-Z\u4E00-\u9FA5\uF900-\uFA2D][_a-zA-Z0-9\u4E00-\u9FA5 \uF900-\uFA2D]*)",RegexOptions.Compiled);
        internal static string GetColNameFromExpression<T>(this DataContextMoudle<T> dataContext, string expressionBody) where T : new()
        {
            string propname= varNameReg.Match(expressionBody).Groups[1].Value;
            var reportattr= dataContext.rpAttrList.Find(p => p.Property.Name.Equals(propname, StringComparison.OrdinalIgnoreCase));
            if (reportattr == null)
                throw new Exception(string.Format("找不到属性'{0}'对应的表列。",propname));

            return reportattr.Column;
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

        public static DataContextMoudle<T> WhereIn<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string,string,object>(column," in ",val));

            return dataContext;
        }

        public static DataContextMoudle<T> WhereIn<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereIn<T>(dataContext.GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereEq<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereEq(dataContext.GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereNotEq<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, "<>", val));

            return dataContext;
        }

        public static DataContextMoudle<T> WhereNotEq<T>(this DataContextMoudle<T> dataContext,Expression<Func<T,object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereNotEq(dataContext.GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereBiger<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, ">", val));
            return dataContext;
        }

        public static DataContextMoudle<T> WhereBiger<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereBiger(dataContext.GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereBigerEq<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, ">=", val));
            return dataContext;
        }

        public static DataContextMoudle<T> WhereBigerEq<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereBigerEq(dataContext.GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereSmaller<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, "<", val));
            return dataContext;
        }

        public static DataContextMoudle<T> WhereSmaller<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereSmaller(dataContext.GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> WhereSmallerEq<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            dataContext.selPara.Addp(new Mess_Three<string, string, object>(column, "<=", val));
            return dataContext;
        }

        public static DataContextMoudle<T> WhereSmallerEq<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.WhereSmallerEq(dataContext.GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static DataContextMoudle<T> OrderBy<T>(this DataContextMoudle<T> dataContext, string col) where T : new()
        {
            dataContext.orderByList.Add(new DBOrderby
            {
                OrderbyColumnName=col,
                OrderbyDirection=DBOrderbyDirection.asc,
            });
            return dataContext;
        }

        public static DataContextMoudle<T> OrderBy<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress) where T : new()
        {
            return dataContext.OrderBy<T>(dataContext.GetColNameFromExpression(colExpress.Body.ToString()));
        }

        public static DataContextMoudle<T> OrderByDesc<T>(this DataContextMoudle<T> dataContext, string col) where T : new()
        {
            dataContext.orderByList.Add(new DBOrderby
            {
                OrderbyColumnName = col,
                OrderbyDirection = DBOrderbyDirection.desc,
            });
            return dataContext;
        }

        public static DataContextMoudle<T> OrderByDesc<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress) where T : new()
        {
            return dataContext.OrderByDesc<T>(dataContext.GetColNameFromExpression(colExpress.Body.ToString()));
        }

        public static bool Exists<T>(this DataContextMoudle<T> dataContext, string column, object val) where T : new()
        {
            return dataContext.WhereEq(column, val).ExecuteEntity() != null;
        }

        public static bool Exists<T>(this DataContextMoudle<T> dataContext, Expression<Func<T, object>> colExpress, object val) where T : new()
        {
            return dataContext.Exists(dataContext.GetColNameFromExpression(colExpress.Body.ToString()), val);
        }

        public static bool Exists<T>(this DataContextMoudle<T> dataContext) where T : new()
        {
            return dataContext.Exists(dataContext.keyName, dataContext.GetKeyValue());
        }
    }
}
