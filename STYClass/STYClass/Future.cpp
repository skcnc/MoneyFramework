#include "StdAfx.h"
#include "Future.h"


CFuture::CFuture(void)
{
	this->m_DepthMarketData.msecurity.cSecuritytype = 'f';
	this->m_DepthMarketData.nMarketdepth = 5;
}


CFuture::~CFuture(void)
{
}

int  CFuture::getfuturetime()
{
	return indextimes;
}

double  CFuture::getrealmarketvalue(int namount)
{
	return this->getlastprice()*namount;

}
bool  CFuture::setcode(char * temp)
{
	strcpy(this->m_DepthMarketData.msecurity.cSecurity_code, temp);
	const char* ch = this->m_DepthMarketData.msecurity.cSecurity_code;

	if (ch[1] == 'C')
		indextimes = 200;
	else
		indextimes = 300;

	return true;
}
