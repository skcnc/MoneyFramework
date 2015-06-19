#include "Stdafx.h"
#include "TraderBassClass.h"

using namespace System;



namespace MCStockLib
{
	

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
			nPort = nPort;
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
}

