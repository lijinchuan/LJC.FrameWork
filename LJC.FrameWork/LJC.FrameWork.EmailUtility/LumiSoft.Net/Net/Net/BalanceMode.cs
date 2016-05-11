using System;
using System.Collections.Generic;
using System.Text;

namespace LJC.FrameWork.Net
{
    /// <summary>
    /// This enum specified balance mode.
    /// </summary>
    public enum BalanceMode
    {
        /// <summary>
        /// Operation is load balanched by all workers.
        /// </summary>
        LoadBalance,

        /// <summary>
        /// Operation will be handed over to next worker, if last one fails.
        /// </summary>
        FailOver,
    }
}
