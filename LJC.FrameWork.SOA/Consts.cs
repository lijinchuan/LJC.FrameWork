using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public static class Consts
    {
        public const int ESBServerServiceNo = 0;
        public const int FunNo_ECHO = 1;
        public const int FunNo_Environment = 2;
        public const int FunNo_ExistsAServiceNo = 3;
        public const int FunNo_GetRegisterServiceInfo = 4;
        public const int FunNo_ListServiceInfos = 5;

        public const string ERRORSERVICEMSG = "请求的服务号错误";
        public const string MISSINGFUNCTION = "服务未实现";

        public const string HeaderKey_ContentType = "contentType";
        public const string HeaderValue_ContentType_JSONValue = "application/json";

        public const string CustomParam_SendAll = "SendAll";
    }
}
