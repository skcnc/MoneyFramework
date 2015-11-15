// STYClass.h
#include "SyntheticIndex.h"
#include "Future.h"
#include "CInterfaceStruct.h"
#include "StrategyBase.h"
#pragma once

using namespace System;

////////////公用函数//////////////////
inline char *  getExchangeNumByStockCode(char *Security_code) 
{
	int stkCode = atoi(Security_code);
	int nflag = stkCode / 10000;
	switch (nflag){
	case  0:return "SZ";
	case 16:return "SZ";
	case 15:return "SZ";
	case 20:return "SZ";
	case 18:return "SZ";
	case 30:return "SZ";
	}
	return "SH";
}


inline  char *  weightlisttostring(indexweightstruct *weightlist, int  weightlistnum)
{
	char tempstr[65535];
	tempstr[0] = 0;  //INIT
	char temprow[255];
	for (int i = 0; i <weightlistnum; i++)  //获取权重列表
	{
		sprintf(temprow, "%s;%c;%.12f|", weightlist[i].sSecurity.cSecurity_code, weightlist[i].sSecurity.cSecuritytype,
			weightlist[i].dweight);
		strcat_s(tempstr, temprow);

	}
	strcat_s(tempstr, "*/0");
	return tempstr;
}

inline  char *  positionlisttostring(stockpotionstruct  *positionlist, int   positionlistnum)
{
	
	char tempstr[65535];
	tempstr[0] = 0;  //INIT
	char temprow[255];
	for (int i = 0; i < positionlistnum; i++)  //获取权重列表
	{
		sprintf(temprow, "%s;%c;%d|", positionlist[i].sSecurity.cSecurity_code, positionlist[i].sSecurity.cSecuritytype, positionlist[i].ntradervolume);
		strcat(tempstr, temprow);

	}
	strcat(tempstr, "*/0");
	return tempstr;
}

inline  bool stringtoweightlist(const char * tempstr,  indexweightstruct *weightlist, int & realweightlistnum)
{
	int colnun[2];
	int i = 0;
	int weightlistnum = 0;
	string strtypetemp(tempstr);
	int rowflag = -1, preflag = -1;
	if (tempstr == 0)
		return 0;

	char temp = tempstr[i];
	while (tempstr[i] != '/0')
	{
		temp = tempstr[i];
		if (tempstr[i] == '*')
			break;
		if (tempstr[i] == '|')  //row
		{
			preflag = rowflag;
			rowflag = i;
			int  k = 0;
			for (int j = preflag; j < rowflag; j++)
			{
				if (tempstr[j] == ';')
				{
					colnun[k] = j;
					k++;
				}
			}
			if (k != 2)
				return false;

			strcpy_s(weightlist[weightlistnum].sSecurity.cSecurity_code, strtypetemp.substr(preflag + 1, colnun[0] - preflag - 1).c_str());
			weightlist[weightlistnum].sSecurity.cSecuritytype = tempstr[colnun[0] + 1];
			weightlist[weightlistnum].dweight = atof(strtypetemp.substr(colnun[1] + 1, rowflag - colnun[1] - 1).c_str());
			weightlistnum++;
		if (weightlistnum > realweightlistnum)
				return false;
		}
		i++;
	}
	return true;
}

inline  bool  stringtopositionlist(const char * tempstr,stockpotionstruct  *positionlist, int  & realpositionlistnum)
{

	int colnun[2];
	int positionlistnum = 0;
	string strtypetemp(tempstr);
	int rowflag=-1, preflag = -1;
	if (tempstr == 0)
		return 0;
	int i = 0;
	while (tempstr[i] != '/0')
	{
		if (tempstr[i] == '*')
			break;
		if (tempstr[i] == '|')  //row
		{
			preflag = rowflag;
			rowflag = i;
			int  k = 0;
			for (int j = preflag; j < rowflag; j++)
			{
				if (tempstr[j] == ';')
				{
					colnun[k] = j;
					k++;
				}
			}
			if (k != 2)
				return false;

			strcpy_s(positionlist[positionlistnum].sSecurity.cSecurity_code, strtypetemp.substr(preflag + 1, colnun[0] - preflag - 1).c_str());
			positionlist[positionlistnum].sSecurity.cSecuritytype = tempstr[colnun[0] + 1];
			positionlist[positionlistnum].ntradervolume= atoi(strtypetemp.substr(colnun[1] + 1, rowflag - colnun[1] - 1).c_str());
			positionlistnum++;
		}
		i++;
		if (positionlistnum > realpositionlistnum)
			return false;
	}
	return true;
}

namespace STYClass
{

	class CIndexFutureArbitrage_close
	{
	public:
		CIndexFutureArbitrage_close(void);
		~CIndexFutureArbitrage_close(void);

	private:
		CFuture                  m_future;             //期货
		CIndex                   m_index;              //指数

		CSyntheticIndex			 m_SyntheticIndex;     //模拟指数

		int    nHands;			//手数
		char   indexCode[32];   //指数
		char   contractCode[32]; //期货合约

		double dStockBonus;		//分红
		double dGiftValue;		//送股
		double dStockOpenCost;  //开仓成本
		double dFutureSellPoint; //期货开仓点
		double dOpenedPoint;   //开仓点位
		double dExpectedGain;  //预计收益
		double dShortCharge;  //预估费率

	private:  //内部变量

		double dTotalStockMarketValue;  //股票市值
		double 	dStopedStockValue;   //停牌市值
		double 	dDownlimitStockValue;   //跌停市值
		double dTotalStockSellStrike;  //股票冲击
		double drealStockIncome; //真实股票卖出收益
		double dActualStockGain; //真实股票收益（考虑费用 冲击）

		double dFutureBuyStrike; //期货买入冲击
		double dActualFutureGain;  //真实期货收益

		double  dzerobpgain;  //到0基差收益
		char statusmsg[255];         //错误原因 或状态

	public:
		/************行情部分********/
		bool    updateSecurityInfo(MarketInforStruct *, int &num);      //获得行情信息  
		bool    getsubscribelist(securityindex**, int& num);            //获得订阅的股票 必须在初始化结束后调用


		/**********策略执行*******/
		bool    init();		//初始化设置，导入权重数据  更新股票列表  
		bool	calculateSimTradeStrikeAndDelta(); //计算模拟指数，交易指数，调整基差
		bool	isOpenPointReached();				//是否达到开仓点，行情，资金

		//void logOpenRecord();					//记录开仓记录  写入数据库
		/*****显示参数****/
		bool   gettaderargs(IndexFutureArbitragecloseinputargs &realargs);    //获得实际运行中的参数 包含samp文件
		bool   getshowstatus(IndexFutureArbitragecloseshowargs & msg);

		/**********获取交易*******/
		bool   gettaderlist(Traderorderstruct * *, int &num);

	public:
		IndexFutureArbitragecloseinputargs* m_args;

	};


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
		IndexFutureArbitrageopeninputargs* m_args;


	public:
		CIndexFutureArbitrage_open(void);
		~CIndexFutureArbitrage_open(void);

	public:
		/************行情部分********/
		bool    updateSecurityInfo(MarketInforStruct *, int &num);      //获得行情信息  
		bool    getsubscribelist(securityindex**, int& num);            //获得订阅的股票 必须在初始化结束后调用


		/**********策略执行*******/
		//bool    init(IndexFutureArbitrageopeninputargs* m);		//初始化设置，导入权重数据  更新股票列表  
		bool   init();		//初始化设置，导入权重数据  更新股票列表  
		bool	calculateSimTradeStrikeAndDelta(); //计算模拟指数，交易指数，调整基差
		bool	isOpenPointReached();				//是否达到开仓点，行情，资金

		//void logOpenRecord();					//记录开仓记录  写入数据库
		/*****显示参数****/
		bool   gettaderargs(IndexFutureArbitrageopeninputargs &realargs);    //获得实际运行中的参数 包含samp文件
		bool   getshowstatus(IndexFutureArbitrageopenshowargs & msg);

		/**********获取交易*******/
		bool   gettaderlist(Traderorderstruct **, int &num);
	};

}
