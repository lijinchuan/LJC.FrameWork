using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    /// <summary>
    /// 二元操作基类
    /// </summary>
    internal class DualCalSign : CalSign
    {
        /// <summary>
        /// 进行集合操作时取左操作数,
        /// 因为左边可能是集合数，也可能是单个数字
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected object CLOperData(int i)
        {
            bool b = leftCollVal != null;
            return b ? leftCollVal[i] : LeftSigelVal;
        }

        /// <summary>
        /// 进行集合操作时取右操作数,
        /// 因为右边可能是集合数，也可能是单个数字
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected object CROperData(int i)
        {
            bool b = RightCollVal != null;
            return b ? RightCollVal[i] : RightSigelVal;
        }

        /// <summary>
        /// 集合操作内部实现
        /// </summary>
        /// <param name="lVal"></param>
        /// <param name="rVal"></param>
        /// <returns></returns>
        protected virtual object DoSingleOperate(object lVal, object rVal)
        {
            return new object();
        }

        /// <summary>
        /// 重写集合操作
        /// </summary>
        /// <returns></returns>
        protected sealed override CalResult CollectOperate()
        {
            CalResult result = new CalResult();
            result.Results = new object[CLLen()];
            result.ResultType = typeof(bool);


            for (int i = 0; i < result.Results.Length; i++)
            {
                if (CLOperData(i) == DelayCalResult.Delay || CROperData(i) == DelayCalResult.Delay)
                {
                    result.Results[i] = DelayCalResult.Delay;
                }
                else
                {
                    result.Results[i] = DoSingleOperate(CLOperData(i), CROperData(i));
                }
            }

            return result;
        }

        protected sealed override CalResult SingOperate()
        {
            //Type T = Comm.GetType(LeftSigelVal.ToString());

            return new CalResult
            {
                //ResultType = T,
                Result = DoSingleOperate(LeftSigelVal, RightSigelVal)
            };
        }
    }
}
