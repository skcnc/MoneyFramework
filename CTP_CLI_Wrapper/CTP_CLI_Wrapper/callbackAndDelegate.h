#ifndef CALLBACKANDDELEGATE_H
#define CALLBACKANDDELEGATE_H

//#include "..\..\..\Lib\CTPDlls\ThostFtdcUserApiDataType.h"
//#include "..\..\..\lib\CTPDlls\ThostFtdcUserApiStruct.h"
#include "ThostFtdcTraderApi.h"
#include "struct_m.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace CTP_CLI
{
#pragma region 连接	
	typedef void (*FrontConnectedCallback)();	

	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void FrontConnectedCallbackDelegate();

	public delegate void OnFrontConnected();
    //------------------------------------
	typedef void (*FrontDisconnectedCallback)(int nReason);

	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void FrontDisconnectedCallbackDelegate(int nReason);

	public delegate void OnFrontDisconnected(System::Int32 nReason);
#pragma endregion 连接

#pragma region 心跳
	typedef void (*HeartBeatWarningCallback)(int nTimeLapse);

	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void HeartBeatWarningCallbackDelegate(int nTimeLapes);

	public delegate void OnHeartBeatWarning(int nTimeLapes);
#pragma endregion 心跳
	
#pragma region 登录
	typedef void (*RspUserLoginCallback)(CThostFtdcRspUserLoginField *pRspUserLogin, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspUserLoginCallbackDelegate(CThostFtdcRspUserLoginField *pRspUserLogin, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

	public delegate void OnRspUserLogin(CThostFtdcRspUserLoginField_M^ pRspUserLogin, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;
	
	typedef void (*RspUserLogoutCallback)(CThostFtdcUserLogoutField *pUserLogout, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspUserLogoutCallbackDelegate(CThostFtdcUserLogoutField* pUserLogout, CThostFtdcRspInfoField* pRspInfo, int nRequestID, bool bIsLast);

	public delegate void OnRspUserLogout(CThostFtdcUserLogoutField_M^ pUserLogout, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast);
#pragma endregion 登录

#pragma region 错误回报
	typedef void (*RspErrorCallback)(CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspErrorCallbackDelegate(CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspError(CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast);
#pragma endregion 错误回报

#pragma region 交易
	typedef void (*RtnOrderCallback)(CThostFtdcOrderField *pOrder) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RtnOrderCallbackDelegate(CThostFtdcOrderField *pOrder) ;
	public delegate void OnRtnOrder(CThostFtdcOrderField_M^ pOrder);

	typedef void (*RtnTradeCallback)(CThostFtdcTradeField *pTrade) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RtnTradeCallbackDelegate(CThostFtdcTradeField *pTrade) ;
	public delegate void OnRtnTrade(CThostFtdcTradeField_M^ pTrade);

	typedef void (*RspOrderActionCallback)(CThostFtdcInputOrderActionField *pInputOrderAction, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspOrderActionCallbackDelegate(CThostFtdcInputOrderActionField *pInputOrderAction, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspOrderAction(CThostFtdcInputOrderActionField_M^ pInputOrderAction, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;

	//报单录入请求响应
	typedef void (*RspOrderInsertCallback)(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspOrderInsertCallbackDelegate(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspOrderInsert(CThostFtdcInputOrderField_M^ pInputOrder, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;

	//报单操作错误回报
	typedef void (*ErrRtnOrderActionCallback)(CThostFtdcOrderActionField *pOrderAction, CThostFtdcRspInfoField *pRspInfo) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void ErrRtnOrderActionCallbackDelegate(CThostFtdcOrderActionField *pOrderAction, CThostFtdcRspInfoField *pRspInfo) ;
	public delegate void OnErrRtnOrderAction(CThostFtdcOrderActionField_M^ pOrderAction, CThostFtdcRspInfoField_M^ pRspInfo) ;

	typedef void (*ErrRtnOrderInsertCallback)(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo);
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void ErrRtnOrderInsertCallbackDelegate(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo);
	public delegate void OnErrRtnOrderInsert(CThostFtdcInputOrderField_M^ pInputOrder, CThostFtdcRspInfoField_M^ pRspInfo);

	typedef void (*RspQryTradeCallback)(CThostFtdcTradeField *pTrade, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspQryTradeCallbackDelegate(CThostFtdcTradeField *pTrade, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspQryTrade(CThostFtdcTradeField_M^ pTrade, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;
	
	///请求查询交易编码响应
	typedef void (*RspQryTradingCodeCallback)(CThostFtdcTradingCodeField *pTradingCode, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspQryTradingCodeCallbackDelegate(CThostFtdcTradingCodeField *pTradingCode, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspQryTradingCode(CThostFtdcTradingCodeField_M^ pTradingCode, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;

	typedef void (*RspQryTradingNoticeCallback)(CThostFtdcTradingNoticeField *pTradingNotice, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspQryTradingNoticeCallbackDelegate(CThostFtdcTradingNoticeField *pTradingNotice, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspQryTradingNotice(CThostFtdcTradingNoticeField_M^ pTradingNotice, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;

	///请求查询报单响应
	typedef void (*RspQryOrderCallback)(CThostFtdcOrderField *pOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspQryOrderCallbackDelegate(CThostFtdcOrderField *pOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspQryOrder(CThostFtdcOrderField_M^ pOrder, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;
#pragma endregion 交易

#pragma region 账户
	///投资者结算结果确认响应
	typedef void (*RspSettlementInfoConfirmCallback)(CThostFtdcSettlementInfoConfirmField *pSettlementInfoConfirm, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspSettlementInfoConfirmCallbackDelegate(CThostFtdcSettlementInfoConfirmField *pSettlementInfoConfirm, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);
	public delegate void OnRspSettlementInfoConfirm(CThostFtdcSettlementInfoConfirmField_M^ pSettlementInfoConfirm, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast);


	typedef void (*RspQryTradingAccountCallback)(CThostFtdcTradingAccountField *pTradingAccount, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspQryTradingAccountCallbackDelegate(CThostFtdcTradingAccountField *pTradingAccount, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspQryTradingAccount(CThostFtdcTradingAccountField_M^ pTradingAccount, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;

	///请求查询投资者响应
	typedef void (*RspQryInvestorCallback)(CThostFtdcInvestorField *pInvestor, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspQryInvestorCallbackDelegate(CThostFtdcInvestorField *pInvestor, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspQryInvestor(CThostFtdcInvestorField_M^ pInvestor, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;


	typedef void (*RspQryInvestorPositionCallback)(CThostFtdcInvestorPositionField *pInvestorPosition, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate void RspQryInvestorPositionCallbackDelegate(CThostFtdcInvestorPositionField *pInvestorPosition, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	public delegate void OnRspQryInvestorPosition(CThostFtdcInvestorPositionField_M^ pInvestorPosition, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) ;
#pragma endregion 账户	

}

#endif