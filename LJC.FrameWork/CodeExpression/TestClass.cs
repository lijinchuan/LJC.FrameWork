using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    public class TestClass
    {
        public void TestDateAdd()
        {
            var code = @"x:DateTime('2020-01-01 08:30');
                 lastyear:DateAdd(x,-1,'y');
                 nextyear:DateAdd(x,1,'y');
                 lastmon:DateAdd(x,-1,'mon');
                 nextmon:DateAdd(x,1,'mon');
                 lastday:DateAdd(x,-1,'day');
                 nextday:DateAdd(x,1,'day');
                 lasthouer:DateAdd(x,-1,'h');
                 nexthouer:DateAdd(x,1,'h');";
            /*code = @"x:0;
            if x<=10 then 3+2 else x:x+1 end";*/
            LJC.FrameWork.CodeExpression.ExpressCode ec = new LJC.FrameWork.CodeExpression.ExpressCode(code);
            //var dt = Convert.ToDateTime("2021-04-10");
            var ts = DateTime.Now;
            var rslt = ec.CallResult();
            

        }
    }
}
