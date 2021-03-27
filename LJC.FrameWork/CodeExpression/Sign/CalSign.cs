using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    [Serializable]
    /// <summary>
    /// 运算符
    /// </summary>
    public abstract class CalSign : IExpressPart
    {
        /// <summary>
        /// 变量存储
        /// </summary>
        protected CalCurrent CalCurrent
        {
            get;
            set;
        }

        /// <summary>
        /// 执行运算之前执行
        /// </summary>
        public Action OnBeginOperator;

        public virtual string SignName
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 符号
        /// </summary>
        public virtual string Sign
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 优先级
        /// </summary>
        public virtual int Priority
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 操作时传过来的左值，值里面包含单个的值result和集合值results
        /// </summary>
        public virtual CalResult LeftVal
        {
            get;
            set;
        }

        public object LeftSigelVal
        {
            get
            {
                if (LeftVal != null)
                    return LeftVal.Result;

                return null;
            }
        }

        /// <summary>
        /// 上一步左值计算的结果如果是一个集合，则此值有值
        /// </summary>
        public object[] leftCollVal
        {
            get
            {
                if (LeftVal != null)
                    return LeftVal.Results;

                return null;
            }
        }

        /// <summary>
        /// 操作时传过来的右值，值里面包含单个的值result和集合值results
        /// </summary>
        public CalResult RightVal
        {
            get;
            set;
        }

        /// <summary>
        /// 上一步右值计算的结果如果是一个单值，则此值有值
        /// </summary>
        public object RightSigelVal
        {
            get
            {
                if (RightVal != null)
                    return RightVal.Result;

                return null;
            }
        }

        /// <summary>
        /// 集合操作时取集合长度
        /// </summary>
        /// <returns></returns>
        protected int CLLen()
        {
            if (leftCollVal != null)
                return leftCollVal.Length;

            if (RightCollVal != null)
                return RightCollVal.Length;

            throw new ExpressErrorException("左值和右值都不包含一个集合数，无法取集合操作数长度。");
        }

        /// <summary>
        /// 上一步右值计算的结果如果是一个集合，则此值有值
        /// </summary>
        public object[] RightCollVal
        {
            get
            {
                if (RightVal != null)
                    return RightVal.Results;

                return null;
            }
        }

        /// <summary>
        /// 如果左值或者右值是集合数，则调用此方法，计算出来的结果也是一个集合数
        /// </summary>
        /// <returns></returns>
        protected virtual CalResult CollectOperate()
        {
            return default(CalResult);
        }

        /// <summary>
        /// 如果左值或者右值是单数，则调用此方法，计算出来的结果也是一个单数
        /// </summary>
        /// <returns></returns>
        protected virtual CalResult SingOperate()
        {
            ExeTimes++;
            return default(CalResult);
        }

        protected virtual bool Check()
        {
            return true;
        }

        public virtual CalResult Operate()
        {
            ExeTimes++;
            var now = DateTime.Now;
            try
            {
                if (this.OnBeginOperator != null)
                {
                    OnBeginOperator();
                }
                if (Params > 0 && !(this is FunSign) && RightVal == null)
                {
                    throw new ExpressErrorException(this.SignName + "缺少右值！");
                }

                if (Params > 1 && LeftVal == null)
                {
                    throw new ExpressErrorException(this.SignName + "缺少左值！");
                }

                if ((this is FunSign))
                {
                    FunSign fs = (FunSign)this;

                    if (fs.HasArrayParam())
                    {
                        return CollectOperate();
                    }
                }

                //CommFun.Assert(Check());

                if ((LeftVal == null || LeftVal.Results == null)
                    && (RightVal == null || RightVal.Results == null))
                {
                    return SingOperate();
                }

                if (LeftVal != null && LeftVal.Results != null
                    && RightVal != null && RightVal.Results != null
                    && LeftVal.Results.Length != RightVal.Results.Length
                    )
                {
                    throw new ExpressErrorException("无法操作，左右集合维数不相同。");
                }

                return CollectOperate();
            }
            finally
            {
                ExeTicks += (DateTime.Now.Subtract(now).TotalMilliseconds);
            }
        }



        /// <summary>
        /// 参数个数
        /// </summary>
        public virtual int Params
        {
            get
            {
                return 2;
            }
        }

        private int modeid;
        /// <summary>
        /// 分解表达式时赋予的一个步骤的序列号
        /// </summary>
        public int ModeID
        {
            get
            {
                return modeid;
            }
            set
            {
                modeid = value;
            }
        }


        public int StartIndex
        {
            get;
            set;
        }

        public int EndIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 执行总时长
        /// </summary>
        public double ExeTicks
        {
            get;
            set;
        }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int ExeTimes
        {
            get;
            private set;
        }


        public int CodeLine
        {
            get;
            set;
        }
    }
}
