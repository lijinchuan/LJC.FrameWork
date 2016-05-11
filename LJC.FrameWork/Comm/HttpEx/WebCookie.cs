using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;

namespace LJC.FrameWork.Comm.HttpEx
{
    [Serializable]
    public class WebCookie
    {
        public WebCookie(string name)
        {
            Name = name;
        }
        //
        // 摘要:
        //     创建和命名新的 Cookie，并为其赋值。
        //
        // 参数:
        //   name:
        //     新 Cookie 的名称。
        //
        //   value:
        //     新 Cookie 的值。
        public WebCookie(string name, string value)
        {
            Name = name;
        }

        // 摘要:
        //     获取或设置将此 Cookie 与其关联的域。
        //
        // 返回结果:
        //     要将此 Cookie 与其关联的域名。默认值为当前域。
        public string Domain { get; set; }
        //
        // 摘要:
        //     获取或设置此 Cookie 的过期日期和时间。
        //
        // 返回结果:
        //     此 Cookie 的过期时间（在客户端）。
        public DateTime Expires { get; set; }
        //
        // 摘要:
        //     获取一个值，通过该值指示 Cookie 是否具有子键。
        //
        // 返回结果:
        //     如果 Cookie 具有子键，则为 true；否则为 false。默认为 false。
        public bool HasKeys { get; set; }
        //
        // 摘要:
        //     获取或设置一个值，该值指定 Cookie 是否可通过客户端脚本访问。
        //
        // 返回结果:
        //     如果 Cookie 具有 HttpOnly 属性且不能通过客户端脚本访问，则为 true；否则为 false。默认值为 false。
        public bool HttpOnly { get; set; }
        //
        // 摘要:
        //     获取或设置 Cookie 的名称。
        //
        // 返回结果:
        //     除非构造函数另外指定，否则默认值为 null 引用（在 Visual Basic 中为 Nothing）。
        public string Name { get; set; }
        //
        // 摘要:
        //     获取或设置要与当前 Cookie 一起传输的虚拟路径。
        //
        // 返回结果:
        //     要与此 Cookie 一起传输的虚拟路径。默认值为当前请求的路径。
        public string Path { get; set; }
        //
        // 摘要:
        //     获取或设置一个值，该值指示是否使用安全套接字层 (SSL)（即仅通过 HTTPS）传输 Cookie。
        //
        // 返回结果:
        //     如果通过 SSL 连接 (HTTPS) 传输 Cookie，则为 true；否则为 false。默认为 false。
        public bool Secure { get; set; }
        //
        // 摘要:
        //     获取或设置单个 Cookie 值。
        //
        // 返回结果:
        //     Cookie 的值。默认值为 null 引用（在 Visual Basic 中为 Nothing）。
        public string Value { get; set; }
        //
        // 摘要:
        //     获取单个 Cookie 对象所包含的键值对的集合。
        //
        // 返回结果:
        //     Cookie 值的集合。
        public NameValueCollection Values
        {
            get;
            set;
        }

        public WebCookie(HttpCookie cookie)
        {
            this.Name = cookie.Name;
            this.Value = cookie.Value;
            this.Domain = cookie.Domain;
            this.Expires = cookie.Expires;
            this.HasKeys = cookie.HasKeys;
            this.HttpOnly = cookie.HttpOnly;
            this.Path = cookie.Path;
            this.Secure = cookie.Secure;
            this.Values = cookie.Values;
        }

        public HttpCookie ToHttpCookie()
        {
            HttpCookie httpCookie = new HttpCookie(Name, Value);
            httpCookie.Domain = "taobao.com";
            httpCookie.Expires = Expires;
            httpCookie.HttpOnly = HttpOnly;
            httpCookie.Path = Path;
            httpCookie.Secure = Secure;

            return httpCookie;
        }
    }
}
