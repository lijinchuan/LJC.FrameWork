using LJC.FrameWork.Comm;
using LJC.FrameWork.LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.WebApi
{
    /// <summary>
    /// 配置项：errorapi，例子：http://ljcserver/WebWatcher/api/reporterror
    /// </summary>
    public class ErrorReporter
    {
        private static string errorCenterBaseUrl = ConfigHelper.AppConfig("errorapi");

        public static void Error(string resource, string msg)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(errorCenterBaseUrl))
                    return;

                APIClient.GetAPIResponse<bool>(errorCenterBaseUrl + "reporterror", new
                {
                    errorresource = System.Web.HttpUtility.UrlEncode(resource),
                    errmsg = System.Web.HttpUtility.UrlEncode(msg),
                    lv = 0,
                });
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error("[ErrorReporter][Error]上报异常失败", ex);
            }
        }

        public static void Info(string resource, string msg)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(errorCenterBaseUrl))
                    return;

                APIClient.GetAPIResponse<bool>(errorCenterBaseUrl + "reporterror", new
                {
                    errorresource = System.Web.HttpUtility.UrlEncode(resource),
                    errmsg = System.Web.HttpUtility.UrlEncode(msg),
                    lv = 2,
                });
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error("[ErrorReporter][Error]上报异常失败", ex);
            }
        }

        public static void ClearError(string resource)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(errorCenterBaseUrl))
                    return;

                APIClient.GetAPIResponse<bool>(errorCenterBaseUrl + "clearerror", new
                {
                    errorresource = System.Web.HttpUtility.UrlEncode(resource),
                });
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error("[ErrorReporter][Error]上报异常失败", ex);
            }
        }
    }
}
