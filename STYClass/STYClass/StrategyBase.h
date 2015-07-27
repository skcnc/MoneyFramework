#pragma once
#include "Security.h"
class CStrategyBase
{
public:
	CStrategyBase(void);
	~CStrategyBase(void);
private:
	bool bisnewsubscribed;  //是否有新的行情需要订阅
public:
	/************行情部分********/
	virtual bool    updateSecurityInfo(MarketInforStruct *,int &num)=0;      //获得行情信息  
	virtual bool    getsubscribelist(securityindex *,int& num)=0;            //获得订阅的股票    
	virtual bool    isnewsubscribed(){return bisnewsubscribed;};            //获取是否有行情需要更新
	virtual void    setsubscribestatus(bool bstatus ){bisnewsubscribed=bstatus;};    //设置行情订阅状态

	/**********界面交换数据*******/
	virtual bool    onUpdateArgs()=0;		        //参数接收
	virtual bool    showStatus()=0;                 //推送显示参数

	/**********策略执行*******/
	//virtual bool    init()=0;				      //初始化设置，导入权重数据  更新股票列表   订阅行情    
	//bool   loadargs();				//初始化设置，导入权重数据  更新股票列表   订阅行情   
	bool   cal();
	bool   istraderallowed();

//	bool   gettadersamp(Traderorderstruct *, int &num);  
	bool   getshowstatus(char *);
	/*************交易****************************/

	bool   gettaderlist(Traderorderstruct *, int &num);  
	
};

