#include "Stdafx.h"

#include "managedDataStruct.h"
using namespace System::Collections::Generic;

#pragma once

#using <mscorlib.dll>

#include "CDataStruct.h"

namespace MCStockLib
{
	public ref class managedStockClass
	{
	public:
		managedStockClass(void);
		virtual ~managedStockClass(void);

		bool Init(managedLogin^ mylogininfor,String^ Errormsg);
		bool SingleTrade(managedTraderorderstruct^  mytraderoder, managedQueryEntrustorderstruct^ myEntrust, String^ Errormsg);
		bool BatchTrade(array<managedTraderorderstruct^>^ mytraderoder, int nSize, array<managedQueryEntrustorderstruct^>^ myEntrust, String^ Errormsg);
		bool getConnectStatus();
		bool getWorkStatus();
		void HeartBeat();
		int cal(String^ msg);

		
	private:
		CStockTrader* m_cstockTrader;
	};
}