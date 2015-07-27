#include "StdAfx.h"
#include "Option.h"


COption::COption(void)
{
	this->m_DepthMarketData.msecurity.cSecuritytype='O';
	this->m_DepthMarketData.nMarketdepth=10;
}


COption::~COption(void)
{
}
