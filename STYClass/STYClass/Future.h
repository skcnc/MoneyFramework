#pragma once
#include "security.h"
class CFuture :
	public CSecurity
{
public:
	CFuture(void);
	~CFuture(void);

public:
	int   indextimes;   //指数与对应标的物的倍数  
public:
	int   getfuturetime();  //根据期货代码获得指数倍数
	double  getrealmarketvalue(int namount);

};

