using LJC.FrameWork.Data.QuickDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    /// <summary>
    /// 运行配置
    /// </summary>
    [Serializable]
    [ReportAttr(TableName = "tb_RunConfig")]
    public class RunConfig : IDBCachAble
    {
        public object ParentControl
        {
            get;
            set;
        }

        [NonSerialized]
        public Action UIRefrash;

        /// <summary>
        /// 控件装载完后发生
        /// </summary>
        [NonSerialized]
        public Action UILoadFinished;

        protected void Refrash()
        {
            if (UIRefrash != null)
                UIRefrash();
        }

        public void ClearUIRefrashEvent()
        {
            UIRefrash = null;
        }

        [NonSerialized]
        protected bool _isLoad;
        public virtual bool IsLoad
        {
            get
            {
                return _isLoad;
            }
            set
            {

                if (_isLoad && !value)
                {
                    if (UILoadFinished != null)
                        UILoadFinished();
                }

                _isLoad = value;
            }
        }

        /// <summary>
        /// UI是否创建
        /// </summary>
        public bool IsUICreate
        {
            get;
            set;
        }

        [ReportAttr(Column = "ID", isKey = true)]
        public int ID
        {
            get;
            set;
        }
        /// <summary>
        /// 运行自动交易指令服务,生成交易指令
        /// </summary>
        [ReportAttr(Column = "RunCmdServer")]
        public bool RunCmdServer
        {
            get;
            set;
        }
        /// <summary>
        /// 运行自动交易程序,执行自动买卖指令
        /// </summary>
        [ReportAttr(Column = "RunCmdClient")]
        public bool RunCmdClient
        {
            get;
            set;
        }

        /// <summary>
        /// 测试模式，不会发送自动买卖指令
        /// </summary>
        [ReportAttr(Column = "TestMode")]
        public bool TestMode
        {
            get;
            set;
        }

        [ReportAttr(Column = "LogDebug")]
        public bool LogDebug
        {
            get;
            set;
        }

        public string GetCollectCachKey()
        {
            return "tb_RunConfig_TableData";
        }

        public bool Update
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    if (this.ID == 0)
                    {
                        DataContextMoudelFactory<RunConfig>.GetDataContext(this).Add();
                    }
                    else
                    {
                        DataContextMoudelFactory<RunConfig>.GetDataContext(this).Update();
                    }
                }
            }
        }

        [ReportAttr(Column = "AdjustCostPrice")]
        public bool AdjustCostPrice
        {
            get;
            set;
        }

        private double _stopLoss;
        [ReportAttr(Column = "StopLoss")]
        public double StopLoss
        {
            get
            {
                return _stopLoss;
            }
            set
            {
                if (value < 1 || value > 15)
                {

                    return;
                }

                _stopLoss = value;
            }
        }

        [ReportAttr(Column = "StopEarn")]
        public double StopEarn
        {
            get;
            set;
        }

        [ReportAttr(Column = "AutoStopLost")]
        public bool AutoStopLost
        {
            get;
            set;
        }

        [ReportAttr(Column = "AutoStopEarn")]
        public bool AutoStopEarn
        {
            get;
            set;
        }

        [ReportAttr(Column = "StopEarnRate")]
        public double StopEarnRate
        {
            get;
            set;
        }

        [ReportAttr(Column = "UseHXDayQuote")]
        public bool UseHXDayQuote
        {
            get;
            set;
        }

        [ReportAttr(Column = "UseSHDayQuote")]
        public bool UseSHDayQuote
        {
            get;
            set;
        }

        private string _defaultEmailAccount;
        [ReportAttr(Column = "DefaultEmailAccount", IsEncry = true)]
        public string DefaultEmailAccount
        {
            get
            {
                return _defaultEmailAccount;
            }
            set
            {
                if (!value.ToLower().Contains("@sina.")
                    && !value.ToLower().Contains("@qq.com"))
                {
                    return;
                }

                this._defaultEmailAccount = value;
            }
        }

        private string _defaultEmailPwd;
        [ReportAttr(Column = "DefaultEailPwd", IsEncry = true)]
        public string DefaultEmailPwd
        {
            get
            {
                return _defaultEmailPwd;
            }
            set
            {
                _defaultEmailPwd = value;
            }
        }

        private bool _sellOrBuyNeedEmailCommit;
        [ReportAttr(Column = "SellOrBuyNeedEmailCommit")]
        public bool SellOrBuyNeedEmailCommit
        {
            get
            {
                return _sellOrBuyNeedEmailCommit;
            }
            set
            {
                _sellOrBuyNeedEmailCommit = value;
            }
        }

        [ReportAttr(Column = "DefaultReciveEmailAccount")]
        public string DefaultReciveEmailAccount
        {
            get;
            set;
        }

        [ReportAttr(Column = "CmdPhoneNumber")]
        public string CmdPhoneNumber
        {
            get;
            set;
        }

        [ReportAttr(Column = "StockPostions")]
        public int StockPostions
        {
            get;
            set;
        }

        [ReportAttr(Column = "SMSState")]
        public bool SMSState
        {
            get;
            set;
        }

        [ReportAttr(Column = "UserID")]
        public string UserID
        {
            get;
            set;
        }

        public RunConfig()
        {
            //this.UILoadFinished += () => this.encryptTypes = null;
        }
    }
}
