﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 进行中的交易记录
    /// </summary>
    public class TradeRecord : ConcurrentDictionary<int, RecordItem>
    {

        //全局记录采用单例模式
        private static readonly TradeRecord Instance = new TradeRecord();

        public static TradeRecord GetInstance()
        {
            return Instance;
        }

        public void CreateOrder(int OrderRef, String StrategyId)
        {


            RecordItem _record = new RecordItem()
            {
                OrderRef = OrderRef,
                StrategyId = StrategyId
            };

            this.AddOrUpdate(OrderRef, _record, (key, oldValue) =>
                oldValue = _record
            );

        }

        /// <summary>
        /// 填写新的委托
        /// </summary>
        /// <param name="type">交易类型</param>
        /// <param name="code">交易代码</param>
        /// <param name="orientation">交易方向</param>
        /// <param name="VolumeTotalOriginal">原始交易数量</param>
        /// <param name="price">交易价格</param>
        /// <param name="StrID">策略号</param>
        /// <param name="ComboOffsetFlag">开平标志</param>
        public void SubscribeOrder(String type, String code, String orientation, int VolumeTotalOriginal, decimal price, int OrderRef, int orderStatus, int ComboOffsetFlag)
        {

            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(OrderRef, _record);

            _record.OrderTime_Start = DateTime.Now;
            _record.Code = code;
            _record.Orientation = orientation;
            _record.VolumeTotalOriginal = VolumeTotalOriginal;
            _record.VolumeTotal = 0;
            _record.Price = price;
            _record.AveragePrice = 0;
            _record.VolumeTraded = 0;
            _record.ErrMsg = String.Empty;
            _record.OrderRef = OrderRef;
            _record.OrderStatus = orderStatus;
            _record.Trades = new List<TradeItem>();
            _record.CombOffsetFlag = ComboOffsetFlag;
            _record.Status = TradeDealStatus.PREORDER;

            
            this.AddOrUpdate(OrderRef, _record, (key, oldValue) =>
                oldValue = _record
            );


            //TODO ： 更新数据库
        }

        /// <summary>
        /// 更新委托信息
        /// </summary>
        /// <param name="volumeTraded"></param>
        /// <param name="OrderRef"></param>
        /// <param name="OrderSysID"></param>
        /// <param name="StatusMsg"></param>
        /// <param name="OrderStatus"></param>
        /// <param name="VolumeTotal"></param>
        public void UpdateOrder(int volumeTraded, int OrderRef, String OrderSysID, String StatusMsg,int OrderStatus,int VolumeTotal)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(OrderRef, _record);
            _record.VolumeTraded = volumeTraded;
            _record.VolumeTotal = VolumeTotal;
            _record.ErrMsg = StatusMsg;
            _record.OrderStatus = OrderStatus;
            _record.OrderSysID = OrderSysID;
            _record.Status = TradeDealStatus.ORDERING;

            //若交易已撤单 或者 全部成交则标记该稳妥完成
            if(OrderStatus == 53 || _record.VolumeTotalOriginal == _record.VolumeTraded)
            {
                _record.OrderTime_Completed = DateTime.Now;
                _record.Status = TradeDealStatus.ORDERCOMPLETED;
            }

            this.AddOrUpdate(OrderRef, _record, (key, oldValue) =>
                oldValue = _record
            );

            if (_record.Status == TradeDealStatus.ORDERCOMPLETED)
            {
                CompletedTradeRecord.GetInstance().Update(_record.OrderRef, _record);
            }
        }

        /// <summary>
        /// 更新成交信息
        /// </summary>
        /// <param name="OrderRef">交易委托本地索引</param>
        /// <param name="Volume">成交量</param>
        /// <param name="Price">成交价格</param>
        /// <param name="TradeId">成交编号</param>
        public void UpdateTrade(int OrderRef,int Volume, decimal Price, String TradeId)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(OrderRef, _record);

            _record.Trades.Add(new TradeItem()
            {
                TradeID = TradeId,
                Price = Price,
                Volume = Volume
            });

            decimal totalVolume = 0;
            decimal totalCost = 0;

            foreach (TradeItem item in _record.Trades)
            {
                totalVolume += item.Volume;
                totalCost += (item.Volume * item.Price);
            }

            _record.AveragePrice = (totalCost / totalVolume);

            this.AddOrUpdate(OrderRef, _record, (key, oldValue) =>
                oldValue = _record
            );

            if(_record.Status == TradeDealStatus.ORDERCOMPLETED || _record.VolumeTotalOriginal == totalVolume)
            {
                CompletedTradeRecord.GetInstance().Update(_record.OrderRef, _record);
            }
        }

        /// <summary>
        /// 更新失败记录
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Err"></param>
        public void MarkFailure(int OrderRef, String Err)
        {
            RecordItem _record = new RecordItem();
            _record = this.GetOrAdd(OrderRef, _record);
            _record.ErrMsg = Err;
            _record.Status = TradeDealStatus.ORDERFAILURE;

            _record.OrderStatus = 77; //委托失败特定返回码

            this.AddOrUpdate(OrderRef, _record, (key, oldValue) =>
                oldValue = _record
            );

            CompletedTradeRecord.GetInstance().Update(_record.OrderRef, _record);
        }
    }

    /// <summary>
    /// 成功完成交易记录
    /// </summary>
    public class CompletedTradeRecord : ConcurrentDictionary<int, RecordItem>
    {
        //全局记录采用单例模式
        private static readonly CompletedTradeRecord Instance = new CompletedTradeRecord();

        public static CompletedTradeRecord GetInstance()
        {
            return Instance;
        }

        public void Update(int OrderRef, RecordItem Record)
        {
            this.AddOrUpdate(OrderRef, Record, (key, oldValue) => oldValue = Record);

            //交易完成或者失败情况下才会记录数据库，清除正在交易列表
            if (Record.Status == TradeDealStatus.ORDERCOMPLETED || Record.Status == TradeDealStatus.ORDERFAILURE)
            {
                int _k = 0; //尝试删除次数

                while (_k < 5)
                {
                    if(TradeRecord.GetInstance().TryRemove(OrderRef, out Record))
                    {
                        _k = 6;
                    }
                    _k++;
                }

                if (DBAccessLayer.DBEnable == true)
                {
                    //TODO : 更新数据库
                }
            }
        }

        public void ADD(int OrderRef, RecordItem Record)
        {
            this.AddOrUpdate(OrderRef, Record, (key, oldValue) => oldValue = Record);

            if (DBAccessLayer.DBEnable == true)
            {
                //TODO : 创建库中数据记录
            }
        }
    }


    /// <summary>
    /// 委托记录内容
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
        public String OrderSysID { get; set; }

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
        /// 当前尚未成交数量，撤销数量按该值计算
        /// </summary>
        public int VolumeTotal { get; set; }

        /// <summary>
        /// 设定价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 成交均价
        /// </summary>
        public decimal AveragePrice { get; set; }

        /// <summary>
        /// 已经成交数量
        /// </summary>
        public int VolumeTraded { get; set; }


        /// <summary>
        /// 备注说明
        /// </summary>
        public String ErrMsg { get; set; }

        /// <summary>
        /// 请求ID
        /// 按照机制确认唯一性，并在重启后清0
        /// </summary>
        public int OrderRef { get; set; }


        /// <summary>
        /// 交易状态
        /// 48  全部成交报单已提交   全部成交
        /// 49  部分成交报单已提交
        /// 51  未成交
        /// 53  已撤单
        /// 97  报单已提交
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// 组合开平标志 0： 开仓 · 1： 平仓
        /// </summary>
        public int CombOffsetFlag { get; set; }

        /// <summary>
        /// 交易状态
        /// </summary>
        public TradeDealStatus Status { get; set; }

        /// <summary>
        /// 成交记录
        /// </summary>
        public List<TradeItem> Trades { get; set; }
    }

    /// <summary>
    /// 交易记录内容
    /// </summary>
    public class TradeItem
    {
        /// <summary>
        /// 成交编号
        /// </summary>
        public String TradeID { get; set; }

        /// <summary>
        /// 成交均价
        /// </summary>
        public Decimal Price { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int Volume { get; set; }
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