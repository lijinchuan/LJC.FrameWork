using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;

namespace LJC.FrameWork.Comm
{
    public class WebSecurityUtility
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roles"></param>
        /// <param name="holdHours">保存时长</param>
        public static void UserLogin(string userName,int holdHours,object userData,string path="/",string domain=null)
        {
            if (HttpContext.Current == null)
                return;
            
            FormsAuthentication.SetAuthCookie(userName, false);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userName, DateTime.Now, DateTime.Now.AddHours(holdHours), true, userData.ToJson());
            string encryptTicket = FormsAuthentication.Encrypt(ticket);
            HttpCookie userCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptTicket);
            if (domain != null)
                userCookie.Domain = domain;
            userCookie.Path = path;
            userCookie.Expires = DateTime.Now.AddHours(holdHours);
            HttpContext.Current.Response.Cookies.Add(userCookie);
            HttpContext.Current.User = new GenericPrincipal(HttpContext.Current.User.Identity, null);
        }

        public static void UserLogout()
        {
            if (HttpContext.Current == null)
                return;

            FormsAuthentication.SignOut();
        }

        /// <summary>
        /// 设置当前用户user权限
        /// </summary>
        public static T GetUserData<T>()
        {
            if (HttpContext.Current == null)
                return default(T);
            if (HttpContext.Current.User.Identity is FormsIdentity)
            {
                var user = (FormsIdentity)HttpContext.Current.User.Identity;
                if (user.IsAuthenticated)
                {
                    var Ticket = user.Ticket;
                    if (!string.IsNullOrEmpty(Ticket.UserData))
                    {

                        return Ticket.UserData.JsonToEntity<T>();
                    }
                }
            }
            return default(T);
        }
    }
}
