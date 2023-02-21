using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Net.HTTP.Server
{
    /// <summary>
    /// RESTfull处理器的接口
    /// </summary>

    public class RESTfulApiHandlerBase : IHttpHandler
    {
        public RESTfulApiHandlerBase(HMethod method, string sUrl, List<string> param, IRESTfulHandler handler)
        {
            m_method = method;
            m_sUrl = sUrl;
            m_listParam = param;
            m_handler = handler;
        }
        private HMethod m_method = HMethod.GET;
        private string m_sUrl = "";
        private List<string> m_listParam = new List<string>();
        private IRESTfulHandler m_handler = null;

        public bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            if (request.Method != m_method.ToString())
                return false;
            if (!request.Page.Equals(m_sUrl, StringComparison.CurrentCultureIgnoreCase))
                return false;

            //处理查询参数
            string sQueryString = request.QueryString;
            if (m_method == HMethod.DELETE || m_method == HMethod.PUT)
                sQueryString = request.GetContent();

            Dictionary<string, string> queryParam = GetParam(sQueryString);
            if (queryParam.Count != m_listParam.Count)
                return false;

            HashSet<string> set1 = new HashSet<string>(m_listParam);
            HashSet<string> set2 = new HashSet<string>(queryParam.Keys);
            //两个集合是否相同
            if (!set1.SetEquals(set2))
                return false;

            if (m_handler != null)
                return m_handler.Process(server, request, response, queryParam);
            return false;
        }

        private static Dictionary<String, String> GetParam(String sQueryString)
        {
            Dictionary<String, String> param = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(sQueryString))
                return param;
            string[] ar = sQueryString.Split(new char[] { '&' });

            foreach (string item in ar)
            {
                int nFind = item.IndexOf('=');
                string sKey = item.Substring(0, nFind);
                string sValue = item.Substring(nFind + 1, item.Length - nFind - 1);
                param[sKey] = sValue;

            }

            return param;
        }
    }
}
