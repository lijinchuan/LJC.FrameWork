﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LJC.FrameWork.Net.DNS
{
    /// <summary>
    /// This enum holds DNS QCLASS value. Defined in RFC 1035 3.2.4.
    /// </summary>
    public enum DNS_QClass
    {
        /// <summary>
        /// Internet class.
        /// </summary>
        IN = 1,

        /// <summary>
        /// Any class.
        /// </summary>
        Any = 255,
    }
}
