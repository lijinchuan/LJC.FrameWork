using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Data.QuickDataBase
{
    [Serializable]
    public class Mess_Three<F, S, T>
    {
        public F First
        {
            get;
            set;
        }

        public S Second
        {
            get;
            set;
        }

        public T Thrid
        {
            get;
            set;
        }

        public Mess_Three(F first, S second, T thrid)
        {
            First = first;
            Second = second;
            Thrid = thrid;
        }
    }

    [Serializable]
    public class Mess_Four<F, S, T, FF>
    {
        public F First
        {
            get;
            set;
        }

        public S Second
        {
            get;
            set;
        }

        public T Thrid
        {
            get;
            set;
        }

        public FF four
        {
            get;
            set;
        }

        public Mess_Four(F first, S second, T thrid, FF fur)
        {
            First = first;
            Second = second;
            Thrid = thrid;
            four = fur;
        }
    }
}
