using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Collections
{
    public class StackEx : Stack
    {
        public bool TryPop(out object pop)
        {
            pop = null;
            if (IsEmpty())
                return false;

            pop = Pop();
            return true;
        }

        public bool TryPop<T>(out T pop)
        {
            pop = default(T);
            if (IsEmpty())
                return false;

            var t = Pop();
            if (t is T)
            {
                pop = (T)t;
            }
            return true;
        }

        public T Peek<T>()
        {
            if (IsEmpty())
                return default(T);

            var t = Peek();
            if (t is T)
            {
                return (T)t;
            }

            return default(T);
        }

        public bool IsEmpty()
        {
            return this.Count == 0;
        }
    }
}
