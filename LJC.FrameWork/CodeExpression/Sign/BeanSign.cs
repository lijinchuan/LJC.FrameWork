using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class BeanSign : CalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.bean;
            }
        }

        public override string Sign
        {
            get
            {
                return ",";
            }
        }

        public override string SignName
        {
            get
            {
                return ",";
            }
        }

        public override int Params
        {
            get
            {
                return 2;
            }
        }

        protected override CalResult SingOperate()
        {

            List<object> li = new List<object>();
            if (LeftSigelVal is object[])
            {
                object[] vals = (object[])LeftSigelVal;
                foreach (object o in vals)
                {
                    if (o is CalResult)
                    {
                        li.Add(o);
                    }
                }
            }
            else
            {
                li.Add(LeftVal);
            }

            if (RightSigelVal is CalResult[])
            {
                li.AddRange((CalResult[])RightSigelVal);
            }
            else
            {
                li.Add(RightVal);
            }

            //Console.WriteLine("执行时长:"+this.ExeTicks+",执行次数:"+this.ExeTimes);
            return new CalResult
            {
                Result = li.ToArray(),
                ResultType = typeof(CalResult[])
            };
        }

        protected override CalResult CollectOperate()
        {
            return SingOperate();
        }
    }
}
