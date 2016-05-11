using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LJC.FrameWork.WebApi
{
    public class APIEmptyHandler : APIHandler
    {
        private new Func<object> _func;
        public APIEmptyHandler(Type returnType,APIMethod apiMethod,Func<object> func)
        {
            _func = func;
            _responseType = returnType;
            ApiMethodProp = apiMethod;
            _resultType = typeof(APIResult<>).MakeGenericType(_responseType);
        }

        public override void ProcessRequest(HttpContext context)
        {
            Exception lastexp = null;

            int retCode = 1;
            string retMsg = string.Empty;
            object retResult = null;

            try
            {
                retResult = _func();

            }
            catch (Exception ex)
            {
                retCode = 0;
                retMsg = GetInnerException(ex).Message;
                lastexp = ex;
            }

            object result = null;
            if (ApiMethodProp.OutPutContentType == OutPutContentType.apiObject)
            {
                result = retResult;
                context.Response.BinaryWrite(this.SerializeObject(context, result));
            }
            else if (ApiMethodProp.OutPutContentType==OutPutContentType.text)
            {
                context.Response.Write(retResult.ToString());
            }
            else
            {
                result = Activator.CreateInstance(_resultType, retResult, retCode, retMsg);
                context.Response.BinaryWrite(this.SerializeObject(context, result));
            }
            
            context.Response.Flush();

            context.Response.End();
            if (lastexp != null)
            {
                throw lastexp;
            }
        }
    }
}
