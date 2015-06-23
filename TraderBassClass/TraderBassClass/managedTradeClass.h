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
		bool SingleTrade(managedTraderorderstruct  mytraderoder, QueryEntrustorderstruct &myEntrust, char * Errormsg);
		bool BatchTrade(array<managedTraderorderstruct^>^ mytraderoder, int nSize, QueryEntrustorderstruct * myEntrust, int &num, char * Errormsg);
		bool getConnectStatus();
		bool getWorkStatus();
		void HeartBeat();
		int cal(int i, int j, Traderorderstruct k[] );

		
	private:
		CStockTrader* m_cstockTrader;
	};
}