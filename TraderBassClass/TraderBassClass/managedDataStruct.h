#include "Stdafx.h"
#include "TraderBassClass.h"

using namespace System;
using namespace System::Runtime::InteropServices;

#pragma pack(8)

namespace MCStockLib
{

	[StructLayout(LayoutKind::Sequential)]
	public ref struct TradeOrderStructT{

		int SecurityAmount = 0;
		

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 21)]
		String^ ExchangeID;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 31)]
		String^ SecurityCode;

		//[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 55)]
		//String^ SecurityName;

		char TradeDirection;
		char OffsetFlag;
		char OrderPriceType;
		char SecurityType;
		char OrderLevel;
		char OrderExecutedDetail;

		int OrderPrice;
		

	};

	[StructLayout(LayoutKind::Sequential)]
	public ref struct TradeOrderStruct_M{

		int SecurityAmount = 0;
		Double OrderPrice = 0;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 21)]
		String^ ExchangeID;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 31)]
		String^ SecurityCode;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 55)]
		String^ SecurityName;

		char TradeDirection;
		char OffsetFlag;
		char OrderPriceType;
		char SecurityType;
		char OrderLevel;
		char OrderExecutedDetail;

		


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

	[StructLayout(LayoutKind::Sequential)]
	public ref struct QueryEntrustOrderStruct_M
	{
		char SecurityType;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 21)]
		String^ ExchangeID;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 21)]
		String^ OrderSysID;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 21)]
		String^ Code;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 21)]
		String^ StrategyId;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 21)]
		String^ User;

		int Direction;

		int Amount;

		int OrderRef;

		int OffsetFlag;

		double OrderPrice;
	};


	public ref struct managedEntrustreturnstruct{
		//证券代码
		String^ cSecurity_code;
		String^ security_name;
		//委托编号
		String^ cOrderSysID;
		//委托状态（见数据字典说明）
		// L -- 委托取消
		// l -- 委托临时状态
		// 0 -- 未申报
		// 1 -- 待申报
		// 2 -- 已申报
		// 4 -- 无效委托
		// 5 -- 部分撤单
		// 6 -- 已撤单
		// 7 -- 部分成交
		// 8 -- 已成交
		char cOrderStatus;
		//委托方式（见数据字典说明）
		char cOrderType;
		//委托数量
		int nVolumeTotalOriginal;
		//已成交数量
		int nVolumeTraded;
		//剩余未成交数量 （委托数量-已成交数量-已撤销数量）
		int nVolumeTotal;
		//撤单数量
		int withdraw_ammount;
		//冻结金额
		float frozen_money;
		//冻结证券数量
		float frozen_amount;
		//委托日期
		String^ cInsertDate;
		//委托时间
		String^ cInsertTime;
		String^ cCancelTime;

		Entrustreturnstruct CreateInstance(){
			Entrustreturnstruct instance;

			strcpy_s(instance.cSecurity_code, 31, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(cSecurity_code));
			strcpy_s(instance.security_name, 18, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(security_name));
			strcpy_s(instance.cOrderSysID, 21, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(cOrderSysID));
			instance.cOrderStatus = cOrderStatus;
			instance.cOrderType = cOrderType;
			instance.nVolumeTotalOriginal = nVolumeTotalOriginal;
			instance.nVolumeTraded = nVolumeTraded;
			instance.nVolumeTotal = nVolumeTotal;
			instance.withdraw_ammount = withdraw_ammount;
			instance.frozen_money = frozen_money;
			instance.frozen_amount =(int) frozen_amount;
			strcpy_s(instance.cInsertDate, 9, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(cInsertDate));
			strcpy_s(instance.cInsertTime, 9, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(cInsertTime));
			strcpy_s(instance.cCancelTime, 9, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(cCancelTime));

			return instance;
		}
	};

	[StructLayout(LayoutKind::Sequential)]
	public ref struct BargainReturnStruct_M
	{
		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 31)]
		String^ SecurityCode;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 18)]
		String^ SecurityName;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 21)]
		String^ OrderSysID;

		char OrderStatus;

		char OrderType;

		long StockAmount;

		double BargainPrice;

		double BargainMoney;

		[MarshalAs(UnmanagedType::ByValTStr, SizeConst = 9)]
		String^ BargainTime;

		long BargainNo;

		double UnFrozenMoney;

		long UnFrozenAmount;
	};

	public ref struct managedBargainreturnstruct{
		
		String^ Security_code;
		String^ Security_name;
		String^ OrderSysID;
		char OrderStatus;
		char OrderType;
		long stock_amount;
		double bargain_price;
		double bargain_money;
		String^ bargain_time;
		long bargain_no;
		String^ strategyId;
		int direction;
		int offsetflag;
		String^ User;
		String^ OrderMark;


		Bargainreturnstruct CreateInstance(){
			Bargainreturnstruct instance;

			strcpy_s(instance.cSecurity_code, 31, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Security_code));
			strcpy_s(instance.security_name, 31, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Security_name));
			strcpy_s(instance.cOrderSysID, 31, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(OrderSysID));
			instance.cOrderStatus = OrderStatus;
			instance.cOrderType = OrderType;
			instance.stock_ammount = stock_amount;
			instance.bargain_price = bargain_price;
			instance.bargain_money = bargain_money;
			strcpy_s(instance.bargain_time, 9, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(bargain_time));
			instance.bargain_no = bargain_no;

			return instance;

		}
	};
}

