// STYClass.h
#include "SyntheticIndex.h"
#include "Future.h"
#include "CInterfaceStruct.h"
#include "StrategyBase.h"
#pragma once

using namespace System;

namespace STYClass 
{



class CIndexFutureArbitrage_open
{

private: //控制参数

	CFuture                  m_future;             //期货
	CIndex                   m_index;              //指数
	int nHands;									   //期货交易手数
	CSyntheticIndex			 m_SyntheticIndex;     //模拟指数

   double dExpectOpenDelta;							 //开仓点位
   bool   bTradingAllowed;//是否允许交易,勾"允许"时置为true	 用于返回 

private:  //内部变量

	double dSimIndex;			   //模拟指数大小
	double dSimerrorPre;           //模拟误差，百分比 
	double dSimtraderPre;		 	   //交易指数大小

	double dTotalStockBuyStrike;	//买入股票的冲击	
	double dTotalStocksMarketValue;//要买入的股票的市值(不包含停牌)
	double stopmarketvalue;			 //停牌市值
	double dFutureSellStrike;		//卖出期货的冲击

	double dOrgDeltaPre;				//原始基差 = 期货 -指数
	double dPositiveDelta;			//调整后的基差 = 	期货 - 模拟指数 -冲击（换算成点）

	char statusmsg[255];         //错误原因 或状态


public:
	 CIndexFutureArbitrage_open(void);
	~CIndexFutureArbitrage_open(void);

public:
    /************行情部分********/
	bool    updateSecurityInfo(MarketInforStruct *,int &num);      //获得行情信息  
    bool    getsubscribelist(securityindex*,int& num);            //获得订阅的股票 必须在初始化结束后调用

	
   /**********策略执行*******/
	bool    init(IndexFutureArbitrageopeninputargs* m);		//初始化设置，导入权重数据  更新股票列表  
	bool	calculateSimTradeStrikeAndDelta(); //计算模拟指数，交易指数，调整基差
	bool	isOpenPointReached();				//是否达到开仓点，行情，资金
	
	//void logOpenRecord();					//记录开仓记录  写入数据库
	/*****显示参数****/
	bool   gettaderargs(IndexFutureArbitrageopeninputargs &realargs);    //获得实际运行中的参数 包含samp文件
	bool   getshowstatus(IndexFutureArbitrageopenshowargs & msg);

	 /**********获取交易*******/
	bool   gettaderlist(Traderorderstruct *, int &num);  
};

}
