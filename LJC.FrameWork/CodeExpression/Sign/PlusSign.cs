using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class PlusSign : DualCalSign
    {

        public override string Sign
        {
            get
            {
                return "+";
            }

        }

        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.plusMinus; ;
            }
        }

        public override string SignName
        {
            get
            {
                return "和";
            }
        }

        protected override object DoSingleOperate(object lVal, object rVal)
        {
            try
            {
                return (decimal)lVal + (decimal)rVal;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public override CalResult Operate()
        //{
        //    base.Operate();

        //    if (Comm.GetType(LeftVal) != typeof(decimal))
        //    {
        //        throw new Exception("运算符错误，和操作符要求number类型的左值！");
        //    }

        //    if (Comm.GetType(RightVal) != typeof(decimal))
        //    {
        //        throw new Exception("运算符错误，和操作符要求number类型的右值！");
        //    }

        //    decimal result1 = decimal.Parse(LeftVal);
        //    decimal result2 = decimal.Parse(RightVal);

        //    return new CalResult
        //    {
        //        Result = (result1 + result2).ToString(),
        //        ResultType = typeof(decimal)
        //    };
        //}

    }
}
