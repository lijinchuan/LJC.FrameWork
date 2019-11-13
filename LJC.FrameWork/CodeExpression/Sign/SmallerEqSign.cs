using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class SmallerEqSign : DualCalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.compare; ;
            }
        }

        public override string Sign
        {
            get
            {
                return "<=";
            }
        }

        public override string SignName
        {
            get
            {
                return "比较";
            }
        }

        protected override object DoSingleOperate(object lVal, object rVal)
        {
            return lVal.ToDouble() <= rVal.ToDouble();
        }

        //public override CalResult Operate()
        //{
        //    base.Operate();

        //    if (Comm.GetType(LeftVal) != typeof(decimal))
        //    {
        //        throw new Exception("运算符错误，比较 操作符要求number类型的左值！");
        //    }

        //    if (Comm.GetType(RightVal) != typeof(decimal))
        //    {
        //        throw new Exception("运算符错误，比较 操作符要求number类型的右值！");
        //    }

        //    decimal result1 = decimal.Parse(LeftVal);
        //    decimal result2 = decimal.Parse(RightVal);

        //    return new CalResult
        //    {
        //        Result = (result1 <= result2).ToString(),
        //        ResultType = typeof(bool)
        //    };
        //}
    }
}
