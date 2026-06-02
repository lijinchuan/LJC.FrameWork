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

        public bool IsChunked
        {
            get;
            set;
        }

        public bool IsMeta
        {
            get;
            set;
        }

        public bool IsLastChunk
        {
            get;
            set;
        }

        public int ChunkNo
        {
            get;
            set;
        }

        public int InputDataLength
        {
            get;
            set;
        }
    }
}
