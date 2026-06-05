using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.SOA.Contract
{
    public class AckChunkRequest
    {
        public string ClientId
        {
            get;
            set;
        }


        public int ChunkNo
        {
            get;
            set;
        }
    }
}
