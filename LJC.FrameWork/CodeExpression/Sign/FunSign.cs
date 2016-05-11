using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    public class FunSign : CalSign
    {
        public bool HasArrayParam()
        {
            for (int i = 0; ; i++)
            {
                if (GetPara(i) == null)
                    return false;

                if (GetPara(i) != null
                    && (GetPara(i) is object[]))
                {
                    return true;
                }

            }
        }

        public override CalResult Operate()
        {
            for (int i = 0; i < this.Params; i++)
            {
                if (GetPara(i) == null)
                {
                    throw new ExpressErrorException("第" + (i + 1) + "个参数为空！");
                }
            }

            return base.Operate();
        }

        private object[] paras;
        /// <summary>
        /// 最多提供16个参数，超出16个，调用些方法获取
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected object GetPara(int i)
        {

            if (RightVal != null)
            {
                if (paras == null)
                {
                    if (RightSigelVal is object[])
                        paras = (object[])RightSigelVal;
                    else if (RightVal != null)
                    {
                        paras = new object[] { RightVal };
                    }
                    else
                    {
                        return null;
                    }
                }


                if (i >= paras.Length)
                    return null;

                CalResult cr = paras[i] as CalResult;
                return cr.Results ?? cr.Result;
            }

            return null;

        }

        #region 快速得到参数
        protected object param1
        {
            get
            {
                return GetPara(0);
            }
        }

        protected object param2
        {
            get
            {
                return GetPara(1);
            }
        }

        protected object param3
        {
            get
            {
                return GetPara(2);
            }
        }
        protected object param4
        {
            get
            {
                return GetPara(3);
            }
        }
        protected object param5
        {
            get
            {
                return GetPara(4);
            }
        }
        protected object param6
        {
            get
            {
                return GetPara(5);
            }
        }
        protected object param7
        {
            get
            {
                return GetPara(6);
            }
        }
        protected object param8
        {
            get
            {
                return GetPara(7);
            }
        }
        protected object param9
        {
            get
            {
                return GetPara(8);
            }
        }
        protected object param10
        {
            get
            {
                return GetPara(9);
            }
        }
        protected object param11
        {
            get
            {
                return GetPara(10);
            }
        }

        protected object param12
        {
            get
            {
                return GetPara(11);
            }
        }
        protected object param13
        {
            get
            {
                return GetPara(12);
            }
        }
        protected object param14
        {
            get
            {
                return GetPara(13);
            }
        }
        protected object param15
        {
            get
            {
                return GetPara(14);
            }
        }
        protected object param16
        {
            get
            {
                return GetPara(15);
            }
        }
        #endregion

        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.fun;
            }
        }

        public override string Sign
        {
            get
            {
                return "()";
            }
        }

        private string funName;
        /// <summary>
        /// 函数名
        /// </summary>
        public string FunName
        {
            get
            {
                return "fun";
            }
            set
            {
                funName = value;
            }
        }

        public override int Params
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ParamString
        {
            get;
            set;
        }
    }
}
