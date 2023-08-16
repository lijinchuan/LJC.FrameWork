using LJC.FrameWork.Comm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace LJC.FrameWork.WebApi
{
    public class APIFactory : IHttpHandlerFactory
    {
        internal static ConcurrentDictionary<string, APIHandler> apiFunMapper = new ConcurrentDictionary<string, APIHandler>();
        internal static ConcurrentDictionary<string, bool> apiPermission = new ConcurrentDictionary<string, bool>();

        static APIFactory()
        {
            //配置
            ConfigApi();

            //内置api
            Init("LJC.FrameWork.WebApi");

        }

        static void ConfigApi()
        {
            var domainname = System.Reflection.Assembly.GetCallingAssembly().GetName().Name;
            string cfgfile = System.AppDomain.CurrentDomain.BaseDirectory + "\\Web.config";
            if (File.Exists(cfgfile))
            {
                bool save = false;
                XmlDocument doc = new XmlDocument();
                doc.Load(cfgfile);
                #region 经典模式
                var syswebcfg = doc.DocumentElement.SelectSingleNode("//system.web");
                if (syswebcfg != null)
                {
                    var httphandlers = syswebcfg.SelectSingleNode("httpHandlers");
                    if (httphandlers == null)
                    {
                        //httphandlers = doc.CreateElement("httpHandlers");
                        //syswebcfg.AppendChild(httphandlers);
                    }
                    else
                    {
                        if (httphandlers.SelectSingleNode("add[@path='API/*/Json']") == null)
                        {
                            var apiNode = doc.CreateElement("add");
                            apiNode.SetAttribute("path", "API/*/Json");
                            apiNode.SetAttribute("verb", "*");
                            apiNode.SetAttribute("type", "LJC.FrameWork.WebApi.APIFactory, LJC.FrameWork.WebApi");
                            httphandlers.AppendChild(apiNode);

                            var apiNode2 = doc.CreateElement("add");
                            apiNode2.SetAttribute("path", "API/*");
                            apiNode2.SetAttribute("verb", "*");
                            apiNode2.SetAttribute("type", "LJC.FrameWork.WebApi.APIFactory, LJC.FrameWork.WebApi");
                            httphandlers.AppendChild(apiNode2);

                            save = true;
                        }
                    }
                }
                #endregion

                #region 集成模式
                var webServerNode = doc.DocumentElement.SelectSingleNode("//system.webServer");
                if (webServerNode == null)
                {
                    //webServerNode = doc.CreateElement("system.webServer");
                    //var modules = doc.CreateElement("modules");
                    //modules.SetAttribute("runAllManagedModulesForAllRequests", "true");
                    //doc.DocumentElement.AppendChild(webServerNode);
                }
                else
                {
                    var handlerNode = webServerNode.SelectSingleNode("handlers");
                    if (handlerNode == null)
                    {
                        //handlerNode = doc.CreateElement("handlers");

                        //webServerNode.AppendChild(handlerNode);
                    }
                    else
                    {
                        if (handlerNode.SelectSingleNode("add[@name='APIJsonHandler']") == null)
                        {
                            var removeNode = doc.CreateElement("remove");
                            removeNode.SetAttribute("verb", "*");
                            removeNode.SetAttribute("path", "API/*/Json");
                            handlerNode.AppendChild(removeNode);

                            var removeNode2 = doc.CreateElement("remove");
                            removeNode2.SetAttribute("verb", "*");
                            removeNode2.SetAttribute("path", "API/*");
                            handlerNode.AppendChild(removeNode2);

                            var apiNode = doc.CreateElement("add");
                            apiNode.SetAttribute("name", "APIJsonHandler");
                            apiNode.SetAttribute("path", "API/*/Json");
                            apiNode.SetAttribute("verb", "*");
                            apiNode.SetAttribute("type", "LJC.FrameWork.WebApi.APIFactory, LJC.FrameWork.WebApi");
                            //apiNode.SetAttribute("modules", "IsapiModule");
                            //apiNode.SetAttribute("scriptProcessor", "%windir%\\Microsoft.NET\\Framework64\\V4.0.30319\\aspnet_isapi.dll");
                            apiNode.SetAttribute("resourceType", "Unspecified");
                            apiNode.SetAttribute("requireAccess", "Script");
                            //apiNode.SetAttribute("preCondition", "bitness64");
                            apiNode.SetAttribute("preCondition", "integratedMode");
                            handlerNode.AppendChild(apiNode);

                            var apiNode2 = doc.CreateElement("add");
                            apiNode2.SetAttribute("name", "APIFunHandler");
                            apiNode2.SetAttribute("path", "API/*");
                            apiNode2.SetAttribute("verb", "*");
                            apiNode2.SetAttribute("type", "LJC.FrameWork.WebApi.APIFactory, LJC.FrameWork.WebApi");
                            //apiNode.SetAttribute("modules", "IsapiModule");
                            //apiNode.SetAttribute("scriptProcessor", "%windir%\\Microsoft.NET\\Framework64\\V4.0.30319\\aspnet_isapi.dll");
                            apiNode2.SetAttribute("resourceType", "Unspecified");
                            apiNode2.SetAttribute("requireAccess", "Script");
                            //apiNode.SetAttribute("preCondition", "bitness64");
                            apiNode2.SetAttribute("preCondition", "integratedMode");
                            handlerNode.AppendChild(apiNode2);

                            save = true;
                        }
                    }
                }
                #endregion

                if (save)
                {
                    doc.Save(cfgfile);
                }
            }
        }


        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            try
            {
                try
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
                catch
                {

                }
                var methed = url.Substring(url.LastIndexOf('/') + 1).ToLower();

                if ("json".Equals(methed))
                {
                    var urlnodes = url.Split('/');

                    APIHandler fun;
                    methed = urlnodes[urlnodes.Length - 2].ToLower();
                    if (apiFunMapper.TryGetValue(methed, out fun))
                    {
                        if (!fun.ApiMethodProp.IsVisible)
                        {
                            throw new NotSupportedException();
                        }
                        var jsonfun = new APIJsonHandler(methed, fun);
                        jsonfun._ipLimit = ConfigHelper.AppConfig(fun.ApiMethodProp.IpLimitConfig);
                        return jsonfun;
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("找不到api方法:{0}", methed));
                    }
                }
                else
                {

                    APIHandler fun;
                    if (!apiFunMapper.TryGetValue(methed, out fun))
                    {
                        throw new NotSupportedException(string.Format("找不到api方法:{0}", methed));
                    }

                    if (!string.IsNullOrEmpty(fun.ApiMethodProp.IpLimitConfig))
                    {
                        APIPermission permission = new APIPermission(fun.ApiMethodProp.IpLimitConfig);
                        if (!permission.CheckPermission(context.Request.UserHostAddress))
                        {
                            throw new Exception(string.Format("ip[{0}]没有调用权限！", context.Request.UserHostAddress));
                        }
                    }

                    return fun;
                }
            }
            catch (Exception ex)
            {
                return new ErrorHandler(ex);
            }
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            //throw new NotImplementedException();
        }

        public static void Init(params string[] apidomains)
        {
            foreach (var apidomain in apidomains)
            {
                Init(apidomains);
            }
        }

        public static void Init(string apidomain)
        {
            var domain = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p => p.FullName.Equals(apidomain, StringComparison.OrdinalIgnoreCase));
            if (domain == null)
            {
                domain = AppDomain.CurrentDomain.Load(apidomain);
            }

            if (domain == null)
                throw new Exception("找不到应用程序集:" + apidomain);

            foreach (var mode in domain.GetModules())
            {
                foreach (Type tp in mode.GetTypes())
                {
                    if (tp.IsClass)
                    {
                        foreach (var method in tp.GetMethods())
                        {
                            if (method.IsStatic)
                            {
                                continue;
                            }
                            var apimethod = (APIMethod)method.GetCustomAttributes(typeof(APIMethod), true).FirstOrDefault();
                            if (apimethod != null)
                            {
                                string methodname = string.IsNullOrWhiteSpace(apimethod.Aliname) ? method.Name.ToLower() : apimethod.Aliname.ToLower();
                                if ("json".Equals(methodname))
                                {
                                    throw new Exception("json不能用于api方法名");
                                }
                                if (apiFunMapper.ContainsKey(methodname))
                                {
                                    throw new Exception(string.Format("api方法已经被注册:{0}", methodname));
                                }
                                var param = method.GetParameters();

                                var tpInstance = Activator.CreateInstance(tp);
                                if (param.Length == 0)
                                {
                                    apiFunMapper.TryAdd(methodname, new APIEmptyHandler(method.ReturnType, apimethod, () =>
                                    method.Invoke(tpInstance, null)));
                                }
                                else if (param.Length == 1)
                                {
                                    Type t = param[0].ParameterType;
                                    apiFunMapper.TryAdd(methodname, new APIHandler(t, method.ReturnType, apimethod, (o) =>
                                    method.Invoke(tpInstance, new[] { o })));
                                }
                                else
                                {
                                    throw new NotSupportedException("api方法参数太多，建议包装成一个对象传参。");
                                }

                            }
                        }
                    }
                }
            }
        }
    }
}
