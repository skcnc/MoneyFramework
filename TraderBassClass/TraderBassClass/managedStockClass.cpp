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

	char* errmsg = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Errormsg);

	rt_value = m_cstockTrader->init(info,errmsg);

	return rt_value;
}

void managedStockClass::HeartBeat()
{
	
}

//单笔交易
bool managedStockClass::SingleTrade(managedTraderorderstruct^  mytraderoder, managedQueryEntrustorderstruct^ myEntrust, String^ Errormsg)
{
	Traderorderstruct trade ;
	QueryEntrustorderstruct entrust;
	//trade.getInit(mytraderoder);
	char* errmsg = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Errormsg);
	char err[255];
	trade = mytraderoder->createInstance();
	//(entrust).getInit(myEntrust);

	entrust = myEntrust->createInstance();

	bool rt_value = false;

	rt_value = m_cstockTrader->trader(trade,entrust,err);

	return rt_value;

}

//批量交易： 大于单支股票走该接口
bool managedStockClass::BatchTrade(array<managedTraderorderstruct^>^ mytraderoder, int nSize, array<managedQueryEntrustorderstruct^>^ myEntrust, String^ Errormsg)
{
	bool rt_value = false;
	Traderorderstruct* trades = new Traderorderstruct[nSize];
	QueryEntrustorderstruct* query = new QueryEntrustorderstruct[nSize];
	char* errmsg = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Errormsg);
	int num = 0;
	char err[255];
	for (int i = 0; i < nSize; i++)
	{
		trades[i] = mytraderoder[i]->createInstance();
		query[i] = myEntrust[i]->createInstance();
	}
	rt_value = m_cstockTrader->Batchstocktrader(trades, nSize, query, num, err);


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

int managedStockClass::cal(String^ msg)
{
	char* errmsg = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(msg);
	int length = m_cstockTrader->cal(errmsg);

	char* msgg = errmsg;
	return length;

}