using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LJC.FrameWork.WebApi
{
    public class ErrorHandler : IHttpHandler
    {
        private Exception _ex;

        public bool IsReusable
        {
            get { throw new NotImplementedException(); }
        }

        public ErrorHandler(Exception ex)
        {
            this._ex = ex;
        }

        public void ProcessRequest(HttpContext context)
        {
            APIResult<string> result = new APIResult<string>
            {
                ResponseBody=_ex.StackTrace,
                ResultCode=0,
                ResultMessage=_ex.Message
            };

            context.Response.Write(LJC.FrameWork.Comm.JsonHelper.ToJson(result));
        }
    }
}
