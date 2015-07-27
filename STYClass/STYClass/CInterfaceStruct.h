#ifndef __INTERFACESTRUCT_H
#define __INTERFACESTRUCT_H
#include "CDataStruct.h"

struct IndexFutureArbitrageopeninputargs  //界面输入参数
{
	indexweightstruct *       weightlist;			 //权重文件
	int   weightlistnum;  //列表数量

	stockpotionstruct   *    positionlist;		 //显示持仓
	int   positionlistnum;  //列表数量

	int    nHands;      //手数
	char   indexCode[32];   //指数
	char   contractCode[32]; //期货合约
    double dPositiveOpenDelta;    //开仓点位

	bool  bTradingAllowed;//是否允许交易,勾"允许"时置为true	
};



struct IndexFutureArbitragecloseargs
{
	list<stockpotionstruct>       m_positionlist;		 //显示持仓
	int    nHands;      //手数
	char   indexCode[32];   //指数类型
	char   contractCode[32]; //期货合约
    double dPositiveOpenDelta;    //开仓点位

	bool  bTradingAllowed;//是否允许交易,勾"允许"时置为true	
};

#endif