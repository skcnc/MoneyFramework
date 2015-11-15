// TraderBassClass.h
#include "AGCClient.h"
#include "CDataStruct.h"
#include "ThostFtdcTraderApi.h"
#include <windows.h>
#include <map>
#pragma once
#pragma comment(lib, "WS2_32")


class CStockTrader
{
public:
	CStockTrader(void);
	~CStockTrader(void);


public:
	bool init(const Logininfor mylogininfor, char * Errormsg);  //加载参数,登陆
	bool trader(const Traderorderstruct  mytraderoder, QueryEntrustorderstruct &myEntrust, char * Errormsg);      //单个证券交易,myEntrust引用返回委托编号
	bool Batchstocktrader(const Traderorderstruct * mytraderoder,const  int nSize, QueryEntrustorderstruct **  pmyEntrust, int &num, char * Errormsg); //多个证券交易,myEntrust引用返回委托编号

	bool canceltrader(const QueryEntrustorderstruct myEntrust, char * Errormsg);      //撤单 
	bool queryorder(const QueryEntrustorderstruct myEntrust, Entrustreturnstruct * myoderreturn, int &num, char * Errormsg);      //查询委托
	bool querytrader(const QueryEntrustorderstruct myEntrust, Bargainreturnstruct * mytraderreturn, int &num, char * Errormsg);      //查询成交

	bool heartBeat();  //心跳函数
	bool getconnectstate();//  返回交易情况 未连接 
	bool getworkstate(); //返回是否被占用
	int  cal(char* msg);
private:
	void loadArgs(char * SERVER_IP, int N_PORT, char *ZJ_ACCOUNT, char *PASSWORD);//加载参数(账号，密码，服务器地址）
	bool connectToServer();//连接到服务器
	bool openAccount();//先打开账号
	bool login();//然后登陆用户,得到上证A股和深圳A股的账号	

	//bool buyBatchStocks(SampleStock * stocksToBuy,int nSize);//批量买入
	//bool sellBatchStocks(SampleStock * stocksToSell,int nSize);//批量卖出
	char * getErrorCodeMsg(int nErrorCode);//通过错误号得到错误文本消息
	bool queryBalanceSheet(SWI_QueryBalanceSheetReturn * pRetunResult);//查询资金状态

private:
	/************变量************/
	SOCKET sock;//socket

	char  serverIP[49];//ip地址
	char  ZjAccount[49];//资金账号
	int nPort;//端口
	char  ShAccount[49];//上海A股账号(login时会自动获得）
	char  SzAccount[49];//深圳A股账号(login时会自动获得）
	char  password[49];//交易密码
	int nDept;//部门编号，通过资金账号解析得出（在loadArgs中）


	bool bConnected;//连接成功时置为true	
	bool bRunning;  //是否正在运行
	int Id;//设置一个编号，以便在调试过程中

};
