#ifndef __INTERFACESTRUCT_H
#define __INTERFACESTRUCT_H
#include "CDataStruct.h"

struct IndexFutureArbitrageopeninputargs  //界面输入参数
{
	indexweightstruct *       weightlist;			 //权重文件
	int   weightlistnum;  //列表数量

	stockpotionstruct   *    positionlist;		 //显示持仓
	int   positionlistnum;  //列表数量

	int    nHands;      //手数
	char   indexCode[32];   //指数
	char   contractCode[32]; //期货合约
    double dPositiveOpenDelta;    //开仓点位

	bool  bTradingAllowed;//是否允许交易,勾"允许"时置为true	 用于返回 
};


struct IndexFutureArbitrageopenshowargs  //界面显示变量
{
	/*公共部分*/
	double  futureprice;       //期货价格
	double  indexprice;        //指数大小
	double  SimIndex;		 //模拟指数大小
	double OrgDeltaPre;           //模拟误差，百分比 
	double SimerrorPre;           //模拟误差，百分比 

	/*个体部分*/
	double SimtraderPre;		 	   //交易误差
	double TotalStocksMarketValue;	//要买入的股票的市值(不包含停牌)
	double stopmarketvalue;			 //停牌市值
	double uplimitmarketvalue;       //涨停市值
	double dFutureSellStrike;		//卖出期货的冲击
	double TotalStockBuyStrike;		//买入股票的冲击	
	double dPositiveDelta;			//调整后的基差 = 期货 - 模拟指数 -冲击（换算成点）
	char statusmsg[255];			//错误原因 或状态
};

struct IndexFutureArbitragecloseargs
{
	list<stockpotionstruct>       m_positionlist;		 //显示持仓
	int    nHands;      //手数
	char   indexCode[32];   //指数类型
	char   contractCode[32]; //期货合约
    double dPositiveOpenDelta;    //开仓点位

	bool  bTradingAllowed;//是否允许交易,勾"允许"时置为true	
};

#endif