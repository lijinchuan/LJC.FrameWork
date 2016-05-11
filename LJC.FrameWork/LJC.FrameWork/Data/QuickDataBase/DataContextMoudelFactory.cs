using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public static class DataContextMoudelFactory<T> where T : new()
    {
        public static DataContextMoudle<T> GetDataContext(string database = "DefaultDB")
        {
            var connSet = ConfigurationManager.ConnectionStrings[database];
            if (connSet == null)
                throw new Exception(string.Concat("未配置name为", database, "连接设置"));
            DataContextMoudle<T> moudle = null;
            if (connSet.ProviderName.Equals("MySql.Data.MySqlClient", StringComparison.OrdinalIgnoreCase))
            {
                moudle = new MySqlDataContextMoudle<T>();
            }
            else if (connSet.ProviderName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
            {
                moudle = new MSSqlDataContextMoudle<T>();
            }
            else
            {
                //moudle= new DataContextMoudle<T>();
                throw new Exception("找不到对应的数据操作上下文");
            }

            moudle.database = database;
            return moudle;
        }

        public static DataContextMoudle<T> GetDataContext(T instance, string database = "DefaultDB")
        {
            var connSet = ConfigurationManager.ConnectionStrings[database];
            if (connSet == null)
                throw new Exception(string.Concat("未配置name为", database, "连接设置"));
            DataContextMoudle<T> moudle = null;
            if (connSet.ProviderName.Equals("MySql.Data.MySqlClient", StringComparison.OrdinalIgnoreCase))
            {
                moudle = new MySqlDataContextMoudle<T>(instance);
            }
            else if (connSet.ProviderName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
            {
                moudle = new MSSqlDataContextMoudle<T>(instance);
            }
            else
            {
                //moudle= new DataContextMoudle<T>(instance);
                throw new Exception("找不到对应的数据操作上下文");
            }
            moudle.database = database;
            return moudle;
        }
    }
}
