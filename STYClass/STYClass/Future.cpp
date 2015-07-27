#include "StdAfx.h"
#include "Future.h"


CFuture::CFuture(void)
{
	this->m_DepthMarketData.msecurity.cSecuritytype='f';
	this->m_DepthMarketData.nMarketdepth=1;
}


CFuture::~CFuture(void)
{
}

int  CFuture::   getfuturetime()
	{
		const char* ch = this->m_DepthMarketData.msecurity.cSecurity_code;
	if(ch[1]=='C')
		return 200;
	return 300;
	}

double  CFuture::getrealmarketvalue(int namount)
	{
		return this->getlastprice()*(this->indextimes)*namount;

	}
