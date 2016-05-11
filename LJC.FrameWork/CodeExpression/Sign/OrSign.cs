using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class OrSign : DualCalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.andOr; ;
            }
        }

        public override string Sign
        {
            get
            {
                return "||";
            }
        }

        public override string SignName
        {
            get
            {
                return "或";
            }
        }

        protected override object DoSingleOperate(object lVal, object rVal)
        {
            return (bool)lVal || (bool)rVal;
        }

        //public override CalResult Operate()
        //{
        //    base.Operate();

        //    if (Comm.GetType(LeftVal) != typeof(bool))
        //    {
        //        throw new Exception("运算符错误，"+this.SignName+" 操作符要求bool类型的左值！");
        //    }

        //    if (Comm.GetType(RightVal) != typeof(bool))
        //    {
        //        throw new Exception("运算符错误，"+this.SignName+" 操作符要求bool类型的右值！");
        //    }

        //    bool result1 = bool.Parse(LeftVal);
        //    bool result2 = bool.Parse(RightVal);

        //    return new CalResult
        //    {
        //        ResultType = typeof(bool),
        //        Result = (result1 ||  result2).ToString()
        //    };
        //}
    }
}
