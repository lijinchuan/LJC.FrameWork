using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LJC.FrameWork.Comm
{
    public static class ConfigHelper
    {
        public static string AppConfig(string appSettingName)
        {
            if (ConfigurationManager.AppSettings[appSettingName] != null)
                return ConfigurationManager.AppSettings[appSettingName];

            return string.Empty;
        }

        public static bool SaveConfig(string exePath,string appSettingName, string appSettingValue)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);

                config.AppSettings.Settings.Remove(appSettingName);
                config.AppSettings.Settings.Add(appSettingName, appSettingValue);

                config.Save();

                return true;

            }
            catch
            {
                return false;
            }
        }

    }
}
