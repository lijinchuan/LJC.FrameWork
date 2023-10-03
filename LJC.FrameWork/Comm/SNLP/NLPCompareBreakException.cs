using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm.SNLP
{
    public class NLPCompareBreakException:Exception
    {
        public NLPCompareBreakException(List<NLPCompareDetail> details)
        {
            NLPCompareDetails = details;
        }
        public List<NLPCompareDetail> NLPCompareDetails
        {
            get;
            private set;
        }
    }
}
