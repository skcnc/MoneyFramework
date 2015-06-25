#include "Stdafx.h"
#include "TraderBassClass.h"

using namespace System;



namespace MCStockLib
{

	

	public ref struct managedTraderorderstruct
	{
		//交易部分
		char*    cExchangeID;            //交易所
		char*    cSecurity_code;     // 证券代码
		char*    security_name;      //证券名称
		long    nSecurity_amount;      // 委托数量
		double  dOrderprice;           // 委托价格
		char    cTraderdirection;      // 买卖类别（见数据字典说明）
		char    cOffsetFlag;           //开平标志
		char    cOrderPriceType;       //报单条件(限价  市价)

		//控制部分
		char    cSecuritytype;          //证券类型	
		char    cOrderlevel;             //报单优先级 执行顺序
		char    cOrderexecutedetail;     //报单执行细节

		managedTraderorderstruct(String^ mcExchangeID, String^ mcSecurity_code, String^ msecurity_name
			, long mnSecurity_amount, double mdOrderprice, char mcTraderdirection, char mcOffsetFlag
			, char mcOrderPriceType, char mcSecuritytype, char mcOrderlevel, char mcOrderexecutedetail)
		{
			cExchangeID = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(mcExchangeID);
			cSecurity_code = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(mcSecurity_code);
			security_name = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(msecurity_name);
			nSecurity_amount = mnSecurity_amount;
			dOrderprice = mdOrderprice;
			cTraderdirection = mcTraderdirection;
			cOffsetFlag = mcOffsetFlag;
			cOrderPriceType = mcOrderPriceType;

			cSecuritytype = mcSecuritytype;
			cOrderlevel = mcOrderlevel;
			cOrderexecutedetail = mcOrderexecutedetail;
				
		};

		Traderorderstruct createInstance()
		{
			Traderorderstruct unmanagedTraderorderstruct;
			//unmanagedTraderorderstruct.cExchangeID = cExchangeID;
			strcpy_s(unmanagedTraderorderstruct.cExchangeID, 21, cExchangeID);
			//unmanagedTraderorderstruct.cSecurity_code = cSecurity_code;
			strcpy_s(unmanagedTraderorderstruct.cSecurity_code, 31, cSecurity_code);
			//unmanagedTraderorderstruct.security_name = security_name;
			strcpy_s(unmanagedTraderorderstruct.security_name, 55, security_name);
			unmanagedTraderorderstruct.nSecurity_amount = nSecurity_amount;
			unmanagedTraderorderstruct.dOrderprice = dOrderprice;
			unmanagedTraderorderstruct.cTraderdirection = cTraderdirection;
			unmanagedTraderorderstruct.cOffsetFlag = cOffsetFlag;
			unmanagedTraderorderstruct.cOrderPriceType = cOrderPriceType;

			unmanagedTraderorderstruct.cSecuritytype = cSecuritytype;
			unmanagedTraderorderstruct.cOrderlevel = cOrderlevel;
			unmanagedTraderorderstruct.cOrderexecutedetail = cOrderexecutedetail;

			return unmanagedTraderorderstruct;
		};
	};


	public ref struct managedLogin
	{
		char* serverAddr;// 地址
		int nPort;//端口
		char* ZjAccount; //资金
		char* BROKER_ID  ;   //部门编号
		char*  PASSWORD ;    //密码
		char* INVESTOR_ID ;  //账户

		Logininfor createInstance()
		{
			Logininfor unmanagedInfo;
			unmanagedInfo.serverAddr = serverAddr;
			unmanagedInfo.nPort = nPort;
			unmanagedInfo.ZjAccount = ZjAccount;
			unmanagedInfo.BROKER_ID = BROKER_ID;
			unmanagedInfo.PASSWORD = PASSWORD;
			unmanagedInfo.INVESTOR_ID = INVESTOR_ID;

			return unmanagedInfo;
		}

		managedLogin(String^ server_addr,int nport,String^ account,String^ broker_id ,String^ password, String^ investor_id)
		{
			serverAddr = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(server_addr);
			ZjAccount = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(account);
			BROKER_ID = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(broker_id);
			PASSWORD = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(password);
			INVESTOR_ID = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(investor_id);
			nPort = nport;
		}
	};

	public ref struct managedQueryEntrustorderstruct
	{
		char cSecurityType;
		char* cExchangeID;
		char* cOrderSysID;

		managedQueryEntrustorderstruct(char mcSecurityType, String^ mcExchangeID ,String^ mcOrderSysID)
		{
			cExchangeID = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(mcExchangeID);
			cOrderSysID = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(mcOrderSysID);
			cSecurityType = mcSecurityType;
		}

		managedQueryEntrustorderstruct()
		{
			cExchangeID = new char[255];
			cOrderSysID = new char[255];
			cSecurityType = 's';
		}

		QueryEntrustorderstruct createInstance()
		{
			QueryEntrustorderstruct queryEntrust;

			memset(queryEntrust.cExchangeID,0,21);
			memset(queryEntrust.cOrderSysID,0,21);
			
			queryEntrust.cSecuritytype = 0;

			return queryEntrust;
		}
	};
}

