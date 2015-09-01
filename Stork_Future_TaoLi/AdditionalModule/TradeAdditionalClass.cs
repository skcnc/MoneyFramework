using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace Stork_Future_TaoLi
{
    public class TradeRecord : ConcurrentDictionary<String, RecordItem>
    {
        private static readonly TradeRecord Instance = new TradeRecord();
        public static TradeRecord GetInstance()
        {
            return Instance;
        }

        public void CreateOrder(String type, String code, String orientation, int amount, decimal price, String StrID)
        {
            RecordItem _record = new RecordItem()
            {
                StrategyId = StrID,
                LocalRequestID = StrID + code,
                OrderTime_Start = DateTime.Now,
                Type = type,
                Code = code,
                Orientation = orientation,
                Amount = amount,
                Price = price,
                ParialDealAmount = 0,
                QuitAmount = 0,
                Status = TradeDealStatus.PREORDER
            };

            if (this.Keys.Contains(_record.LocalRequestID))
            {
                //已经存在Key，采用新的记录
                RecordItem _oldRecord = new RecordItem();
                this.TryRemove(_record.LocalRequestID, out _oldRecord);
            }

            this.TryAdd(_record.LocalRequestID, _record);

        }

        public void UpdateOrder(int partialAmount, int quitAmount, string key)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(key, _record);
            _record.ParialDealAmount = partialAmount;
            _record.QuitAmount = quitAmount;
            _record.Status = TradeDealStatus.ORDERING;

            this.TryAdd(_record.LocalRequestID, _record);
        }

        public void MarkFailure(String key, String Err)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(key, _record);
            _record.ErrMsg = Err;
            _record.Status = TradeDealStatus.ORDERFAILURE;
        }

        public void CompleteOrder(String key, decimal dealPrice, int partialAmount, int quitAmount)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(key, _record);

            _record.DealPrice = dealPrice;
            _record.ParialDealAmount = partialAmount;
            _record.QuitAmount = quitAmount;
            _record.Status = TradeDealStatus.ORDERCOMPLETED;
        }
    }


    /// <summary>
    /// 交易管理控制类
    /// </summary>
    public class RecordItem
    {
        /// <summary>
        /// 策略号
        /// </summary>
        public String StrategyId { get; set; }

        /// <summary>
        ///KEY 策略ID号+CODE 
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
        /// 数量
        /// </summary>
        public int Amount { get; set; }

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
        public int ParialDealAmount { get; set; }

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
        /// 交易状态
        /// </summary>
        public TradeDealStatus Status { get; set; }
    }
}