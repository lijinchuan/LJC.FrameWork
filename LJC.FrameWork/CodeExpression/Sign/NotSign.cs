using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class NotSign : CalSign
    {
        public override int Params
        {
            get
            {
                return 1;
            }
        }

        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.not;
            }
        }

        protected override CalResult CollectOperate()
        {
            return new CalResult
            {
                Results = this.RightCollVal.Select(c => (object)!c.ToBool()).ToArray(),
                ResultType = typeof(bool)

            };
        }

        protected override CalResult SingOperate()
        {
            return new CalResult
            {
                Result = !(bool)this.RightSigelVal,
                ResultType = typeof(bool)
            };
        }

    }
}
