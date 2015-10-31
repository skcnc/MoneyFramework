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
bool managedStockClass::SingleTrade(TradeOrderStruct_M^  mytraderoder, QueryEntrustOrderStruct_M^ myEntrust, String^ Errormsg)
{
	Traderorderstruct* trade = new Traderorderstruct();
	QueryEntrustorderstruct* entrust;
	//trade.getInit(mytraderoder);
	char* errmsg = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Errormsg);
	char err[255];

	IntPtr ptr = Marshal::AllocHGlobal(Marshal::SizeOf(mytraderoder));
	Marshal::StructureToPtr(mytraderoder, ptr, false);
	trade = (Traderorderstruct*)(ptr.ToPointer());

	entrust = new  QueryEntrustorderstruct();
	   
	bool rt_value = false;

	rt_value = m_cstockTrader->trader(*trade,*entrust,err);

	myEntrust->OrderSysID = gcnew String(entrust->cOrderSysID);
	myEntrust->ExchangeID = gcnew String(trade->cExchangeID);
	

	return rt_value;

}

//批量交易： 大于单支股票走该接口
bool managedStockClass::BatchTrade(array<TradeOrderStruct_M^>^ mytraderoder, int nSize, array<QueryEntrustOrderStruct_M^>^ myEntrust, String^ Errormsg)
{
	bool rt_value = false;
	Traderorderstruct** trades = new Traderorderstruct*[nSize];
	QueryEntrustorderstruct* query = new QueryEntrustorderstruct[nSize];

	for (int i = 0; i < nSize; i++){
		trades[i] = new Traderorderstruct();
	}
	char* errmsg = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Errormsg);
	int num = 0;
	char err[255];
	for (int i = 0; i < nSize; i++)
	{
		IntPtr ptr = Marshal::AllocHGlobal(Marshal::SizeOf(mytraderoder[i]));
		Marshal::StructureToPtr(mytraderoder, ptr, false);
		trades[i] = (Traderorderstruct*)(ptr.ToPointer());

		strcpy_s(query[i].cExchangeID, 21, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(myEntrust[i]->ExchangeID));
		strcpy_s(query[i].cOrderSysID, 21, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(myEntrust[i]->OrderSysID));
		query[i].cSecuritytype = myEntrust[i]->SecurityType;
	}
	rt_value = m_cstockTrader->Batchstocktrader(*trades, nSize, query, num, err);


	return rt_value;
}

//查询交易回报
array<managedEntrustreturnstruct^>^  managedStockClass::QueryEntrust(QueryEntrustOrderStruct_M^ queryEntrust, String^ Errormsg)
{
	QueryEntrustorderstruct* query = new QueryEntrustorderstruct();

	IntPtr ptr = Marshal::AllocHGlobal(Marshal::SizeOf(queryEntrust));
	Marshal::StructureToPtr(queryEntrust, ptr, false);
	query = (QueryEntrustorderstruct*)(ptr.ToPointer());

	Entrustreturnstruct* ret = new Entrustreturnstruct[1];
	int count = 0;
	char errmsg[255];
	

	bool  b= m_cstockTrader->queryorder(*query, ret, count, errmsg);
	array<managedEntrustreturnstruct^>^ managedRet = gcnew array<managedEntrustreturnstruct^>(count);

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

array<managedBargainreturnstruct^>^ managedStockClass::QueryTrader(QueryEntrustOrderStruct_M^ queryEntrust, String^ Errormsg){
	QueryEntrustorderstruct* query = new QueryEntrustorderstruct();
	Bargainreturnstruct* ret = new Bargainreturnstruct[1];

	int count = 0;
	char* errmsg;

	array<managedBargainreturnstruct^>^ managedRet = gcnew array<managedBargainreturnstruct^>(1);

	bool b = m_cstockTrader->querytrader(*query, ret, count, errmsg);

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