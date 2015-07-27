#pragma once
#include "security.h"
class COption :
	public CSecurity
{
public:
	COption(void);
	~COption(void);

public:
	int     ndate_expire;     //到期日
	double  dstrikeprice;  //执行价
};

