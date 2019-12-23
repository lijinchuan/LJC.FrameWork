using LJC.FrameWork.Attr;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    [ProtoContract]
    public class APIResult<T>
    {
        public APIResult()
        {

        }

        public APIResult(T result, int retcode = 1, string errmsg = null)
        {
            this.ResponseBody = result;
            ResultCode = retcode;
            ResultMessage = errmsg;
        }

        [ProtoMember(1)]
        [JsonProperty(PropertyName = "re")]
        [PropertyDescription("接口返回的结果json实体，<T>类型")]
        public T ResponseBody
        {
            get;
            set;
        }

        private int _resultCode = 1;
        [ProtoMember(2)]
        [JsonProperty(PropertyName = "rc")]
        [PropertyDescription("接口返回的状态，0-失败，1-成功")]
        public int ResultCode
        {
            get
            {
                return _resultCode;
            }
            set
            {
                _resultCode = value;
            }
        }

        [ProtoMember(3)]
        [JsonProperty(PropertyName = "me")]
        [PropertyDescription("接口返回的文字信息，如果是失败的，返回失败的信息描述")]
        public string ResultMessage
        {
            get;
            set;
        }

    }
}
