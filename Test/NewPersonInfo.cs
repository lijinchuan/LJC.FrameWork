using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    public class NewPersonInfo
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public String Name
        {
            get;
            set;
        }

        public int Age
        {
            get;
            set;
        }

        public DateTime Birth
        {
            get;
            set;
        }

        /// <summary>
        /// 朋友列表
        /// </summary>
        public String[] Friends
        {
            get;
            set;
        }

        public Dictionary<String, string> FriendsInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 学校信息
        /// </summary>
        public List<String> Schools
        {
            get;
            set;
        }
    }
}
