#pragma once
#include "syntheticsecurity.h"
class CSyntheticOptionUnderlying :
	public CSyntheticSecurity
{
public:
	CSecurity * m_underlying;  //±êµÄ
public:
	CSyntheticOptionUnderlying(void);
	~CSyntheticOptionUnderlying(void);
};

