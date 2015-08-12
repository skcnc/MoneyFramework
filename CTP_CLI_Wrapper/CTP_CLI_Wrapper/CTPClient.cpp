#include "StdAfx.h"
#include "CTPClient.h"

//#include "..\..\..\Lib\CTPDlls\ThostFtdcUserApiDataType.h"
//#include "..\..\..\Lib\CTPDlls\ThostFtdcUserApiStruct.h"
#include "DataTypeConvertor.h"
#include "callbackAndDelegate.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace CTP_CLI
{


#pragma region 构造函数
	CCTPClient::CCTPClient( System::String^ _investor, System::String^ _pwd, System::String^ _broker, System::String^ _addr)
	{		
		this->investor = _investor;
		this->password = _pwd;
		this->broker = _broker;
		this->address = _addr;
		this->spi = new CCTPSpi();
		SetFunction2Callback();
	}

	CCTPClient::CCTPClient( System::String^ _investor, System::String^ _pwd )
	{
		this->investor = _investor;
		this->password = _pwd;
		this->broker = "2030";
		this->address = "tcp://asp-sim2-front1.financial-trading-platform.com:26205";
		
		this->spi = new CCTPSpi();
		SetFunction2Callback();	
	}
#pragma endregion 构造函数

	int iRequestID =0;
	

#pragma region 关联回调和delegate
	void CCTPClient::SetFunction2Callback()
	{
#pragma region 连接
		FrontConnectedCallbackDelegate^ fccd = gcnew FrontConnectedCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseFrontConnected);
		GCHandle gch = GCHandle::Alloc(fccd);
		IntPtr ip = Marshal::GetFunctionPointerForDelegate(fccd);
		FrontConnectedCallback mecb = static_cast<FrontConnectedCallback>(ip.ToPointer());
		this->spi->SetFrontConnectedCallback(mecb);

		FrontDisconnectedCallbackDelegate^ fdcd = gcnew FrontDisconnectedCallbackDelegate(this,&CCTPClient::RaiseFrontDisconnected);
		GCHandle gch_fdcd = GCHandle::Alloc(fdcd);
		IntPtr ip_fdcd = Marshal::GetFunctionPointerForDelegate(fdcd);
		FrontDisconnectedCallback fdc = static_cast<FrontDisconnectedCallback>(ip_fdcd.ToPointer());
		this->spi->SetFrontDisconnectedCallback(fdc);
#pragma endregion 连接


#pragma region 登录
		RspUserLoginCallbackDelegate^ rulcd = gcnew RspUserLoginCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseResUserLogin);
		GCHandle gch_rulcd = GCHandle::Alloc(rulcd);
		IntPtr ip_rulcd = Marshal::GetFunctionPointerForDelegate(rulcd);
		RspUserLoginCallback mecb2 = static_cast<RspUserLoginCallback>(ip_rulcd.ToPointer());
		this->spi->SetRspUserLoginCallback(mecb2);	

		RspUserLogoutCallbackDelegate^ rulocd = gcnew RspUserLogoutCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspUserLogout);
		GCHandle gch_rulocd = GCHandle::Alloc(rulocd);
		IntPtr ip_rulocd = Marshal::GetFunctionPointerForDelegate(rulocd);
		RspUserLogoutCallback ruloc = static_cast<RspUserLogoutCallback>(ip_rulocd.ToPointer());
		this->spi->SetRspUserLogoutCallback(ruloc);
#pragma endregion 登录

#pragma region 心跳
		HeartBeatWarningCallbackDelegate^ hbwcd = gcnew HeartBeatWarningCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseHeartBeatWarning);
		GCHandle gch_hbwcd = GCHandle::Alloc(hbwcd);
		IntPtr ip_hbwcd = Marshal::GetFunctionPointerForDelegate(hbwcd);
		HeartBeatWarningCallback hbwc = static_cast<HeartBeatWarningCallback>(ip_hbwcd.ToPointer());
		this->spi->SetHeartBeatWarningCallback(hbwc);
#pragma endregion 心跳

#pragma region 错误回报
		RspErrorCallbackDelegate^ recbd = gcnew RspErrorCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspError);
		GCHandle gch_recbd = GCHandle::Alloc(recbd);
		IntPtr ip_recbd = Marshal::GetFunctionPointerForDelegate(recbd);
		RspErrorCallback recb = static_cast<RspErrorCallback>(ip_recbd.ToPointer());
		this->spi->SetRspErrorCallback(recb);		
#pragma endregion 错误回报

#pragma region 交易
		RtnOrderCallbackDelegate^ rocd = gcnew RtnOrderCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRtnOrder);
		GCHandle gch_rocd = GCHandle::Alloc(rocd);
		IntPtr ip_rocd = Marshal::GetFunctionPointerForDelegate(rocd);
		RtnOrderCallback roc = static_cast<RtnOrderCallback>(ip_rocd.ToPointer());
		this->spi->SetRtnOrderCallback(roc);

		RtnTradeCallbackDelegate^ rtcd = gcnew RtnTradeCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRtnTrade);
		GCHandle gch_rtcd = GCHandle::Alloc(rtcd);
		IntPtr ip_rtcd = Marshal::GetFunctionPointerForDelegate(rtcd);
		RtnTradeCallback rtc = static_cast<RtnTradeCallback>(ip_rtcd.ToPointer());
		this->spi->SetRtnTradeCallback(rtc);

		RspOrderActionCallbackDelegate^ roacd = gcnew RspOrderActionCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspOrderAction);
		GCHandle gch_roacd = GCHandle::Alloc(roacd);
		IntPtr ip_roacd = Marshal::GetFunctionPointerForDelegate(roacd);
		RspOrderActionCallback roac = static_cast<RspOrderActionCallback>(ip_roacd.ToPointer());
		this->spi->SetRspOrderActionCallback(roac);

		//报单录入请求响应
		RspOrderInsertCallbackDelegate^ roicd = gcnew RspOrderInsertCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspOrderInsert);
		GCHandle gch_roicd = GCHandle::Alloc(roicd);
		IntPtr ip_roicd = Marshal::GetFunctionPointerForDelegate(roicd);
		RspOrderInsertCallback roic = static_cast<RspOrderInsertCallback>(ip_roicd.ToPointer());
		this->spi->SetRspOrderInsertCallback(roic);

		//报单操作错误回报
		ErrRtnOrderActionCallbackDelegate^ eroacd = gcnew ErrRtnOrderActionCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseErrRtnOrderAction);
		GCHandle gch_eroacd = GCHandle::Alloc(eroacd);
		IntPtr ip_eroacd = Marshal::GetFunctionPointerForDelegate(eroacd);
		ErrRtnOrderActionCallback eroac = static_cast<ErrRtnOrderActionCallback>(ip_eroacd.ToPointer());
		this->spi->SetErrRtnOrderActionCallback(eroac);

		ErrRtnOrderInsertCallbackDelegate^ eroicd = gcnew ErrRtnOrderInsertCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseErrRtnOrderInsert);
		GCHandle gch_eroicd = GCHandle::Alloc(eroicd);
		IntPtr ip_eroicd = Marshal::GetFunctionPointerForDelegate(eroicd);
		ErrRtnOrderInsertCallback eroic = static_cast<ErrRtnOrderInsertCallback>(ip_eroicd.ToPointer());
		this->spi->SetErrRtnOrderInsertCallback(eroic);

		RspQryTradeCallbackDelegate^ rqtcd = gcnew RspQryTradeCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspQryTrade);
		GCHandle gch_rqtcd = GCHandle::Alloc(rqtcd);
		IntPtr ip_rqtcd = Marshal::GetFunctionPointerForDelegate(rqtcd);
		RspQryTradeCallback rqtc = static_cast<RspQryTradeCallback>(ip_rqtcd.ToPointer());
		this->spi->SetRspQryTradeCallback(rqtc);

		///请求查询交易编码响应
		RspQryTradingCodeCallbackDelegate^ rqtccd = gcnew RspQryTradingCodeCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspQryTradingCode);
		GCHandle gch_rqtccd = GCHandle::Alloc(rqtccd);
		IntPtr ip_rqtccd = Marshal::GetFunctionPointerForDelegate(rqtccd);
		RspQryTradingCodeCallback rqtcc = static_cast<RspQryTradingCodeCallback>(ip_rqtccd.ToPointer());
		this->spi->SetRspQryTradingCodeCallback(rqtcc);

		RspQryTradingNoticeCallbackDelegate^ rqtncd = gcnew RspQryTradingNoticeCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspQryTradingNotice);
		GCHandle gch_rqtncd = GCHandle::Alloc(rqtncd);
		IntPtr ip_rqtncd = Marshal::GetFunctionPointerForDelegate(rqtncd);
		RspQryTradingNoticeCallback rqtnc = static_cast<RspQryTradingNoticeCallback>(ip_rqtncd.ToPointer());
		this->spi->SetRspQryTradingNoticeCallback(rqtnc);

		///请求查询报单响应
		RspQryOrderCallbackDelegate^ rqocd = gcnew RspQryOrderCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspQryOrder);
		GCHandle gch_rqocd = GCHandle::Alloc(rqocd);
		IntPtr ip_rqocd = Marshal::GetFunctionPointerForDelegate(rqocd);
		RspQryOrderCallback rqoc = static_cast<RspQryOrderCallback>(ip_rqocd.ToPointer());
		this->spi->SetRspQryOrderCallback(rqoc);
#pragma endregion 交易

#pragma region 账户

		RspQryTradingAccountCallbackDelegate^ rqtacd = gcnew RspQryTradingAccountCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspQryTradingAccount);
		GCHandle gch_rqtacd = GCHandle::Alloc(rqtacd);
		IntPtr ip_rqtacd = Marshal::GetFunctionPointerForDelegate(rqtacd);
		RspQryTradingAccountCallback rqtac = static_cast<RspQryTradingAccountCallback>(ip_rqtacd.ToPointer());
		this->spi->SetRspQryTradingAccountCallback(rqtac);

		///请求查询投资者响应
		RspQryInvestorCallbackDelegate^ rqicd = gcnew RspQryInvestorCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspQryInvestor);
		GCHandle gch_rqicd = GCHandle::Alloc(rqicd);
		IntPtr ip_rqicd = Marshal::GetFunctionPointerForDelegate(rqicd);
		RspQryInvestorCallback rqic = static_cast<RspQryInvestorCallback>(ip_rqicd.ToPointer());
		this->spi->SetRspQryInvestorCallback(rqic);


		RspQryInvestorPositionCallbackDelegate^ rqipcd = gcnew RspQryInvestorPositionCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspQryInvestorPosition);
		GCHandle gch_rqipcd = GCHandle::Alloc(rqipcd);
		IntPtr ip_rqipcd = Marshal::GetFunctionPointerForDelegate(rqipcd);
		RspQryInvestorPositionCallback rqipc = static_cast<RspQryInvestorPositionCallback>(ip_rqipcd.ToPointer());
		this->spi->SetRspQryInvestorPositionCallback(rqipc);		

		RspSettlementInfoConfirmCallbackDelegate^ rsiccd = gcnew RspSettlementInfoConfirmCallbackDelegate(this,&CTP_CLI::CCTPClient::RaiseRspSettlementInfoConfirm);
		GCHandle gch_rsiccd = GCHandle::Alloc(rsiccd);
		IntPtr ip_rsiccd = Marshal::GetFunctionPointerForDelegate(rsiccd);
		RspSettlementInfoConfirmCallback rsicc = static_cast<RspSettlementInfoConfirmCallback>(ip_rsiccd.ToPointer());
		this->spi->SetRspSettlementInfoConfirmCallback(rsicc);	
#pragma endregion 账户	

	}
#pragma endregion 关联回调和delegate


#pragma region RaiseEvnet实现
	void CCTPClient::RaiseFrontConnected()
	{		
		this->FrontConnected();
	}

	void CCTPClient::RaiseResUserLogin( CThostFtdcRspUserLoginField* pRspUserLogin, CThostFtdcRspInfoField* pRspInfo, int nRequestID, bool bIsLast )
	{
		CTP_CLI::CThostFtdcRspUserLoginField_M^ rspUserLogin = (CTP_CLI::CThostFtdcRspUserLoginField_M^)Marshal::PtrToStructure(IntPtr(pRspUserLogin),CTP_CLI::CThostFtdcRspUserLoginField_M::typeid);
		CTP_CLI::CThostFtdcRspInfoField_M^ rspInfo = (CTP_CLI::CThostFtdcRspInfoField_M^)Marshal::PtrToStructure(IntPtr(pRspInfo),CTP_CLI::CThostFtdcRspInfoField_M::typeid);
		this->RspUserLogin(rspUserLogin,rspInfo,nRequestID,bIsLast);
	}    

	void CCTPClient::RaiseRspUserLogout( CThostFtdcUserLogoutField* pUserLogout, CThostFtdcRspInfoField* pRspInfo, int nRequestID, bool bIsLast )
	{
		CTP_CLI::CThostFtdcUserLogoutField_M^ rspUserLogout = (CTP_CLI::CThostFtdcUserLogoutField_M^)Marshal::PtrToStructure(IntPtr(pUserLogout),CTP_CLI::CThostFtdcUserLogoutField_M::typeid);
		CTP_CLI::CThostFtdcRspInfoField_M^ rspInfo = (CTP_CLI::CThostFtdcRspInfoField_M^)Marshal::PtrToStructure(IntPtr(pRspInfo),CTP_CLI::CThostFtdcRspInfoField_M::typeid);
		this->RspUserLogout(rspUserLogout,rspInfo,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseFrontDisconnected( int nReason )
	{
		this->FrontDisconnected(nReason);
	}

	void CCTPClient::RaiseHeartBeatWarning( int nTimeLapse )
	{
		this->HeartBeatWarning(nTimeLapse);
	}

	void CCTPClient::RaiseRspError( CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CTP_CLI::CThostFtdcRspInfoField_M^ rspInfo = (CTP_CLI::CThostFtdcRspInfoField_M^)Marshal::PtrToStructure(IntPtr(pRspInfo),CTP_CLI::CThostFtdcRspInfoField_M::typeid);
		this->RspError(rspInfo,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseRtnOrder( CThostFtdcOrderField *pOrder )
	{
		CThostFtdcOrderField_M^ order = (CThostFtdcOrderField_M^)Marshal::PtrToStructure((IntPtr)(pOrder),CThostFtdcOrderField_M::typeid);
		this->RtnOrder(order);
	}

	void CCTPClient::RaiseRtnTrade( CThostFtdcTradeField *pTrade )
	{
		CThostFtdcTradeField_M^ trade = (CThostFtdcTradeField_M^)Marshal::PtrToStructure((IntPtr)(pTrade),CThostFtdcTradeField_M::typeid);
		this->RtnTrade(trade);
	}

	void CCTPClient::RaiseRspOrderAction( CThostFtdcInputOrderActionField *pInputOrderAction, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CThostFtdcInputOrderActionField_M^ inputOrderAction = (CThostFtdcInputOrderActionField_M^)Marshal::PtrToStructure((IntPtr)(pInputOrderAction),CThostFtdcInputOrderActionField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfoField = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pInputOrderAction),CThostFtdcRspInfoField_M::typeid);
		this->RspOrderAction(inputOrderAction,rspInfoField,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseRspOrderInsert( CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CThostFtdcInputOrderField_M^ inputOrder = (CThostFtdcInputOrderField_M^)Marshal::PtrToStructure((IntPtr)(pInputOrder),CThostFtdcInputOrderField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfo = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pInputOrder),CThostFtdcRspInfoField_M::typeid);
		this->RspOrderInsert(inputOrder,rspInfo,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseErrRtnOrderAction( CThostFtdcOrderActionField *pOrderAction, CThostFtdcRspInfoField *pRspInfo )
	{
		CThostFtdcOrderActionField_M^ orderAction = (CThostFtdcOrderActionField_M^)Marshal::PtrToStructure((IntPtr)(pOrderAction),CThostFtdcOrderActionField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfo = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);
		this->ErrRtnOrderAction(orderAction,rspInfo);
	}

	void CCTPClient::RaiseErrRtnOrderInsert( CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo )
	{
		CThostFtdcInputOrderField_M^ inputOrder = (CThostFtdcInputOrderField_M^)Marshal::PtrToStructure((IntPtr)(pInputOrder),CThostFtdcInputOrderField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfo = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);

		this->ErrRtnOrderInsert(inputOrder,rspInfo);
	}

	void CCTPClient::RaiseRspQryTrade( CThostFtdcTradeField *pTrade, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CThostFtdcTradeField_M^ trade = (CThostFtdcTradeField_M^)Marshal::PtrToStructure((IntPtr)(pTrade),CThostFtdcTradeField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfo = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);
		this->RspQryTrade(trade,rspInfo,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseRspQryTradingCode( CThostFtdcTradingCodeField *pTradingCode, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CThostFtdcTradingCodeField_M^ tradingCode = (CThostFtdcTradingCodeField_M^)Marshal::PtrToStructure((IntPtr)(pTradingCode),CThostFtdcTradingCodeField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfo = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);
		RspQryTradingCode(tradingCode,rspInfo,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseRspQryTradingNotice( CThostFtdcTradingNoticeField *pTradingNotice, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CThostFtdcTradingNoticeField_M^ tradingNotice = (CThostFtdcTradingNoticeField_M^)Marshal::PtrToStructure((IntPtr)(pTradingNotice),CThostFtdcTradingNoticeField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfo = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);
		RspQryTradingNotice(tradingNotice,rspInfo,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseRspQryOrder( CThostFtdcOrderField *pOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CThostFtdcOrderField_M^ order = (CThostFtdcOrderField_M^)Marshal::PtrToStructure((IntPtr)(pOrder),CThostFtdcOrderField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfo = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);
		RspQryOrder(order,rspInfo,nRequestID,bIsLast);

	}

	void CCTPClient::RaiseRspQryTradingAccount( CThostFtdcTradingAccountField *pTradingAccount, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CThostFtdcTradingAccountField_M^ tradingAccount = (CThostFtdcTradingAccountField_M^)Marshal::PtrToStructure((IntPtr)(pTradingAccount),CThostFtdcTradingAccountField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfo = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);
		RspQryTradingAccount(tradingAccount,rspInfo,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseRspQryInvestor( CThostFtdcInvestorField *pInvestor, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CThostFtdcInvestorField_M^ investor = (CThostFtdcInvestorField_M^)Marshal::PtrToStructure((IntPtr)(pInvestor),CThostFtdcInvestorField_M::typeid);
		CThostFtdcRspInfoField_M^ rspInfo = (CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);
		RspQryInvestor(investor,rspInfo,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseRspQryInvestorPosition( CThostFtdcInvestorPositionField *pInvestorPosition, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CTP_CLI::CThostFtdcInvestorPositionField_M^ investorPosition = (CTP_CLI::CThostFtdcInvestorPositionField_M^)Marshal::PtrToStructure((IntPtr)(pInvestorPosition),CThostFtdcInvestorPositionField_M::typeid);
		CTP_CLI::CThostFtdcRspInfoField_M^ rspInfo = (CTP_CLI::CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);
		RspQryInvestorPosition(investorPosition,rspInfo,nRequestID,bIsLast);
	}

	void CCTPClient::RaiseRspSettlementInfoConfirm( CThostFtdcSettlementInfoConfirmField *pSettlementInfoConfirm, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
	{
		CTP_CLI::CThostFtdcSettlementInfoConfirmField_M^ settlementInfoConfirm = (CTP_CLI::CThostFtdcSettlementInfoConfirmField_M^)Marshal::PtrToStructure((IntPtr)(pSettlementInfoConfirm),CThostFtdcSettlementInfoConfirmField_M::typeid);
		CTP_CLI::CThostFtdcRspInfoField_M^ rspInfo = (CTP_CLI::CThostFtdcRspInfoField_M^)Marshal::PtrToStructure((IntPtr)(pRspInfo),CThostFtdcRspInfoField_M::typeid);
		RspSettlementInfoConfirm(settlementInfoConfirm,rspInfo,nRequestID,bIsLast); 
	}

#pragma endregion RaiseEvnetÊµÏÖ

#pragma region public functions
void CCTPClient::Connect()
	{
		this->tradeApi = CThostFtdcTraderApi::CreateFtdcTraderApi(".\\futuresData\\");			// 创建UserApi
		this->tradeApi->RegisterSpi((CThostFtdcTraderSpi*)spi);			// 注册事件类
		this->tradeApi->SubscribePublicTopic(THOST_TERT_QUICK/*THOST_TERT_RESTART*/);					// 注册公有流
		this->tradeApi->SubscribePrivateTopic(THOST_TERT_QUICK/*THOST_TERT_RESTART*/);					// 注册私有流
		this->tradeApi->RegisterFront(DataTypeConvertor::GetStr(this->address));							// connect
		this->tradeApi->Init();
	}

	int CCTPClient::ReqUserLogin()
	{
		CThostFtdcReqUserLoginField req;
		memset(&req, 0, sizeof(req));
		strcpy_s(req.BrokerID, DataTypeConvertor::GetConstStr(this->broker));
		strcpy_s(req.UserID,  DataTypeConvertor::GetConstStr(this->investor));
		strcpy_s(req.Password,  DataTypeConvertor::GetConstStr(this->password));
		strcpy_s(req.UserProductInfo,"HF");
		return tradeApi->ReqUserLogin(&req, ++iRequestID);
	}

	void CCTPClient::Disconnect()
	{
		this->tradeApi->Release();
	}

	int CCTPClient::ReqUserLogOut()
	{
		CThostFtdcUserLogoutField req;
		memset(&req, 0, sizeof(req));
		strcpy_s(req.BrokerID, DataTypeConvertor::GetConstStr(this->broker));
		strcpy_s(req.UserID, DataTypeConvertor::GetConstStr(this->investor));
		return tradeApi->ReqUserLogout(&req, ++iRequestID);
	}

	int CCTPClient::OrderInsert( CThostFtdcInputOrderField_M^ order )
	{
		IntPtr ptr = Marshal::AllocHGlobal(Marshal::SizeOf(order));
		Marshal::StructureToPtr(order, ptr, false);
		CThostFtdcInputOrderField* pOrder = (CThostFtdcInputOrderField*)(ptr.ToPointer());
		//strcpy_s(pOrder->BusinessUnit,"HF");		
		return tradeApi->ReqOrderInsert(pOrder, ++iRequestID);
	}
	
	//限价单
	int CCTPClient::OrderInsert( System::String^ InstrumentID, EnumOffsetFlagType OffsetFlag, EnumDirectionType Direction, System::Double Price, System::Int32 Volume )
	{
		CThostFtdcInputOrderField_M^ tmp = gcnew CThostFtdcInputOrderField_M();
		tmp->BrokerID = this->broker;
		tmp->ContingentCondition = (Byte)EnumContingentConditionType::Immediately;
		tmp->ForceCloseReason = (Byte)EnumForceCloseReasonType::NotForceClose;
		tmp->InvestorID = this->investor;
		tmp->IsAutoSuspend = (int)EnumBoolType::No;
		tmp->MinVolume = 0;
		tmp->OrderPriceType = (Byte)EnumOrderPriceTypeType::LimitPrice;
		tmp->OrderRef = System::String::Format("{0,12}",++this->MaxOrderRef);
		tmp->TimeCondition = (Byte)EnumTimeConditionType::GFD;	//本节有效
		tmp->UserForceClose = (int)EnumBoolType::No;
		tmp->UserID = this->investor;
		tmp->VolumeCondition = (Byte)EnumVolumeConditionType::AV;
		tmp->CombHedgeFlag_0 = (Byte)EnumHedgeFlagType::Speculation;

		tmp->InstrumentID = InstrumentID;
		tmp->CombOffsetFlag_0 = (Byte)OffsetFlag;
		tmp->Direction = (Byte)Direction;
		tmp->LimitPrice = Price;
		tmp->VolumeTotalOriginal = Volume;
		return OrderInsert(tmp);
	}

	//市价单
	int CCTPClient::OrderInsert( System::String^ InstrumentID, EnumOffsetFlagType OffsetFlag, EnumDirectionType Direction, System::Int32 Volume )
	{
		CThostFtdcInputOrderField_M^ tmp = gcnew CThostFtdcInputOrderField_M();
		tmp->BrokerID = this->broker;
		tmp->BusinessUnit = "";
		tmp->ContingentCondition = (Byte)EnumContingentConditionType::Immediately;
		tmp->ForceCloseReason = (Byte)EnumForceCloseReasonType::NotForceClose;
		tmp->InvestorID = this->investor;
		tmp->IsAutoSuspend = (int)EnumBoolType::No;
		tmp->MinVolume = 0;
		tmp->OrderPriceType = (Byte)EnumOrderPriceTypeType::AnyPrice;
		tmp->OrderRef = System::String::Format("{0,12}",++this->MaxOrderRef);
		tmp->TimeCondition = (Byte)EnumTimeConditionType::GFD;	//本节有效
		tmp->UserForceClose = (int)EnumBoolType::No;
		tmp->UserID = this->investor;
		tmp->VolumeCondition = (Byte)EnumVolumeConditionType::AV;
		tmp->CombHedgeFlag_0 = (Byte)EnumHedgeFlagType::Speculation;

		tmp->InstrumentID = InstrumentID;
		tmp->CombOffsetFlag_0 = (Byte)OffsetFlag;
		tmp->Direction = (Byte)Direction;
		tmp->LimitPrice = 0;
		tmp->VolumeTotalOriginal = Volume;
		return OrderInsert(tmp);
	}

	int CCTPClient::OrderInsert( System::String^ InstrumentID, EnumContingentConditionType ConditionType, System::Double ConditionPrice, EnumOffsetFlagType OffsetFlag, EnumDirectionType Direction, EnumOrderPriceTypeType PriceType, System::Double Price, System::Int32 Volume )
	{
		CThostFtdcInputOrderField_M^ tmp = gcnew CThostFtdcInputOrderField_M();
		tmp->BrokerID = this->broker;
		tmp->BusinessUnit = nullptr;
		tmp->ForceCloseReason = (Byte)EnumForceCloseReasonType::NotForceClose;
		tmp->InvestorID = this->investor;
		tmp->IsAutoSuspend = (int)EnumBoolType::No;
		tmp->MinVolume = 1;
		tmp->OrderRef = (++this->MaxOrderRef).ToString();
		tmp->TimeCondition = (Byte)EnumTimeConditionType::GFD;
		tmp->UserForceClose = (int)EnumBoolType::No;
		tmp->UserID = this->investor;
		tmp->VolumeCondition = (Byte)EnumVolumeConditionType::AV;
		tmp->CombHedgeFlag_0 = (Byte)EnumHedgeFlagType::Speculation;

		tmp->InstrumentID = InstrumentID;
		tmp->CombOffsetFlag_0 = (Byte)OffsetFlag;
		tmp->Direction = (Byte)Direction;
		tmp->ContingentCondition = (Byte)ConditionType;	//触发类型
		tmp->StopPrice = Price;						//触发价格
		tmp->OrderPriceType = (Byte)PriceType;				//下单类型
		tmp->LimitPrice = Price;						//下单价格:Price = LimitPrice 时有效
		tmp->VolumeTotalOriginal = Volume;
		return OrderInsert(tmp);
	}


	//报单操作请求
	int CCTPClient::ReqOrderAction( CThostFtdcInputOrderActionField_M^ Order )
	{
		IntPtr ptr = Marshal::AllocHGlobal(Marshal::SizeOf(Order));
		Marshal::StructureToPtr(Order, ptr, false);
		CThostFtdcInputOrderActionField* pOrder = (CThostFtdcInputOrderActionField*)(ptr.ToPointer());		
		return tradeApi->ReqOrderAction(pOrder, ++iRequestID);
	}

	int CCTPClient::QryOrder( System::String^ _exchangeID, System::String^ _timeStart,System::String^ _timeEnd,System::String^ _instrumentID, System::String^ _orderSysID )
	{
		CThostFtdcQryOrderField_M^ tmp = gcnew CThostFtdcQryOrderField_M();
		tmp->BrokerID = this->broker;
		tmp->InvestorID = this->investor;
		tmp->ExchangeID = _exchangeID;
		tmp->InsertTimeStart = _timeStart;
		tmp->InsertTimeEnd = _timeEnd;
		tmp->InstrumentID = _instrumentID;
		tmp->OrderSysID = _orderSysID;

		IntPtr ptr = Marshal::AllocHGlobal(Marshal::SizeOf(tmp));
		Marshal::StructureToPtr(tmp, ptr, false);
		CThostFtdcQryOrderField* pOrder = (CThostFtdcQryOrderField*)(ptr.ToPointer());	
		return tradeApi->ReqQryOrder(pOrder,++iRequestID);
	}

	int CCTPClient::QryOrder()
	{
		return QryOrder("","","","","");
	}

	int CCTPClient::QryTrade( System::DateTime _timeStart, System::DateTime _timeEnd , System::String^ _instrumentID, System::String^ _exchangeID, System::String^ _tradeID )
	{
		CThostFtdcQryTradeField_M^ tmp = gcnew CThostFtdcQryTradeField_M();
		tmp->BrokerID = this->broker;
		tmp->InvestorID = this->investor;
		tmp->ExchangeID = _exchangeID;
		tmp->TradeTimeStart = _timeStart == DateTime::MinValue ? "" : _timeStart.ToString("HH:mm:ss");
		tmp->TradeTimeEnd = _timeEnd == DateTime::MinValue ? "" : _timeEnd.ToString("HH:mm:ss");
		tmp->InstrumentID = _instrumentID;
		tmp->TradeID = _tradeID;

		IntPtr ptr = Marshal::AllocHGlobal(Marshal::SizeOf(tmp));
		Marshal::StructureToPtr(tmp, ptr, false);
		CThostFtdcQryTradeField* pTrade = (CThostFtdcQryTradeField*)(ptr.ToPointer());	

		return tradeApi->ReqQryTrade(pTrade,++iRequestID);
	}

	int CCTPClient::QryTrade()
	{
		return QryTrade(DateTime::MinValue,DateTime::MinValue,"","","");
	}

	int CCTPClient::QryInvestorPosition( System::String^ instrument )
	{
		CThostFtdcQryInvestorPositionField req;
		memset(&req, 0, sizeof(req));
		strcpy_s(req.BrokerID, DataTypeConvertor::GetConstStr(this->broker));
		strcpy_s(req.InvestorID, DataTypeConvertor::GetConstStr(this->investor));
		if(instrument)
			strcpy_s(req.InstrumentID, DataTypeConvertor::GetConstStr(instrument));
		return tradeApi->ReqQryInvestorPosition(&req, ++iRequestID);
	}

	int CCTPClient::QryTradingAccount()
	{
		CThostFtdcQryTradingAccountField req;
		memset(&req, 0, sizeof(req));
		strcpy_s(req.BrokerID, DataTypeConvertor::GetConstStr(this->broker));
		strcpy_s(req.InvestorID, DataTypeConvertor::GetConstStr(this->investor));
		return tradeApi->ReqQryTradingAccount(&req, ++iRequestID);
	}

	int CCTPClient::QryInvestor()
	{
		CThostFtdcQryInvestorField req;
		memset(&req, 0, sizeof(req));
		strcpy_s(req.BrokerID ,DataTypeConvertor::GetConstStr(this->broker));
		strcpy_s(req.InvestorID,DataTypeConvertor::GetConstStr(this->investor));
		return tradeApi->ReqQryInvestor(&req, ++iRequestID);
	}

	int CCTPClient::SettlementInfoConfirm()
	{
		CThostFtdcSettlementInfoConfirmField req;
		memset(&req, 0, sizeof(req));
		strcpy_s(req.BrokerID, DataTypeConvertor::GetConstStr(broker));
		strcpy_s(req.InvestorID, DataTypeConvertor::GetConstStr(investor));
		return tradeApi->ReqSettlementInfoConfirm(&req, ++iRequestID);
	}	



#pragma endregion public functions



}




