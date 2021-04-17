using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class ArrayOf: SysFun
    {
        public override string SignName => "ArrayOf";

        public override int Params => 2;

        public override CalResult Operate()
        {
            if (!(param1 is object[]))
            {
                throw new ArgumentException("ArrayOf第一个参数必须是数组");
            }
            var arr = (object[])param1;
            
            var index = Convert.ToInt32(param2);
            if (index < 0)
            {
                index = arr.Length + index;
            }
            if (index < 0 || index >= arr.Length)
            {
                throw new ArgumentException("ArrayOf下标错误");
            }
            return new CalResult
            {
                Result = ((object[])param1)[index]
            };
        }
    }
}
