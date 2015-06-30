#include "StdAfx.h"
#include "FutureTrader.h"
#include "defines.h"
#include "ThostFtdcTraderApi.h"
#include <iostream>
using namespace std;
#pragma warning(disable : 4996)
// USER_API参数

CFutureTrader::CFutureTrader(void)
{		
	hFutureDataMutex =  CreateMutex( NULL, FALSE, NULL );
	this->bConnected=false;
	this->bRunning=false;
}
CFutureTrader::~CFutureTrader(){
//	pTraderApi->Join();
}


bool CFutureTrader::init(Logininfor mylogininfor)
{	
	strcpy_s(addr,mylogininfor.serverAddr);
	strcpy_s(userAccount,mylogininfor.ZjAccount);
	strcpy_s(pwd,mylogininfor.PASSWORD);
	strcpy_s(BROKER_ID,mylogininfor.BROKER_ID);

	this->bConnected = false;
	pTraderApi = CThostFtdcTraderApi::CreateFtdcTraderApi();			// 创建UserApi
	pTraderApi->RegisterSpi((CThostFtdcTraderSpi*)this);				// 注册事件类
	pTraderApi->SubscribePublicTopic(TERT_QUICK);						// 注册公有流
	pTraderApi->SubscribePrivateTopic(TERT_QUICK);						// 注册私有流
	pTraderApi->RegisterFront(addr);									// connect
	pTraderApi->Init();
	return true;
}

bool CFutureTrader::sendtrader(Traderorderstruct  mytraderoder,char * OrderRef)   //交易请求
{
	this->bRunning=true;
	CThostFtdcInputOrderField req;
	memset(&req, 0, sizeof(req));
	///合约代码

	strcpy_s(req.BrokerID, BROKER_ID);
	///投资者代码
	strcpy_s(req.InvestorID, this->userAccount);	

	strcpy_s(req.InstrumentID, mytraderoder.cSecurity_code);
	///买卖方向: 
	req.Direction = mytraderoder.cTraderdirection;
	///组合开平标志: 开仓
	req.CombOffsetFlag[0] =mytraderoder.cOffsetFlag; 
	////数量: 1
	req.VolumeTotalOriginal =mytraderoder.nSecurity_amount;
	///价格
	req.LimitPrice = mytraderoder.dOrderprice;
		
	///报单引用
	strcpy_s(req.OrderRef,OrderRef);
	///用户代码
	///报单价格条件
	req.OrderPriceType =mytraderoder.cOrderPriceType;

	///组合投机套保标志
	req.CombHedgeFlag[0] = 
		THOST_FTDC_HF_Speculation;	
	///有效期类型: 当日有效
	req.TimeCondition = THOST_FTDC_TC_GFD;
	///GTD日期
	//	TThostFtdcDateType	GTDDate;
	///成交量类型: 任何数量
	req.VolumeCondition = THOST_FTDC_VC_AV;
	///最小成交量: 1
	req.MinVolume = 1;
	///触发条件: 立即
	req.ContingentCondition = THOST_FTDC_CC_Immediately;
	///止损价
	//	TThostFtdcPriceType	StopPrice;
	///强平原因: 非强平
	req.ForceCloseReason = THOST_FTDC_FCC_NotForceClose;
	///自动挂起标志: 否
	req.IsAutoSuspend = 0;
	///业务单元
	//	TThostFtdcBusinessUnitType	BusinessUnit;
	///请求编号
	//	TThostFtdcRequestIDType	RequestID;
	///用户强评标志: 否
	req.UserForceClose = 0;

	int iResult = pTraderApi->ReqOrderInsert(&req, ++iRequestID);
	if(iResult ==0)
	{		
		this->bRunning=false;
		return true;
	}
	this->bRunning=false;
	return false;
}

bool CFutureTrader::getconnectstate()//  返回交易情况 未连接 
{
	return this->bConnected;
}

bool  CFutureTrader::getworkstate() //返回是否被占用
{
	return this->bRunning;
}
void CFutureTrader::OnFrontConnected()
{
	
	///用户登录请求
	ReqUserLogin();
}

void CFutureTrader::ReqUserLogin()
{
	CThostFtdcReqUserLoginField req;
	memset(&req, 0, sizeof(req));
	strcpy_s(req.BrokerID, this->BROKER_ID);
	strcpy_s(req.UserID, this->userAccount);
	strcpy_s(req.Password, pwd);
	int iResult = pTraderApi->ReqUserLogin(&req, ++iRequestID);
	cerr << "--->>> 发送用户登录请求: " << ((iResult == 0) ? "成功" : "失败") << endl;
}

void CFutureTrader::OnRspUserLogin(CThostFtdcRspUserLoginField *pRspUserLogin,
		CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast)
{
	cerr << "--->>> " << "OnRspUserLogin" << endl;
	if (bIsLast && !IsErrorRspInfo(pRspInfo))
	{
		// 保存会话参数
		//CSysLoger::logMessage("CFutureTrader::OnRspUserLogin:登陆成功");
		FRONT_ID = pRspUserLogin->FrontID;
		SESSION_ID = pRspUserLogin->SessionID;
		m_nNextOrderRef = atoi(pRspUserLogin->MaxOrderRef);
		m_nNextOrderRef++;
		sprintf(ORDER_REF, "%d", m_nNextOrderRef);
		///获取当前交易日
		cerr << "--->>> 获取当前交易日 = " << pTraderApi->GetTradingDay() << endl;
		///投资者结算结果确认
		ReqSettlementInfoConfirm();		
		this->bConnected = true;
	}else
	{
		this->bConnected = false;
	}
}

void CFutureTrader::ReqSettlementInfoConfirm()
{
	CThostFtdcSettlementInfoConfirmField req;
	memset(&req, 0, sizeof(req));
	strcpy_s(req.BrokerID, BROKER_ID);
	strcpy_s(req.InvestorID, this->userAccount);
	int iResult = pTraderApi->ReqSettlementInfoConfirm(&req, ++iRequestID);
	cerr << "--->>> 投资者结算结果确认: " << ((iResult == 0) ? "成功" : "失败") << endl;
}

void CFutureTrader::OnRspSettlementInfoConfirm(CThostFtdcSettlementInfoConfirmField *pSettlementInfoConfirm, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast)
{
	
	if (bIsLast && !IsErrorRspInfo(pRspInfo))
	{
		
		this->bConnected = true;
	}else{
		
		this->bConnected = false;
	}
}

void CFutureTrader::ReqQryInstrument()
{
	CThostFtdcQryInstrumentField req;
	memset(&req, 0, sizeof(req));
	strcpy_s(req.InstrumentID, "IF1111");
	int iResult = pTraderApi->ReqQryInstrument(&req, ++iRequestID);
	cerr << "--->>> 请求查询合约: " << ((iResult == 0) ? "成功" : "失败") << endl;
}

void CFutureTrader::OnRspQryInstrument(CThostFtdcInstrumentField *pInstrument, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast)
{
	cerr << "--->>> " << "OnRspQryInstrument" << endl;
	if (bIsLast && !IsErrorRspInfo(pRspInfo))
	{
		///请求查询合约
		ReqQryTradingAccount();
	}
}

void CFutureTrader::ReqQryTradingAccount()
{
	CThostFtdcQryTradingAccountField req;
	memset(&req, 0, sizeof(req));
	strcpy_s(req.BrokerID, BROKER_ID);
	strcpy_s(req.InvestorID, userAccount);
	int iResult = pTraderApi->ReqQryTradingAccount(&req, ++iRequestID);
	cerr << "--->>> 请求查询资金账户: " << ((iResult == 0) ? "成功" : "失败") << endl;
}

void CFutureTrader::OnRspQryTradingAccount(CThostFtdcTradingAccountField *pTradingAccount, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast)
{
	cerr << "--->>> " << "OnRspQryTradingAccount" << endl;
	this->dBalance=0;
	this->dDeposit=0;	
	this->dUsable =0;
	this->dMargin=0;
	if (bIsLast && !IsErrorRspInfo(pRspInfo))
	{
		///请求查询投资者持仓
		//ReqQryInvestorPosition();
		this->dUsable = pTradingAccount->Available;
		this->dMargin = pTradingAccount->CurrMargin;
		this->dBalance = pTradingAccount->Balance;
		this->dDeposit = pTradingAccount->Deposit;

	}else{
		//AfxMessageBox("查询资金失败");
	}
}

void CFutureTrader::ReqQryInvestorPosition()
{
	CThostFtdcQryInvestorPositionField req;
	memset(&req, 0, sizeof(req));
	strcpy_s(req.BrokerID, BROKER_ID);
	strcpy_s(req.InvestorID, INVESTOR_ID);
	//strcpy_s(req.InstrumentID, INSTRUMENT_ID);
	int iResult = pTraderApi->ReqQryInvestorPosition(&req, ++iRequestID);
	cerr << "--->>> 请求查询投资者持仓: " << ((iResult == 0) ? "成功" : "失败") << endl;
}

void CFutureTrader::OnRspQryInvestorPosition(CThostFtdcInvestorPositionField *pInvestorPosition, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast)
{
	cerr << "--->>> " << "OnRspQryInvestorPosition" << endl;
	if (bIsLast && !IsErrorRspInfo(pRspInfo))
	{
		///报单录入请求
		//ReqOrderInsert();
	}
}


//只有在报单失败时才会回调这个函数
void CFutureTrader::OnRspOrderInsert(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast)
{
	if(IsErrorRspInfo(pRspInfo))
	{		
		//CSysLoger::logMessage("CFutureTrader::OnRspOrderInsert:报单失败");	
		//AfxMessageBox(pRspInfo->ErrorMsg);
		WaitForSingleObject( hFutureDataMutex, INFINITE ); 	

		strcpy(orderDb[pInputOrder->OrderRef].OrderRef,pInputOrder->OrderRef);
		orderDb[pInputOrder->OrderRef].issendfail=true;

		ReleaseMutex( hFutureDataMutex );
	}
	 
}
///报单通知
void CFutureTrader::OnRtnOrder(CThostFtdcOrderField *pOrder)
{	
	if (IsMyOrder(pOrder))
	{	
		WaitForSingleObject( hFutureDataMutex, INFINITE ); 	
		strcpy(orderDb[pOrder->OrderRef].OrderRef,pOrder->OrderRef);
		ReleaseMutex( hFutureDataMutex );
		
		switch(pOrder->OrderSubmitStatus)
		{
			case THOST_FTDC_OSS_InsertRejected:
			{
				
				//AfxMessageBox("报单失败:报单已经被拒绝");
			}
		}
	}
}
///成交通知
void CFutureTrader::OnRtnTrade(CThostFtdcTradeField *pTrade)
{	
	if(IsMytrader(pTrade))
	{
		WaitForSingleObject( hFutureDataMutex, INFINITE ); 	//成交回报全部存入数据
		strcpy(orderDb[pTrade->OrderRef].OrderRef,pTrade->OrderRef);
		ReleaseMutex( hFutureDataMutex );
	}
	
}

//暂时没有用到
void CFutureTrader::ReqOrderAction(CThostFtdcOrderField *pOrder)
{
	static bool ORDER_ACTION_SENT = false;		//是否发送了报单
	if (ORDER_ACTION_SENT)
		return;

	CThostFtdcInputOrderActionField req;
	memset(&req, 0, sizeof(req));
	///经纪公司代码
	strcpy_s(req.BrokerID, pOrder->BrokerID);
	///投资者代码
	strcpy_s(req.InvestorID, pOrder->InvestorID);
	///报单操作引用
//	TThostFtdcOrderActionRefType	OrderActionRef;
	///报单引用
	strcpy_s(req.OrderRef, pOrder->OrderRef);
	///请求编号
//	TThostFtdcRequestIDType	RequestID;
	///前置编号
	req.FrontID = FRONT_ID;
	///会话编号
	req.SessionID = SESSION_ID;
	///交易所代码
//	TThostFtdcExchangeIDType	ExchangeID;
	///报单编号
//	TThostFtdcOrderSysIDType	OrderSysID;
	///操作标志
	req.ActionFlag = THOST_FTDC_AF_Delete;
	///价格
//	TThostFtdcPriceType	LimitPrice;
	///数量变化
//	TThostFtdcVolumeType	VolumeChange;
	///用户代码
//	TThostFtdcUserIDType	UserID;
	///合约代码
	strcpy_s(req.InstrumentID, pOrder->InstrumentID);

	int iResult = pTraderApi->ReqOrderAction(&req, ++iRequestID);
	cerr << "--->>> 报单操作请求: " << ((iResult == 0) ? "成功" : "失败") << endl;
	ORDER_ACTION_SENT = true;
}
//暂时没用到
void CFutureTrader::OnRspOrderAction(CThostFtdcInputOrderActionField *pInputOrderAction, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast)
{
	cerr << "--->>> " << "OnRspOrderAction" << endl;
	IsErrorRspInfo(pRspInfo);
}


void CFutureTrader:: OnFrontDisconnected(int nReason)
{
	cerr << "--->>> " << "OnFrontDisconnected" << endl;
	cerr << "--->>> Reason = " << nReason << endl;
}		
void CFutureTrader::OnHeartBeatWarning(int nTimeLapse)
{	
	this->bConnected = false;
	cerr << "--->>> " << "OnHeartBeatWarning" << endl;
	cerr << "--->>> nTimerLapse = " << nTimeLapse << endl;
}
void CFutureTrader::OnRspError(CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast)
{
	cerr << "--->>> " << "OnRspError" << endl;
	IsErrorRspInfo(pRspInfo);
}

bool CFutureTrader::IsErrorRspInfo(CThostFtdcRspInfoField *pRspInfo)
{
	// 如果ErrorID != 0, 说明收到了错误的响应
	bool bResult = ((pRspInfo) && (pRspInfo->ErrorID != 0));
	if (bResult)
	{
		cerr << "--->>> ErrorID=" << pRspInfo->ErrorID << ", ErrorMsg=" << pRspInfo->ErrorMsg << endl;
	}
	return bResult;
}
bool CFutureTrader::IsMyOrder(CThostFtdcOrderField *pOrder)
{
	return ((pOrder->FrontID == FRONT_ID) &&
		(pOrder->SessionID == SESSION_ID));
}
bool CFutureTrader::IsMytrader(CThostFtdcTradeField *pTrade)
{
	return true;
}
bool CFutureTrader::IsTradingOrder(CThostFtdcOrderField *pOrder)
{
	return ((pOrder->OrderStatus != THOST_FTDC_OST_PartTradedNotQueueing) &&
			(pOrder->OrderStatus != THOST_FTDC_OST_Canceled) &&
			(pOrder->OrderStatus != THOST_FTDC_OST_AllTraded));
}

bool CFutureTrader::buyOpen(char * code,double price,int Qty){
	
	CThostFtdcInputOrderField req;
	memset(&req, 0, sizeof(req));
	///合约代码
	strcpy_s(req.InstrumentID, code);
	///买卖方向: 
	req.Direction = THOST_FTDC_D_Buy;
	///组合开平标志: 开仓
	req.CombOffsetFlag[0] = THOST_FTDC_OF_Open;
	///价格
	req.VolumeTotalOriginal = Qty;
	///数量: 1
	req.LimitPrice = price;
		
	///报单引用
	strcpy_s(req.OrderRef, "12345");
	m_nNextOrderRef++;
	sprintf(ORDER_REF, "%d", m_nNextOrderRef);
	///用户代码
	///报单价格条件: 限价
	req.OrderPriceType = THOST_FTDC_OPT_LimitPrice;
	///经纪公司代码
	strcpy_s(req.BrokerID, BROKER_ID);
	///投资者代码
	strcpy_s(req.InvestorID, this->userAccount);	
	///组合投机套保标志
	req.CombHedgeFlag[0] = THOST_FTDC_HF_Speculation;	
	///有效期类型: 当日有效
	req.TimeCondition = THOST_FTDC_TC_GFD;
	///GTD日期
	//	TThostFtdcDateType	GTDDate;
	///成交量类型: 任何数量
	req.VolumeCondition = THOST_FTDC_VC_AV;
	///最小成交量: 1
	req.MinVolume = 1;
	///触发条件: 立即
	req.ContingentCondition = THOST_FTDC_CC_Immediately;
	///止损价
	//	TThostFtdcPriceType	StopPrice;
	///强平原因: 非强平
	req.ForceCloseReason = THOST_FTDC_FCC_NotForceClose;
	///自动挂起标志: 否
	req.IsAutoSuspend = 0;
	///业务单元
	//	TThostFtdcBusinessUnitType	BusinessUnit;
	///请求编号
	//	TThostFtdcRequestIDType	RequestID;
	///用户强评标志: 否
	req.UserForceClose = 0;

	int iResult = pTraderApi->ReqOrderInsert(&req, ++iRequestID);
	if(iResult ==0){		
	//	CString logMsg;
		//logMsg.Format("买入开仓发单成功;合约:%s;价格:%.2f;数量:%d;order_ref:%s",code,price,Qty,ORDER_REF);		
		//CSysLoger::logMessage(logMsg);
		return true;
	}
	return false;
}
bool CFutureTrader::sellOpen(char * code,double price,int Qty){//卖出开仓
//	return true;
	
	CThostFtdcInputOrderField req;
	memset(&req, 0, sizeof(req));
	///合约代码
	strcpy_s(req.InstrumentID, code);
	///买卖方向: 
	req.Direction = THOST_FTDC_D_Sell;
	///组合开平标志: 开仓
	req.CombOffsetFlag[0] = THOST_FTDC_OF_Open;
	///价格
	req.VolumeTotalOriginal = Qty;
	///数量: 1
	req.LimitPrice =price;

	///报单引用
	//strcpy_s(req.OrderRef, ORDER_REF);
	m_nNextOrderRef++;
	sprintf(ORDER_REF, "%d", m_nNextOrderRef);
	///用户代码
	///报单价格条件: 限价
	req.OrderPriceType = THOST_FTDC_OPT_LimitPrice;
	///经纪公司代码
	strcpy_s(req.BrokerID, BROKER_ID);
	///投资者代码
	strcpy_s(req.InvestorID,  this->userAccount);	
	///组合投机套保标志
	req.CombHedgeFlag[0] = THOST_FTDC_HF_Speculation;	
	///有效期类型: 当日有效
	req.TimeCondition = THOST_FTDC_TC_GFD;
	///GTD日期
	//	TThostFtdcDateType	GTDDate;
	///成交量类型: 任何数量
	req.VolumeCondition = THOST_FTDC_VC_AV;
	///最小成交量: 1
	req.MinVolume = 1;
	///触发条件: 立即
	req.ContingentCondition = THOST_FTDC_CC_Immediately;
	///止损价
	//	TThostFtdcPriceType	StopPrice;
	///强平原因: 非强平
	req.ForceCloseReason = THOST_FTDC_FCC_NotForceClose;
	///自动挂起标志: 否
	req.IsAutoSuspend = 0;
	///业务单元
	//	TThostFtdcBusinessUnitType	BusinessUnit;
	///请求编号
	//	TThostFtdcRequestIDType	RequestID;
	///用户强评标志: 否
	req.UserForceClose = 0;

	int iResult = pTraderApi->ReqOrderInsert(&req, ++iRequestID);
	if(iResult ==0){
		//CString logMsg;
		//logMsg.Format("卖出开仓发单成功;合约:%s;价格:%.2f;数量:%d;order_ref:%s",code,price,Qty,ORDER_REF);		
		//CSysLoger::logMessage(logMsg);
		return true;
	}
	return false;
}
bool CFutureTrader::buyClose(char * code,double price,int Qty){//买入平仓
	
	CThostFtdcInputOrderField req;
	memset(&req, 0, sizeof(req));
	///合约代码
	strcpy_s(req.InstrumentID,  code);
	///买卖方向: 
	req.Direction = THOST_FTDC_D_Buy;
	///组合开平标志: 开仓
	req.CombOffsetFlag[0] = THOST_FTDC_OF_Close;
	///价格
	req.VolumeTotalOriginal = Qty;
	///数量: 1
	req.LimitPrice = price;

	///报单引用
	strcpy_s(req.OrderRef, ORDER_REF);
	m_nNextOrderRef++;
	sprintf(ORDER_REF, "%d", m_nNextOrderRef);
	///用户代码
	///报单价格条件: 限价
	req.OrderPriceType = THOST_FTDC_OPT_LimitPrice;
	///经纪公司代码
	strcpy_s(req.BrokerID, BROKER_ID);
	///投资者代码
	strcpy_s(req.InvestorID, this->userAccount);	
	///组合投机套保标志
	req.CombHedgeFlag[0] = THOST_FTDC_HF_Speculation;	
	///有效期类型: 当日有效
	req.TimeCondition = THOST_FTDC_TC_GFD;
	///GTD日期
	//	TThostFtdcDateType	GTDDate;
	///成交量类型: 任何数量
	req.VolumeCondition = THOST_FTDC_VC_AV;
	///最小成交量: 1
	req.MinVolume = 1;
	///触发条件: 立即
	req.ContingentCondition = THOST_FTDC_CC_Immediately;
	///止损价
	//	TThostFtdcPriceType	StopPrice;
	///强平原因: 非强平
	req.ForceCloseReason = THOST_FTDC_FCC_NotForceClose;
	///自动挂起标志: 否
	req.IsAutoSuspend = 0;
	///业务单元
	//	TThostFtdcBusinessUnitType	BusinessUnit;
	///请求编号
	//	TThostFtdcRequestIDType	RequestID;
	///用户强评标志: 否
	req.UserForceClose = 0;

	int iResult = pTraderApi->ReqOrderInsert(&req, ++iRequestID);
	if(iResult ==0){		
		//CString logMsg;
		//logMsg.Format("买入平仓发单成功;合约:%s;价格:%.2f;数量:%d;order_ref:%s",code,price,Qty,ORDER_REF);		
		//CSysLoger::logMessage(logMsg);
		return true;
	}
	return false;
}
bool CFutureTrader::sellClose(char * code,double price,int Qty){
	
	CThostFtdcInputOrderField req;
	memset(&req, 0, sizeof(req));
	///合约代码
	strcpy_s(req.InstrumentID,  code);
	///买卖方向: 
	req.Direction = THOST_FTDC_D_Sell;
	///组合开平标志: 开仓
	req.CombOffsetFlag[0] = THOST_FTDC_OF_Close;
	///价格
	req.VolumeTotalOriginal = Qty;
	///数量: 1
	req.LimitPrice = price;

	///报单引用
	strcpy_s(req.OrderRef, ORDER_REF);
	m_nNextOrderRef++;
	sprintf(ORDER_REF, "%d", m_nNextOrderRef);
	///用户代码
	///报单价格条件: 限价
	req.OrderPriceType = THOST_FTDC_OPT_LimitPrice;
	///经纪公司代码
	strcpy_s(req.BrokerID, BROKER_ID);
	///投资者代码
	strcpy_s(req.InvestorID,  this->userAccount);	
	///组合投机套保标志
	req.CombHedgeFlag[0] = THOST_FTDC_HF_Speculation;	
	///有效期类型: 当日有效
	req.TimeCondition = THOST_FTDC_TC_GFD;
	///GTD日期
	//	TThostFtdcDateType	GTDDate;
	///成交量类型: 任何数量
	req.VolumeCondition = THOST_FTDC_VC_AV;
	///最小成交量: 1
	req.MinVolume = 1;
	///触发条件: 立即
	req.ContingentCondition = THOST_FTDC_CC_Immediately;
	///止损价
	//	TThostFtdcPriceType	StopPrice;
	///强平原因: 非强平
	req.ForceCloseReason = THOST_FTDC_FCC_NotForceClose;
	///自动挂起标志: 否
	req.IsAutoSuspend = 0;
	///业务单元
	//	TThostFtdcBusinessUnitType	BusinessUnit;
	///请求编号
	//	TThostFtdcRequestIDType	RequestID;
	///用户强评标志: 否
	req.UserForceClose = 0;

	int iResult = pTraderApi->ReqOrderInsert(&req, ++iRequestID);
	if(iResult ==0){
		//CString logMsg;
		//logMsg.Format("卖出平仓发单成功;合约:%s;价格:%.2f;数量:%d;order_ref:%s",code,price,Qty,ORDER_REF);		
		//CSysLoger::logMessage(logMsg);
		return true;
	}
	return false;
}
