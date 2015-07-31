#pragma once
#include "SyntheticSecurity.h"
#include "Stock.h"
#include "Index.h"
#include"list"
#include"map"
using namespace std;  



class CSyntheticIndex:public CSyntheticSecurity  //如果不考虑weight文件 则可以
{
  private:
	list<indexweightstruct>  stockweight;  //权重文件CSecurity
	CIndex      m_index;              //指数

 public:
    map<securityindex ,CSecurity *>  stockDb;  //行情数据
	list<stockpotionstruct>   m_positionlist;   //持仓交易文件  外部可更改

 public:
	CSyntheticIndex(void);
   ~CSyntheticIndex(void);

public:
	double  getSimIndex();  //获得weight文件模拟指数

	double  getrealmarketvalue(double & stopmarketvalue);  //postion列表市值
	double  getrealbuycost();      //买入成本
	double  getrealsellgain();     //卖出收益

	bool    isupdated();    //DB行情是否更新

	bool   init(indexweightstruct * indexweightlist,int weightnum, stockpotionstruct * stockpotionlist,int potionnum,char *indexCode);  //初始行情  

	bool   updatepositioninfor();  //更新positon文件，获取价格和状态

	bool   isSecurityFocused(securityindex   SecurityCode);

	void updateInfo(MarketInforStruct *); //更新stockDB中的行情
};

