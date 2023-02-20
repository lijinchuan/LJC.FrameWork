using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA.Contract
{
    /// <summary>
    /// 
    /// </summary>
    public class SOATransferWebResponse
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

        //public WebResponse WebResponse
        //{
        //    get;
        //    set;
        //}

        public byte[] Result
        {
            get;
            set;
        }
        public bool IsSuccess { get; set; } = false;

        public DateTime ResponseTime
        {
            get;
            set;
        }

        public string ErrMsg
        {
            get;
            set;
        }
    }
}
