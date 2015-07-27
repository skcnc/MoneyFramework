#include "StdAfx.h"
#include "Stock.h"


CStock::CStock(void)
{
	this->m_DepthMarketData.msecurity.cSecuritytype='s';
	this->m_DepthMarketData.nMarketdepth=10;
}


CStock::~CStock(void)
{
}
