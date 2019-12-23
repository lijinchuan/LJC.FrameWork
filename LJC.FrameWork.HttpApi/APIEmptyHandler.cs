using LJC.FrameWork.Net.HTTP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.HttpApi
{
    public class APIEmptyHandler : APIHandler
    {
        private new Func<object> _func;
        public APIEmptyHandler(Type returnType, APIMethodAttribute apiMethod, Func<object> func)
        {
            _func = func;
            _responseType = returnType;
            ApiMethodProp = apiMethod;
            _resultType = typeof(APIResult<>).MakeGenericType(_responseType);
        }

        public override bool Process(HttpServer server, HttpRequest request, HttpResponse response)
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
                response.RawContent = this.SerializeObject(request, response, result);
            }
            else if (ApiMethodProp.OutPutContentType == OutPutContentType.text)
            {
                response.Content = retResult.ToString();
            }
            else
            {
                result = Activator.CreateInstance(_resultType, retResult, retCode, retMsg);
                response.RawContent = this.SerializeObject(request, response, result);
            }

            if (lastexp != null)
            {
                response.ReturnCode = 500;
                throw lastexp;
            }

            response.ReturnCode = 200;
            return true;
        }
    }
}
