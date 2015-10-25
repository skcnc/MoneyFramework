#pragma once
#include"thostftdctraderapi.h"
#include "ThostFtdcUserApiStruct.h"
#include "callbackAndDelegate.h"

namespace CTP_CLI
{


class CCTPSpi :	public CThostFtdcTraderSpi
{
private:
	FrontConnectedCallback frontConnectedCallback;
	FrontDisconnectedCallback frontDisconnectedCallback;
	
	HeartBeatWarningCallback heartBeatWarningCallback;
	RspUserLoginCallback rspUserLoginCallback;
	RspUserLogoutCallback rspUserLogoutCallback;

	RspErrorCallback rspErrorCallback;

	RtnOrderCallback rtnOrderCallback;
	RtnTradeCallback rtnTradeCallback;
	RspOrderActionCallback rspOrderActionCallback;
	RspOrderInsertCallback rspOrderInsertCallback;
	ErrRtnOrderActionCallback errRtnOrderActionCallback;
	ErrRtnOrderInsertCallback errRtnOrderInsertCallback;
	RspQryTradeCallback rspQryTradeCallback;
	RspQryTradingCodeCallback rspQryTradingCodeCallback;
	RspQryTradingNoticeCallback rspQryTradingNoticeCallback;
	RspQryOrderCallback rspQryOrderCallback;
	
	RspQryTradingAccountCallback rspQryTradingAccountCallback;
	RspQryInvestorPositionCallback rspQryInvestorPositionCallback;
	RspQryInvestorCallback rspQryInvestorCallback;

	RspSettlementInfoConfirmCallback rspSettlementInfoConfirmCallback;

public:
	CCTPSpi(void);
	~CCTPSpi(void);
public:

#pragma region 虚函数
	
	//当客户端与交易后台建立起通信连接时（还未登录前），该方法被调用
#pragma region  连接
virtual void OnFrontConnected();
	void SetFrontConnectedCallback(FrontConnectedCallback ptr2F);

	///当客户端与交易后台通信连接断开时，该方法被调用。当发生这个情况后，API会自动重新连接，客户端可不做处理
	virtual void OnFrontDisconnected(int nReason);
	void SetFrontDisconnectedCallback(FrontDisconnectedCallback ptr2F);
#pragma endregion  连接


#pragma region 心跳
	///心跳超时警告。当长时间未收到报文时，该方法被调用
	virtual void OnHeartBeatWarning(int nTimeLapse);
	void SetHeartBeatWarningCallback(HeartBeatWarningCallback ptr2F);
#pragma endregion 心跳

#pragma region 登录
	///登录请求响应
	virtual void OnRspUserLogin(CThostFtdcRspUserLoginField *pRspUserLogin, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);	
	void SetRspUserLoginCallback(RspUserLoginCallback ptr2F);
	///登出请求响应
	virtual void OnRspUserLogout(CThostFtdcUserLogoutField *pUserLogout, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspUserLogoutCallback(RspUserLogoutCallback ptr2F);
#pragma endregion 登录

#pragma region 错误应答
	virtual void OnRspError(CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspErrorCallback(RspErrorCallback pt2F);
#pragma endregion 错误应答


#pragma region 交易
	///报单通知
	virtual void OnRtnOrder(CThostFtdcOrderField *pOrder) ;
	void SetRtnOrderCallback(RtnOrderCallback pt2F);

	///成交通知
	virtual void OnRtnTrade(CThostFtdcTradeField *pTrade) ;
	void SetRtnTradeCallback(RtnTradeCallback pt2F);

	///报单操作请求响应
	virtual void OnRspOrderAction(CThostFtdcInputOrderActionField *pInputOrderAction, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspOrderActionCallback(RspOrderActionCallback pt2F);

	///报单录入请求响应
	virtual void OnRspOrderInsert(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspOrderInsertCallback(RspOrderInsertCallback pt2F);

	///报单操作错误回报
	virtual void OnErrRtnOrderAction(CThostFtdcOrderActionField *pOrderAction, CThostFtdcRspInfoField *pRspInfo) ;
	void SetErrRtnOrderActionCallback(ErrRtnOrderActionCallback pt2F);

	///报单录入错误回报
	virtual void OnErrRtnOrderInsert(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo) ;
	void SetErrRtnOrderInsertCallback(ErrRtnOrderInsertCallback pt2F);

	///请求查询成交响应
	virtual void OnRspQryTrade(CThostFtdcTradeField *pTrade, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspQryTradeCallback(RspQryTradeCallback pt2F);

	///请求查询交易编码响应
	virtual void OnRspQryTradingCode(CThostFtdcTradingCodeField *pTradingCode, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspQryTradingCodeCallback(RspQryTradingCodeCallback pt2F);

	///请求查询交易通知响应
	virtual void OnRspQryTradingNotice(CThostFtdcTradingNoticeField *pTradingNotice, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspQryTradingNoticeCallback(RspQryTradingNoticeCallback pt2F);

	///请求查询报单响应
	virtual void OnRspQryOrder(CThostFtdcOrderField *pOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspQryOrderCallback(RspQryOrderCallback pt2F);
#pragma endregion 交易


#pragma region 账户
	///请求查询资金账户响应
	virtual void OnRspQryTradingAccount(CThostFtdcTradingAccountField *pTradingAccount, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspQryTradingAccountCallback(RspQryTradingAccountCallback pt2F);
	
	///请求查询投资者响应
	virtual void OnRspQryInvestor(CThostFtdcInvestorField *pInvestor, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspQryInvestorCallback(RspQryInvestorCallback pt2F);

	///请求查询投资者持仓响应
	virtual void OnRspQryInvestorPosition(CThostFtdcInvestorPositionField *pInvestorPosition, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
	void SetRspQryInvestorPositionCallback(RspQryInvestorPositionCallback pt2F);

	///投资者结算结果确认响应
	virtual void OnRspSettlementInfoConfirm(CThostFtdcSettlementInfoConfirmField *pSettlementInfoConfirm, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);
	void SetRspSettlementInfoConfirmCallback(RspSettlementInfoConfirmCallback pt2F);
#pragma endregion 账户
#pragma endregion 虚函数

	private:		
			CThostFtdcRspInfoField* repareInfo(CThostFtdcRspInfoField *pRspInfo);
			// 是否收到成功的响应
			bool IsErrorRspInfo(CThostFtdcRspInfoField *pRspInfo);
			// 是否我的报单回报
			bool IsMyOrder(CThostFtdcOrderField *pOrder);
			// 是否正在交易的报单
			bool IsTradingOrder(CThostFtdcOrderField *pOrder);

		
};

}