#include "Stdafx.h"

#include "managedDataStruct.h"
using namespace System::Collections::Generic;

#pragma once

#using <mscorlib.dll>

namespace MCStockLib
{
	public ref class managedStockClass
	{
	public:
		managedStockClass(void);
		virtual ~managedStockClass(void);

		bool Init(managedLogin^ mylogininfor,String^ Errormsg);
		bool SingleTrade(TradeOrderStruct_M^  mytraderoder, QueryEntrustOrderStruct_M^ myEntrust, String^ Errormsg);
		bool BatchTrade(array<TradeOrderStruct_M^>^ mytraderoder, int nSize, array<QueryEntrustOrderStruct_M^>^ myEntrust, String^ Errormsg);
		bool getConnectStatus();
		bool getWorkStatus();
		void HeartBeat();
		int cal(String^ msg);
		array<managedEntrustreturnstruct^>^  QueryEntrust(QueryEntrustOrderStruct_M^ queryEntrust, String^ Errormsg);
		array<managedBargainreturnstruct^>^ QueryTrader(QueryEntrustOrderStruct_M^ queryEntrust, String^ Errormsg);

		
	private:
		CStockTrader* m_cstockTrader;
	};
}