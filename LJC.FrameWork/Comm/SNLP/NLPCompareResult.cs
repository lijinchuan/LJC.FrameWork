using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm.SNLP
{
    public class NLPCompareResult
    {
        public List<NLPCompareDetail> NLPCompareDetails
        {
            get;
            set;
        } = new List<NLPCompareDetail>();
    }
}
