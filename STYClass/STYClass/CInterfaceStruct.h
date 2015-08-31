#ifndef __INTERFACESTRUCT_H
#define __INTERFACESTRUCT_H
#include "CDataStruct.h"

struct IndexFutureArbitrageopeninputargs  //界面输入参数
{
	indexweightstruct *       weightlist=0;			 //权重文件
	int   weightlistnum = 0;  //列表数量

	stockpotionstruct   *    positionlist=0;		 //显示持仓
	int   positionlistnum = 0;  //列表数量

	char   weightliststr[65535];
	char   positionliststr[65535];
	int    nHands = 0;      //手数
	char   indexCode[32];   //指数
	char   contractCode[32]; //期货合约
	double dPositiveOpenDelta = 0;    //开仓点位

	bool  bTradingAllowed;//是否允许交易,勾"允许"时置为true	 用于返回 
};

struct OPENARGS
{
	

	char weightliststr[65535];
	char positionliststr[65535];

	char   indexCode[32];   //指数
	char   contractCode[32]; //期货合约

	int    nHands = 0;      //手数
	double dPositiveOpenDelta = 0;    //开仓点位

	int   weightlistnum = 0;  //列表数量
	int   positionlistnum = 0;  //列表数量

	bool  bTradingAllowed;//是否允许交易,勾"允许"时置为true	 用于返回 
};


struct IndexFutureArbitrageopenshowargs  //界面显示变量
{
	/*公共部分*/
	double  futureprice = 0;       //期货价格
	double  indexprice = 0;        //指数大小
	double  SimIndex = 0;		 //模拟指数大小
	double OrgDeltaPre = 0;           //模拟误差，百分比 
	double SimerrorPre = 0;           //模拟误差，百分比 

	/*个体部分*/
	double SimtraderPre = 0;		 	   //交易误差
	double TotalStocksMarketValue = 0;	//要买入的股票的市值(不包含停牌)
	double stopmarketvalue = 0;			 //停牌市值
	double uplimitmarketvalue = 0;       //涨停市值
	double dFutureSellStrike = 0;		//卖出期货的冲击
	double TotalStockBuyStrike = 0;		//买入股票的冲击	
	double dPositiveDelta;			//调整后的基差 = 期货 - 模拟指数 -冲击（换算成点）
	char statusmsg[255];			//错误原因 或状态
};

struct IndexFutureArbitragecloseinputargs
{
	indexweightstruct *       weightlist=0;			 //权重文件
	int   weightlistnum = 0;  //列表数量

	stockpotionstruct   *    positionlist=0;		 //显示持仓
	int   positionlistnum = 0;  //列表数量

	char  weightliststr[65525];
	char   positionliststr[65535];
	int    nHands = 0;			//手数
	char   indexCode[32];   //指数
	char   contractCode[32]; //期货合约

	double dStockBonus = 0;		//分红
	double dGiftValue = 0;		//送股
	double dStockOpenCost = 0;  //开仓成本
	double dFutureSellPoint = 0; //期货开仓点

	double dOpenedPoint = 0;   //开仓基差点位
	double dExpectedGain = 0;  //预计收益
	double dShortCharge = 0;  //预估费率

	bool  bTradingAllowed;//是否允许交易,勾"允许"时置为true	
};


struct IndexFutureArbitragecloseshowargs  //界面显示变量
{
	double dTotalStockMarketValue = 0;  //股票市值
	double 	dStopedStockValue = 0;   //停牌市值
	double 	dDownlimitStockValue = 0;   //跌停市值
	double dTotalStockSellStrike = 0;  //股票冲击
	double drealStockIncome = 0; //真实股票卖出收益
	double dActualStockGain = 0; //真实股票收益（考虑费用 冲击）

	double dFutureBuyStrike = 0; //期货买入冲击
	double dActualFutureGain = 0;  //真实期货收益
	double dtotalgain = 0;		 //全部收益
	double  dzerobpgain = 0;	 //到0基差收益
	char statusmsg[255];			//错误原因 或状态
};
#endif