using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    /// <summary>
    /// 网络请求
    /// </summary>
    public class SOATransferWebRequest
    {
        public string ClientId
        {
            get;
            set;
        }

        public string ClientTransactionID
        {
            get;
            set;
        }

        public int FundId
        {
            get;
            set;
        }

        public DateTime RequestTime
        {
            get;
            set;
        }

        public byte[] Param
        {
            get;
            set;
        }
    }
}
