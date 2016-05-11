using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using LJC.FrameWork.Data.QuickDataBase;
using ProtoBuf;

namespace LJC.FrameWork.Test
{
    [ProtoContract()]
    public class O_PkgOrderEntityList
    {
        [ProtoMember(1)]
        public List<O_PkgOrderEntity> ItemList
        {
            get;
            set;
        }
    }

    [ProtoContract()]
    [ReportAttr(TableName = "O_PkgOrder")]
    public class O_PkgOrderEntity
    {
        //表名
        public const string TbName = "PkgOrderDB.O_PkgOrder";

        private int _orderID;
        [ProtoMember(1)]
        [ReportAttr(Column = "orderID", isKey = true)]
        [Display(Name = "订单id")]
        public int OrderID
        {
            get
            {
                return _orderID;
            }
            set
            {
                _orderID = value;
            }
        }
        private int _pkg;
        [ProtoMember(2)]
        [ReportAttr(Column = "pkg")]
        [Display(Name = "bbbb")]
        public int Pkg
        {
            get
            {
                return _pkg;
            }
            set
            {
                _pkg = value;
            }
        }
        private string _pkgName;
        [ProtoMember(3)]
        [ReportAttr(Column = "pkgName")]
        [Display(Name = "度假产品名")]
        public string PkgName
        {
            get
            {
                return _pkgName;
            }
            set
            {
                _pkgName = value;
            }
        }
        private DateTime _orderDate;
        [ProtoMember(4)]
        [ReportAttr(Column = "orderDate")]
        [Display(Name = "订单时间")]
        public DateTime OrderDate
        {
            get
            {
                return _orderDate;
            }
            set
            {
                _orderDate = value;
            }
        }
        private string _uid;
        [ProtoMember(5)]
        [ReportAttr(Column = "uid")]
        [Display(Name = "Ctrip用户名")]
        public string Uid
        {
            get
            {
                return _uid;
            }
            set
            {
                _uid = value;
            }
        }
        private string _ctripCardNo;
        [ProtoMember(6)]
        [ReportAttr(Column = "ctripCardNo")]
        [Display(Name = "ctrip卡号")]
        public string CtripCardNo
        {
            get
            {
                return _ctripCardNo;
            }
            set
            {
                _ctripCardNo = value;
            }
        }
        private string _eid;
        [ProtoMember(7)]
        [ReportAttr(Column = "eid")]
        [Display(Name = "订单操作员")]
        public string Eid
        {
            get
            {
                return _eid;
            }
            set
            {
                _eid = value;
            }
        }
        private string _orderStatus;
        [ProtoMember(8)]
        [ReportAttr(Column = "orderStatus")]
        [Display(Name = "订单状态S 成交 P 处理中 C 取消 U 未提交 W 已提交待处理")]
        public string OrderStatus
        {
            get
            {
                return _orderStatus;
            }
            set
            {
                _orderStatus = value;
            }
        }
        private int _processStatus;
        [ProtoMember(9)]
        [ReportAttr(Column = "processStatus")]
        [Display(Name = "处理状态0未操作1确认产品2确认客户4已发件 8已收款 16已付款64已取件128已出票 256已发旅游协议512通知出票1024申请中2048已扣款4096满位其它已操作")]
        public int ProcessStatus
        {
            get
            {
                return _processStatus;
            }
            set
            {
                _processStatus = value;
            }
        }
        private string _currency;
        [ProtoMember(10)]
        [ReportAttr(Column = "currency")]
        [Display(Name = "底价货币")]
        public string Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                _currency = value;
            }
        }
        private decimal _roomDiscount;
        [ProtoMember(11)]
        [ReportAttr(Column = "roomDiscount")]
        [Display(Name = "房间折扣")]
        public decimal RoomDiscount
        {
            get
            {
                return _roomDiscount;
            }
            set
            {
                _roomDiscount = value;
            }
        }
        private decimal _amount;
        [ProtoMember(12)]
        [ReportAttr(Column = "amount")]
        [Display(Name = "订单卖价")]
        public decimal Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;
            }
        }
        private decimal _cost;
        [ProtoMember(13)]
        [ReportAttr(Column = "cost")]
        [Display(Name = "订单底价")]
        public decimal Cost
        {
            get
            {
                return _cost;
            }
            set
            {
                _cost = value;
            }
        }
        private string _prepayType;
        [ProtoMember(14)]
        [ReportAttr(Column = "prepayType")]
        [Display(Name = "收款方式")]
        public string PrepayType
        {
            get
            {
                return _prepayType;
            }
            set
            {
                _prepayType = value;
            }
        }
        private DateTime _takeoffDate;
        [ProtoMember(15)]
        [ReportAttr(Column = "takeoffDate")]
        [Display(Name = "出发日期")]
        public DateTime TakeoffDate
        {
            get
            {
                return _takeoffDate;
            }
            set
            {
                _takeoffDate = value;
            }
        }
        private DateTime _returnDate;
        [ProtoMember(16)]
        [ReportAttr(Column = "returnDate")]
        [Display(Name = "返回日期")]
        public DateTime ReturnDate
        {
            get
            {
                return _returnDate;
            }
            set
            {
                _returnDate = value;
            }
        }
        private DateTime _arrival;
        [ProtoMember(17)]
        [ReportAttr(Column = "arrival")]
        [Display(Name = "入住时间")]
        public DateTime Arrival
        {
            get
            {
                return _arrival;
            }
            set
            {
                _arrival = value;
            }
        }
        private DateTime _departure;
        [ProtoMember(18)]
        [ReportAttr(Column = "departure")]
        [Display(Name = "离店时间")]
        public DateTime Departure
        {
            get
            {
                return _departure;
            }
            set
            {
                _departure = value;
            }
        }
        private string _includingHotel;
        [ProtoMember(19)]
        [ReportAttr(Column = "includingHotel")]
        [Display(Name = "包含酒店")]
        public string IncludingHotel
        {
            get
            {
                return _includingHotel;
            }
            set
            {
                _includingHotel = value;
            }
        }
        private string _includingFlight;
        [ProtoMember(20)]
        [ReportAttr(Column = "includingFlight")]
        [Display(Name = "包含航班")]
        public string IncludingFlight
        {
            get
            {
                return _includingFlight;
            }
            set
            {
                _includingFlight = value;
            }
        }
        private int _startCity;
        [ProtoMember(21)]
        [ReportAttr(Column = "startCity")]
        [Display(Name = "出发城市")]
        public int StartCity
        {
            get
            {
                return _startCity;
            }
            set
            {
                _startCity = value;
            }
        }
        private int _hotel;
        [ProtoMember(22)]
        [ReportAttr(Column = "hotel")]
        [Display(Name = "酒店")]
        public int Hotel
        {
            get
            {
                return _hotel;
            }
            set
            {
                _hotel = value;
            }
        }
        private int _room;
        [ProtoMember(23)]
        [ReportAttr(Column = "room")]
        [Display(Name = "房型")]
        public int Room
        {
            get
            {
                return _room;
            }
            set
            {
                _room = value;
            }
        }
        private int _roomQty;
        [ProtoMember(24)]
        [ReportAttr(Column = "roomQty")]
        [Display(Name = "房间数")]
        public int RoomQty
        {
            get
            {
                return _roomQty;
            }
            set
            {
                _roomQty = value;
            }
        }
        private int _flightOption;
        [ProtoMember(25)]
        [ReportAttr(Column = "flightOption")]
        [Display(Name = "航班选项")]
        public int FlightOption
        {
            get
            {
                return _flightOption;
            }
            set
            {
                _flightOption = value;
            }
        }
        private int _numAdult;
        [ProtoMember(26)]
        [ReportAttr(Column = "numAdult")]
        [Display(Name = "成人数")]
        public int NumAdult
        {
            get
            {
                return _numAdult;
            }
            set
            {
                _numAdult = value;
            }
        }
        private int _numChild;
        [ProtoMember(27)]
        [ReportAttr(Column = "numChild")]
        [Display(Name = "儿童数")]
        public int NumChild
        {
            get
            {
                return _numChild;
            }
            set
            {
                _numChild = value;
            }
        }
        private int _numBaby;
        [ProtoMember(28)]
        [ReportAttr(Column = "numBaby")]
        [Display(Name = "婴儿数")]
        public int NumBaby
        {
            get
            {
                return _numBaby;
            }
            set
            {
                _numBaby = value;
            }
        }
        private int _numChildNoBed;
        [ProtoMember(29)]
        [ReportAttr(Column = "numChildNoBed")]
        [Display(Name = "不占床儿童数")]
        public int NumChildNoBed
        {
            get
            {
                return _numChildNoBed;
            }
            set
            {
                _numChildNoBed = value;
            }
        }
        private string _confirmType;
        [ProtoMember(30)]
        [ReportAttr(Column = "confirmType")]
        [Display(Name = "联系人确认方式 NON不用确认 CSM短信确认 TEL电话确认 EML邮件确认 FAX传真确认")]
        public string ConfirmType
        {
            get
            {
                return _confirmType;
            }
            set
            {
                _confirmType = value;
            }
        }
        private string _contactName;
        [ProtoMember(31)]
        [ReportAttr(Column = "contactName")]
        [Display(Name = "联系人姓名")]
        public string ContactName
        {
            get
            {
                return _contactName;
            }
            set
            {
                _contactName = value;
            }
        }
        private string _contactTel;
        [ProtoMember(32)]
        [ReportAttr(Column = "contactTel")]
        [Display(Name = "联系人电话")]
        public string ContactTel
        {
            get
            {
                return _contactTel;
            }
            set
            {
                _contactTel = value;
            }
        }
        private string _contactFax;
        [ProtoMember(33)]
        [ReportAttr(Column = "contactFax")]
        [Display(Name = "联系人传真")]
        public string ContactFax
        {
            get
            {
                return _contactFax;
            }
            set
            {
                _contactFax = value;
            }
        }
        private string _contactMobile;
        [ProtoMember(34)]
        [ReportAttr(Column = "contactMobile")]
        [Display(Name = "联系人手机")]
        public string ContactMobile
        {
            get
            {
                return _contactMobile;
            }
            set
            {
                _contactMobile = value;
            }
        }
        private string _contactEmail;
        [ProtoMember(35)]
        [ReportAttr(Column = "contactEmail")]
        [Display(Name = "联系人email")]
        public string ContactEmail
        {
            get
            {
                return _contactEmail;
            }
            set
            {
                _contactEmail = value;
            }
        }
        private string _contactAddress;
        [ProtoMember(36)]
        [ReportAttr(Column = "contactAddress")]
        [Display(Name = "联系人地址")]
        public string ContactAddress
        {
            get
            {
                return _contactAddress;
            }
            set
            {
                _contactAddress = value;
            }
        }
        private string _getTicketType;
        [ProtoMember(37)]
        [ReportAttr(Column = "getTicketType")]
        [Display(Name = "取票方式 SND市内配送 NUL不取不送 JOU邮寄行程单 EMS GET市内自取")]
        public string GetTicketType
        {
            get
            {
                return _getTicketType;
            }
            set
            {
                _getTicketType = value;
            }
        }
        private int _attrib1;
        [ProtoMember(38)]
        [ReportAttr(Column = "attrib1")]
        [Display(Name = "产品类型 同产品")]
        public int Attrib1
        {
            get
            {
                return _attrib1;
            }
            set
            {
                _attrib1 = value;
            }
        }
        private int _attrib2;
        [ProtoMember(39)]
        [ReportAttr(Column = "attrib2")]
        [Display(Name = "产品类型2")]
        public int Attrib2
        {
            get
            {
                return _attrib2;
            }
            set
            {
                _attrib2 = value;
            }
        }
        private string _standard;
        [ProtoMember(40)]
        [ReportAttr(Column = "standard")]
        [Display(Name = "处理类型 T标准 F1非标准 F2团体 产品中默认T4 所以这里用T4")]
        public string Standard
        {
            get
            {
                return _standard;
            }
            set
            {
                _standard = value;
            }
        }
        private string _remarks;
        [ProtoMember(41)]
        [ReportAttr(Column = "remarks")]
        [Display(Name = "备注")]
        public string Remarks
        {
            get
            {
                return _remarks;
            }
            set
            {
                _remarks = value;
            }
        }
        private string _serverFrom;
        [ProtoMember(42)]
        [ReportAttr(Column = "serverFrom")]
        [Display(Name = "下单服务器")]
        public string ServerFrom
        {
            get
            {
                return _serverFrom;
            }
            set
            {
                _serverFrom = value;
            }
        }
        private string _isOnline;
        [ProtoMember(43)]
        [ReportAttr(Column = "isOnline")]
        [Display(Name = "是否online")]
        public string IsOnline
        {
            get
            {
                return _isOnline;
            }
            set
            {
                _isOnline = value;
            }
        }
        private Int16 _urgencyLevel;
        [ProtoMember(44)]
        [ReportAttr(Column = "urgencyLevel")]
        [Display(Name = "紧急程度")]
        public Int16 UrgencyLevel
        {
            get
            {
                return _urgencyLevel;
            }
            set
            {
                _urgencyLevel = value;
            }
        }
        private DateTime _processDeadLine;
        [ProtoMember(45)]
        [ReportAttr(Column = "processDeadLine")]
        [Display(Name = "处理时间")]
        public DateTime ProcessDeadLine
        {
            get
            {
                return _processDeadLine;
            }
            set
            {
                _processDeadLine = value;
            }
        }
        private string _cancelReason;
        [ProtoMember(46)]
        [ReportAttr(Column = "cancelReason")]
        [Display(Name = "取消原因 MODIORDER重下修改单 CC_CLNT客户原因取消 CC_PRIC变价客户取消 FLTFULL机票满位 HTLFULL酒店满位 OTHFULL其他满位 DBLORDER重复预定 PRICONVERT价格倒挂取消 OTHER其他原因 TESTING测试 SPLITORDER分开预订")]
        public string CancelReason
        {
            get
            {
                return _cancelReason;
            }
            set
            {
                _cancelReason = value;
            }
        }
        private string _processDesc;
        [ProtoMember(47)]
        [ReportAttr(Column = "processDesc")]
        [Display(Name = "操作描述")]
        public string ProcessDesc
        {
            get
            {
                return _processDesc;
            }
            set
            {
                _processDesc = value;
            }
        }
        private int _postStatus;
        [ProtoMember(48)]
        [ReportAttr(Column = "postStatus")]
        [Display(Name = "处理状态 0未操作 1确认产品 2确认客户 4已发件 8已收款 16已付款 64已取件 128已出票 256已发旅游协议 512通知出票 1024申请中 2048已扣款 4096满位其它已操作")]
        public int PostStatus
        {
            get
            {
                return _postStatus;
            }
            set
            {
                _postStatus = value;
            }
        }
        private DateTime _finishDate;
        [ProtoMember(49)]
        [ReportAttr(Column = "finishDate")]
        [Display(Name = "成交日期")]
        public DateTime FinishDate
        {
            get
            {
                return _finishDate;
            }
            set
            {
                _finishDate = value;
            }
        }
        private string _pkgType;
        [ProtoMember(50)]
        [ReportAttr(Column = "pkgType")]
        [Display(Name = "查询结果页的产品类型分类 All全部 GroupTravel团队游 FreedomToTravel自由行 Cruise邮轮 LocalTour当地游")]
        public string PkgType
        {
            get
            {
                return _pkgType;
            }
            set
            {
                _pkgType = value;
            }
        }
        private DateTime _sendTicketETime;
        [ProtoMember(51)]
        [ReportAttr(Column = "sendTicketETime")]
        [Display(Name = "送票时间")]
        public DateTime SendTicketETime
        {
            get
            {
                return _sendTicketETime;
            }
            set
            {
                _sendTicketETime = value;
            }
        }
        private DateTime _sendTicketLTime;
        [ProtoMember(52)]
        [ReportAttr(Column = "sendTicketLTime")]
        [Display(Name = "送票时间")]
        public DateTime SendTicketLTime
        {
            get
            {
                return _sendTicketLTime;
            }
            set
            {
                _sendTicketLTime = value;
            }
        }
        private DateTime _fRADate;
        [ProtoMember(53)]
        [ReportAttr(Column = "fRADate")]
        [Display(Name = "日期")]
        public DateTime FRADate
        {
            get
            {
                return _fRADate;
            }
            set
            {
                _fRADate = value;
            }
        }
        private string _hotelRemarks;
        [ProtoMember(54)]
        [ReportAttr(Column = "hotelRemarks")]
        [Display(Name = "酒店描述")]
        public string HotelRemarks
        {
            get
            {
                return _hotelRemarks;
            }
            set
            {
                _hotelRemarks = value;
            }
        }
        private decimal _eMoney;
        [ProtoMember(55)]
        [ReportAttr(Column = "eMoney")]
        [Display(Name = "抵用券金额")]
        public decimal EMoney
        {
            get
            {
                return _eMoney;
            }
            set
            {
                _eMoney = value;
            }
        }
        private Int16 _vipGrade;
        [ProtoMember(56)]
        [ReportAttr(Column = "vipGrade")]
        [Display(Name = "客户等级 0普通 10金牌 20白金 30钻石")]
        public Int16 VipGrade
        {
            get
            {
                return _vipGrade;
            }
            set
            {
                _vipGrade = value;
            }
        }
        private decimal _sendTicketFee;
        [ProtoMember(57)]
        [ReportAttr(Column = "sendTicketFee")]
        [Display(Name = "送票费用")]
        public decimal SendTicketFee
        {
            get
            {
                return _sendTicketFee;
            }
            set
            {
                _sendTicketFee = value;
            }
        }
        private decimal _commission;
        [ProtoMember(58)]
        [ReportAttr(Column = "commission")]
        [Display(Name = "佣金比率")]
        public decimal Commission
        {
            get
            {
                return _commission;
            }
            set
            {
                _commission = value;
            }
        }
        private decimal _adultTax;
        [ProtoMember(59)]
        [ReportAttr(Column = "adultTax")]
        [Display(Name = "成人税")]
        public decimal AdultTax
        {
            get
            {
                return _adultTax;
            }
            set
            {
                _adultTax = value;
            }
        }
        private decimal _childTax;
        [ProtoMember(60)]
        [ReportAttr(Column = "childTax")]
        [Display(Name = "儿童税")]
        public decimal ChildTax
        {
            get
            {
                return _childTax;
            }
            set
            {
                _childTax = value;
            }
        }
        private int _recommend;
        [ProtoMember(61)]
        [ReportAttr(Column = "recommend")]
        [Display(Name = "酒店推荐级别")]
        public int Recommend
        {
            get
            {
                return _recommend;
            }
            set
            {
                _recommend = value;
            }
        }
        private int _provConfirmTime;
        [ProtoMember(62)]
        [ReportAttr(Column = "provConfirmTime")]
        [Display(Name = "供应商确认时间")]
        public int ProvConfirmTime
        {
            get
            {
                return _provConfirmTime;
            }
            set
            {
                _provConfirmTime = value;
            }
        }
        private string _balPeriod;
        [ProtoMember(63)]
        [ReportAttr(Column = "balPeriod")]
        [Display(Name = "未知")]
        public string BalPeriod
        {
            get
            {
                return _balPeriod;
            }
            set
            {
                _balPeriod = value;
            }
        }
        private string _commissionType;
        [ProtoMember(64)]
        [ReportAttr(Column = "commissionType")]
        [Display(Name = "返佣类型 1扣除 2后返 对应表PkgAgent_Balance的balance字段")]
        public string CommissionType
        {
            get
            {
                return _commissionType;
            }
            set
            {
                _commissionType = value;
            }
        }
        private int _specType;
        [ProtoMember(65)]
        [ReportAttr(Column = "specType")]
        [Display(Name = "特殊标志 1南航产品 17招商银行活动产品 18团对行 ")]
        public int SpecType
        {
            get
            {
                return _specType;
            }
            set
            {
                _specType = value;
            }
        }
        private string _typeDesc;
        [ProtoMember(66)]
        [ReportAttr(Column = "typeDesc")]
        [Display(Name = "产品形态")]
        public string TypeDesc
        {
            get
            {
                return _typeDesc;
            }
            set
            {
                _typeDesc = value;
            }
        }
        private int _billheadID;
        [ProtoMember(67)]
        [ReportAttr(Column = "billheadID")]
        [Display(Name = "付款申请单id")]
        public int BillheadID
        {
            get
            {
                return _billheadID;
            }
            set
            {
                _billheadID = value;
            }
        }
        private decimal _flighttax;
        [ProtoMember(68)]
        [ReportAttr(Column = "flighttax")]
        [Display(Name = "机票税")]
        public decimal Flighttax
        {
            get
            {
                return _flighttax;
            }
            set
            {
                _flighttax = value;
            }
        }
        private decimal _fltAmount;
        [ProtoMember(69)]
        [ReportAttr(Column = "fltAmount")]
        [Display(Name = "机票卖价")]
        public decimal FltAmount
        {
            get
            {
                return _fltAmount;
            }
            set
            {
                _fltAmount = value;
            }
        }
        private decimal _fltCost;
        [ProtoMember(70)]
        [ReportAttr(Column = "fltCost")]
        [Display(Name = "机票成本")]
        public decimal FltCost
        {
            get
            {
                return _fltCost;
            }
            set
            {
                _fltCost = value;
            }
        }
        private decimal _htlAmount;
        [ProtoMember(71)]
        [ReportAttr(Column = "htlAmount")]
        [Display(Name = "酒店卖价")]
        public decimal HtlAmount
        {
            get
            {
                return _htlAmount;
            }
            set
            {
                _htlAmount = value;
            }
        }
        private decimal _htlCost;
        [ProtoMember(72)]
        [ReportAttr(Column = "htlCost")]
        [Display(Name = "酒店底价")]
        public decimal HtlCost
        {
            get
            {
                return _htlCost;
            }
            set
            {
                _htlCost = value;
            }
        }
        private decimal _oldAmount;
        [ProtoMember(73)]
        [ReportAttr(Column = "oldAmount")]
        [Display(Name = "原价")]
        public decimal OldAmount
        {
            get
            {
                return _oldAmount;
            }
            set
            {
                _oldAmount = value;
            }
        }
        private decimal _oldCost;
        [ProtoMember(74)]
        [ReportAttr(Column = "oldCost")]
        [Display(Name = "底价")]
        public decimal OldCost
        {
            get
            {
                return _oldCost;
            }
            set
            {
                _oldCost = value;
            }
        }
        private decimal _bail;
        [ProtoMember(75)]
        [ReportAttr(Column = "bail")]
        [Display(Name = "保证金")]
        public decimal Bail
        {
            get
            {
                return _bail;
            }
            set
            {
                _bail = value;
            }
        }
        private decimal _priceAdjust;
        [ProtoMember(76)]
        [ReportAttr(Column = "priceAdjust")]
        [Display(Name = "报价修正")]
        public decimal PriceAdjust
        {
            get
            {
                return _priceAdjust;
            }
            set
            {
                _priceAdjust = value;
            }
        }
        private int _clientConfirmTime;
        [ProtoMember(77)]
        [ReportAttr(Column = "clientConfirmTime")]
        [Display(Name = "最晚客户确认时间")]
        public int ClientConfirmTime
        {
            get
            {
                return _clientConfirmTime;
            }
            set
            {
                _clientConfirmTime = value;
            }
        }
        private bool _isBail;
        [ProtoMember(78)]
        [ReportAttr(Column = "isBail")]
        [Display(Name = "是否有保证金")]
        public bool IsBail
        {
            get
            {
                return _isBail;
            }
            set
            {
                _isBail = value;
            }
        }
        private DateTime _lateBackDate;
        [ProtoMember(79)]
        [ReportAttr(Column = "lateBackDate")]
        [Display(Name = "最后备份日期")]
        public DateTime LateBackDate
        {
            get
            {
                return _lateBackDate;
            }
            set
            {
                _lateBackDate = value;
            }
        }
        private decimal _chdPriceAdjust;
        [ProtoMember(80)]
        [ReportAttr(Column = "chdPriceAdjust")]
        [Display(Name = "儿童卖价调整")]
        public decimal ChdPriceAdjust
        {
            get
            {
                return _chdPriceAdjust;
            }
            set
            {
                _chdPriceAdjust = value;
            }
        }
        private decimal _allbail;
        [ProtoMember(81)]
        [ReportAttr(Column = "allbail")]
        [Display(Name = "?保证金")]
        public decimal Allbail
        {
            get
            {
                return _allbail;
            }
            set
            {
                _allbail = value;
            }
        }
        private string _fCurrency;
        [ProtoMember(82)]
        [ReportAttr(Column = "fCurrency")]
        [Display(Name = "底价币种")]
        public string FCurrency
        {
            get
            {
                return _fCurrency;
            }
            set
            {
                _fCurrency = value;
            }
        }
        private decimal _fCost;
        [ProtoMember(83)]
        [ReportAttr(Column = "fCost")]
        [Display(Name = "个人电子帐户支付金")]
        public decimal FCost
        {
            get
            {
                return _fCost;
            }
            set
            {
                _fCost = value;
            }
        }
        private DateTime _lastPayDate;
        [ProtoMember(84)]
        [ReportAttr(Column = "lastPayDate")]
        [Display(Name = "最后收款日期")]
        public DateTime LastPayDate
        {
            get
            {
                return _lastPayDate;
            }
            set
            {
                _lastPayDate = value;
            }
        }
        private string _isBankUp;
        [ProtoMember(85)]
        [ReportAttr(Column = "isBankUp")]
        [Display(Name = "是否备份订单")]
        public string IsBankUp
        {
            get
            {
                return _isBankUp;
            }
            set
            {
                _isBankUp = value;
            }
        }
        private string _isConfirm;
        [ProtoMember(86)]
        [ReportAttr(Column = "isConfirm")]
        [Display(Name = "是否确认")]
        public string IsConfirm
        {
            get
            {
                return _isConfirm;
            }
            set
            {
                _isConfirm = value;
            }
        }
        private string _isSuperOrder;
        [ProtoMember(87)]
        [ReportAttr(Column = "isSuperOrder")]
        [Display(Name = "是否有子定单")]
        public string IsSuperOrder
        {
            get
            {
                return _isSuperOrder;
            }
            set
            {
                _isSuperOrder = value;
            }
        }
        private string _isNoHotelsuborer;
        [ProtoMember(88)]
        [ReportAttr(Column = "isNoHotelsuborer")]
        [Display(Name = "是否有酒店子定单")]
        public string IsNoHotelsuborer
        {
            get
            {
                return _isNoHotelsuborer;
            }
            set
            {
                _isNoHotelsuborer = value;
            }
        }
        private string _cntactEMSZip;
        [ProtoMember(89)]
        [ReportAttr(Column = "cntactEMSZip")]
        [Display(Name = "ems邮编")]
        public string CntactEMSZip
        {
            get
            {
                return _cntactEMSZip;
            }
            set
            {
                _cntactEMSZip = value;
            }
        }
        private string _isIncUid;
        [ProtoMember(90)]
        [ReportAttr(Column = "isIncUid")]
        [Display(Name = "内部id")]
        public string IsIncUid
        {
            get
            {
                return _isIncUid;
            }
            set
            {
                _isIncUid = value;
            }
        }
        private string _iP;
        [ProtoMember(91)]
        [ReportAttr(Column = "iP")]
        [Display(Name = "下单ip")]
        public string IP
        {
            get
            {
                return _iP;
            }
            set
            {
                _iP = value;
            }
        }
        private int _cardMonth;
        [ProtoMember(92)]
        [ReportAttr(Column = "cardMonth")]
        [Display(Name = "分期付款")]
        public int CardMonth
        {
            get
            {
                return _cardMonth;
            }
            set
            {
                _cardMonth = value;
            }
        }
        private decimal _monthCommission;
        [ProtoMember(93)]
        [ReportAttr(Column = "monthCommission")]
        [Display(Name = "分期付款佣金")]
        public decimal MonthCommission
        {
            get
            {
                return _monthCommission;
            }
            set
            {
                _monthCommission = value;
            }
        }
        private string _isTour;
        [ProtoMember(94)]
        [ReportAttr(Column = "isTour")]
        [Display(Name = "是否团队订单")]
        public string IsTour
        {
            get
            {
                return _isTour;
            }
            set
            {
                _isTour = value;
            }
        }
        private int _paymentType;
        [ProtoMember(95)]
        [ReportAttr(Column = "paymentType")]
        [Display(Name = "1现金 32信用卡 1和32 33现金和信用卡")]
        public int PaymentType
        {
            get
            {
                return _paymentType;
            }
            set
            {
                _paymentType = value;
            }
        }
        private int _canton;
        [ProtoMember(96)]
        [ReportAttr(Column = "canton")]
        [Display(Name = "行政区")]
        public int Canton
        {
            get
            {
                return _canton;
            }
            set
            {
                _canton = value;
            }
        }
        private string _operateModule;
        [ProtoMember(97)]
        [ReportAttr(Column = "operateModule")]
        [Display(Name = "操作描述")]
        public string OperateModule
        {
            get
            {
                return _operateModule;
            }
            set
            {
                _operateModule = value;
            }
        }
        private int _taxiAdultNum;
        [ProtoMember(98)]
        [ReportAttr(Column = "taxiAdultNum")]
        [Display(Name = "带驾用车成人数")]
        public int TaxiAdultNum
        {
            get
            {
                return _taxiAdultNum;
            }
            set
            {
                _taxiAdultNum = value;
            }
        }
        private string _foreignMobile;
        [ProtoMember(99)]
        [ReportAttr(Column = "foreignMobile")]
        [Display(Name = "外国手机")]
        public string ForeignMobile
        {
            get
            {
                return _foreignMobile;
            }
            set
            {
                _foreignMobile = value;
            }
        }
        private string _lang;
        [ProtoMember(100)]
        [ReportAttr(Column = "lang")]
        [Display(Name = "语言")]
        public string Lang
        {
            get
            {
                return _lang;
            }
            set
            {
                _lang = value;
            }
        }
        private string _isLeadorder;
        [ProtoMember(101)]
        [ReportAttr(Column = "isLeadorder")]
        [Display(Name = "是否领队订单")]
        public string IsLeadorder
        {
            get
            {
                return _isLeadorder;
            }
            set
            {
                _isLeadorder = value;
            }
        }
        private string _isAirChina;
        [ProtoMember(102)]
        [ReportAttr(Column = "isAirChina")]
        [Display(Name = "是否国程订单")]
        public string IsAirChina
        {
            get
            {
                return _isAirChina;
            }
            set
            {
                _isAirChina = value;
            }
        }
        private string _cCardSystem;
        [ProtoMember(103)]
        [ReportAttr(Column = "cCardSystem")]
        [Display(Name = "卡系统")]
        public string CCardSystem
        {
            get
            {
                return _cCardSystem;
            }
            set
            {
                _cCardSystem = value;
            }
        }
        private int _sourceid;
        [ProtoMember(104)]
        [ReportAttr(Column = "sourceid")]
        [Display(Name = "渠道id")]
        public int Sourceid
        {
            get
            {
                return _sourceid;
            }
            set
            {
                _sourceid = value;
            }
        }
        private string _sales;
        [ProtoMember(105)]
        [ReportAttr(Column = "sales")]
        [Display(Name = "销售员id")]
        public string Sales
        {
            get
            {
                return _sales;
            }
            set
            {
                _sales = value;
            }
        }
        private string _isFlightelOrder;
        [ProtoMember(106)]
        [ReportAttr(Column = "isFlightelOrder")]
        [Display(Name = "是否机酒套餐订单")]
        public string IsFlightelOrder
        {
            get
            {
                return _isFlightelOrder;
            }
            set
            {
                _isFlightelOrder = value;
            }
        }
        private string _unSystemOrder;
        [ProtoMember(107)]
        [ReportAttr(Column = "unSystemOrder")]
        [Display(Name = "未知")]
        public string UnSystemOrder
        {
            get
            {
                return _unSystemOrder;
            }
            set
            {
                _unSystemOrder = value;
            }
        }
        private DateTime _setOutTime;
        [ProtoMember(108)]
        [ReportAttr(Column = "setOutTime")]
        [Display(Name = "未知")]
        public DateTime SetOutTime
        {
            get
            {
                return _setOutTime;
            }
            set
            {
                _setOutTime = value;
            }
        }

        private string _paymentWayID;
        [ProtoMember(110)]
        [ReportAttr(Column = "paymentWayID")]
        [Display(Name = "支付方式 支付方式编码 TravelMoney=游票 EMoney=抵用券")]
        public string PaymentWayID
        {
            get
            {
                return _paymentWayID;
            }
            set
            {
                _paymentWayID = value;
            }
        }
        private int _subPaySystem;
        [ProtoMember(111)]
        [ReportAttr(Column = "subPaySystem")]
        [Display(Name = "支付子系统ID 1=CASH 现金类支付 2=CARD 银行卡支付 3=THIRD 第三方支付 4=CCNT 预存款支付如公司账户 5=TMPAY 游票 6=EMPAY 抵用券支付")]
        public int SubPaySystem
        {
            get
            {
                return _subPaySystem;
            }
            set
            {
                _subPaySystem = value;
            }
        }
        private decimal _tMoney;
        [ProtoMember(112)]
        [ReportAttr(Column = "tMoney")]
        [Display(Name = "游票")]
        public decimal TMoney
        {
            get
            {
                return _tMoney;
            }
            set
            {
                _tMoney = value;
            }
        }
        private string _bookingType;
        [ProtoMember(113)]
        [ReportAttr(Column = "bookingType")]
        [Display(Name = "预定类型 欧铁预定-EUR机票 酒店预订-AHD度假预定-PKG")]
        public string BookingType
        {
            get
            {
                return _bookingType;
            }
            set
            {
                _bookingType = value;
            }
        }
        private string _extLink;
        [ProtoMember(114)]
        [ReportAttr(Column = "extLink")]
        [Display(Name = "未知")]
        public string ExtLink
        {
            get
            {
                return _extLink;
            }
            set
            {
                _extLink = value;
            }
        }
        private string _supplier;
        [ProtoMember(115)]
        [ReportAttr(Column = "supplier")]
        [Display(Name = "供应商ID")]
        public string Supplier
        {
            get
            {
                return _supplier;
            }
            set
            {
                _supplier = value;
            }
        }
        private int _confirmWay;
        [ProtoMember(116)]
        [ReportAttr(Column = "confirmWay")]
        [Display(Name = "支付方式")]
        public int ConfirmWay
        {
            get
            {
                return _confirmWay;
            }
            set
            {
                _confirmWay = value;
            }
        }
        private int _chargeType;
        [ProtoMember(117)]
        [ReportAttr(Column = "chargeType")]
        [Display(Name = "收款方式 0 预付费 1 前台现付 默认值为0")]
        public int ChargeType
        {
            get
            {
                return _chargeType;
            }
            set
            {
                _chargeType = value;
            }
        }
        private string _isbackCash;
        [ProtoMember(118)]
        [ReportAttr(Column = "isbackCash")]
        [Display(Name = "产品是否返现")]
        public string IsbackCash
        {
            get
            {
                return _isbackCash;
            }
            set
            {
                _isbackCash = value;
            }
        }
        private int _bookingSpecialType;
        [ProtoMember(119)]
        [ReportAttr(Column = "bookingSpecialType")]
        [Display(Name = "自由行套餐类别 1 自由行套餐包低价套餐 2 自由行套餐包常规套餐 3 自由行DIY ")]
        public int BookingSpecialType
        {
            get
            {
                return _bookingSpecialType;
            }
            set
            {
                _bookingSpecialType = value;
            }
        }
        private int _invoiceCity;
        [ProtoMember(120)]
        [ReportAttr(Column = "invoiceCity")]
        [Display(Name = "开票城市")]
        public int InvoiceCity
        {
            get
            {
                return _invoiceCity;
            }
            set
            {
                _invoiceCity = value;
            }
        }
        private string _ledger;
        [ProtoMember(121)]
        [ReportAttr(Column = "ledger")]
        [Display(Name = "存放“PKG_LEDGER_CTRIP”，“PKG_LEDGER_HHTRAVEL” 隐藏模块NAME ")]
        public string Ledger
        {
            get
            {
                return _ledger;
            }
            set
            {
                _ledger = value;
            }
        }
        private string _displayInOnline;
        [ProtoMember(122)]
        [ReportAttr(Column = "displayInOnline")]
        [Display(Name = "Onilne我的携程中是否可见：T 可见，F不可见；默认可见")]
        public string DisplayInOnline
        {
            get
            {
                return _displayInOnline;
            }
            set
            {
                _displayInOnline = value;
            }
        }
        private string _bigBedFirst;
        [ProtoMember(123)]
        [ReportAttr(Column = "bigBedFirst")]
        [Display(Name = "尽量安排大床（T-大床、F-双床、Null-无要求，默认值）")]
        public string BigBedFirst
        {
            get
            {
                return _bigBedFirst;
            }
            set
            {
                _bigBedFirst = value;
            }
        }
        private int _manualFlag;
        [ProtoMember(124)]
        [ReportAttr(Column = "manualFlag")]
        [Display(Name = "订单人工处理标识")]
        public int ManualFlag
        {
            get
            {
                return _manualFlag;
            }
            set
            {
                _manualFlag = value;
            }
        }
        private string _noSmokingRoomFirst;
        [ProtoMember(125)]
        [ReportAttr(Column = "noSmokingRoomFirst")]
        [Display(Name = "尽量安排无烟房（T-无烟房、F-无要求，默认为F）")]
        public string NoSmokingRoomFirst
        {
            get
            {
                return _noSmokingRoomFirst;
            }
            set
            {
                _noSmokingRoomFirst = value;
            }
        }
        private string _pkgSaleMode;
        [ProtoMember(126)]
        [ReportAttr(Column = "pkgSaleMode")]
        [Display(Name = "产品模式 “代理模式” ：S 自研自售;O OEM自售;P 代理模式;G 联合发团;")]
        public string PkgSaleMode
        {
            get
            {
                return _pkgSaleMode;
            }
            set
            {
                _pkgSaleMode = value;
            }
        }
        private int _mFlightOrder;
        [ProtoMember(127)]
        [ReportAttr(Column = "mFlightOrder")]
        [Display(Name = "机加车订单机票订单号")]
        public int MFlightOrder
        {
            get
            {
                return _mFlightOrder;
            }
            set
            {
                _mFlightOrder = value;
            }
        }
        private string _pickUpCard;
        [ProtoMember(128)]
        [ReportAttr(Column = "pickUpCard")]
        [Display(Name = "接机牌")]
        public string PickUpCard
        {
            get
            {
                return _pickUpCard;
            }
            set
            {
                _pickUpCard = value;
            }
        }
        private Int16 _riskStatus;
        [ProtoMember(129)]
        [ReportAttr(Column = "riskStatus")]
        [Display(Name = "风控状态 0 待风控处理（默认新增订单的状态）1 风控申请中（首次风控申请后的状态）2 风控异常(有风险) 3 风控正常(无风险) 4 风控失败（有异常发生)")]
        public Int16 RiskStatus
        {
            get
            {
                return _riskStatus;
            }
            set
            {
                _riskStatus = value;
            }
        }
        private decimal _orderAmountAgainst;
        [ProtoMember(130)]
        [ReportAttr(Column = "orderAmountAgainst")]
        [Display(Name = "订单销售额抵充")]
        public decimal OrderAmountAgainst
        {
            get
            {
                return _orderAmountAgainst;
            }
            set
            {
                _orderAmountAgainst = value;
            }
        }
        private DateTime _autoFilterOverTime;
        [ProtoMember(131)]
        [ReportAttr(Column = "autoFilterOverTime")]
        [Display(Name = "过滤处理超时时间，默认为预订时间 5分钟（时长可配置）")]
        public DateTime AutoFilterOverTime
        {
            get
            {
                return _autoFilterOverTime;
            }
            set
            {
                _autoFilterOverTime = value;
            }
        }
        private int _autoFilterStatus;
        [ProtoMember(132)]
        [ReportAttr(Column = "autoFilterStatus")]
        [Display(Name = "0：待过滤处理（默认新订单的状态）1：过滤处理中2：过滤完成")]
        public int AutoFilterStatus
        {
            get
            {
                return _autoFilterStatus;
            }
            set
            {
                _autoFilterStatus = value;
            }
        }
        private DateTime _riskStartTime;
        [ProtoMember(133)]
        [ReportAttr(Column = "riskStartTime")]
        [Display(Name = "风控开始时间，用于判断是否需要重置风控状态")]
        public DateTime RiskStartTime
        {
            get
            {
                return _riskStartTime;
            }
            set
            {
                _riskStartTime = value;
            }
        }
        private string _vendorProductName;
        [ProtoMember(134)]
        [ReportAttr(Column = "vendorProductName")]
        [Display(Name = "产品内部使用名称")]
        public string VendorProductName
        {
            get
            {
                return _vendorProductName;
            }
            set
            {
                _vendorProductName = value;
            }
        }
        private int _salesCity;
        [ProtoMember(135)]
        [ReportAttr(Column = "salesCity")]
        [Display(Name = "售卖城市ID")]
        public int SalesCity
        {
            get
            {
                return _salesCity;
            }
            set
            {
                _salesCity = value;
            }
        }
        private string _isBillOrder;
        [ProtoMember(136)]
        [ReportAttr(Column = "isBillOrder")]
        [Display(Name = "是否采用Bill单：T，是；F，否")]
        public string IsBillOrder
        {
            get
            {
                return _isBillOrder;
            }
            set
            {
                _isBillOrder = value;
            }
        }
        private string _isCreditCardDivided;
        [ProtoMember(137)]
        [ReportAttr(Column = "isCreditCardDivided")]
        [Display(Name = "是否信用卡分期：T，是；F，否")]
        public string IsCreditCardDivided
        {
            get
            {
                return _isCreditCardDivided;
            }
            set
            {
                _isCreditCardDivided = value;
            }
        }
        private int _tmpOrderID;
        [ProtoMember(138)]
        [ReportAttr(Column = "tmpOrderID")]
        [Display(Name = "onlien预订时的临时订单ID")]
        public int TmpOrderID
        {
            get
            {
                return _tmpOrderID;
            }
            set
            {
                _tmpOrderID = value;
            }
        }
        private decimal _cachAccount;
        [ProtoMember(139)]
        [ReportAttr(Column = "cachAccount")]
        [Display(Name = "现金账户金额")]
        public decimal CachAccount
        {
            get
            {
                return _cachAccount;
            }
            set
            {
                _cachAccount = value;
            }
        }
        private int _orderSaleMode;
        [ProtoMember(140)]
        [ReportAttr(Column = "orderSaleMode")]
        [Display(Name = "订单售卖模式：0-产品售卖、1-套餐打包售卖（自行行、机酒）、2-资源裸卖")]
        public int OrderSaleMode
        {
            get
            {
                return _orderSaleMode;
            }
            set
            {
                _orderSaleMode = value;
            }
        }
        private string _isDisplay;
        [ProtoMember(141)]
        [ReportAttr(Column = "isDisplay")]
        [Display(Name = "是否显示T：显示，F：隐藏，默认显示")]
        public string IsDisplay
        {
            get
            {
                return _isDisplay;
            }
            set
            {
                _isDisplay = value;
            }
        }
        private int _destCity;
        [ProtoMember(142)]
        [ReportAttr(Column = "destCity")]
        [Display(Name = "目的城市")]
        public int DestCity
        {
            get
            {
                return _destCity;
            }
            set
            {
                _destCity = value;
            }
        }
        private DateTime _dataChange_LastTime;
        [ProtoMember(143)]
        [ReportAttr(Column = "dataChange_LastTime")]
        [Display(Name = "上次修改时间")]
        public DateTime DataChange_LastTime
        {
            get
            {
                return _dataChange_LastTime;
            }
            set
            {
                _dataChange_LastTime = value;
            }
        }
        private string _bookingChannel;
        [ProtoMember(144)]
        [ReportAttr(Column = "bookingChannel")]
        [Display(Name = "增加字段-预订渠道BookingChannel：Online Booking、Offline Booking、Wireless App、H5、Distribution Platform，记录正式单的下单渠道")]
        public string BookingChannel
        {
            get
            {
                return _bookingChannel;
            }
            set
            {
                _bookingChannel = value;
            }
        }
    }
}

