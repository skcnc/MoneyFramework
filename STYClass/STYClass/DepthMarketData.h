#include <string.h>
#include "CDataStruct.h"
#pragma once
struct MarketInforStruct
{
	securityindex msecurity;  //key
	int		LastUpdateTime;				//行情最新接受更新时间(HHMMSSmmm)
	int		nInfotLag;   //行情更新间隔
	bool    bStoped;     //状态位   停牌  正常交易  熔断
	/********原始行情****/
	char    security_name[18];		//名称
	int		nTime;					//推送时间(HHMMSSmmm)
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
    securityindex msecurity;    //key
public:
	bool  bupdated;			    //根据时间判断行情更新延迟时间
            
public:
     MarketInforStruct marketinfor;

};

