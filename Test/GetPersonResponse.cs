using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    public class GetPersonResponse
    {
        /// <summary>
        /// 人类ID
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// 人类信息
        /// </summary>
        public NewPersonInfo Info
        {
            get;
            set;
        }
    }
}
