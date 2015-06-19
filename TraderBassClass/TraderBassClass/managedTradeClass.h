#include "Stdafx.h"

#include "managedDataStruct.h"

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
		bool SingleTrade(Traderorderstruct  mytraderoder, QueryEntrustorderstruct &myEntrust,char * Errormsg);
		bool BatchTrade(Traderorderstruct * mytraderoder,int nSize,QueryEntrustorderstruct * myEntrust,int &num,char * Errormsg); 
		bool getConnectStatus();
		bool getWorkStatus();


	private:
		CStockTrader* m_cstockTrader;
	};
}