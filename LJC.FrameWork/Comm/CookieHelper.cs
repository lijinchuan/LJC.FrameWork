using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace LJC.FrameWork.Comm
{
    public static class CookieHelper
    {
        public static CookieContainer GetCookieContainer(this List<Cookie> cookies)
        {
            CookieContainer cc = new CookieContainer();

            if (cookies != null)
            {
                cookies.RemoveAll(c => c.Expired);
                cookies.ForEach((c) => cc.Add(c));
            }

            return cc;

        }

        /// <summary>
        /// 清除无效的
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static List<Cookie> ClearExpired(this List<Cookie> cookies)
        {
            cookies.RemoveAll(c => c.Expired);
            return cookies;
        }

        public static void AddCookie(this List<Cookie> cookieListContainer, Cookie addCooke)
        {
            if (addCooke != null && !addCooke.Expired)
            {
                Cookie findCookie = cookieListContainer.Find(c => c.Name == addCooke.Name);
                if (findCookie==null)
                {
                    cookieListContainer.Add(addCooke);
                }
                else
                {
                    if (findCookie.TimeStamp < addCooke.TimeStamp)
                    {
                        cookieListContainer.Remove(findCookie);
                        cookieListContainer.Add(addCooke);
                    }
                }
            }
        }

        public static void AddRangeCookie(this List<Cookie> cookieListContainer, List<Cookie> addCookes)
        {
            if (addCookes != null)
            {
                addCookes.ForEach(c => cookieListContainer.AddCookie(c));
            }
        }
    }
}
