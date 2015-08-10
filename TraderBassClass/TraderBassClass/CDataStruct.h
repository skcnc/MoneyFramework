#include "stdafx.h"
#include <stdio.h>
#include <string.h>

#ifndef __DATASTRUCT_H
#define __DATASTRUCT_H

struct indexweightstruct  //指数型配置文件
{
	char       cSecuritytype;          // 证券类型
	char       cSecurity_code[31];     // 证券代码
	double     dweight;				   // 权重
};

struct stockpotionstruct    //持仓显示文件 
{
	char       cSecuritytype;          // 证券类型
	char       cSecurity_code[31];     // 证券代码
	int        ntradervolume;		   //数量
	bool       bstoped;                //当前状态（是否停牌）
	double     dlastprice;             // 当前价格
};

/*****************交易相关***********************************************/
struct  Clientinfor //客户端编码信息 含策略类型
{
};

struct Logininfor // 登陆信息
{
	char * serverAddr;// 地址
	int nPort;//端口
	char * ZjAccount; //资金
	char * BROKER_ID;   //部门编号
	char *  PASSWORD;    //密码
	char * INVESTOR_ID;  //账户

	Logininfor()
	{
		serverAddr = "0";
		nPort = 0;
		ZjAccount = "0";
		BROKER_ID = "0";
		PASSWORD = "0";
		INVESTOR_ID = "0";
	}

	void getInit(Logininfor info)
	{
		//strcpy(serverAddr,info.serverAddr);
		strcpy_s(serverAddr, strlen(info.serverAddr), info.serverAddr);
		nPort = info.nPort;
		//strcpy(ZjAccount,info.ZjAccount);
		strcpy_s(ZjAccount, strlen(info.ZjAccount), info.ZjAccount);
		//strcpy(BROKER_ID,info.BROKER_ID);
		strcpy_s(BROKER_ID, strlen(info.BROKER_ID), info.BROKER_ID);
		strcpy(PASSWORD, info.PASSWORD);
		strcpy(INVESTOR_ID, info.INVESTOR_ID);
	}

};

struct Traderorderstruct   //交易报单（买卖 申购 ）
{

	//交易部分
	char    cExchangeID[21];            //交易所
	char    cSecurity_code[31];     // 证券代码
	char    security_name[55];      //证券名称
	long    nSecurity_amount;      // 委托数量
	double  dOrderprice;           // 委托价格
	char    cTraderdirection;      // 买卖类别（见数据字典说明）
	char    cOffsetFlag;           //开平标志
	char    cOrderPriceType;       //报单条件(限价  市价)

	//控制部分
	char    cSecuritytype;          //证券类型	
	char    cOrderlevel;             //报单优先级 执行顺序
	char    cOrderexecutedetail;     //报单执行细节
	//标志信息
	Clientinfor myclientinfor; //

	void getInit(Traderorderstruct stu)
	{
		memcpy(cExchangeID, stu.cExchangeID, 21);
		memcpy(cSecurity_code, stu.cSecurity_code, 31);
		memcpy(security_name, stu.security_name, 55);
		nSecurity_amount = stu.nSecurity_amount;
		dOrderprice = stu.dOrderprice;
		cTraderdirection = stu.cTraderdirection;
		cOffsetFlag = stu.cOffsetFlag;
		cOrderPriceType = stu.cOrderPriceType;

		cSecuritytype = stu.cSecuritytype;
		cOrderlevel = stu.cOrderlevel;
		cOrderexecutedetail = stu.cOrderexecutedetail;

		myclientinfor = stu.myclientinfor;
	}
};

struct  QueryEntrustorderstruct
{
	char    cSecuritytype;          //证券类型	
	char    cExchangeID[21];            //交易所
	char	cOrderSysID[21];  	   ///报单编号
	//标志信息
	Clientinfor myclientinfor;

	void getInit(QueryEntrustorderstruct entrust)
	{
		cSecuritytype = entrust.cSecuritytype;
		memcpy(cExchangeID, entrust.cExchangeID, 21);
		memcpy(cOrderSysID, entrust.cOrderSysID, 21);
		myclientinfor = entrust.myclientinfor;
	}
};

struct Entrustreturnstruct //委托回报
{
	char    cSecurity_code[31];     // 证券代码
	char    security_name[18];
	char	cOrderSysID[21]; 	///报单编号
	char	cOrderStatus;///报单状态
	char	cOrderType;///报单类型
	long    nVolumeTotalOriginal;  //委托数量
	long	nVolumeTraded;	///今成交数量
	long	nVolumeTotal;   ///剩余数量
	long     withdraw_ammount;     //撤单数量
	double   frozen_money;    //  冻结金额/基金申请金额
	long     frozen_amount;     //冻结证券数量
	char	cInsertDate[9];	///报单日期
	char	cInsertTime[9];	///委托时间
	char	cCancelTime[9];///撤销时间
	char  error_msg[255];   //错误原因
	//标志信息
	Clientinfor myclientinfor;
};

struct Bargainreturnstruct  //成交回报
{
	char    cSecurity_code[31];         // 证券代码
	char    security_name[18];
	char	cOrderSysID[21]; 			///报单编号
	char	cOrderStatus;				///报单状态
	char	cOrderType;					///报单类型
	long    stock_ammount;         		// 成交数量
	double  bargain_price;        		// 成交价格
	double  bargain_money;             	// 成交金额
	char    bargain_time[9];       		// 成交时间
	long    bargain_no;            		// 成交编号
	double  unfrozen_money;        //解冻资金金额(A股有效，B股返回0)
	long    unfrozen_amount;         //解冻证券数量(A股有效，B股返回0)
	//标志信息
	Clientinfor myclientinfor;
};

struct QueryBalanceSheetReturn
{

};

/*******************期货映射数据****************/
struct  FuTuremapinfor
{
	char  OrderRef[31];  //自定义
	bool issendfail; //发送是否失败（CTP拒绝发送）
	bool isreject;   //交易所是否拒绝
	char  errormsg[255];
	QueryEntrustorderstruct  myEntrust;
	Entrustreturnstruct      myoderreturn;
	Bargainreturnstruct      mytraderreturn;
	FuTuremapinfor()
	{
		issendfail = false;
		isreject = false;
	}

};

#endif