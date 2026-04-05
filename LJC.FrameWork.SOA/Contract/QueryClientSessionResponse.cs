using System;

namespace LJC.FrameWork.SOA.Contract
{
    [Serializable]
    public class QueryClientSessionResponse
    {
        public bool Exists { get; set; }

        public int LastNo
        {
            get;
            set;
        }
    }
}
