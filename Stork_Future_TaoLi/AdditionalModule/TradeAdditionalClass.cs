using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 自然日交易记录，记录当日交易的所有进度，处委托回报、成交回报 在部分成交的状态外，其余均同步至数据库。
    /// </summary>
    public class TradeRecord : ConcurrentDictionary<int, RecordItem>
    {

        //全局记录采用单例模式
        private static readonly TradeRecord Instance = new TradeRecord();

        public static TradeRecord GetInstance()
        {
            return Instance;
        }

        /// <summary>
        /// 创建新的委托
        /// </summary>
        /// <param name="type"></param>
        /// <param name="code"></param>
        /// <param name="orientation"></param>
        /// <param name="amount"></param>
        /// <param name="price"></param>
        /// <param name="StrID"></param>
        public void CreateOrder(String type, String code, String orientation, int amount, decimal price, String StrID)
        {
            RecordItem _record = new RecordItem()
            {
                StrategyId = StrID,
                OrderTime_Start = DateTime.Now,
                Type = type,
                Code = code,
                Orientation = orientation,
                VolumeTotalOriginal = amount,
                Price = price,
                VolumeTraded = 0,
                QuitAmount = 0,
                ErrMsg = String.Empty,
                RequestID = REQUEST_ID.ApplyNewID(),
                Status = TradeDealStatus.PREORDER
            };

            if (this.Keys.Contains(_record.RequestID))
            {
                //已经存在Key，采用新的记录
                RecordItem _oldRecord = new RecordItem();
                this.TryRemove(_record.RequestID, out _oldRecord);
            }

            this.TryAdd(_record.RequestID, _record);

        }

        /// <summary>
        /// 更新委托信息
        /// </summary>
        /// <param name="partialAmount"></param>
        /// <param name="quitAmount"></param>
        /// <param name="key"></param>
        public void UpdateOrder(int volumeTraded,int requestId, String LocalRequstId, String StatusMsg)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(requestId, _record);
            _record.VolumeTraded = volumeTraded;
            _record.ErrMsg = StatusMsg;
            _record.LocalRequestID = LocalRequstId;
            _record.Status = TradeDealStatus.ORDERING;

            this.TryAdd(_record.RequestID, _record);
        }

        public void UpdateTrade(int volumeTraded, int requestId, double tradePrice, String OrderRef)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(requestId, _record);
            _record.VolumeTraded = volumeTraded;
            _record.DealPrice = Convert.ToDecimal(tradePrice);
            _record.OrderRef = OrderRef;

            if (_record.VolumeTotalOriginal == _record.VolumeTraded)
            {
                _record.Status = TradeDealStatus.ORDERCOMPLETED;
            }

        }

        /// <summary>
        /// 更新失败记录
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Err"></param>
        public void MarkFailure(int requestId, String Err)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(requestId, _record);
            _record.ErrMsg = Err;
            _record.Status = TradeDealStatus.ORDERFAILURE;
        }

        /// <summary>
        /// 记录完成
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dealPrice"></param>
        /// <param name="partialAmount"></param>
        /// <param name="quitAmount"></param>
        public void CompleteOrder(int requestId, decimal dealPrice, int partialAmount, int quitAmount)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(requestId, _record);

            _record.DealPrice = dealPrice;
            _record.VolumeTraded = partialAmount;
            _record.QuitAmount = quitAmount;
            _record.Status = TradeDealStatus.ORDERCOMPLETED;
        }
    }

    /// <summary>
    /// 交易记录内容
    /// </summary>
    public class RecordItem
    {
        /// <summary>
        /// 策略号
        /// </summary>
        public String StrategyId { get; set; }

        /// <summary>
        /// 系统交易号
        /// </summary>
        public String LocalRequestID { get; set; }

        /// <summary>
        /// 交易开始时间
        /// </summary>
        public DateTime OrderTime_Start { get; set; }

        /// <summary>
        /// 交易完成时间
        /// </summary>
        public DateTime OrderTime_Completed { get; set; }

        /// <summary>
        /// 交易类型 ： 0 股票 1： 期货
        /// </summary>
        public String Type { get; set; }

        /// <summary>
        /// 交易代码
        /// </summary>
        public String Code { get; set; }

        /// <summary>
        /// 交易方向 0：买入 1：卖出
        /// </summary>
        public String Orientation { get; set; }

        /// <summary>
        /// 原始交易数量
        /// </summary>
        public int VolumeTotalOriginal { get; set; }

        /// <summary>
        /// 最新交易数量，存在撤单情况
        /// </summary>
        public int VolumeTotal { get; set; }

        /// <summary>
        /// 设定价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public decimal DealPrice { get; set; }

        /// <summary>
        /// 部分成交量
        /// </summary>
        public int VolumeTraded { get; set; }

        /// <summary>
        /// 撤销量
        /// </summary>
        public int QuitAmount { get; set; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public String ErrMsg { get; set; }

        /// <summary>
        /// 请求ID
        /// 按照机制确认唯一性，并在重启后清0
        /// </summary>
        public int RequestID { get; set; }

        /// <summary>
        /// 报单引用
        /// </summary>
        public String OrderRef { get; set; }

        /// <summary>
        /// 交易状态
        /// </summary>
        public TradeDealStatus Status { get; set; }
    }

    /// <summary>
    /// 期货交易RequestID 分发类
    /// </summary>
    public class REQUEST_ID
    {
        private static object _syncRoot = new object();
        private static int _id = 0;

        /// <summary>
        /// 申请新的ID值
        /// </summary>
        /// <returns>id值</returns>
        public static int ApplyNewID()
        {
            lock (_syncRoot)
            {
                _id++;
                return _id;
            }
        }

    }
}