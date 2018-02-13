using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.Comm.Coroutine
{
    public interface ICoroutineUnit
    {
        bool IsSuccess();

        bool IsDone();

        bool IsTimeOut();

        /// <summary>
        /// 执行体
        /// </summary>
        void Exceute();

        object GetResult();

        void CallBack(CoroutineCallBackEventArgs args);
    }
}
