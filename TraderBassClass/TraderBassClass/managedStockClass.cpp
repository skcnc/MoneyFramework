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

//查询交易回报
array<managedEntrustreturnstruct^>^  managedStockClass::QueryEntrust(managedQueryEntrustorderstruct^ queryEntrust, String^ Errormsg)
{
	QueryEntrustorderstruct query = queryEntrust->createInstance();
	Entrustreturnstruct* ret = new Entrustreturnstruct[1];
	int count = 0;
	char* errmsg;
	array<managedEntrustreturnstruct^>^ managedRet;

	//C++ 版查询委托函数尚未实现
	//bool  b= m_cstockTrader->queryorder(query, ret, count, errmsg);
	bool b = true;

	if (b == true)
	{
		for (int i = 0; i < count; i++)
		{
			managedRet[i] = gcnew managedEntrustreturnstruct();

			managedRet[i]->cSecurity_code = gcnew String(ret[i].cSecurity_code);
			managedRet[i]->security_name = gcnew String(ret[i].security_name);
			managedRet[i]->cOrderSysID = gcnew String(ret[i].cOrderSysID);
			managedRet[i]->cInsertDate = gcnew String(ret[i].cInsertDate);
			managedRet[i]->cInsertTime = gcnew String(ret[i].cInsertTime);
			managedRet[i]->cCancelTime = gcnew String(ret[i].cCancelTime);

			managedRet[i]->nVolumeTotalOriginal = (int)(ret[i].nVolumeTotalOriginal);
			managedRet[i]->nVolumeTraded = (int)(ret[i].nVolumeTraded);
			managedRet[i]->nVolumeTotal = (int)(ret[i].nVolumeTotal);
			managedRet[i]->withdraw_ammount = (int)(ret[i].withdraw_ammount);
			managedRet[i]->frozen_money = (float)(ret[i].frozen_money);
			managedRet[i]->frozen_amount = (int)(ret[i].frozen_amount);

			managedRet[i]->cOrderStatus = ret[i].cOrderStatus;
			managedRet[i]->cOrderType = ret[i].cOrderType;
		}
	}
	return managedRet;
}

array<managedBargainreturnstruct^>^ managedStockClass::QueryTrader(managedQueryEntrustorderstruct^ queryEntrust, String^ Errormsg){
	QueryEntrustorderstruct query = queryEntrust->createInstance();
	Bargainreturnstruct* ret = new Bargainreturnstruct[1];

	int count = 0;
	char* errmsg;

	array<managedBargainreturnstruct^>^ managedRet;

	//C++ 版成交查询还未完成
	//bool b = m_cstockTrader->querytrader(query, ret, count, errmsg);

	bool b = true;

	if (b == true)
	{
		for (int i = 0; i < count; i++){
			managedRet[i] = gcnew managedBargainreturnstruct();
			managedRet[i]->Security_code = gcnew String(ret[i].cSecurity_code);
			managedRet[i]->Security_name = gcnew String(ret[i].security_name);
			managedRet[i]->OrderSysID = gcnew String(ret[i].cOrderSysID);
			managedRet[i]->OrderStatus = ret[i].cOrderStatus;
			managedRet[i]->OrderType = ret[i].cOrderType;
			managedRet[i]->stock_amount = ret[i].stock_ammount;
			managedRet[i]->bargain_price = ret[i].bargain_price;
			managedRet[i]->bargain_money = ret[i].bargain_money;
			managedRet[i]->bargain_time = gcnew String(ret[i].bargain_time);
			managedRet[i]->bargain_no = ret[i].bargain_no;
		}
	}
	
	return managedRet;
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