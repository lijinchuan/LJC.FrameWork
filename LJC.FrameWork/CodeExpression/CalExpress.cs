using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    /// <summary>
    /// 表达式
    /// </summary>
    internal class CalExpress : IExpressPart
    {

        public string Express
        {
            get;
            set;
        }

        private object _value;
        public object Value
        {
            get
            {
                if (_value != null)
                {
                    return _value;
                }
                else
                {
                    if (this is StringSign)
                    {
                        _value = Express;
                    }
                    else
                    {
                        _value = Comm.Parse(Express);
                    }
                    return _value;
                }
            }
        }

        public CalExpress(string express, int modeId = 0)
        {
            if (string.IsNullOrWhiteSpace(express))
                throw new ExpressErrorException("表达式不能为空！");

            Express = express;
            ModeID = modeId;
        }

        private int modeid;

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


        public int CodeLine
        {
            get;
            set;
        }
    }
}
