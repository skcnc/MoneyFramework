#include <string.h>
#include "CDataStruct.h"
#pragma once
struct MarketInforStruct
{
	securityindex msecurity;
	char    security_name[18];		//名称
    int		nTime;					//时间(HHMMSSmmm)
    int		nStatus;				//状态
    double  nPreClose;				//前收盘价
    double  dLastPrice;				//最新价
    double  dAskPrice[10];			//申卖价
    double  dAskVol[10];			//申卖量
    double  dBidPrice[10];			//申买价
    double  dBidVol[10];			//申买量
    double  dHighLimited;			//涨停价
    double  dLowLimited;			//跌停价

	void update(MarketInforStruct * lastmarketinfor )
	{
		memcpy(this,lastmarketinfor,sizeof(MarketInforStruct));
	  
	};
};

class CDepthMarketData
{
public:
	 CDepthMarketData(void);
	~CDepthMarketData(void);
 
public:
	
	int    nMarketdepth;        //市场深度
    securityindex msecurity;
	
public:
	bool  bupdated;			    //根据时间判断行情更新延迟时间
	bool  bStoped;              //状态位   停牌  正常交易  熔断
	int LastUpdateTime;  //行情最新更新时间
	int nInfotLag;   //行情更新间隔

public:
   MarketInforStruct marketinfor;

};

