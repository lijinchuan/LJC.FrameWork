using System;

namespace LJC.FrameWork.SOA.Contract
{
    [Serializable]
    public class QueryClientSessionRequest
    {
        public string ClientTransactionID { get; set; }
    }
}
