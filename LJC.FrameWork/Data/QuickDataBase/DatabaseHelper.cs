using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public class DatabaseHelper
    {
        private static int _cmdTimeOut = 600;
        public static int CommandTimeout
        {
            get
            {
                return _cmdTimeOut;
            }
            set
            {
                if (value < 0)
                    _cmdTimeOut = 30;

                _cmdTimeOut = value;
            }
        }

        /// <summary>
        /// 为sql语句准备参数
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        private static void PrepareSQLParameters(Database db, DbCommand cmd, params DbParameter[] parameters)
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
        }

        public static int ExecuteNonQuery(string database, string sql, params DbParameter[] parameters)
        {
            Database db = DatabaseFactory.CreateDatabase(database);

            DbCommand cmd = db.GetSqlStringCommand(sql);
            cmd.CommandTimeout = CommandTimeout;

            PrepareSQLParameters(db, cmd, parameters);

            return db.ExecuteNonQuery(cmd);
        }

        public static object ExecuteScalar(string database, string sql, params DbParameter[] parameters)
        {
            Database db = DatabaseFactory.CreateDatabase(database);
            var cmd = db.GetSqlStringCommand(sql);
            cmd.CommandTimeout = CommandTimeout;
            PrepareSQLParameters(db, cmd, parameters);

            return db.ExecuteScalar(cmd);
        }

        public static DataTable ExecuteSQLTable(string database, string sql, params DbParameter[] parameters)
        {
            Database db = DatabaseFactory.CreateDatabase(database);
            var cmd = db.GetSqlStringCommand(sql);
            cmd.CommandTimeout = CommandTimeout;
            PrepareSQLParameters(db, cmd, parameters);

            var ds = db.ExecuteDataSet(cmd);
            if (ds.Tables.Count > 0)
            {
                var table0 = ds.Tables[0];
                return table0;
            }

            return null;
        }

        public static int ExecuteProcNonQuery(string database, string proc, params DbParameter[] parameters)
        {
            Database db = DatabaseFactory.CreateDatabase(database);
            var cmd = db.GetStoredProcCommand(proc);
            cmd.CommandTimeout = CommandTimeout;
            PrepareSQLParameters(db, cmd, parameters);

            return db.ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// 执行语句，如果失败则回滚
        /// </summary>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQueryTrans(string database, string sql, params DbParameter[] parameters)
        {
            var result = 0;

            Database db = DatabaseFactory.CreateDatabase(database);

            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                var trans = conn.BeginTransaction();
                try
                {
                    var cmd = db.GetSqlStringCommand(sql);
                    cmd.CommandTimeout = CommandTimeout;
                    PrepareSQLParameters(db, cmd, parameters);

                    result = db.ExecuteNonQuery(cmd, trans);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }

            return result;
        }
    }
}
