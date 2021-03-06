﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    internal class OutputValueSign : CalSign
    {
        public override int Priority
        {
            get
            {
                return (int)SignPriorityEnum.outputSetVal;
            }
        }

        public override string Sign
        {
            get
            {
                return ":=";
            }
        }

        public override int Params
        {
            get
            {
                return 2;
            }
        }

        public OutputValueSign(CalCurrent pool)
        {
            this.CalCurrent = pool;
        }

        protected override CalResult CollectOperate()
        {
            return SingOperate();
        }

        protected override CalResult SingOperate()
        {
            string key = (string)(LeftVal as CalResult).Result;
            //if (!new Regex(@"^[A-z]{1}[A-z0-9]{0,}$").IsMatch(key))
            //if(!Comm.MatchValNameExpress(key).Success)
            if (!Comm.IsValName(key))
            {
                throw new ExpressErrorException("错误的变量命名，只能以英文字母开头的以字母和数字组成的字符串构成！");
            }

            //KeyValuePair<string, CalResult> kvl = CurrStockDataCalPool.VarDataPool.FirstOrDefault(f => f.Key == key);
            //if (kvl.Value == null)
            //{
            //    CurrStockDataCalPool.VarDataPool.Add(key, RightVal);
            //}
            //else
            //{
            //    throw new ExpressErrorException("变量" + key + "已定义。");
            //}
            if (this.CalCurrent.VarDataPool.ContainsKey(key))
            {
                this.CalCurrent.VarDataPool[key] = RightVal;
            }
            else
            {
                throw new ExpressErrorException("变量" + key + "未定义。");
            }

            return RightVal;
        }
    }
}
