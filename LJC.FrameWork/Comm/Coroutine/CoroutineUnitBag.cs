using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.Comm.Coroutine
{
    internal class CoroutineUnitBag
    {
        public long Id
        {
            get;
            private set;
        }

        private bool _hasError = false;
        public bool HasError
        {
            get
            {
                return _hasError;
            }
            set
            {
                _hasError = value;
            }
        }

        public Exception Error
        {
            get;
            set;
        }

        public ICoroutineUnit CUnit
        {
            get;
            private set;
        }

        public CoroutineUnitBag(long id, ICoroutineUnit cunit)
        {
            Id = id;
            CUnit = cunit;
        }

        public void Exceute()
        {
            CUnit.Exceute();
        }

        public bool IsDone()
        {
            return CUnit.IsDone();
        }

        public bool IsSuccess()
        {
            return CUnit.IsSuccess();
        }

        public bool IsTimeOut()
        {
            return CUnit.IsTimeOut();
        }

        public void CallBack(CoroutineCallBackEventArgs args)
        {
            CUnit.CallBack(args);
        }
    }
}
