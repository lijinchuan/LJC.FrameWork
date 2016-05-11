using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class T : FunSign
    {
        public override int Params
        {
            get
            {
                return 0;
            }
        }

        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.fun;
            }
        }

        protected override CalResult CollectOperate()
        {
            return SingOperate();
        }

        protected override CalResult SingOperate()
        {
            return new CalResult
            {
                Result = true,
                ResultType = typeof(bool)
            };
        }
    }
}
