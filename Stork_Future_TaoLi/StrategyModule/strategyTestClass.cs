using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi.StrategyModule
{
    public interface strategyDLLInterface
    {
        /// <summary>
        /// 策略实例DLL初始化
        /// </summary>
        /// <param name="StrategyName">新策略使用的名称</param>
        void Init(String StrategyName);

        /// <summary>
        /// 更新市场信息
        /// 每次执行策略时需要先更新市场信息
        /// </summary>
        /// <param name="MarketInfo">市场行情更新列表</param>
        void UpdateMarket(List<object> MarketInfo);

        /// <summary>
        /// 获取订阅列表
        /// </summary>
        /// <returns>订阅列表</returns>
        List<object> GetSubscribe();

        /// <summary>
        /// 获取交易列表
        /// </summary>
        /// <returns>交易列表</returns>
        List<object> GetTradeList();

        /// <summary>
        /// 判断策略触发点
        /// 每次计算后判断
        /// </summary>
        /// <returns></returns>
        bool TriggerPoint();

        /// <summary>
        /// 根据当前行情计算交易参数
        /// </summary>
        void Caculation();

        /// <summary>
        /// 获取策略执行中间数据
        /// 该数据会显示在客户端
        /// </summary>
        /// <returns>中间数据</returns>
        String GetStrategyValue();

        /// <summary>
        /// 获取策略内引用行情信息
        /// </summary>
        /// <returns>行情信息</returns>
        String GetMarketValue();

    }
}