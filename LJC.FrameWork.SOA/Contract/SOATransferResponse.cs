﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    public class SOATransferResponse
    {
        public string ClientId
        {
            get;
            set;
        }

        public string ClientTransactionID
        {
            get;
            set;
        }

        private bool _isSuccess = false;
        public bool IsSuccess
        {
            get
            {
                return _isSuccess;
            }
            set
            {
                _isSuccess = value;
            }
        }

        public DateTime ResponseTime
        {
            get;
            set;
        }

        public string ErrMsg
        {
            get;
            set;
        }

        public byte[] Result
        {
            get;
            set;
        }
    }
}
