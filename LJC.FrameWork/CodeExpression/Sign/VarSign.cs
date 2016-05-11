using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class VarSign : CalSign
    {
        public string VarName
        {
            get;
            private set;
        }

        public bool IsSetValue
        {
            get;
            set;
        }

        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.fun;
            }
        }

        public VarSign(string varName,CalCurrent current)
        {
            VarName = varName;
            this.CalCurrent = current;
        }

        protected override CalResult CollectOperate()
        {
            return SingOperate();
        }

        protected override CalResult SingOperate()
        {
            if (!IsSetValue)
            {
                if (CalCurrent.VarDataPool.ContainsKey(VarName))
                {
                    if (CalCurrent.CurrentIndex == -1)
                        return CalCurrent.VarDataPool[VarName];
                    else
                    {
                        var y = CalCurrent.VarDataPool[VarName];
                        if (y.Results != null)
                        {
                            return new CalResult
                            {
                                Result = y.Results[CalCurrent.CurrentIndex],
                                ResultType = y.ResultType
                            };
                        }
                        else
                        {
                            return y;
                        }
                    }
                }
                else
                    throw new ExpressErrorException("变量" + VarName + "未赋值！");
            }


            return new CalResult
            {
                Result = VarName.ToString(),
                ResultType = typeof(string)
            };
        }

        public override int Params
        {
            get
            {
                return 0;
            }
        }
    }
}
