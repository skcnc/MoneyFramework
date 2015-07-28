#pragma once
#include "SyntheticSecurity.h"
#include "Stock.h"
#include "Index.h"
#include"list"
#include"map"
using namespace std;  



class CSyntheticIndex:public CSyntheticSecurity
{
  private:
	double  dSimIndex;
	double  marketvalue;
	list<indexweightstruct>  stockweight;  //权重文件CSecurity
	CIndex      m_index;              //指数

 public:
    map<securityindex ,CSecurity *>  stockDb;  //行情数据
	list<stockpotionstruct>   m_positionlist;   //持仓交易文件  外部可更改

 public:
	CSyntheticIndex(void);
   ~CSyntheticIndex(void);

public:
	double  getSimIndex();  //获得模拟指数

	double  getrealmarketvalue();  //市值
	double  getrealbuycost();  
	double  getrealsellgain();

	bool    isupdated();    //行情是否更新

	bool   init(indexweightstruct * indexweightlist,int weightnum, stockpotionstruct * stockpotionlist,int potionnum,char *indexCode);  //初始行情  

	bool   updatepositioninfor();  //根据手数更新positon文件

	bool   isSecurityFocused(securityindex   SecurityCode);
	void updateInfo(MarketInforStruct *);
};

