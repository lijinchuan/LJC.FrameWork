using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    [AttributeUsage(AttributeTargets.Method)]
    public class APIMethodAttribute : Attribute
    {
        /// <summary>
        /// 方法别名
        /// </summary>
        public string Aliname
        {
            get;
            set;
        }

        public string MethodName
        {
            get;
            set;
        }

        private bool _standApiOutPut = true;
        [Obsolete("请用OutPutContentType代替。")]
        /// <summary>
        /// 是否是标准的API输出，如果不是，则只输出me里面的对象,默认true
        /// </summary>
        public bool StandApiOutPut
        {
            get
            {
                return _standApiOutPut;
            }
            set
            {
                _standApiOutPut = value;
            }
        }

        private OutPutContentType _outputtype = OutPutContentType.standApi;
        public OutPutContentType OutPutContentType
        {
            get
            {
                return _outputtype;
            }
            set
            {
                _outputtype = value;
            }
        }

        private bool _isvisible = true;
        /// <summary>
        /// 接口是否对外可见，默认true
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _isvisible;
            }
            set
            {
                _isvisible = value;
            }
        }


        /// <summary>
        /// ip限制,如果为空则不限制,如果不为空，则读取appconfig配置
        /// </summary>
        public string IpLimitConfig
        {
            get;
            set;
        }

        /// <summary>
        /// 功能描述
        /// </summary>
        public string Function
        {
            get;
            set;
        }
    }
}
