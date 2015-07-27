#pragma once
#include "security.h"


class CSyntheticSecurity :
	public CSecurity
{
public:
	CSyntheticSecurity(void);
   ~CSyntheticSecurity(void);

private:
	double  dsimprice;     //模拟价格/指数
	double  dtraderprice;  //交易价格（考虑停牌 冲击）

public:
   virtual  double  getSimIndex()=0;
   double  getSimError();
};

