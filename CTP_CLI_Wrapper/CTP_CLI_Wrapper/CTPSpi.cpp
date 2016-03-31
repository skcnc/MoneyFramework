#include "Stdafx.h"
#include "CTPSpi.h"

//#include "..\..\..\Lib\CTPDlls\ThostFtdcUserApiStruct.h"
#include "callbackAndDelegate.h"
#include "ThostFtdcUserApiStruct.h"

bool a = false;

namespace CTP_CLI
{


CCTPSpi::CCTPSpi(void)
{
}


CCTPSpi::~CCTPSpi(void)
{
}

#pragma region SetCallback and ResponseCallback
void CCTPSpi::SetFrontConnectedCallback( FrontConnectedCallback ptr2F )
{
	this->frontConnectedCallback = ptr2F;
}


void CCTPSpi::OnFrontConnected()
{
	this->frontConnectedCallback();
}



void CCTPSpi::SetHeartBeatWarningCallback( HeartBeatWarningCallback ptr2F )
{
	this->heartBeatWarningCallback = ptr2F;
}

void CCTPSpi::OnHeartBeatWarning( int nTimeLapse )
{
	this->heartBeatWarningCallback(nTimeLapse);
}

void CCTPSpi::OnFrontDisconnected( int nReason )
{
	if(this->frontDisconnectedCallback)
		this->frontDisconnectedCallback(nReason);
}

void CCTPSpi::SetFrontDisconnectedCallback( FrontDisconnectedCallback ptr2F )
{
	this->frontDisconnectedCallback = ptr2F;
}

void CCTPSpi::OnRspUserLogout( CThostFtdcUserLogoutField *pUserLogout, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pUserLogout==nullptr)
	{
		CThostFtdcUserLogoutField req;
		memset(&req,0,sizeof(req));
		this->rspUserLogoutCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);		
	}
	else
		this->rspUserLogoutCallback(pUserLogout,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspUserLogoutCallback( RspUserLogoutCallback ptr2F )
{
	this->rspUserLogoutCallback = ptr2F;
}

void CCTPSpi::OnRspError( CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{	
	this->rspErrorCallback(repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspErrorCallback( RspErrorCallback pt2F )
{
	this->rspErrorCallback = pt2F;
}

void CCTPSpi::OnRtnOrder( CThostFtdcOrderField *pOrder )
{
	try
	{
		if (pOrder == nullptr)
		{
			CThostFtdcOrderField req;
			memset(&req, 0, sizeof(req));
			this->rtnOrderCallback(&req);
		}
		else
		{
			if (a == false)
			{
				a = true;
				this->rtnOrderCallback(pOrder);
			}

		}

		a = false;
	}
	catch(int e){}
}

void CCTPSpi::SetRtnOrderCallback( RtnOrderCallback pt2F )
{
	this->rtnOrderCallback=pt2F;
}

void CCTPSpi::OnRtnTrade( CThostFtdcTradeField *pTrade )
{
	try
	{
		if (pTrade == nullptr)
		{
			CThostFtdcTradeField req;
			memset(&req, 0, sizeof(req));
			this->rtnTradeCallback(&req);
		}
		else{
			this->rtnTradeCallback(pTrade);
		}
	}
	catch (int e){}
}

void CCTPSpi::SetRtnTradeCallback( RtnTradeCallback pt2F )
{
	this->rtnTradeCallback = pt2F;
}

void CCTPSpi::OnRspOrderAction( CThostFtdcInputOrderActionField *pInputOrderAction, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pInputOrderAction==nullptr)
	{
		CThostFtdcInputOrderActionField req;
		memset(&req,0,sizeof(&req));
		this->rspOrderActionCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
		this->rspOrderActionCallback(pInputOrderAction,repareInfo(pRspInfo),nRequestID,bIsLast);	
}

void CCTPSpi::SetRspOrderActionCallback( RspOrderActionCallback pt2F )
{
	this->rspOrderActionCallback = pt2F;
}

void CCTPSpi::OnRspOrderInsert( CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pInputOrder==nullptr)
	{
		CThostFtdcInputOrderField req;
		memset(&req,0,sizeof(req));
		this->rspOrderInsertCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
		this->rspOrderInsertCallback(pInputOrder,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspOrderInsertCallback( RspOrderInsertCallback pt2F )
{
	this->rspOrderInsertCallback = pt2F;
}

void CCTPSpi::OnErrRtnOrderAction( CThostFtdcOrderActionField *pOrderAction, CThostFtdcRspInfoField *pRspInfo )
{
	if(pOrderAction==nullptr)
	{
		CThostFtdcOrderActionField req;
		memset(&req,0,sizeof(req));
		this->errRtnOrderActionCallback(&req,repareInfo(pRspInfo));
	}
	else
		this->errRtnOrderActionCallback(pOrderAction,repareInfo(pRspInfo));
	
}

void CCTPSpi::SetErrRtnOrderActionCallback( ErrRtnOrderActionCallback pt2F )
{
	this->errRtnOrderActionCallback = pt2F;
}

void CCTPSpi::OnErrRtnOrderInsert( CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo )
{
	if(pInputOrder==nullptr)
	{
		CThostFtdcInputOrderField req;
		memset(&req,0,sizeof(req));
		this->errRtnOrderInsertCallback(&req,repareInfo(pRspInfo));
	}
	else
		this->errRtnOrderInsertCallback(pInputOrder,repareInfo(pRspInfo));
}

void CCTPSpi::SetErrRtnOrderInsertCallback( ErrRtnOrderInsertCallback pt2F )
{
	this->errRtnOrderInsertCallback = pt2F;
}

void CCTPSpi::OnRspQryTrade( CThostFtdcTradeField *pTrade, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pTrade ==nullptr)
	{
		CThostFtdcTradeField req;
		memset(&req,0,sizeof(req));
		this->rspQryTradeCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
		this->rspQryTradeCallback(pTrade,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspQryTradeCallback( RspQryTradeCallback pt2F )
{
	this->rspQryTradeCallback = pt2F;
}

void CCTPSpi::OnRspQryTradingCode( CThostFtdcTradingCodeField *pTradingCode, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pTradingCode == nullptr)
	{
		CThostFtdcTradingCodeField req;
		memset(&req,0,sizeof(req));
		this->rspQryTradingCodeCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
		this->rspQryTradingCodeCallback(pTradingCode,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspQryTradingCodeCallback( RspQryTradingCodeCallback pt2F )
{
	this->rspQryTradingCodeCallback = pt2F;
}

void CCTPSpi::OnRspQryTradingNotice( CThostFtdcTradingNoticeField *pTradingNotice, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pTradingNotice==nullptr)
	{
		CThostFtdcTradingNoticeField req;
		memset(&req,0,sizeof(req));
		this->rspQryTradingNoticeCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
		this->rspQryTradingNoticeCallback(pTradingNotice,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspQryTradingNoticeCallback( RspQryTradingNoticeCallback pt2F )
{
	this->rspQryTradingNoticeCallback = pt2F;
}

void CCTPSpi::OnRspQryOrder( CThostFtdcOrderField *pOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pOrder==nullptr)
	{
		CThostFtdcOrderField req;
		memset(&req,0,sizeof(req));
		//this->rspQryOrderCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
	{
		try
		{
			
			this->rspQryOrderCallback(pOrder, repareInfo(pRspInfo), nRequestID, bIsLast); 
		}
		catch (int a){}
	}
}

void CCTPSpi::SetRspQryOrderCallback( RspQryOrderCallback pt2F )
{
	this->rspQryOrderCallback = pt2F;
}

void CCTPSpi::OnRspQryTradingAccount( CThostFtdcTradingAccountField *pTradingAccount, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pTradingAccount==nullptr)
	{
		CThostFtdcTradingAccountField req;
		memset(&req,0,sizeof(req));
		this->rspQryTradingAccountCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
		this->rspQryTradingAccountCallback(pTradingAccount,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspQryTradingAccountCallback( RspQryTradingAccountCallback pt2F )
{
	this->rspQryTradingAccountCallback = pt2F;
}

void CCTPSpi::OnRspQryInvestor( CThostFtdcInvestorField *pInvestor, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pInvestor==nullptr)
	{
		CThostFtdcInvestorField req;
		memset(&req,0,sizeof(req));
		this->rspQryInvestorCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
		this->rspQryInvestorCallback(pInvestor,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspQryInvestorCallback( RspQryInvestorCallback pt2F )
{
	this->rspQryInvestorCallback = pt2F;
}

void CCTPSpi::OnRspQryInvestorPosition( CThostFtdcInvestorPositionField *pInvestorPosition, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pInvestorPosition==nullptr)
	{
		CThostFtdcInvestorPositionField req;
		memset(&req,0,sizeof(req));
		this->rspQryInvestorPositionCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else		
		this->rspQryInvestorPositionCallback(pInvestorPosition,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspQryInvestorPositionCallback( RspQryInvestorPositionCallback pt2F )
{
	this->rspQryInvestorPositionCallback = pt2F;
}

void CCTPSpi::SetRspUserLoginCallback( RspUserLoginCallback ptr2F )
{
	this->rspUserLoginCallback = ptr2F;
}

void CCTPSpi::OnRspUserLogin( CThostFtdcRspUserLoginField *pRspUserLogin, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pRspUserLogin==nullptr)
	{
		CThostFtdcRspUserLoginField req;
		memset(&req,0,sizeof(req));
		this->rspUserLoginCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
		this->rspUserLoginCallback(pRspUserLogin,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::OnRspSettlementInfoConfirm( CThostFtdcSettlementInfoConfirmField *pSettlementInfoConfirm, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast )
{
	if(pSettlementInfoConfirm==nullptr)
	{
		CThostFtdcSettlementInfoConfirmField req;
		memset(&req,0,sizeof(req));
		this->rspSettlementInfoConfirmCallback(&req,repareInfo(pRspInfo),nRequestID,bIsLast);
	}
	else
		this->rspSettlementInfoConfirmCallback(pSettlementInfoConfirm,repareInfo(pRspInfo),nRequestID,bIsLast);
}

void CCTPSpi::SetRspSettlementInfoConfirmCallback( RspSettlementInfoConfirmCallback pt2F )
{
	this->rspSettlementInfoConfirmCallback = pt2F;
}

#pragma endregion SetCallback and ResponseCallback


//针对收到空反馈的处理
CThostFtdcRspInfoField rif;
CThostFtdcRspInfoField* CCTPSpi::repareInfo( CThostFtdcRspInfoField *pRspInfo )
{
	if(pRspInfo==NULL)
	{
		memset(&rif,0,sizeof(rif));
		return &rif;
	}
	else
		return pRspInfo;
}

bool CCTPSpi::IsErrorRspInfo( CThostFtdcRspInfoField *pRspInfo )
{
	return true;
}

bool CCTPSpi::IsTradingOrder( CThostFtdcOrderField *pOrder )
{
	return true;
}
















}