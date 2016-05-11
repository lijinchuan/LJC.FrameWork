using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Reflection;

namespace LJC.FrameWork.Data.QuickDataBase
{
    public static class DataContextMoudelFactory<T> where T : new()
    {
        static void CheckMysqlConfig()
        {
            var domainname = System.Reflection.Assembly.GetCallingAssembly().GetName().Name;
            string cfgfile = System.AppDomain.CurrentDomain.BaseDirectory + "\\Web.config";

            if (!File.Exists(cfgfile))
            {
                cfgfile = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + Assembly.GetEntryAssembly().FullName.Split(',')[0] + ".exe.config";
            }

            bool save = false;
            if (File.Exists(cfgfile))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(cfgfile);

                //检查是否有用到mysql
                var connectionStrings = doc.DocumentElement.SelectSingleNode("connectionStrings");
                if (connectionStrings == null)
                    return;

                var mysqlinstances = connectionStrings.SelectNodes("add[@providerName='MySql.Data.MySqlClient']");
                if (mysqlinstances == null || mysqlinstances.Count == 0)
                {
                    return;
                }

                //第一个节点
                var configSectionsNode = doc.DocumentElement.SelectSingleNode("configSections");
                if (configSectionsNode == null)
                {
                    var newconfigsections = doc.CreateElement("configSections");
                    configSectionsNode = doc.DocumentElement.InsertBefore(newconfigsections, doc.DocumentElement.FirstChild);
                    var comment = doc.CreateComment("MySql 企业库支持");
                    doc.DocumentElement.InsertBefore(comment, configSectionsNode);
                    save = true;
                }

                var sectionNodes = configSectionsNode.SelectSingleNode("section[@name='dataConfiguration']");
                if (sectionNodes == null)
                {
                    var comment = doc.CreateComment("01.声明mysql数据库配置节");
                    configSectionsNode.AppendChild(comment);
                    var newsection = doc.CreateElement("section");
                    newsection.SetAttribute("name", "dataConfiguration");
                    newsection.SetAttribute("type", "Microsoft.Practices.EnterpriseLibrary.Data.Configuration.DatabaseSettings,Microsoft.Practices.EnterpriseLibrary.Data");
                    sectionNodes = configSectionsNode.AppendChild(newsection);
                    save = true;
                }

                //第二个节点
                var dataConfiguration = doc.DocumentElement.SelectSingleNode("dataConfiguration");
                if (dataConfiguration == null)
                {
                    var newdataConfiguration = doc.CreateElement("dataConfiguration");
                    //newdataConfiguration.InnerXml = "<!--02.注册MySql数据提供者-->" + newdataConfiguration.InnerXml;
                    dataConfiguration = doc.DocumentElement.InsertAfter(newdataConfiguration, configSectionsNode);
                    var comment = doc.CreateComment("02.注册MySql数据提供者");
                    doc.DocumentElement.InsertBefore(comment, dataConfiguration);
                    save = true;
                }

                var providerMappingsNode = dataConfiguration.SelectSingleNode("providerMappings");
                if (providerMappingsNode == null)
                {
                    var newproviderMappingsNode = doc.CreateElement("providerMappings");
                    providerMappingsNode = dataConfiguration.AppendChild(newproviderMappingsNode);
                    save = true;
                }

                var mysqlClientAdd = providerMappingsNode.SelectSingleNode("add[@name='MySql.Data.MySqlClient']");
                if (mysqlClientAdd == null)
                {
                    var newmysqlClientAdd = doc.CreateElement("add");
                    newmysqlClientAdd.SetAttribute("name", "MySql.Data.MySqlClient");
                    newmysqlClientAdd.SetAttribute("databaseType", "Microsoft.Practices.EnterpriseLibrary.Data.MySql.MySqlDatabase, Microsoft.Practices.EnterpriseLibrary.Data");
                    mysqlClientAdd = providerMappingsNode.AppendChild(newmysqlClientAdd);
                    save = true;
                }

                //第三个节点
                var systemdata = doc.DocumentElement.SelectSingleNode("system.data");
                if (systemdata == null)
                {
                    var newsystemdata = doc.CreateElement("system.data");
                    systemdata = doc.DocumentElement.InsertAfter(newsystemdata, dataConfiguration);
                    save = true;
                }

                var dbProviderFactories = systemdata.SelectSingleNode("DbProviderFactories");
                if (dbProviderFactories == null)
                {
                    var newdbProviderFactories = doc.CreateElement("DbProviderFactories");
                    dbProviderFactories = systemdata.AppendChild(newdbProviderFactories);
                    save = true;
                }

                var mysqlremove = dbProviderFactories.SelectSingleNode("remove[@invariant='MySql.Data.MySqlClient']");
                if (mysqlremove == null)
                {
                    var newmysqlremove = doc.CreateElement("remove");
                    newmysqlremove.SetAttribute("invariant", "MySql.Data.MySqlClient");
                    mysqlremove = dbProviderFactories.AppendChild(newmysqlremove);
                    save = true;
                }

                var mysqldpf = dbProviderFactories.SelectSingleNode("add[@name='MySql Data Provider Factory']");
                if (mysqldpf == null)
                {
                    var newmysqlpdf = doc.CreateElement("add");
                    newmysqlpdf.SetAttribute("name", "MySql Data Provider Factory");
                    newmysqlpdf.SetAttribute("invariant", "MySql.Data.MySqlClient");
                    newmysqlpdf.SetAttribute("description", "MySql Data Provider");
                    newmysqlpdf.SetAttribute("type", "MySql.Data.MySqlClient.MySqlClientFactory");
                    //newmysqlpdf.InnerXml = "<!--03.注册MySql数据提供者工厂类-->" + newmysqlpdf.InnerXml;
                    mysqldpf = dbProviderFactories.InsertAfter(newmysqlpdf, mysqlremove);

                    var comment = doc.CreateComment("03.注册MySql数据提供者工厂类");
                    dbProviderFactories.InsertBefore(comment, mysqldpf);
                    save = true;
                }

                if (save)
                {
                    doc.Save(cfgfile);
                }
            }
        }

        static DataContextMoudelFactory()
        {
            //检测是否有mysql的配置
            CheckMysqlConfig();

        }

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
