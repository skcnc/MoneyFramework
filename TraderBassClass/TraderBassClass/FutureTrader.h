#pragma once
#include <windows.h>
#include "ThostFtdcTraderApi.h"
#include "TraderBassClass.h"
#include <map>
using namespace std;
//#pragma pack(4)
class CFutureTrader : public CThostFtdcTraderSpi
{
public:
	bool init(const Logininfor mylogininfor);  //加载参数,登陆 异步通信  调用需等待回报

	bool sendtrader(Traderorderstruct  mytraderoder,char * OrderRef);    //交易请求
	

	bool sendcanceltrader(QueryEntrustorderstruct myEntrust);      //撤单请求 

	bool sendqueryorder(QueryEntrustorderstruct  myEntrust);      //发送查询委托请求
	bool sendquerytrader(QueryEntrustorderstruct myEntrust);      //发送查询成交请求

	bool sendqueryBalanceSheet();   //查询资金状态
	bool getQueryBalance(QueryBalanceSheetReturn & pRetunResult);//获得资金

	bool getorderstute (char * OrderRef, Entrustreturnstruct &myoderreturn,char * Errormsg); //根据OrderRef活动报单状态  报单状态中含有委托编号
	bool gettraderstute(char * OrderRef,Bargainreturnstruct  &mytraderreturn,char * Errormsg);

	bool getorderstute(QueryEntrustorderstruct myEntrust, Entrustreturnstruct &myoderreturn,char * Errormsg);  //重载 根据委托编号获取报单状态
	bool gettraderstute(QueryEntrustorderstruct myEntrust,Bargainreturnstruct &mytraderreturn,char * Errormsg); //重载
	

	bool heartBeat();  //心跳函数
	bool getconnectstate();//  返回交易情况 未连接 
	bool getworkstate();  //  返回是否被占用

	CFutureTrader(void);
	~CFutureTrader();
private:
	map<char *,FuTuremapinfor> orderDb;
	HANDLE hFutureDataMutex;//期货行情更新的锁
	/************变量******************/
	TThostFtdcFrontIDType	FRONT_ID;	//前置编号
    TThostFtdcSessionIDType	SESSION_ID;	//会话编号
    TThostFtdcOrderRefType	ORDER_REF;	//报单引用
	// 配置参数
	char    FRONT_ADDR[255] ;
	char   BROKER_ID[255]  ;
	char   INVESTOR_ID[255];
	char   PASSWORD[55] ;
	char   addr[255];
	char   pwd[55];
	char   userAccount[55];
	int iRequestID;

	bool bConnected;//登陆成功后为true
	bool bRunning;  //
	int m_nNextOrderRef;
	
	TThostFtdcMoneyType dBalance;
	TThostFtdcMoneyType dDeposit;
	TThostFtdcMoneyType dUsable;	
	TThostFtdcMoneyType dMargin;

	CThostFtdcTraderApi* pTraderApi;
	/************方法******************/



	bool buyOpen(char * code,double price,int Qty);
	bool buyClose(char * code,double price,int Qty);
	bool sellOpen(char *code,double price,int Qty);
	bool sellClose(char * code,double price,int Qty);

	///当客户端与交易后台建立起通信连接时（还未登录前），该方法被调用。
	virtual void OnFrontConnected();

	///登录请求响应
	virtual void OnRspUserLogin(CThostFtdcRspUserLoginField *pRspUserLogin,	CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

	///投资者结算结果确认响应
	virtual void OnRspSettlementInfoConfirm(CThostFtdcSettlementInfoConfirmField *pSettlementInfoConfirm, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);
	
	///请求查询合约响应
	virtual void OnRspQryInstrument(CThostFtdcInstrumentField *pInstrument, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

	///请求查询资金账户响应
	virtual void OnRspQryTradingAccount(CThostFtdcTradingAccountField *pTradingAccount, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

	///请求查询投资者持仓响应
	virtual void OnRspQryInvestorPosition(CThostFtdcInvestorPositionField *pInvestorPosition, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

	///报单录入请求响应
	virtual void OnRspOrderInsert(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

	///报单操作请求响应
	virtual void OnRspOrderAction(CThostFtdcInputOrderActionField *pInputOrderAction, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

	///错误应答
	virtual void OnRspError(CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);
	
	///当客户端与交易后台通信连接断开时，该方法被调用。当发生这个情况后，API会自动重新连接，客户端可不做处理。
	virtual void OnFrontDisconnected(int nReason);
		
	///心跳超时警告。当长时间未收到报文时，该方法被调用。
	virtual void OnHeartBeatWarning(int nTimeLapse);
	
	///报单通知
	virtual void OnRtnOrder(CThostFtdcOrderField *pOrder);

	///成交通知
	virtual void OnRtnTrade(CThostFtdcTradeField *pTrade);
	

	///用户登录请求
	void ReqUserLogin();
	///投资者结算结果确认
	void ReqSettlementInfoConfirm();
	///请求查询合约
	void ReqQryInstrument();
	///请求查询资金账户
	void ReqQryTradingAccount();
	///请求查询投资者持仓
	void ReqQryInvestorPosition();

	///报单操作请求
	void ReqOrderAction(CThostFtdcOrderField *pOrder);

	// 是否收到成功的响应
	bool IsErrorRspInfo(CThostFtdcRspInfoField *pRspInfo);
	// 是否我的报单回报
	bool IsMyOrder(CThostFtdcOrderField *pOrder);
	// 是否我的成交回报
	bool IsMytrader(CThostFtdcTradeField *pTrade);

	// 是否正在交易的报单
	bool IsTradingOrder(CThostFtdcOrderField *pOrder);
	
};

