#pragma once
#include "DepthMarketData.h"
#include "CDataStruct.h"
#include <Windows.h>
class CSecurity
{
public:
	CSecurity(void);
	~CSecurity(void);
private:
	//HANDLE hlocalDataMutex;//股票，指数行情更新的锁
	
public:
	CDepthMarketData m_DepthMarketData;

public:
	double  getlastprice();
	bool    isstoped();
	virtual bool    isupdated(); 
	virtual double  getrealmarketvalue(int namount);
	virtual double  getrealbuycost(int namount);
	virtual double  getrealsellgain(int namount);

	virtual bool    setcode(char *);      //设置代码  订阅行情 

	void updateInfo(MarketInforStruct *);
};

