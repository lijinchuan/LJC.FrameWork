﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class MinusSign : DualCalSign
    {
        public override CalResult LeftVal
        {
            get
            {
                return base.LeftVal;
            }
            set
            {
                if (value == null)
                    value = new CalResult
                    {
                        Result = 0d,
                        ResultType = typeof(double)
                    };

                base.LeftVal = value;
            }
        }


        public override string Sign
        {
            get { return "-"; }
        }

        public override int Priority
        {
            get { return (int)SignPriorityEnum.plusMinus; }
        }

        public override string SignName
        {
            get
            {
                return "差";
            }
        }

        protected override object DoSingleOperate(object lVal, object rVal)
        {
            try
            {
                if (lVal == null || object.Equals(lVal, string.Empty))
                    return -rVal.ToDouble();
            
                return lVal.ToDouble() - rVal.ToDouble();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
