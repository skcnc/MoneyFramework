#include "Stdafx.h"

#include "managedTradeClass.h"


using namespace MCStockLib;
using namespace System::Collections::Generic;
using namespace System;

managedStockClass::managedStockClass(void)
{
	m_cstockTrader = new CStockTrader();
}

managedStockClass::~managedStockClass(void)
{
	delete m_cstockTrader;
}

//初始化连接
bool managedStockClass::Init(managedLogin^ mylogininfor,System::String^ Errormsg)
{
	Logininfor info ;
	
	info = mylogininfor->createInstance();

	bool rt_value = false;

	//rt_value = m_cstockTrader->init(info,Errormsg);

	return rt_value;
}

void managedStockClass::HeartBeat()
{
	
}

//单笔交易
bool managedStockClass::SingleTrade(managedTraderorderstruct  mytraderoder, QueryEntrustorderstruct &myEntrust, char * Errormsg)
{
	Traderorderstruct trade ;
	QueryEntrustorderstruct entrust;
	//trade.getInit(mytraderoder);

	trade = mytraderoder.createInstance();
	(entrust).getInit(myEntrust);

	bool rt_value = false;

	rt_value = m_cstockTrader->trader(trade,entrust,Errormsg);

	return rt_value;

}

//批量交易： 大于单支股票走该接口
bool managedStockClass::BatchTrade(array<managedTraderorderstruct^>^ mytraderoder, int nSize, QueryEntrustorderstruct * myEntrust, int &num, char * Errormsg)
{
	bool rt_value = false;
	Traderorderstruct* trades = new Traderorderstruct[nSize];
	for (int i = 0; i < nSize; i++)
	{
		trades[i] = mytraderoder[i]->createInstance();
	}
	rt_value = m_cstockTrader->Batchstocktrader(trades, nSize, myEntrust, num, Errormsg);
	return rt_value;
}



//查询连接状态
bool managedStockClass::getConnectStatus()
{
	bool rt_value = false;
	rt_value = m_cstockTrader->getconnectstate();
	return rt_value;
}

//获取工作状态
bool managedStockClass::getWorkStatus()
{
	return m_cstockTrader->getworkstate();
}

int managedStockClass::cal(int i, int j, Traderorderstruct k[])
{
	return m_cstockTrader->cal(i, j);
}